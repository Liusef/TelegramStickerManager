using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TgApi.Types;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;

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


    public PackPage()
	{
		this.InitializeComponent();
	}

    protected async override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (pack is null) pack = e.Parameter as StickerPack;
        if (stickers.Count == 0) await LoadStickers();
        StickerGrid.Visibility = Visibility.Visible;
        Loading.Visibility = Visibility.Collapsed;
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        // This block is to ensure that bitmaps and other large objects are garbage collected, as pages aren't disposed by the garbage collector
        // NOTE: Lots of objects that need to be garbage collected are RefCounted from Unmanaged memory
        // TODO This is not a good solution for memory management. Find a way to dispose of pages instead.
        stickers = new ObservableCollection<Sticker>();
        StickerGrid.ItemsSource = null;
        UnloadObject(StickerGrid);
        Bindings.StopTracking();
        base.OnNavigatedFrom(e);
    }

    public async Task LoadStickers()
    {
        foreach (Sticker s in pack.Stickers)
        {
            await s.GetPathEnsureDownloaded(App.GetInstance().Client);
            stickers.Add(s);
        }
    }


	private void Back(object sender, RoutedEventArgs e) =>
		App.GetInstance().RootFrame.GoBack();
	
    private void AddSticker(object sender, RoutedEventArgs e)
    {
        App.GetInstance().RootFrame.Navigate(typeof(BaseCommand), 
            new BaseCommand.BaseCommandParams(pack, 
            BaseCommand.CommandType.ADDSTICKER),
            new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
    }
}


