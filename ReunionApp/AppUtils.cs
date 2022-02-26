using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using ReunionApp.Pages;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using TdLib;
using TgApi.Telegram;

namespace ReunionApp;

public static class AppUtils
{
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
        var cd = new ContentDialog
        {
            Title = title,
            IsPrimaryButtonEnabled = false,
            IsSecondaryButtonEnabled = false,
            CloseButtonText = closeText
        };
        var b = new DialogBody();
        b.Body.Text = body;
        cd.Content = b;
        cd.XamlRoot = app.MainWindow.Content.XamlRoot;
        
        cd.CloseButtonClick += (sender, args) => app.IsCdOpen = false;

        await cd.ShowAsync();
    }

    public static async Task ShowExceptionDialog(this App app, Exception exception) => 
        await app.ShowBasicDialog($"Oops! The program hit a(n) {exception.GetType()} Exception", exception.ToString());

    public static BitmapImage GetBitmapFromPath(string path) => new(new Uri(path));

    public static Uri GetUriFromString(string str) => new(str);

    public static async void CollectLater(int delay) =>
        await Task.Run(async () =>
        {
            await Task.Delay(delay);
            GC.Collect();
        });

    public static async Task<string> ResizeAsync(string path, int width, int height, bool forceFormat, string[] formats = null)
    {
        formats ??= new string[]{"png"};
        string savePath = $"{TgApi.GlobalVars.TempDir}{DateTime.Now.Ticks}.{formats[0]}";
        using (var img = await SixLabors.ImageSharp.Image.LoadAsync(path))
        {
            if (img.Height != height || img.Width != width)
            {
                img.Mutate(x => x.Resize(100, 100));
                await img.SaveAsync(savePath);
            }
            else if (forceFormat && !formats.Contains(TgApi.Utils.GetExtension(path)))
                await img.SaveAsync(savePath);
            else 
                savePath = path;  
        }
        Configuration.Default.MemoryAllocator.ReleaseRetainedResources();
        return savePath;
    }

    public static async Task<string> ResizeFitAsync(string path, int width, int height, bool forceFormat, string[] formats = null)
    {
        formats ??= new string[] { "png" };
        string savePath = $"{TgApi.GlobalVars.TempDir}{DateTime.Now.Ticks}.{formats[0]}";
        using (var img = await SixLabors.ImageSharp.Image.LoadAsync(path))
        {
            if (!(img.Width <= width && img.Height <= height && (img.Width == width || img.Height == height)))
            {
                double widthRatio = (img.Width + 0.0) / width;
                double heightRatio = (img.Height + 0.0) / height;

                bool widthPriority = img.Width > img.Height;

                int newWidth = widthPriority ? width : 0;
                int newHeight = widthPriority ? 0 : height;

                if (width > img.Width || height > img.Height) img.Mutate(x => x.Resize(newWidth, newHeight, KnownResamplers.Lanczos3));
                else img.Mutate(x => x.Resize(width, height, KnownResamplers.Spline));

                await img.SaveAsync(savePath);
            }
            else if (forceFormat && !formats.Contains(TgApi.Utils.GetExtension(path)))
                await img.SaveAsync(savePath);
            else
                savePath = path;

        }
        Configuration.Default.MemoryAllocator.ReleaseRetainedResources();
        return savePath;
    }

    public static async Task<string> ResizeFitWithAlphaBorderAsync(string path, int width, int height, bool forceFormat, string formats = default)
    {
        throw new NotImplementedException();
    }

}
