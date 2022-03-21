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
        var files = await AppUtils.PickMultipleFilesAsync(AppUtils.ImageSharpFormats);
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
        if (await FindErrors()) return;
        if (await FindWarnings()) return;

        processing.Visibility = Visibility.Visible;

        await Task.Run(() => Utils.ClearTemp());
        await Task.Run(async () => await StickerLogic.ResizeAllToStickerParallelAsync(stickers.ToArray(), ScaleImages.IsChecked ?? false));


        CommandRunner runner;
        if (newPackMode)
        {
            runner = new NewPackRunner(pack, stickers.ToArray());
            if (pack.Thumb is not null && File.Exists(pack.Thumb.LocalPath))
                ((NewPackThumb)pack.Thumb).Path = await Task.Run( async () => await StickerLogic.ResizeToThumbAsync(pack.Thumb.BestPath));
        }
        else runner = new AddStickerRunner(pack, stickers.ToArray());
        ImgUtils.CollectImageSharpLater(5000);
        Frame.Navigate(typeof(ProcessingCommand), runner, new DrillInNavigationTransitionInfo());
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
        StickerError[] errs = await Task.Run(() => StickerLogic.GetStickerErrors(stickers.ToArray()));
        if (errs.Length > 0)
        {
            await App.GetInstance().ShowBasicDialog("We found some errors", string.Join("\n", errs.Select((x) => x.ToString())));
            return true;
        }
        return false;
    }

    private async Task<bool> FindWarnings() // TODO Implement Warnings
        => false;
    

    private void SplitButton_Click(SplitButton sender, SplitButtonClickEventArgs args) => Add(sender, default);
    

    private void Emojis_TextChanged(object sender, RoutedEventArgs args)
    {
        var send = sender as TextBox; // TODO Find out how to replace this with dedicated autosuggest control
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
