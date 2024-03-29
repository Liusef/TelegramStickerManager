﻿using System;
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
using TgApi.Types;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReunionApp.Pages.CommandPages;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class EditReplaceSelector : Page
{
    private StickerPack pack;

    public EditReplaceSelector() => this.InitializeComponent();
    

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        pack = e.Parameter as StickerPack;
    }

    private async Task<bool> FindErrors()
    {
        if (Grid.SelectedItems.Count == 0)
        {
            await App.GetInstance().ShowBasicDialog("You can't do that!", "You must select at least 1 item in order to continue");
            return true;
        }
        return false;
    }

    private async void Edit(object sender, RoutedEventArgs e)
    {
        if (await FindErrors()) return;
        var sts = new List<Sticker>();
        foreach (var s in Grid.SelectedItems) if (s is Sticker sticker) sts.Add(sticker);
        Frame.Navigate(typeof(EditSticker), sts.ToArray(), new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight });
    }

    private async void Replace(object sender, RoutedEventArgs e)
    {
        if (await FindErrors()) return;
        var sts = new List<Sticker>();
        foreach (var s in Grid.SelectedItems) if (s is Sticker sticker) sts.Add(sticker);
        Frame.Navigate(typeof(ReplaceSticker), sts.ToArray(), new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight });
    }
}
