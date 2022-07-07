using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TdLib;
using TgApi;
using TgApi.Telegram;

namespace ReunionApp;

/// <summary>
/// The main Application class associated with this project
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// The amount of threads on the user's system (at a maxmimum of 6)
    /// </summary>
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

    /// <summary>
    /// An event handler that is run when the app hits an unhandled exception
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        await this.ShowExceptionDialog(e.Exception);
    }
}
