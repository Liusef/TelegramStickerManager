using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using ReunionApp.Runners;
using TgApi.Types;
using static ReunionApp.Pages.ProcessingCommand;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReunionApp.Pages.CommandPages;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class OrderSticker : Page
{
    private StickerPack pack;

    public ObservableCollection<Sticker> stickers;

    public OrderSticker() => this.InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        pack = e.Parameter as StickerPack;
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
        (Sticker, Sticker)[] swaps = Runners.RunnerDependencies.OrderStickerRunnerSorter.GetSwaps(pack, stickers.ToArray());
        var runner = new OrderStickerRunner(swaps);
        Frame.Navigate(typeof(ProcessingCommand), runner, new DrillInNavigationTransitionInfo());
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
