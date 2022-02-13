using TdLib;
using TgApi.Types;
using static TgApi.Utils;
using static TdLib.TdApi.AuthorizationState;

namespace TgApi.Telegram;

public class AuthHandler
{
    private TdClient client;
    private TdApi.AuthorizationState state;
    private DateTimeOffset stateTime;
    private EventHandler<TdApi.Update> handlerDelegate;

    /// <summary>
    /// The current state of the authorization process
    /// </summary>
    public TdApi.AuthorizationState CurrentState => state;
    
    /// <summary>
    /// The time the last authentication update was received at
    /// </summary>
    public DateTimeOffset LastRequestReceivedAt => stateTime;
    
    /// <summary>
    /// The TdClient associated with this AuthHandler instance
    /// </summary>
    public TdClient Client => client;
    
    /// <summary>
    /// Instantiates an AuthHandler object
    /// </summary>
    /// <param name="Client">An active TdClient</param>
    public AuthHandler(TdClient Client)
    {
        client = Client;
        handlerDelegate = (object? sender, TdApi.Update update) =>
        {
            if (update is TdApi.Update.UpdateAuthorizationState)
            {
                state = ((TdApi.Update.UpdateAuthorizationState) update).AuthorizationState;
                stateTime = DateTimeOffset.Now;
                Console.WriteLine($"State: {state}, Time: {stateTime}");
            }
        };
        Client.UpdateReceived += handlerDelegate;
    }

    /// <summary>
    /// Sets fields to null so the garbage collector hopefully gets rid of it
    /// </summary>
    public void Close()
    {
        Client.UpdateReceived -= handlerDelegate;
        handlerDelegate = null;
        state = null;
        client = null;
    }
    
    /// <summary>
    /// Sign in process through the Command Line
    /// </summary>
    /// <param name="milliTimout">How long to wait between requests before timing out and failing</param>
    /// <exception cref="Exception">Throws an exception when the AuthHandler reaches an unsupported state</exception>
    public async Task SigninCLI(int milliTimout = 20000)
    {
        while (CurrentState == null) await Task.Delay(100); // Waiting for update to signify that client is initialized
        
        // This is to keep track of whether or not a new status has been received
        DateTimeOffset lastRunStatusTime = DateTimeOffset.MinValue; 
        
        while (CurrentState.GetType() != typeof(TdApi.AuthorizationState.AuthorizationStateReady))
        {
            bool unsupported = false;
            TdApi.Ok? val = null;
            switch (GetState(CurrentState)) 
            {
                case AuthState.WaitTdLibParams:
                    val = await Handle_WaitTdLibParameters(new TdApi.TdlibParameters
                    {
                        ApiId = GlobalVars.ApiId,
                        ApiHash = GlobalVars.ApiHash,
                        SystemLanguageCode = GlobalVars.SystemLanguageCode,
                        DeviceModel = GlobalVars.DeviceModel,
                        ApplicationVersion = GlobalVars.ApplicationVersion,
                        DatabaseDirectory = GlobalVars.TdDir
                    });
                    break;
                case AuthState.WaitEncryptionKey:
                    val = await Handle_WaitEncryptionKey();
                    break;
                case AuthState.WaitPhoneNumber:
                    try
                    {
                        val = await Handle_WaitPhoneNumber(Prompt("Enter Phone (include country code): "));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        val = null;
                    }
                    break;
                case AuthState.WaitCode:
                    try
                    {
                        val = await Handle_WaitCode(Prompt("Enter Code: "));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        val = null;
                    }
                    break;
                default:
                    val = null;
                    unsupported = true;
                    break;
            }
            
            if (val == null) // If val is null, then the request was unsuccessful
            {
                Console.WriteLine("TdLib did not return Ok on the last request");
                // Determines if the operation the user performed is unsupported
                if (unsupported) throw new Exception("TdLib returned an unsupported state");
                else lastRunStatusTime = DateTimeOffset.MinValue; // If it errors out, redoes the last operation
            }

            // This section waits for an updated status from telegram before continuing
            int i = 0;
            while (stateTime.Equals(lastRunStatusTime))
            {
                i++;
                Console.WriteLine($"Waiting for reply from Telegram for {i * 100}/{milliTimout} ms");
                await Task.Delay(100);
                if (i > milliTimout / 100) throw new Exception("TdLib did not get a reply from Telegram");
            }
            
            // Used to keep track of whether a new status has arrived
            lastRunStatusTime = stateTime;
        }
        Console.WriteLine("You should be all signed in and ready to go! " + CurrentState);
    }

    /// <summary>
    /// Handles state WaitTdLibParameters
    /// </summary>
    /// <param name="param">The TdLibParameters to pass to the client</param>
    /// <returns>A TdApi.Ok object if the request was successful</returns>
    public async Task<TdApi.Ok?> Handle_WaitTdLibParameters(TdApi.TdlibParameters param) => 
        await Client.ExecuteAsync(new TdApi.SetTdlibParameters {Parameters = param});

    /// <summary>
    /// Handles state WaitEncryptionKey
    /// </summary>
    /// <returns>A TdApi.Ok object if the request was successful</returns>
    public async Task<TdApi.Ok?> Handle_WaitEncryptionKey() =>
        await Client.ExecuteAsync(new TdApi.CheckDatabaseEncryptionKey());

    /// <summary>
    /// Handles state WaitPhoneNumber
    /// </summary>
    /// <param name="phone">The phone number to associate the client with</param>
    /// <returns>A TdApi.Ok object if the request was successful</returns>
    public async Task<TdApi.Ok?> Handle_WaitPhoneNumber(string phone) =>
        await Client.ExecuteAsync(new TdApi.SetAuthenticationPhoneNumber {PhoneNumber = phone});

    /// <summary>
    /// Handles state WaitCode
    /// </summary>
    /// <param name="code">The code that was sent to the user</param>
    /// <returns>A TdApi.Ok object if the request was successful</returns>
    public async Task<TdApi.Ok?> Handle_WaitCode(string code) =>
        await Client.ExecuteAsync(new TdApi.CheckAuthenticationCode {Code = code});

    /// <summary>
    /// Handles state WatiPassword
    /// </summary>
    /// <param name="password">The 2 factor password set by the user</param>
    /// <returns>A TdApi.Ok object if the request was successful</returns>
    public async Task<TdApi.Ok?> Handle_WaitPassword(string password) =>
        await Client.ExecuteAsync(new TdApi.CheckAuthenticationPassword {Password = password});

    /// <summary>
    /// The current state of authentication
    /// </summary>
    public enum AuthState
    {
        Null = 0,
        WaitTdLibParams = 1,
        WaitEncryptionKey = 2,
        WaitPhoneNumber = 3,
        WaitCode = 4,
        WaitPassword = 5,
        WaitRegistration = 6,
        WaitOtherDeviceConfirmation = 7,
        Ready = 8
    }

    /// <summary>
    /// Converts the TdApi.AuthorizationState to an AuthState enum
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public static AuthState GetState(TdApi.AuthorizationState state)
    {
        if (state.GetType() == typeof(AuthorizationStateWaitTdlibParameters)) return AuthState.WaitTdLibParams;
        if (state.GetType() == typeof(AuthorizationStateWaitEncryptionKey)) return AuthState.WaitEncryptionKey;
        if (state.GetType() == typeof(AuthorizationStateWaitPhoneNumber)) return AuthState.WaitPhoneNumber;
        if (state.GetType() == typeof(AuthorizationStateWaitCode)) return AuthState.WaitCode;
        if (state.GetType() == typeof(AuthorizationStateWaitPassword)) return AuthState.WaitPassword;
        if (state.GetType() == typeof(AuthorizationStateWaitRegistration)) return AuthState.WaitRegistration;
        if (state.GetType() == typeof(AuthorizationStateWaitOtherDeviceConfirmation)) return AuthState.WaitOtherDeviceConfirmation;
        if (state.GetType() == typeof(AuthorizationStateReady)) return AuthState.Ready;
        return 0;
    }
    
}
