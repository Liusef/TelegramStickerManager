using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Animation;
using ReunionApp.Pages;
using ReunionApp.Pages.LoginPages;
using TdLib;
using TgApi;
using TgApi.Telegram;

namespace ReunionApp;

/// <summary>
/// Some larger logic required by the app
/// </summary>
public static class AppLogic
{
    /// <summary>
    /// Handles the backend authentication that the user doesn't need to do anything for (like sending app info and verifying authorization)
    /// </summary>
    /// <param name="app">The current app instance</param>
    /// <param name="autoNavigate">Whether or not to automatically navigate to a new home or error page on authentication completion</param>
    /// <returns>An awaitable task</returns>
    public static async Task HandleAuth(this App app, bool autoNavigate = true)
    {
        app.Auth ??= new AuthHandler(app.Client);
        await Task.Delay(50);
        var lastRequest = DateTimeOffset.MinValue;
        app.AuthState = AuthHandler.GetState(app.Auth.CurrentState);
        try
        {
            while (app.AuthState != AuthHandler.AuthState.Ready &&
                   app.AuthState != AuthHandler.AuthState.WaitPhoneNumber &&
                   app.AuthState != AuthHandler.AuthState.WaitCode &&
                   app.AuthState != AuthHandler.AuthState.WaitOtherDeviceConfirmation &&
                   app.AuthState != AuthHandler.AuthState.WaitRegistration &&
                   app.AuthState != AuthHandler.AuthState.WaitPassword)
            {
                switch (app.AuthState)
                {
                    case AuthHandler.AuthState.WaitTdLibParams:
                        await app.Auth.Handle_WaitTdLibParameters(new TdApi.TdlibParameters
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
                        await app.Auth.Handle_WaitEncryptionKey();
                        break;
                }

                while (lastRequest == app.Auth.LastRequestReceivedAt) await Task.Delay(50);

                lastRequest = app.Auth.LastRequestReceivedAt;
                app.AuthState = AuthHandler.GetState(app.Auth.CurrentState);

                if (app.AuthState == AuthHandler.AuthState.Null)
                {
                    app.RootFrame.Navigate(typeof(GenericError), "The application encountered an unknown sign-in state. Please restart the app");
                }
            }

            if (autoNavigate)
            {
                switch (app.AuthState)
                {
                    case AuthHandler.AuthState.Ready:
                        app.RootFrame.Navigate(typeof(Home), true, new DrillInNavigationTransitionInfo());
                        break;
                    default:
                        app.RootFrame.Navigate(typeof(LoginPhone), app.Auth, new DrillInNavigationTransitionInfo());
                        break;
                }
            }

            if (app.AuthState == AuthHandler.AuthState.Ready) app.OnAuthStateReady();

        }
        catch (Exception ex)
        {
            await app.ShowExceptionDialog(ex);
        }
    }

    /// <summary>
    /// An event handler method that is run when the authentication state is ready
    /// </summary>
    /// <param name="app"></param>
    private static void OnAuthStateReady(this App app)
    {
        app.Client.UpdateReceived += app.ReadStickerBotMsgs;
    }

}
