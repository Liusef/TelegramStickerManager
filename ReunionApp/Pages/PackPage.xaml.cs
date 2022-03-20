using System;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using SixLabors.ImageSharp;
using TgApi.Types;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReunionApp.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class PackPage : Page
{

    private StickerPack pack;
    private ObservableCollection<Sticker> stickers = new ObservableCollection<Sticker>();
    private Button[] buttons;

    public PackPage()
    {
        this.InitializeComponent();
        buttons = new []{ Add, Del, Order, SPI};
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        bool update = pack == null || (e.Parameter != null && (e.Parameter as StickerPack) != pack);

        if (update)
        {
            StickerGrid.Visibility = Visibility.Collapsed;
            Load.Visibility = Visibility.Visible;
            DisableAllButtons();

            pack = e.Parameter as StickerPack;
            PackThumb.Source = new BitmapImage(new Uri(pack.EnsuredThumb.BestPath));
            PackTitle.Text = pack.Title;
            PackName.Text = pack.Name;
            CleanUp();

            await pack.EnsuredThumb.GetPathEnsureDownloaded(App.GetInstance().Client);
            await pack.EnsureAllDecodedDownloadedParallel(App.GetInstance().Client, App.Threads);
            stickers = new ObservableCollection<Sticker>(pack.Stickers);
            StickerGrid.ItemsSource = stickers;
            Configuration.Default.MemoryAllocator.ReleaseRetainedResources();
            GC.Collect(); // TODO Do i really need to garbage collect this many times
        }

        base.OnNavigatedTo(e);
        StickerGrid.Visibility = Visibility.Visible;
        Load.Visibility = Visibility.Collapsed;
        EnableAllButtons();
    }

    private void CleanUp()
    {
        // This block is to ensure that bitmaps and other large objects are garbage collected, as pages aren't disposed by the garbage collector
        // NOTE: Lots of objects that need to be garbage collected are RefCounted from Unmanaged memory
        // TODO This is not a good solution for memory management. Find a way to dispose of pages instead.
        stickers = new ObservableCollection<Sticker>();
        StickerGrid.ItemsSource = stickers;
        Bindings.StopTracking();
        GC.Collect();
    }

    private void EnableAllButtons() {
        foreach (Button b in buttons) b.IsEnabled = true;
    }

    private void DisableAllButtons()
    {
        foreach (Button b in buttons) b.IsEnabled = false;
    }

    private void Back(object sender, RoutedEventArgs e) =>
        App.GetInstance().RootFrame.GoBack();

    private void CommandButtonPressed(CommandType command) =>
        App.GetInstance().RootFrame.Navigate(typeof(BaseCommand),
            new BaseCommand.BaseCommandParams(pack, command),
            new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight });

    private async void OpenInTelegram() => await Windows.System.Launcher.LaunchUriAsync(pack.AddUri);

    private void CopyLink() => AppUtils.AddToClipboard(pack.ShareUri.OriginalString);


    private void OpenButton(SplitButton sender, SplitButtonClickEventArgs args) => OpenInTelegram();

    private void OpenFlyout(object sender, RoutedEventArgs e) => OpenInTelegram();

    private void CopyFlyout(object sender, RoutedEventArgs e) => CopyLink();

    private void AddSticker(object sender, RoutedEventArgs e) => CommandButtonPressed(CommandType.AddSticker);

    private void DelSticker(object server, RoutedEventArgs e) => CommandButtonPressed(CommandType.DelSticker);

    private void OrderSticker(object sender, RoutedEventArgs e) => CommandButtonPressed(CommandType.OrderSticker);

    private void SetIcon(object sender, RoutedEventArgs e) => CommandButtonPressed(CommandType.SetPackIcon);
}
