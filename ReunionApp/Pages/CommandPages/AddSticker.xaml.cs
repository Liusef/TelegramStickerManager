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
using TgApi.Types;
using ReunionApp.Runners;
using Microsoft.UI.Xaml.Media.Animation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReunionApp.Pages.CommandPages;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class AddSticker : Page
{
    private StickerPack pack;
    private Button backButton;
    private ObservableCollection<NewSticker> stickers = new ObservableCollection<NewSticker>();

    public record AddStickerParams(StickerPack pack, Button back);

    public AddSticker()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        var parameters = e.Parameter as AddStickerParams;
        pack = parameters.pack;
        backButton = parameters.back;

    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        // This block is to ensure that bitmaps and other large objects are garbage collected, as pages aren't disposed by the garbage collector
        // NOTE: Lots of objects that need to be garbage collected are RefCounted from Unmanaged memory
        // TODO This is not a good solution for memory management. Find a way to dispose of pages instead.
        //foreach (var ns in stickers) ns.Img = null;
        Grid.ItemsSource = new ObservableCollection<NewSticker>();
        //stickers = new ObservableCollection<NewSticker>();
        Grid.ItemsSource = null;
        UnloadObject(Grid);
        Bindings.StopTracking();
        base.OnNavigatedFrom(e);
    }

    private async void Add(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        picker.ViewMode = PickerViewMode.Thumbnail;
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".webp");
        picker.FileTypeFilter.Add(".bmp");
        picker.FileTypeFilter.Add(".tga");

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
        for (int i = selected.Count - 1; i >= 0; i--)
        {
            var ns = (NewSticker) selected[i];
            //ns.Img = null;
            stickers.Remove(ns);
        }
        selected = null;
        stickers = new ObservableCollection<NewSticker>(stickers);
        Grid.ItemsSource = stickers;
        Task.Run(async () => { await Task.Delay(5000); GC.Collect(); });
        // TODO Memory is only freed after navigating off the page
    }

    private async void Finish(object sender, RoutedEventArgs e)
    {
        TgApi.Utils.ClearTemp();

        if (await FindErrors()) return;
        if (await FindWarnings()) return;

        processing.Visibility = Visibility.Visible;
        backButton.IsEnabled = false;

        await Task.Run( async () => await ProcessImgs());

        var runner = new AddStickerRunner(pack, stickers.ToArray());

        processing.Visibility = Visibility.Collapsed;
        ((Frame) Parent).Navigate(typeof(ProcessingCommand), runner, new DrillInNavigationTransitionInfo());
    }

    private async Task ProcessImgs()
    {
        await Parallel.ForEachAsync(stickers, new ParallelOptions { MaxDegreeOfParallelism = App.Threads }, async (sticker, ct) =>
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
                    else if (!(TgApi.Utils.GetExtension(sticker.ImgPath) == "png" ||
                               TgApi.Utils.GetExtension(sticker.ImgPath) == "webp"))
                        await img.SaveAsync($"{temp}{DateTime.Now.Ticks}.png");
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
        });
        Task.Run(async () => { await Task.Delay(5000); Configuration.Default.MemoryAllocator.ReleaseRetainedResources(); });
        }

    private async Task<bool> FindErrors() 
    {
        if (stickers.Count == 0)
        {
            await App.GetInstance().ShowBasicDialog("You didn't add anything!", "Please add some stickers before continuing!");
            return true;
        }

        if (stickers.Count + pack.Count > 120)
		{
            await App.GetInstance().ShowBasicDialog("You added too many stickers!", $"The max size for a sticker pack is 120.\n" +
                                                                                    $"This pack already has {pack.Count}, you added {stickers.Count}\n" +
                                                                                    $"Total: {pack.Count + stickers.Count}");
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
    public string Emojis { get; set; } = "😳"; // TODO Clear this default value, this is only so i can quickly test things
    //public BitmapImage Img {get; set;}

    public string EnsuredPath => File.Exists(TempPath) ? TempPath : ImgPath;
}

