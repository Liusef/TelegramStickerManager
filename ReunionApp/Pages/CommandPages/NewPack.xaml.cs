using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TgApi.Types;
using Windows.Storage.Pickers;
using static ReunionApp.Pages.BaseCommand;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReunionApp.Pages.CommandPages;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class NewPack : Page
{
    private string ThumbPath { get; set; } = " ";

    public NewPack() => this.InitializeComponent();
    
    private void Back(object sender, RoutedEventArgs e) => App.GetInstance().RootFrame.GoBack();
    
    private async Task ChooseThumb()
    {
        var file = await AppUtils.PickSingleFileAsync(AppUtils.ImageSharpFormats);
        if (file == null || !File.Exists(file.Path)) return;
        ThumbPath = file.Path;
        Filename.Text = Path.GetFileName(ThumbPath);
    }

    private async void SplitButton_Click(SplitButton sender, SplitButtonClickEventArgs args) => await ChooseThumb();

    private async void AddFlyout_Click(object sender, RoutedEventArgs e) => await ChooseThumb();

    private void ClearThumb(object sender, RoutedEventArgs e)
    {
        ThumbPath = " ";
        Filename.Text = "No Thumbnail Selected";
    }

    private async void Continue(object sender, RoutedEventArgs e)
    {
        if (await FindErrors()) return;

        App.GetInstance().RootFrame.Navigate(typeof(BaseCommand), new BaseCommandParams(
            new StickerPack
            {
                Title = PackTitle.Text,
                Name = PackName.Text,
                Thumb = new NewPackThumb { Path = ThumbPath }
            }, CommandType.NewPack));
    }

    private async Task<bool> FindErrors()
    {
        if (PackTitle.Text.Length == 0)
        {
            await App.GetInstance().ShowBasicDialog("Please enter a title", "Your pack needs to have a title. Please enter one!");
            return true;
        }
        if (PackName.Text.Length == 0)
        {
            await App.GetInstance().ShowBasicDialog("Please give your pack a name", "Your pack needs a name. Please enter one!\n" +
                                                                                    "If the name you enter isn't valid, or already taken we'll let you know later.");
            return true;
        }
        if (!string.IsNullOrWhiteSpace(ThumbPath) && !File.Exists(ThumbPath))
        {
            await App.GetInstance().ShowBasicDialog("Please choose another thumbnail", "We couldn't find the thumbnail you selected, please choose another one.");
            return true;
        }
        return false;
    }
}

public class NewPackThumb : StickerPackThumb
{
    public string Path { get; set; }
    public override string LocalPath => Path;
    public override string BestPath => Path;
}
