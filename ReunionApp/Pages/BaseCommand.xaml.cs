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
using Microsoft.UI.Xaml.Navigation;
using TgApi.Types;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReunionApp.Pages;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class BaseCommand : Page
{

    private StickerPack pack;
    public Frame ContentFrame => Frame;

    public record BaseCommandParams(StickerPack pack, CommandType commandType);

    public enum CommandType
    {
        NONE = 0,
        ADDSTICKER = 1,
    }

    public BaseCommand()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        var parameters = e.Parameter as BaseCommandParams;
        pack = parameters.pack;
        SelectPage(parameters.commandType);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        // This block is to ensure that bitmaps and other large objects are garbage collected, as pages aren't disposed by the garbage collector
        // NOTE: Lots of objects that need to be garbage collected are RefCounted from Unmanaged memory
        // TODO This is not a good solution for memory management. Find a way to dispose of pages instead.
        NavigationCacheMode = NavigationCacheMode.Disabled;
        ContentFrame.Navigate(typeof(Page));
        UnloadObject(ContentFrame);
        UnloadObject(InfoFrame);
        Bindings.StopTracking();
        Task.Run(async () => { await Task.Delay(5000); GC.Collect();});  // This code ensures that when this method is called and images aren't being displayed,
                                                                          // since the images are in unmanaged memory, they're discarded and most of the memory it used
                                                                          // is freed. (The rest is usually freed on the next page navigation)
                                                                          // This solution is awful, stupid, and terrible, and i have no idea why it works.
                                                                          // TODO Find a better way to deal free memory for images that aren't being displayed
        base.OnNavigatedFrom(e);
    }

    private void SelectPage(CommandType type)
    {
        switch (type)
        {
            case CommandType.ADDSTICKER:
                ContentFrame.Navigate(typeof(CommandPages.AddSticker));
                InfoFrame.Navigate(typeof(CommandPages.InfoPages.AddInfo));
                break;
            default:
                App.GetInstance().ShowBasicDialog("No command was selected",
                    "Somehow, no command for modifying the stickerpack was selected. Please click back.");
                break;
        }
    }

    private void Back(object sender, RoutedEventArgs e) =>
        App.GetInstance().RootFrame.GoBack();
}
