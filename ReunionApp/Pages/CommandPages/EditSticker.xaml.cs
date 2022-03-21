using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using NeoSmart.Unicode;
using ReunionApp.Runners;
using TgApi.Types;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReunionApp.Pages.CommandPages;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class EditSticker : Page
{
    private ObservableCollection<EditStickerUpdate> stickers = new ObservableCollection<EditStickerUpdate>();

    public EditSticker() => this.InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        var p = e.Parameter as Sticker[];
        foreach (var s in p) stickers.Add(new EditStickerUpdate { sticker = s});
    }

    private void Back(object sender, RoutedEventArgs e) => Frame.GoBack();

    private async Task<bool> FindErrors()
    {
        var sl = new List<string>();
        foreach (var s in stickers) sl.Add(s.newEmoji);
        var errs = StickerLogic.GetEmojiErrorsList(sl.ToArray());
        if (errs.Length > 0)
        {
            await App.GetInstance().ShowBasicDialog("Please fix the following issues", string.Join("\n", errs.Select(x => x.ToString())));
            return true;
        }

        return false;
    }

    private async void Finish(object sender, RoutedEventArgs e)
    {
        if (await FindErrors()) return;
        var runner = new EditStickerRunner(stickers.ToArray());
        Frame.Navigate(typeof(ProcessingCommand), runner, new DrillInNavigationTransitionInfo());
    }

    private void Emojis_TextChanged(object sender, RoutedEventArgs args)
    {
        var send = sender as TextBox; // TODO Find out how to replace this with dedicated autosuggest control
        send.Text = GEmojiSharp.Emoji.Emojify(send.Text);
        send.Select(send.Text.Length, send.Text.Length);
    }
}

public record EditStickerUpdate
{
    public Sticker sticker { get; init; }
    public string newEmoji { get; set; } = "";
}
