using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using NeoSmart.Unicode;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;
using Microsoft.UI.Xaml.Media.Imaging;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReunionApp.Pages.CommandPages;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class AddSticker : Page
{

    private ObservableCollection<NewSticker> stickers = new ObservableCollection<NewSticker>();

    public AddSticker()
    {
        this.InitializeComponent();
    }

    private async void Add(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        picker.ViewMode = PickerViewMode.Thumbnail;
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".webp");

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.GetInstance().MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var files = await picker.PickMultipleFilesAsync();
        if (files.Count == 0) return;
        foreach (var file in files)
        {
            stickers.Add(new NewSticker { ImgPath = file.Path });
        }
    }
    
    private void Delete(object sender, RoutedEventArgs e)
    {
        var selected = Grid.SelectedItems;
        if (selected.Count == 0) return;
        foreach (var item in selected.Reverse())
        {
            stickers.Remove(item as NewSticker);
        }
    }

    private async void Finish(object sender, RoutedEventArgs e)
    {
        TgApi.Utils.ClearTemp();

        if (await FindErrors()) return;
        if (await FindWarnings()) return;

        processing.Visibility = Visibility.Visible;

        await Task.Run( async () => ProcessImgs());

        processing.Visibility = Visibility.Collapsed;
        return;
    }

    private async Task ProcessImgs()
    {
        foreach (var sticker in stickers)
        {

            try
            {
                string temp = TgApi.GlobalVars.TempDir;
                using (var img = await SixLabors.ImageSharp.Image.LoadAsync(sticker.ImgPath))
                {
                    int maxSize = 512;
                    if (!(img.Width <= maxSize && img.Height <= maxSize && (img.Width == maxSize || img.Height == maxSize)))
                    {
                        bool widthlarger = img.Width > img.Height;
                        int width = widthlarger ? maxSize : 0;
                        int height = widthlarger ? 0 : maxSize;

                        if (width > img.Width || height > img.Height)
                        {
                            img.Mutate(x => x.Resize(width, height, KnownResamplers.Lanczos3));
                        }
                        else
                        {
                            img.Mutate(x => x.Resize(width, height, KnownResamplers.Spline));
                        }

                        string filename = DateTime.Now.Ticks + "";
                        string path = $"{temp}{filename}.png";
                        await img.SaveAsync(path);
                        sticker.TempPath = path;
                    }
                    img.Dispose();
                }
                Configuration.Default.MemoryAllocator.ReleaseRetainedResources();
            }
            catch (Exception ex)
            {
                await App.GetInstance().ShowBasicDialog("We hit an exception",
                    $"We think the error is because of \"{sticker.ImgPath}\" which likely shows up as a blank image in the add view. " +
                    $"We'll show the exception dialog so you can make sure.", "Continue");
                await App.GetInstance().ShowExceptionDialog(ex);
            }
        }
    }

    private async Task<bool> FindErrors()
    {
        if (stickers.Count == 0)
        {
            await App.GetInstance().ShowBasicDialog("You didn't add anything!", "Please add some stickers before continuing!");
            return true;
        }

        List<string> errs = new List<string>();

        for (int i = 0; i < stickers.Count; i++)
        {
            var s = stickers[i];
            if (!File.Exists(s.ImgPath)) errs.Add($"- Error at Index {i}: File {s.ImgPath} not found.");
            if (string.IsNullOrWhiteSpace(s.Emojis)) errs.Add($"- Error at Index {i}: Emojis list cannot be empty.");
            else if (!Emoji.IsEmoji(s.Emojis)) errs.Add($"- Error at Index {i}: Emojis list contains non-emoji characters.");
        }

        if (errs.Count > 0)
        {
            errs.Insert(0, "Please remember that indices are 0 based. The first item is 0.");
            await App.GetInstance().ShowBasicDialog("We found some errors", String.Join("\n", errs));
            return true;
        }
        return false;
    }

    private async Task<bool> FindWarnings()
    {
  //      List<string> warnings = new List<string>();
  //      for (int i = 0; i < stickers.Count; i++)
		//{
  //          var s = stickers[i];
		//}
  //      if (warnings.Count > 0)
		//{
  //          warnings.Insert(0, "Please remember that indices are 0 based. The first item is 0.");
  //          await App.GetInstance().ShowBasicDialog("Some notes we took:", String.Join("\n", warnings));
  //      }
        return false;
    }

    private void SplitButton_Click(SplitButton sender, SplitButtonClickEventArgs args)
    {
        Add(sender, default);
    }

    private void Emojis_TextChanged(object sender, RoutedEventArgs args)
    {
        var send = sender as TextBox;
        send.Text = GEmojiSharp.Emoji.Emojify(send.Text);
        send.Select(send.Text.Length, send.Text.Length);
    }

    
}

public class NewSticker
{
    public string ImgPath { get; set; }
    public string TempPath { get; set; }
    public string Emojis { get; set; } = "";

    public string EnsuredPath => File.Exists(TempPath) ? TempPath : ImgPath;
}

