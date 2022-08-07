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

/// <summary>
/// An extension class for App.cs that contains Application related utility methods
/// </summary>
public static class AppUtils
{
    // File formats supported by ImageSharp
    // Used for supported formats for file pickers
    public static readonly string[] ImageSharpFormats = new string[]{ "jpg", "jpeg", "png", "webp", "bmp", "tga" };

    /// <summary>
    /// Reloads the TDClient object associated the app. This clears the TDLib cache.
    /// </summary>
    /// <param name="app">The current App instance</param>
    /// <param name="autoNavigate">Whether or not to automatically navigate to the new home 
    /// or error pages after authentication is complete</param>
    /// <returns>An awaitable Task</returns>
    public static async Task ResetTdClient(this App app, bool autoNavigate)
    {
        app.Client.Dispose();
        app.Auth.Close();
        app.Client = new TdClient();
        app.Auth = new AuthHandler(app.Client);
        app.AuthState = AuthHandler.AuthState.Null;
        await app.HandleAuth(autoNavigate);
    }

    /// <summary>
    /// Mark @Stickers messages as read
    /// </summary>
    /// <param name="app">The current App instance</param>
    /// <param name="sender">The sender of the update event</param>
    /// <param name="e">A TDLib update</param>
    internal static async void ReadStickerBotMsgs(this App app, object sender, TdApi.Update e)
    {
        if (e is not TdApi.Update.UpdateNewMessage nMsg) return;
        var chatId = await app.Client.GetIdFromUsernameAsync("Stickers");
        if (nMsg.Message.ChatId == chatId)
            await app.Client.ViewMessagesAsync(chatId, 0, new[] { nMsg.Message.Id }, true);
        
    }

    /// <summary>
    /// Display a ContentDialog with specified text. Will not open if a dialog box is already open
    /// </summary>
    /// <param name="app">The current App instance</param>
    /// <param name="title">The title of the dialog box</param>
    /// <param name="body">The body of the dialog box</param>
    /// <param name="closeText">The text to display on the button that closes the box</param>
    /// <returns>An awaitable task</returns>
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

    /// <summary>
    /// Displays a dialog box with details about an exception
    /// </summary>
    /// <param name="app">The current App instance</param>
    /// <param name="exception">The relevant exception</param>
    /// <returns>An awaitable task</returns>
    public static async Task ShowExceptionDialog(this App app, Exception exception) => 
        await app.ShowBasicDialog($"Oops! The program hit a(n) {exception.GetType()} Exception", exception.ToString());

    /// <summary>
    /// Display a dialog box asking the user if they're sure they want to do something. Pass in Actions to do on each click.
    /// </summary>
    /// <param name="app">The current app instance</param>
    /// <param name="title">The title of the dialog box</param>
    /// <param name="body">The body of the dialog box</param>
    /// <param name="yesText">The text to be displayed on the yes button</param>
    /// <param name="yesClick">The Action to be run if the user clicks on the yes button</param>
    /// <param name="noText">The text to be displayed on the no button</param>
    /// <param name="noClick">The Action to be run if the user clicks on the no button</param>
    /// <returns>An awaitable task</returns>
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

    /// <summary>
    /// Force run he garbage collector after a given delay
    /// </summary>
    /// <param name="delay">The delay in milliseconds</param>
    public static async void CollectLater(int delay) =>
        await Task.Run(async () =>
        {
            await Task.Delay(delay);
            GC.Collect();
        });

    /// <summary>
    /// Add a string to the user's clipboard
    /// </summary>
    /// <param name="content">The content to be added to the clipboard</param>
    public static void AddToClipboard(string content)
    {
        var dp = new DataPackage();
        dp.SetText(content);
        Clipboard.SetContent(dp);
    }

    /// <summary>
    /// Pick a single file 
    /// </summary>
    /// <param name="formats">An array of formats that the user is allowed to pick from</param>
    /// <returns>A StorageFile object representing the file that the user selected</returns>
    public static async Task<StorageFile> PickSingleFileAsync(string[] formats)
    {
        var picker = new FileOpenPicker();
        SetupPicker(picker, formats);

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.GetInstance().MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        return await picker.PickSingleFileAsync();
    }

    /// <summary>
    /// Pick multiple files
    /// </summary>
    /// <param name="formats">An array of formats that the user is allowed to pick from</param>
    /// <returns>A Readonly list of StorageFile objects representing the files that the user selected</returns>
    public static async Task<IReadOnlyList<StorageFile>> PickMultipleFilesAsync(string[] formats)
    {
        var picker = new FileOpenPicker();
        SetupPicker(picker, formats);

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.GetInstance().MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        return await picker.PickMultipleFilesAsync();
    }

    /// <summary>
    /// Sets up the filepicker with the viewmode as well as permitted formats
    /// </summary>
    /// <param name="picker"></param>
    /// <param name="formats"></param>
    private static void SetupPicker(FileOpenPicker picker, string[] formats)
    {
        picker.ViewMode = PickerViewMode.Thumbnail;
        foreach (var format in formats)
        {
            picker.FileTypeFilter.Add('.' + format);
        }
    }

}
