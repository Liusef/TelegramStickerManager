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
using Windows.Foundation;
using Windows.Foundation.Collections;
using TgApi.Types;
using System.Collections.ObjectModel;
using static TdLib.TdApi;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReunionApp.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class Home : Page
{

	public ObservableCollection<StickerPack> packList { get; set; } = new ObservableCollection<StickerPack>();

	public Home()
	{
		this.InitializeComponent();
		LoadStickers();
	}

	public async void LoadStickers()
	{
		try
		{
			var c = App.GetInstance().Client;
			var nameList = await PackList.GetOwnedPacks(c);
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
				Packs.Visibility = Visibility.Visible;
			}
			Loading.Visibility = Visibility.Collapsed;
		}
		catch (Exception ex)
		{
			App.GetInstance().ShowExceptionDialog(ex);
		}
	}
}

