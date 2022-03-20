using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using NeoSmart.Unicode;
using ReunionApp.Runners;
using TgApi;
using TgApi.Types;
using Windows.Storage.Pickers;
using static ReunionApp.Pages.ProcessingCommand;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReunionApp.Pages.CommandPages;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class AddSticker : Page
{
    private StickerPack pack;
    private ObservableCollection<NewSticker> stickers = new ObservableCollection<NewSticker>();
    private bool newPackMode;

    public record AddStickerParams(StickerPack pack, bool NewPackMode);

    public AddSticker() => this.InitializeComponent();
    
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        var (stickerPack, b) = (AddStickerParams) e.Parameter;
        newPackMode = b;
        pack = stickerPack;
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        Grid.ItemsSource = new ObservableCollection<NewSticker>();
        Bindings.StopTracking();
        base.OnNavigatedFrom(e);
    }

    // TODO Consider Adding functionality to move to next textbox when you press "Enter"
    // this could be implemented using an InputInjector with InjectedInputKeyboardInfo with key "Tab"

    private async void Add(object sender, RoutedEventArgs e)
    {
        var files = await AppUtils.PickMultipleFileAsync(AppUtils.ImageSharpFormats);
        if (files.Count == 0) return;
        foreach (var file in files) stickers.Add(new NewSticker { ImgPath = file.Path });
    }

    private void Delete(object sender, RoutedEventArgs e)
    {
        var selected = Grid.SelectedItems;
        for (int i = selected.Count - 1; i >= 0; i--)
            stickers.Remove((NewSticker)selected[i]);
    }

    private async void Finish(object sender, RoutedEventArgs e)
    {
        TgApi.Utils.ClearTemp();

        if (await FindErrors()) return;
        if (await FindWarnings()) return;

        processing.Visibility = Visibility.Visible;

        await Task.Run(async () => await ProcessImgs());

        CommandRunner runner;

        if (newPackMode)
        {
            runner = new NewPackRunner(pack, stickers.ToArray());
            if (pack.Thumb is not null && File.Exists(pack.Thumb.LocalPath))
                ((NewPackThumb)pack.Thumb).Path = await Task.Run( async () => await TgApi.ImgUtils.ResizeAsync(pack.Thumb.BestPath, 100, 100, true, new[] { "png" }));
        }
        else runner = new AddStickerRunner(pack, stickers.ToArray());

        processing.Visibility = Visibility.Collapsed;
        Frame.Navigate(typeof(ProcessingCommand), runner, new DrillInNavigationTransitionInfo());
    }

    private async Task ProcessImgs()
    {
        const int size = 512;
        string[] formats = new[] { "png", "webp" };
        await Parallel.ForEachAsync(stickers, new ParallelOptions { MaxDegreeOfParallelism = App.Threads }, async (sticker, ct) =>
        {
            try
            {
                sticker.TempPath = await TgApi.ImgUtils.ResizeFitAsync(sticker.ImgPath, size, size, true, formats);
            }
            catch (Exception ex)
            {
                await App.GetInstance().ShowBasicDialog("We hit an exception",
                    $"We think the error is because of \"{sticker.ImgPath}\" which likely shows up as a blank image in the add view. " +
                    $"We'll show the exception dialog so you can make sure.\n\n{ex}", "Continue");
            }
        });
        CollectImageSharpLater(5000);
    }

    private static async void CollectImageSharpLater(int delay) =>
        await Task.Run(async () =>
        {
            await Task.Delay(delay);
            ImgUtils.ReleaseImageSharpMemory();
        });

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

    private async Task<bool> FindWarnings() // TODO Implement Warnings
    {
        await Task.Delay(0); // This is a placeholder to suppress warnings
        return false;
    }

    private void SplitButton_Click(SplitButton sender, SplitButtonClickEventArgs args) => Add(sender, default);
    

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

    public string EnsuredPath => File.Exists(TempPath) ? TempPath : ImgPath;
}
