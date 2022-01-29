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
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReunionApp.Controls;

public sealed partial class ImageAsync : UserControl
{
    public ImageAsync()
    {
        this.InitializeComponent();
        //this.Margin = default;
        //LoadImage(path, DecodeWidth);
    }

    private string path;
    public int DecodeWidth {get; set;}
    public string Path { get => path; 
        set 
        {
            path = value;
            LoadImage(path, DecodeWidth);
        } 
    }

    private async Task LoadImage(string path, int decodeWidth)
    {
        Img.Visibility = Visibility.Collapsed;
        Loading.Visibility = Visibility.Visible;

        var bimg = new BitmapImage();

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;

        using (var fs = await FileRandomAccessStream.OpenAsync(path, Windows.Storage.FileAccessMode.Read))
        {
            bimg.DecodePixelWidth = decodeWidth;
            await bimg.SetSourceAsync(fs);
            Img.Source = bimg;
        }

        Img.Visibility = Visibility.Visible;
        Loading.Visibility = Visibility.Collapsed;
    }
}
