using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using TgApi.Types;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReunionApp.Runners.RunnerDependencies;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class NewPackRunnerNameDialog : Page
{
    public TextBox UserInput => UInput;
    public ContentDialog Dialog { get; set; }
    public StickerPack Pack { get; set; }

    public NewPackRunnerNameDialog()
    {
        this.InitializeComponent();
    }

    public bool IsValid()
    {
        var txt = UInput.Text;
        if (string.IsNullOrWhiteSpace(txt)) return false;
        return true;
    }

    private void UInput_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            if (!IsValid()) return;
            Pack.Name = UInput.Text;
            Dialog.Hide();
        }
    }
}
