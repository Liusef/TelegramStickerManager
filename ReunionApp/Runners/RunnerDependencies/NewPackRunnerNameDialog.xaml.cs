using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using TgApi.Types;

namespace ReunionApp.Runners.RunnerDependencies;

/// <summary>
/// The dialog box that is displayed when the user selects an invalid name for their sticker pack
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

    /// <summary>
    /// Whether or not the name is valid when checked locally (only @Stickers can check if the name is truly valid)
    /// </summary>
    /// <returns>Whether or not the name is preliminarily valid</returns>
    public bool IsValid()
    {
        var txt = UInput.Text;
        return !string.IsNullOrWhiteSpace(txt);
    }

    /// <summary>
    /// An event handler method for when a key is pressed. Closes the dialog box on "Enter"
    /// </summary>
    /// <param name="sender">sender</param>
    /// <param name="e">update</param>
    private void UInput_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key != Windows.System.VirtualKey.Enter) return;
        if (!IsValid()) return;
        Pack.Name = UInput.Text;
        Dialog.Hide();
    }
}
