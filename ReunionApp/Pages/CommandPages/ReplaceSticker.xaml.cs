using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using ReunionApp.Runners;
using TgApi.Types;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReunionApp.Pages.CommandPages;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ReplaceSticker : Page
{
    private ObservableCollection<ReplaceStickerUpdate> stickers = new ObservableCollection<ReplaceStickerUpdate>();

    public ReplaceSticker() => this.InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        var p = e.Parameter as Sticker[];
        foreach (var s in p) stickers.Add(new ReplaceStickerUpdate { Sticker = s });
    }

    private void Back(object sender, RoutedEventArgs e) => Frame.GoBack();

    private async Task<bool> FindErrors()
    {
        return false;
    }

    private async void Finish(object sender, RoutedEventArgs e)
    {
        if (await FindErrors()) return;

        processing.Visibility = Visibility.Visible;
        await Task.Run(async () => await StickerLogic.ResizeAllToStickerParallelAsync(stickers.ToArray()));
        var runner = new ReplaceStickerRunner(stickers.ToArray());
        Frame.Navigate(typeof(ProcessingCommand), runner, new DrillInNavigationTransitionInfo());
    }

    private async Task SetNewImg(ReplaceStickerUpdate update)
    {
        var file = await AppUtils.PickSingleFileAsync(AppUtils.ImageSharpFormats);
        update.NewPath = file.Path;
    }

    private async Task ClearImg(ReplaceStickerUpdate update)
    {
        update.NewPath = " ";
    }

    private async void MenuFlyout_SetNewImg(object sender, RoutedEventArgs e)
    {
        var button = (MenuFlyoutItem) sender;
        await SetNewImg(button.DataContext as ReplaceStickerUpdate);
    }

    private async void MenuFlyout_ClearImg(object sender, RoutedEventArgs e)
    {
        var button = (MenuFlyoutItem) sender;
        await ClearImg(button.DataContext as ReplaceStickerUpdate);
    }

    private async void SplitButton_SetNewImg(SplitButton sender, SplitButtonClickEventArgs args) =>
        await SetNewImg(sender.DataContext as ReplaceStickerUpdate);
    
}

public class ReplaceStickerUpdate : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private string _newPath = " ";


    public Sticker Sticker { get; set; }
    public string NewPath
    {
        get => _newPath;
        set
        {
            _newPath = value;
            OnPropertyChanged();
            OnPropertyChanged("OldVisibility");
        }
    }
    public string ThreadsafeNewPath
    {
        get => _newPath;
        set => _newPath = value;
    }
    public Visibility OldVisibility => string.IsNullOrWhiteSpace(NewPath) ? Visibility.Visible : Visibility.Collapsed;

    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
