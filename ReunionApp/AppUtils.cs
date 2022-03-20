using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using ReunionApp.Pages;
using TdLib;
using TgApi.Telegram;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace ReunionApp;

public static class AppUtils
{
    public static readonly string[] ImageSharpFormats = new string[]{ "jpg", "jpeg", "png", "webp", "bmp", "tga" };

    public static async Task ResetTdClient(this App app, bool autoNavigate)
    {
        app.Client.Dispose();
        app.Auth.Close();
        app.Client = new TdClient();
        app.Auth = new AuthHandler(app.Client);
        app.AuthState = AuthHandler.AuthState.Null;
        await app.HandleAuth(autoNavigate);
    }

    internal static async void ReadStickerBotMsgs(this App app, object sender, TdApi.Update e)
    {
        if (e is not TdApi.Update.UpdateNewMessage nMsg) return;
        var chatId = await app.Client.GetIdFromUsernameAsync("Stickers");
        if (nMsg.Message.ChatId == chatId)
            await app.Client.ViewMessagesAsync(chatId, 0, new[] { nMsg.Message.Id }, true);
        
    }

    public static async Task ShowBasicDialog(this App app, string title, string body, string closeText = "Ok")
    {
        if (app.IsCdOpen) return;
        app.IsCdOpen = true;

        var b = new DialogBody();
        b.Body.Text = body;

        var cd = new ContentDialog
        {
            Title = title,
            IsPrimaryButtonEnabled = false,
            IsSecondaryButtonEnabled = false,
            CloseButtonText = closeText,
            Content = b,
            XamlRoot = app.MainWindow.Content.XamlRoot
        };
        
        cd.CloseButtonClick += (sender, args) => app.IsCdOpen = false;

        await cd.ShowAsync();
    }

    public static async Task ShowExceptionDialog(this App app, Exception exception) => 
        await app.ShowBasicDialog($"Oops! The program hit a(n) {exception.GetType()} Exception", exception.ToString());

    public static async Task ShowAreYouSureDialog(this App app, string title, string body, string yesText, Action yesClick, string noText, Action noClick)
    {
        if (app.IsCdOpen) return;
        app.IsCdOpen = true;

        var b = new DialogBody();
        b.Body.Text = body;

        var cd = new ContentDialog
        {
            Title = title,
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = false,
            PrimaryButtonText = yesText,
            CloseButtonText = noText,
            Content = b,
            XamlRoot = app.MainWindow.Content.XamlRoot
        };

        if (yesClick is not null) cd.PrimaryButtonClick += (sender, args) => yesClick();
        if (noClick is not null) cd.CloseButtonClick += (sender, args) => noClick();
        cd.PrimaryButtonClick += (sender, args) => app.IsCdOpen = false;
        cd.CloseButtonClick += (sender, args) => app.IsCdOpen = false;

        await cd.ShowAsync();
    }

    public static async void CollectLater(int delay) =>
        await Task.Run(async () =>
        {
            await Task.Delay(delay);
            GC.Collect();
        });

    public static void AddToClipboard(string content)
    {
        var dp = new DataPackage();
        dp.SetText(content);
        Clipboard.SetContent(dp);
    }

    public static async Task<StorageFile> PickSingleFileAsync(string[] formats)
    {
        var picker = new FileOpenPicker();
        SetupPicker(picker, formats);

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.GetInstance().MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        return await picker.PickSingleFileAsync();
    }

    public static async Task<IReadOnlyList<StorageFile>> PickMultipleFileAsync(string[] formats)
    {
        var picker = new FileOpenPicker();
        SetupPicker(picker, formats);

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.GetInstance().MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        return await picker.PickMultipleFilesAsync();
    }

    private static void SetupPicker(FileOpenPicker picker, string[] formats)
    {
        picker.ViewMode = PickerViewMode.Thumbnail;
        foreach (var format in formats)
        {
            picker.FileTypeFilter.Add('.' + format);
        }
    }

}
