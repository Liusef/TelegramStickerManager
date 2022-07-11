using System.Collections.Generic;
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
public sealed partial class DelSticker : Page
{
    private StickerPack pack;

    public DelSticker() =>  this.InitializeComponent();
    
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        pack = e.Parameter as StickerPack;
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        Bindings.StopTracking();
        //Grid.ItemsSource = new Sticker[0]; // TODO decide if this is necessary
        base.OnNavigatedFrom(e);
    }

    private async void Finish(object sender, RoutedEventArgs e)
    {
        if (await FindErrors()) return;

        var yesClick = () =>
        {
            processing.Visibility = Visibility.Visible;
            var l = new List<Sticker>();
            foreach (object s in Grid.SelectedItems) if (s is Sticker sticker) l.Add(sticker);
            var runner = new DelStickerRunner(l.ToArray());
            Frame.Navigate(typeof(ProcessingCommand), runner, new DrillInNavigationTransitionInfo());
        };

        await App.GetInstance().ShowAreYouSureDialog("Are you sure?",
            "Deleting stickers cannot be undone. Are you sure you would like to continue?",
            "Yes", yesClick,
            "No", null);
    }

    private async Task<bool> FindErrors()
    {
        if (Grid.SelectedItems.Count == 0)
        {
            await App.GetInstance().ShowBasicDialog("You haven't selected anything!", "You must choose at least 1 sticker to delete from the pack.");
            return true;
        }
        if (Grid.SelectedItems.Count == pack.Count)
        {
            await App.GetInstance().ShowBasicDialog("You selected all stickers", "You must keep at least 1 sticker in the pack. " +
                                                                                 "If you want to delete the pack, please message @Stickers on Telegram.");
            return true;
        }
        return false;
    }
}
