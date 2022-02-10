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
public sealed partial class DelSticker : Page
{
    private StickerPack pack;
    private Button back;

    public record DelStickerParams(StickerPack stickerPack, Button back);

    public DelSticker()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        var p = e.Parameter as DelStickerParams;
        if (p != null)
        {
            pack = p.stickerPack;
            back = p.back;
        }
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {

        Bindings.StopTracking();
        base.OnNavigatedFrom(e);
    }

    private async void Finish(object sender, RoutedEventArgs e)
    {
        if (await FindErrors()) return;

        processing.Visibility = Visibility.Visible;
        back.IsEnabled = false;

        var l = new List<Sticker>();
        foreach (var s in Grid.SelectedItems) if (s is Sticker sticker) l.Add(sticker);
        var runner = new DelStickerRunner(l.ToArray());

        processing.Visibility = Visibility.Collapsed;
        ((Frame)Parent).Navigate(typeof(ProcessingCommand), runner, new DrillInNavigationTransitionInfo());
    }

    private async Task<bool> FindErrors()
    {
        if (Grid.SelectedItems.Count == 0)
        {
            await App.GetInstance().ShowBasicDialog("You haven't selected anything!", "You must choose at least 1 sticker to delete from the pack.");
            return true;
        }
        if (Grid.SelectedItems.Count == pack.Count)
        {
            await App.GetInstance().ShowBasicDialog("You selected all stickers", "You must keep at least 1 sticker in the pack. " +
                                                                                 "If you want to delete the pack, use @Stickers in Telegram.");
            return true;
        }
        return false;
    }

}
