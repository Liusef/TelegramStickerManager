using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
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

    private string Path { get; set; } = " ";

    public NewPack()
    {
        this.InitializeComponent();
    }

    private void Back(object sender, RoutedEventArgs e) => App.GetInstance().RootFrame.GoBack();
    
    private async Task ChooseThumb()
    {
        var picker = new FileOpenPicker();
        picker.ViewMode = PickerViewMode.Thumbnail;
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".webp");
        picker.FileTypeFilter.Add(".bmp");
        picker.FileTypeFilter.Add(".tga");

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.GetInstance().MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSingleFileAsync();
        if (file == null || !File.Exists(file.Path)) return;
        Path = file.Path;
        Filename.Text = TgApi.Utils.GetPathFilename(Path);
    }

    private async void SplitButton_Click(SplitButton sender, SplitButtonClickEventArgs args) => await ChooseThumb();

    private async void AddFlyout_Click(object sender, RoutedEventArgs e) => await ChooseThumb();

    private void ClearThumb(object sender, RoutedEventArgs e)
    {
        Path = " ";
        Filename.Text = "No Thumbnail Selected";
    }

    private class NewPackThumb : StickerPackThumb
    {
        public string _Path { get; set; }
        public override string LocalPath => _Path;
        public override string BestPath => _Path;
    }

    private async void Continue(object sender, RoutedEventArgs e)
    {
        if (await FindErrors()) return;
        if (File.Exists(Path))
        {
            try
            {
                string temp = TgApi.GlobalVars.TempDir;
                string saveDir = temp + System.DateTime.Now.Ticks + ".png";
                using (var img = await SixLabors.ImageSharp.Image.LoadAsync(Path))
                {
                    img.Mutate(x => x.Resize(100, 100));
                    await img.SaveAsync(saveDir);
                    img.Dispose();
                }
                Configuration.Default.MemoryAllocator.ReleaseRetainedResources();
                Path = saveDir;
            }
            catch (Exception ex)
            {
                Path = " ";
            }
        }

        App.GetInstance().RootFrame.Navigate(typeof(BaseCommand), new BaseCommandParams(
            new TgApi.Types.StickerPack
            {
                Title = Title.Text,
                Name = Name.Text,
                Thumb = new NewPackThumb { _Path = Path }
            }, CommandType.NEWPACK));
    }

    private async Task<bool> FindErrors()
    {
        if (Title.Text.Length == 0)
        {
            await App.GetInstance().ShowBasicDialog("Please enter a title", "Your pack needs to have a title. Please enter one!");
            return true;
        }
        if (Name.Text.Length == 0)
        {
            await App.GetInstance().ShowBasicDialog("Please give your pack a name", "Your pack needs a name. Please enter one!\n" +
                                                                                    "If the name you enter isn't valid, or already taken we'll let you know later.");
            return true;
        }
        if (!string.IsNullOrWhiteSpace(Path) && !File.Exists(Path))
        {
            await App.GetInstance().ShowBasicDialog("Please choose another thumbnail", "We couldn't find the thumbnail you selected, please choose another one.");
            return true;
        }
        return false;
    }
}
