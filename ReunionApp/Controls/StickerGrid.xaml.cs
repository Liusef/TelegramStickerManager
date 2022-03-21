using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using TgApi.Types;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReunionApp.Controls;
public sealed partial class StickerGrid : UserControl
{
    public object ItemsSource 
    { 
        get => Grid.ItemsSource; 
        set => Grid.ItemsSource = value; 
    }
    public bool CanReorderItems { get; set; } = false;
    public ListViewSelectionMode SelectionMode { get; set; } = ListViewSelectionMode.None;
    public new bool AllowDrop { get; set; } = false;

    public IList<object> SelectedItems => Grid.SelectedItems;

    public StickerGrid() 
    { 
        this.InitializeComponent();
    }
}
