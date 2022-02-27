using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ReunionApp.Pages.CommandPages;
using TdLib;
using TgApi.Telegram;
using TgApi.Types;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReunionApp.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class Home : Page
{

    private ObservableCollection<StickerPack> packList;

    public Home()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        App.GetInstance().RootFrame.BackStack.Clear();
        bool refresh = e.Parameter as bool? ?? false;
        if (refresh && e.NavigationMode == NavigationMode.New) await LoadStickers(refresh);
    }

    public async Task LoadStickers(bool forceNew = false)
    {
        try
        {
            packList = new ObservableCollection<StickerPack>();
            Packs.ItemsSource = packList;
            ShowLoadOnly();
            if (forceNew) await App.GetInstance().ResetTdClient(false);
            TdClient c = App.GetInstance().Client;
            var nameList = forceNew ? await c.GetOwnedPacksAsync() : await PackList.GetOwnedPacks(c);

            if (nameList is null || nameList.Length == 0) None.Visibility = Visibility.Visible;
            else
            {
                foreach (string pack in nameList)
                {
                    if (forceNew) packList.Add(await Task.Run(async () => await StickerPack.GenerateFromName(c, pack)));
                    else packList.Add(await Task.Run(async () => await StickerPack.GetBasicPack(c, pack)));
                }
                foreach 
                    (var pack in packList) await Task.Run(async () => await pack.EnsuredThumb.GetPathEnsureDownloaded(c));
                Packs.Visibility = Visibility.Visible;
            }
            HideLoad();
        }
        catch (Exception ex)
        {
            await App.GetInstance().ShowExceptionDialog(ex);
        }
    }

    private async void Packs_ItemClick(object sender, ItemClickEventArgs e)
    {
        var pack = e.ClickedItem as StickerPack;
        if (pack.Type != StickerType.Standard)
        {
            App.GetInstance().RootFrame.Navigate(typeof(Unsupported), $"Sticker packs of type {pack.Type} are not supported at this time.");
            return;
        }
        ShowLoad();
        if (pack.IsCachedCopy) await Task.Run(async () => pack.InjectCompleteInfo(await StickerPack.GenerateFromName(App.GetInstance().Client, pack.Name)));
        HideLoad();
        App.GetInstance().RootFrame.Navigate(typeof(PackPage), pack);
    }

    private void ShowLoad()
    {
        Load.Visibility = Visibility.Visible;
        LoadBar.IsIndeterminate = true;
    }

    private void ShowLoadOnly()
    {
        None.Visibility = Visibility.Collapsed;
        Packs.Visibility = Visibility.Collapsed;
        Load.Visibility = Visibility.Visible;
        LoadBar.IsIndeterminate = true;
    }

    private void HideLoad()
    {
        Load.Visibility = Visibility.Collapsed;
        LoadBar.IsIndeterminate = false;
    }

    private async void Refresh(object sender, RoutedEventArgs e) 
    {
        RefreshButton.IsEnabled = false;
        await LoadStickers(true);
        RefreshButton.IsEnabled = true;
    }

    private void NewPack(object sender, RoutedEventArgs e) => App.GetInstance().RootFrame.Navigate(typeof(NewPack));

    private void Settings(object sender, RoutedEventArgs e) => App.GetInstance().RootFrame.Navigate(typeof(Settings));

    private void About(object sender, RoutedEventArgs e) => App.GetInstance().RootFrame.Navigate(typeof(About));
}

