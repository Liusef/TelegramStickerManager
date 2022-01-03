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

    public TdApi.AuthorizationState CurrentState { get => state; }
    public TdClient Client { get => client; }
    
    public AuthHandler(TdClient Client)
    {
        client = Client;
        Client.UpdateReceived += (sender, update) =>
        {
            if (update.GetType() == typeof(TdApi.Update.UpdateAuthorizationState))
            {
                var v1 = ((TdApi.Update.UpdateAuthorizationState) update).AuthorizationState;
                var v2 = DateTimeOffset.Now;
                Console.WriteLine($"State: {v1}, Time: {v2}");
                state = v1;
                stateTime = v2;
            }
        };
    }

    private static void ThrowNotOk(Object obj, string? message = "")
    {
        if (!IsOkTd(obj)) throw new NotOkException(message);
    }
    
    public async Task SigninCLI(int milliTimout = 20000)
    {
        while (CurrentState == null) await Task.Delay(100);
        DateTimeOffset lastTime = DateTimeOffset.MinValue;
        while (CurrentState.GetType() != typeof(TdApi.AuthorizationState.AuthorizationStateReady))
        {
            switch (GetState(CurrentState))
            {
                case AuthState.Null:
                    throw new NotSupportedException("TdLib returned unsupported state");
                    break;
                case AuthState.WaitTdLibParams:
                    await Handle_WaitTdLibParameters(new TdApi.TdlibParameters
                    {
                        ApiId = GlobalVars.ApiId,
                        ApiHash = GlobalVars.ApiHash,
                        ApplicationVersion = "1.3.0",
                        DeviceModel = "PC",
                        SystemLanguageCode = "en",
                        SystemVersion = "Win 10.0"
                    });
                    break;
                case AuthState.WaitEncryptionKey:
                    Handle_WaitEncryptionKey();
                    break;
                case AuthState.WaitPhoneNumber:
                    Console.Write("Enter Phone (include +): ");
                    Handle_WaitPhoneNumber(Console.ReadLine());
                    break;
                case AuthState.WaitCode:
                    Console.Write("Enter code: ");
                    Handle_WaitCode(Console.ReadLine());
                    break;
                case AuthState.WaitPassword:
                    Handle_WaitPassword();
                    break;
                case AuthState.WaitRegistration:
                    Handle_WaitRegistration();
                    break;
                default:
                    throw new NotSupportedException("TdLib returned an unsupported state");
            }
            int i = 0;
            while (stateTime.Equals(lastTime))
            {
                i++;
                Console.WriteLine($"Waiting for reply from Telegram for {i * 100}/{milliTimout} ms");
                await Task.Delay(100);
                if (i > milliTimout / 100) throw new Exception("TdLib did not get a reply from Telegram");
            }
            lastTime = stateTime;
        }
        Console.WriteLine("You should be all signed in and ready to go! " + CurrentState);
    }

    public async Task Handle_WaitTdLibParameters(TdApi.TdlibParameters param)
    {
        var val = await Client.ExecuteAsync(new TdApi.SetTdlibParameters {Parameters = param});
        ThrowNotOk(val);
    }

    public async Task Handle_WaitEncryptionKey()
    {
        var val = await Client.ExecuteAsync(new TdApi.CheckDatabaseEncryptionKey());
        ThrowNotOk(val);
    }

    public async Task Handle_WaitPhoneNumber(string phone)
    {
        var val = await Client.ExecuteAsync(new TdApi.SetAuthenticationPhoneNumber {PhoneNumber = phone});
        ThrowNotOk(val);
    }

    public async Task Handle_WaitCode(string code)
    {
        var val = await Client.ExecuteAsync(new TdApi.CheckAuthenticationCode {Code = code});
        ThrowNotOk(val);
    }

    public async Task Handle_WaitPassword()
    {
        throw new NotImplementedException();
    }
    
    public async Task Handle_WaitRegistration()
    {
        throw new NotImplementedException();
    }

    public enum AuthState
    {
        Null = 0,
        WaitTdLibParams = 1,
        WaitEncryptionKey = 2,
        WaitPhoneNumber = 3,
        WaitCode = 4,
        WaitPassword = 5,
        WaitRegistration = 6
    }

    public static AuthState GetState(TdApi.AuthorizationState state)
    {
        if (state.GetType() == typeof(AuthorizationStateWaitTdlibParameters)) return AuthState.WaitTdLibParams;
        if (state.GetType() == typeof(AuthorizationStateWaitEncryptionKey)) return AuthState.WaitEncryptionKey;
        if (state.GetType() == typeof(AuthorizationStateWaitPhoneNumber)) return AuthState.WaitPhoneNumber;
        if (state.GetType() == typeof(AuthorizationStateWaitCode)) return AuthState.WaitCode;
        if (state.GetType() == typeof(AuthorizationStateWaitPassword)) return AuthState.WaitPassword;
        if (state.GetType() == typeof(AuthorizationStateWaitRegistration)) return AuthState.WaitRegistration;
        return 0;
    }
    
}
