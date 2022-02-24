using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Animation;
using ReunionApp.Pages;
using ReunionApp.Pages.LoginPages;
using TdLib;
using TgApi;
using TgApi.Telegram;

namespace ReunionApp;

public static class AppLogic
{
    public static async Task HandleAuth(this App app, bool autoNavigate = true)
    {
        app.auth ??= new AuthHandler(app.Client);
        await Task.Delay(50);
        var lastRequest = DateTimeOffset.MinValue;
        app.authState = AuthHandler.GetState(app.auth.CurrentState);
        try
        {
            while (app.authState != AuthHandler.AuthState.Ready &&
                app.authState != AuthHandler.AuthState.WaitPhoneNumber &&
                app.authState != AuthHandler.AuthState.WaitCode &&
                app.authState != AuthHandler.AuthState.WaitOtherDeviceConfirmation &&
                app.authState != AuthHandler.AuthState.WaitRegistration &&
                app.authState != AuthHandler.AuthState.WaitPassword)
            {
                switch (app.authState)
                {
                    case AuthHandler.AuthState.WaitTdLibParams:
                        await app.auth.Handle_WaitTdLibParameters(new TdApi.TdlibParameters()
                        {
                            ApiId = GlobalVars.ApiId,
                            ApiHash = GlobalVars.ApiHash,
                            SystemLanguageCode = GlobalVars.SystemLanguageCode,
                            DeviceModel = GlobalVars.DeviceModel,
                            ApplicationVersion = GlobalVars.ApplicationVersion,
                            DatabaseDirectory = GlobalVars.TdDir,
                            UseTestDc = false,
                            UseChatInfoDatabase = false,
                            UseFileDatabase = false,
                            UseMessageDatabase = false
                        });
                        break;
                    case AuthHandler.AuthState.WaitEncryptionKey:
                        await app.auth.Handle_WaitEncryptionKey();
                        break;
                    default:
                        await Task.Delay(100);
                        break;
                }

                while (lastRequest == app.auth.LastRequestReceivedAt) await Task.Delay(50);

                lastRequest = app.auth.LastRequestReceivedAt;
                app.authState = AuthHandler.GetState(app.auth.CurrentState);

                if (app.authState == AuthHandler.AuthState.Null)
                {
                    app.RootFrame.Navigate(typeof(GenericError), "The application encountered an unknown sign-in state. Please restart the app");
                }
            }

            if (autoNavigate)
            {
                switch (app.authState)
                {
                    case AuthHandler.AuthState.Ready:
                        app.RootFrame.Navigate(typeof(Home), true, new DrillInNavigationTransitionInfo());
                        break;
                    default:
                        app.RootFrame.Navigate(typeof(LoginPhone), app.auth, new DrillInNavigationTransitionInfo());
                        break;
                }
            }

            if (app.authState == AuthHandler.AuthState.Ready) await app.OnAuthStateReady();

        }
        catch (Exception ex)
        {
            await app.ShowExceptionDialog(ex);
        }
    }

    private static async Task OnAuthStateReady(this App app)
    {
        app.Client.UpdateReceived += app.ReadStickerBotMsgs;
    }

}
