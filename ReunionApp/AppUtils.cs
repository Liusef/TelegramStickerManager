using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using ReunionApp.Pages;
using TdLib;
using TgApi.Telegram;

namespace ReunionApp;

public static class AppUtils
{
    public static async Task ResetTdClient(this App app, bool autoNavigate)
    {
        app.Client.Dispose();
        app.auth.Close();
        app.Client = new TdClient();
        app.auth = new AuthHandler(app.Client);
        app.authState = AuthHandler.AuthState.Null;
        await app.HandleAuth(autoNavigate);
    }

    internal static async void ReadStickerBotMsgs(this App app, object sender, TdApi.Update e)
    {
        if (e is TdApi.Update.UpdateNewMessage nMsg)
        {
            var chatId = await app.Client.GetIdFromUsernameAsync("Stickers");
            if (nMsg.Message.ChatId == chatId)
            {
                await app.Client.ViewMessagesAsync(chatId, 0, new[] { nMsg.Message.Id }, true);
            }
        }
    }

    public static async Task ShowBasicDialog(this App app, string title, string body, string closeText = "Ok")
    {
        if (app.isCdOpen) return;
        app.isCdOpen = true;
        var cd = new ContentDialog();
        cd.Title = title;
        cd.IsPrimaryButtonEnabled = false;
        cd.IsSecondaryButtonEnabled = false;
        cd.CloseButtonText = closeText;
        var b = new DialogBody();
        b.Body.Text = body;
        cd.Content = b;
        cd.XamlRoot = app.MainWindow.Content.XamlRoot;

        cd.CloseButtonClick += (sender, args) => app.isCdOpen = false;

        await cd.ShowAsync();
    }

    public static async Task ShowExceptionDialog(this App app, Exception exception) => 
        await app.ShowBasicDialog($"Oops! The program hit a(n) {exception.GetType()} Exception", exception.ToString());

    public static BitmapImage GetBitmapFromPath(string path) => new BitmapImage(new Uri(path));

    public static Uri GetUriFromString(string str) => new Uri(str);

}
