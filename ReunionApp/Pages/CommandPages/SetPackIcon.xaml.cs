using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using ReunionApp.Runners;
using TgApi.Types;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using static ReunionApp.Pages.ProcessingCommand;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReunionApp.Pages.CommandPages;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SetPackIcon : Page
{
    private StickerPack pack;

    public SetPackIcon() => this.InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        pack = e.Parameter as StickerPack;
    }

    private async void ClearIcon(object sender, RoutedEventArgs e) 
    {
        if (pack.EnsuredThumb.IsDesignatedThumb) await AreYouSure(() => Continue(null));
        else await App.GetInstance().ShowBasicDialog("You can't do that!", "This pack doesn't have a designated thumbnail or icon, thus it cannot be removed");
    }

    private async void UploadNew(object sender, RoutedEventArgs e)
    {
        var file = await AppUtils.PickSingleFileAsync(AppUtils.ImageSharpFormats);
        if (file == null || !File.Exists(file.Path)) return;
        Load.Visibility = Visibility.Visible;
        var path = await Task.Run(async()=>await TgApi.ImgUtils.ResizeAsync(file.Path, 100, 100, true, new[] { "png", "webp" }));

        await AreYouSure(() => Continue(path));
    }

    private async Task AreYouSure(Action yes) => await App.GetInstance().ShowAreYouSureDialog("Are you sure?",
        "Changing the icon of a sticker pack cannot be undone.",
        "Yes", yes, "No", null);

    private void Continue(string path) => Frame.Navigate(typeof(ProcessingCommand),
        new SetPackIconRunner(pack, path),
        new DrillInNavigationTransitionInfo());
}
