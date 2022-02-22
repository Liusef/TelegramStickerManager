using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using ReunionApp.Runners;
using TgApi.Types;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReunionApp.Pages.CommandPages;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class OrderSticker : Page
{
    private StickerPack pack;
    private Button back;

    public record OrderStickerParams(StickerPack packParam, Button backParam);

    public ObservableCollection<Sticker> stickers;

    public OrderSticker()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        var p = e.Parameter as OrderStickerParams;
        if (p != null)
        {
            pack = p.packParam;
            back = p.backParam;
        }
        stickers = new ObservableCollection<Sticker>(pack.Stickers);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        Bindings.StopTracking();
        base.OnNavigatedFrom(e);
    }

    private async void Finish(object sender, RoutedEventArgs e)
    {
        if (await FindErrors()) return;

        processing.Visibility = Visibility.Visible;
        back.IsEnabled = false;

        (Sticker, Sticker)[] swaps = null;

        try
        {
            swaps = Runners.RunnerDependencies.OrderStickerRunnerSorter.GetSwaps(pack, stickers.ToArray());
        }
        catch (Exception ex)
        {
            await App.GetInstance().ShowExceptionDialog(ex);
        }
        var runner = new OrderStickerRunner(swaps);

        processing.Visibility = Visibility.Collapsed;
        ((Frame)Parent).Navigate(typeof(ProcessingCommand), runner, new DrillInNavigationTransitionInfo());
    }

    private void Reset(object sender, RoutedEventArgs e)
    {
        stickers = new ObservableCollection<Sticker>(pack.Stickers);
        Grid.ItemsSource = stickers;
    }

    private async Task<bool> FindErrors()
    {
        if (Runners.RunnerDependencies.OrderStickerRunnerSorter.IsSorted(pack, stickers.ToArray()))
        {
            await App.GetInstance().ShowBasicDialog("You didn't move anything!", "Please reorder at least one sticker to continue");
            return true;
        }
        return false;
    }
}
