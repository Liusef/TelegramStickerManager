﻿using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using static TdLib.TdApi;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReunionApp.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class Settings : Page
{
    public Settings()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
    }

    private void Back(object sender, RoutedEventArgs e) => App.GetInstance().RootFrame.GoBack();

    private async void Clear_Click(object sender, RoutedEventArgs e) => await App.GetInstance().Client.OptimizeStorageAsync();
    

    private async void Logout_Click(object sender, RoutedEventArgs e)
    {
        await App.GetInstance().Client.LogOutAsync();
        await Task.Delay(100); // TODO Make this wait for a logout reply
        Directory.Delete(TgApi.GlobalVars.TdDir, true);
        Environment.Exit(0);
    }

    private async void Local_Click(object sender, RoutedEventArgs e) => await Windows.System.Launcher.LaunchUriAsync(new(TgApi.GlobalVars.TdDir));
}
