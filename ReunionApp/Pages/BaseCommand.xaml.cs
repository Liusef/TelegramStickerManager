using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using TgApi.Types;
using static ReunionApp.Pages.CommandPages.AddSticker;
using static ReunionApp.Pages.CommandPages.DelSticker;
using static ReunionApp.Pages.CommandPages.OrderSticker;
using static ReunionApp.Pages.CommandPages.SetPackIcon;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReunionApp.Pages;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class BaseCommand : Page
{

    private StickerPack pack;
    public bool IsBackEnabled
    {
        get => BackButton.IsEnabled;
        set => BackButton.IsEnabled = value;
    }

    public record BaseCommandParams(StickerPack pack, CommandType commandType);

    public BaseCommand()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        var (stickerPack, commandType) = e.Parameter as BaseCommandParams;
        pack = stickerPack;
        SelectPage(commandType);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        // This block is to ensure that bitmaps and other large objects are garbage collected, as pages aren't disposed by the garbage collector
        // NOTE: Lots of objects that need to be garbage collected are RefCounted from Unmanaged memory
        // TODO This is not a good solution for memory management. Find a way to dispose of pages instead.
        NavigationCacheMode = NavigationCacheMode.Disabled;
        ContentFrame.Navigate(typeof(Page));
        Bindings.StopTracking();
        AppUtils.CollectLater(5000);   // This code ensures that when this method is called and images aren't being displayed,
                                       // since the images are in unmanaged memory, they're discarded and most of the memory it used
                                       // is freed. (The rest is usually freed on the next page navigation)
                                       // This solution is awful, stupid, and terrible, and i have no idea why it works.
                                       // TODO Find a better way to deal free memory for images that aren't being displayed
        base.OnNavigatedFrom(e);
    }

    private async void SelectPage(CommandType type)
    {
        switch (type)
        {
            case CommandType.AddSticker:
                ContentFrame.Navigate(typeof(CommandPages.AddSticker), new AddStickerParams(pack, false));
                InfoFrame.Navigate(typeof(CommandPages.SidePanels.AddInfo));
                Op.Text = "Add New Stickers";
                break;
            case CommandType.DelSticker:
                ContentFrame.Navigate(typeof(CommandPages.DelSticker), pack);
                InfoFrame.Navigate(typeof(CommandPages.SidePanels.DelInfo));
                Op.Text = "Delete Stickers";
                break;
            case CommandType.NewPack:
                ContentFrame.Navigate(typeof(CommandPages.AddSticker), new AddStickerParams(pack, true));
                InfoFrame.Navigate(typeof(CommandPages.SidePanels.AddInfo));
                Op.Text = "Add Some Stickers!";
                break;
            case CommandType.OrderSticker:
                ContentFrame.Navigate(typeof(CommandPages.OrderSticker), pack);
                InfoFrame.Navigate(typeof(CommandPages.SidePanels.OrderInfo));
                Op.Text = "Reorder Stickers";
                break;
            case CommandType.SetPackIcon:
                ContentFrame.Navigate(typeof(CommandPages.SetPackIcon), pack);
                Op.Text = "Set Pack Icon";
                break;
            case CommandType.EditReplaceSticker:
                ContentFrame.Navigate(typeof(CommandPages.EditReplaceSelector), pack);
                InfoFrame.Navigate(typeof(CommandPages.SidePanels.EditReplaceInfo));
                Op.Text = "Edit or Replace Sticker";
                break;
            default:
                await App.GetInstance().ShowBasicDialog("No command was selected",
                    "Somehow, no command for modifying the stickerpack was selected. Please click back.");
                break;
        }
    }

    private void Back(object sender, RoutedEventArgs e) => App.GetInstance().RootFrame.GoBack();
}

public enum CommandType
{
    None = 0,
    AddSticker = 1,
    DelSticker = 2,
    NewPack = 3,
    OrderSticker = 4,
    SetPackIcon = 5,
    EditReplaceSticker = 6,
}
