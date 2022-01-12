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

    public TdApi.AuthorizationState CurrentState => state;
    public DateTimeOffset LastRequestReceivedAt => stateTime;
    public TdClient Client => client;
    
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

    public void Close()
    {
        Client.UpdateReceived -= handlerDelegate;
        handlerDelegate = null;
        state = null;
        client = null;
    }
    
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

    public async Task<TdApi.Ok?> Handle_WaitTdLibParameters(TdApi.TdlibParameters param) => 
        await Client.ExecuteAsync(new TdApi.SetTdlibParameters {Parameters = param});

    public async Task<TdApi.Ok?> Handle_WaitEncryptionKey() =>
        await Client.ExecuteAsync(new TdApi.CheckDatabaseEncryptionKey());

    public async Task<TdApi.Ok?> Handle_WaitPhoneNumber(string phone) =>
        await Client.ExecuteAsync(new TdApi.SetAuthenticationPhoneNumber {PhoneNumber = phone});

    public async Task<TdApi.Ok?> Handle_WaitCode(string code) =>
        await Client.ExecuteAsync(new TdApi.CheckAuthenticationCode {Code = code});

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

    public static AuthState GetState(TdApi.AuthorizationState state)
    {
        if (state is null) return 0;
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
