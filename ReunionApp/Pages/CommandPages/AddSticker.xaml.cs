using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using NeoSmart.Unicode;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReunionApp.Pages.CommandPages;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class AddSticker : Page
{

    private ObservableCollection<NewSticker> stickers = new ObservableCollection<NewSticker>();

    public AddSticker()
    {
        this.InitializeComponent();
    }

    private async void Add(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        picker.ViewMode = PickerViewMode.Thumbnail;
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".webp");

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.GetInstance().MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var files = await picker.PickMultipleFilesAsync();
        if (files.Count == 0) return;
        foreach (var file in files)
        {
            stickers.Add(new NewSticker { imgPath = file.Path });
        }
    }
    
    private void Delete(object sender, RoutedEventArgs e)
    {
        var selected = Grid.SelectedItems;
        if (selected.Count == 0) return;
        foreach (var item in selected.Reverse())
        {
            stickers.Remove(item as NewSticker);
        }
    }

    private async void Finish(object sender, RoutedEventArgs e)
    {
        if (stickers.Count == 0)
		{
            await App.GetInstance().ShowBasicDialog("You didn't add anything!", "Please add some stickers before continuing!");
            return;
		}

        List<string> errs = new List<string>();
        
        for (int i = 0; i < stickers.Count; i++)
		{
            var s = stickers[i];
            if (!File.Exists(s.imgPath)) errs.Add($"- Error at Index {i}: File {s.imgPath} not found.");
            if (string.IsNullOrWhiteSpace(s.emojis)) errs.Add($"- Error at Index {i}: Emojis list cannot be empty.");
            else if (!Emoji.IsEmoji(s.emojis)) errs.Add($"- Error at Index {i}: Emojis list contains non-emoji characters.");
        }

        if (errs.Count > 0)
        {
            errs.Insert(0, "Please remember that indices are 0 based. The first item is 0.");
            await App.GetInstance().ShowBasicDialog("We found some errors", String.Join("\n", errs));
            return;
        }
    }

    private void SplitButton_Click(SplitButton sender, SplitButtonClickEventArgs args)
    {
        Add(sender, default);
    }

    private void Emojis_TextChanged(object sender, RoutedEventArgs args)
    {
        var send = sender as TextBox;
        send.Text = GEmojiSharp.Emoji.Emojify(send.Text);
        send.Select(send.Text.Length, send.Text.Length);
    }

    
}

public class NewSticker
{
    public string imgPath { get; set; }
    public string emojis { get; set; } = "";
}

