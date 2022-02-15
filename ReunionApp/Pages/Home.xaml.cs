using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TgApi.Types;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using TdLib;
using TgApi.Telegram;
using ReunionApp.Pages.CommandPages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReunionApp.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class Home : Page
{

    private ObservableCollection<StickerPack> packList;
    private bool cached = false;

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
            ShowLoading();
            if (forceNew) await App.GetInstance().ResetTdClient(false); 
            TdClient c = App.GetInstance().Client;
			var nameList = forceNew ? await c.GetOwnedPacksAsync() : await PackList.GetOwnedPacks(c);
			if (nameList is null || nameList.Length == 0)
			{
				None.Visibility = Visibility.Visible;
			}
			else
			{
				foreach (string pack in nameList)
				{
					if (forceNew) packList.Add(await Task.Run(async () => await StickerPack.GenerateFromName(c, pack)));
                    else packList.Add(await Task.Run(async () => await StickerPack.GetBasicPack(c, pack)));
                }
				foreach (var pack in packList)
				{
					await Task.Run(async()=>await pack.EnsuredThumb.GetPathEnsureDownloaded(c));
				}
				Packs.Visibility = Visibility.Visible;
			}
			Loading.Visibility = Visibility.Collapsed;
            LoadingBar.IsIndeterminate = false;
        }
		catch (Exception ex)
		{
			await App.GetInstance().ShowExceptionDialog(ex);
		}
	}

	private async void Packs_ItemClick(object sender, ItemClickEventArgs e)
	{
		var pack = e.ClickedItem as StickerPack;

        if (pack.Type != StickerType.STANDARD)
		{
            App.GetInstance().RootFrame.Navigate(typeof(Unsupported), $"Sticker packs of type {pack.Type} are not supported at this time.");
            return;
		}
        Loading.Visibility = Visibility.Visible;
        LoadingBar.IsIndeterminate = true;

        if (pack.IsCachedCopy) await Task.Run(async () => pack.InjectCompleteInfo(await StickerPack.GenerateFromName(App.GetInstance().Client, pack.Name)));

        Loading.Visibility = Visibility.Collapsed;
        App.GetInstance().RootFrame.Navigate(typeof(PackPage), pack);
	}

    private void ShowLoading()
    {
        None.Visibility = Visibility.Collapsed;
        Packs.Visibility = Visibility.Collapsed;
        Loading.Visibility = Visibility.Visible;
        LoadingBar.IsIndeterminate = true;
    }

	private async void Refresh(object sender, RoutedEventArgs e) => 
        await LoadStickers(true);
	

    private void NewPack(object sender, RoutedEventArgs e) =>
        App.GetInstance().RootFrame.Navigate(typeof(NewPack));

	private void Settings(object sender, RoutedEventArgs e) =>
		App.GetInstance().RootFrame.Navigate(typeof(Settings));

	private void About(object sender, RoutedEventArgs e) =>
		App.GetInstance().RootFrame.Navigate(typeof(About));
}

