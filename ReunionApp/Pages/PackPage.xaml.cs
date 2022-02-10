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
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;

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
        bool update = pack == null || (e.Parameter != null && (e.Parameter as StickerPack) != pack);

        if (update)
        {
            StickerGrid.Visibility = Visibility.Collapsed;
            Loading.Visibility = Visibility.Visible;
            DisableAllButtons();

            pack = e.Parameter as StickerPack;
            PackThumb.Source = new BitmapImage(App.GetUriFromString(pack.EnsuredThumb.BestPath));
            Title.Text = pack.Title;
            Name.Text = pack.Name;
            CleanUp();

            await pack.EnsureAllDecodedDownloadedParallel(App.GetInstance().Client, App.Threads);
            stickers = new ObservableCollection<Sticker>(pack.Stickers);
            StickerGrid.ItemsSource = stickers;
            Configuration.Default.MemoryAllocator.ReleaseRetainedResources();
            GC.Collect();
        }

        base.OnNavigatedTo(e);

        //await Task.Run(async () => await Task.Delay(30)); //TODO is this necessary

        StickerGrid.Visibility = Visibility.Visible;
        Loading.Visibility = Visibility.Collapsed;
        EnableAllButtons();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        //thumb = null;
        base.OnNavigatedFrom(e);
    }

    private void CleanUp()
    {
        // This block is to ensure that bitmaps and other large objects are garbage collected, as pages aren't disposed by the garbage collector
        // NOTE: Lots of objects that need to be garbage collected are RefCounted from Unmanaged memory
        // TODO This is not a good solution for memory management. Find a way to dispose of pages instead.
        stickers = new ObservableCollection<Sticker>();
        StickerGrid.ItemsSource = stickers;
        UnloadObject(StickerGrid);
        Bindings.StopTracking();
        GC.Collect();
    }

    private void EnableAllButtons()
    {
        Add.IsEnabled = true;
        Del.IsEnabled = true;
    }

    private void DisableAllButtons()
    {
        Add.IsEnabled = false;
        Del.IsEnabled = false;
    }


	private void Back(object sender, RoutedEventArgs e) =>
		App.GetInstance().RootFrame.GoBack();
	
    private void AddSticker(object sender, RoutedEventArgs e)
    {
        App.GetInstance().RootFrame.Navigate(typeof(BaseCommand), 
            new BaseCommand.BaseCommandParams(pack, 
            CommandType.ADDSTICKER),
            new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
    }

    private void DelSticker(object server, RoutedEventArgs e)
    {
        App.GetInstance().RootFrame.Navigate(typeof(BaseCommand),
            new BaseCommand.BaseCommandParams(pack,
            CommandType.DELSTICKER),
            new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
    }
}




