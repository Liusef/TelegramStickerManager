﻿using System;
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
using ReunionApp.Runners;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReunionApp.Pages.CommandPages;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ProcessingCommand : Page
{
    CommandRunner runner;

    public ProcessingCommand()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        runner = e.Parameter as CommandRunner;
        runner.Outputs.CollectionChanged += Outputs_CollectionChanged;

        await runner.RunCommandsAsync();
        Continue.IsEnabled = true;

        await Task.Run(async () => await Task.Delay(100));
    }

    private async void Outputs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        await Task.Run(async () => await Task.Delay(20));
        Scroll.ChangeView(null, Scroll.ScrollableHeight, null);
    }

    private async void Continue_Click(object sender, RoutedEventArgs e)
    {
        Loading.IsIndeterminate = true;
        Continue.IsEnabled = false;
        await runner.PostTasksAsync();

        App.GetInstance().RootFrame.Navigate(typeof(Home), true, new DrillInNavigationTransitionInfo());
    }
}
