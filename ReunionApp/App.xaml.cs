﻿using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TdLib;
using TgApi;
using TgApi.Telegram;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReunionApp;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    public static int Threads { get; } = Environment.ProcessorCount < 6 ? Environment.ProcessorCount : 6;

    internal bool IsCdOpen = false;
    
    public TdClient Client = new();
    public AuthHandler Auth;
    public AuthHandler.AuthState AuthState;
    
    public MainWindow MainWindow { get; private set; }

    public Frame RootFrame => MainWindow.ContentFrame;

    
    /// <summary>
    /// Gets the current instance of the application as an App object
    /// </summary>
    /// <returns> The current instance of the application as an App object</returns>
    public static App GetInstance() => Current as App;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched normally by the end user.  Other entry points
    /// will be used such as when the application is launched to open a specific file.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow();
        Auth = new AuthHandler(Client);
        Client.Bindings.SetLogVerbosityLevel(1);
        GlobalVars.EnsureDirectories();
        App.Current.UnhandledException += App_UnhandledException;
        MainWindow.Activate();
        await this.HandleAuth();
    }

    private async void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        await this.ShowExceptionDialog(e.Exception);
    }
}
