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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReunionApp.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class Home : Page
{

	private ObservableCollection<StickerPack> packList = new ObservableCollection<StickerPack>();

	public Home()
	{
		this.InitializeComponent();
		LoadStickers();
	}

	public async Task LoadStickers(bool forceNew = false)
	{
		try
		{
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
					packList.Add(await StickerPack.GetBasicPack(c, pack));
				}
				foreach (var pack in packList)
				{
					await pack.EnsuredThumb.GetPathEnsureDownloaded(c);
				}
				Packs.Visibility = Visibility.Visible;
			}
			Loading.Visibility = Visibility.Collapsed;
		}
		catch (Exception ex)
		{
			App.GetInstance().ShowExceptionDialog(ex);
		}
	}

	public static BitmapImage ThumbAbsolutePath(StickerPackThumb thumb) =>
		App.GetBitmapFromPath(TgApi.GlobalVars.TdDir + 
							  (thumb.IsDesignatedThumb ? "thumbnails" : "stickers") + 
							  Path.DirectorySeparatorChar + thumb.Filename);


	private async void Packs_ItemClick(object sender, ItemClickEventArgs e)
	{
		var basicPack = e.ClickedItem as StickerPack;
		var packTask = StickerPack.GenerateFromName(App.GetInstance().Client, basicPack.Name);
		Loading.Visibility = Visibility.Visible;
		var pack = await packTask;
		App.GetInstance().RootFrame.Navigate(typeof(PackPage), pack);
		Loading.Visibility = Visibility.Collapsed;
	}

	private async void Refresh(object sender, RoutedEventArgs e)
	{
		None.Visibility = Visibility.Collapsed;
		Packs.Visibility = Visibility.Collapsed;
		Loading.Visibility = Visibility.Visible;
		packList.Clear();
		await LoadStickers(true);
	}

    private async void NewPack(object sender, RoutedEventArgs e) =>
        await App.GetInstance().ShowBasicDialog("Oops!", "Not implemented yet");

	private void Settings(object sender, RoutedEventArgs e)
	{
		App.GetInstance().RootFrame.Navigate(typeof(Settings));
	}

	private void About(object sender, RoutedEventArgs e)
	{
		App.GetInstance().RootFrame.Navigate(typeof(About));
	}
}

