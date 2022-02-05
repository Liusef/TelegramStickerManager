using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using TgApi;
using TgApi.Telegram;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Animation;
using System.Collections.ObjectModel;
using TgApi.Types;
using Microsoft.UI.Xaml.Media.Imaging;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReunionApp;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
	private MainWindow m_window;
	
	public MainWindow MainWindow => m_window;
	public Frame RootFrame => MainWindow.ContentFrame;
	private bool isCdOpen = false;
	public TdLib.TdClient Client = new TdLib.TdClient();
	public AuthHandler auth;
	public AuthHandler.AuthState authState;
	
	/// <summary>
	/// Gets the current instance of the application as an App object
	/// </summary>
	/// <returns> The current instance of the application as an App object</returns>
	public static App GetInstance() => Application.Current as App;

	/// <summary>
	/// Initializes the singleton application object.  This is the first line of authored code
	/// executed, and as such is the logical equivalent of main() or WinMain().
	/// </summary>
	public App()
	{
		auth = new AuthHandler(Client);
		Client.Bindings.SetLogVerbosityLevel(2);
		GlobalVars.EnsureDirectories();
		HandleAuth();
		this.InitializeComponent();
		App.Current.UnhandledException += App_UnhandledException;
	}

	/// <summary>
	/// Invoked when the application is launched normally by the end user.  Other entry points
	/// will be used such as when the application is launched to open a specific file.
	/// </summary>
	/// <param name="args">Details about the launch request and process.</param>
	protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
	{
		m_window = new MainWindow();
		m_window.Activate();
	}

	private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
	{
		e.Handled = true;
		ShowExceptionDialog(e.Exception);
	}

    public async Task ShowBasicDialog(string title, string body, string closeText = "Ok")
    {
        if (isCdOpen) return;
        isCdOpen = true;
        var cd = new ContentDialog();
        cd.Title = title;
        cd.IsPrimaryButtonEnabled = false;
        cd.IsSecondaryButtonEnabled = false;
        cd.CloseButtonText = closeText;
        var b = new Pages.DialogBody();
        b.Body.Text = body;
        cd.Content = b;
        cd.XamlRoot = m_window.Content.XamlRoot;

        cd.CloseButtonClick += (sender, args) => isCdOpen = false;

        await cd.ShowAsync();
    }

	public async Task ShowExceptionDialog(Exception exception)
	{
        await ShowBasicDialog($"Oops! The program hit a(n) {exception.GetType()} Exception", exception.ToString());
    }

	public async Task HandleAuth()
	{
		if (auth is null) return;
		await Task.Delay(200);
		var lastRequest = DateTimeOffset.MinValue;
		authState = AuthHandler.GetState(auth.CurrentState);
		while (authState != AuthHandler.AuthState.Ready &&
			authState != AuthHandler.AuthState.WaitPhoneNumber &&
			authState != AuthHandler.AuthState.WaitCode &&
			authState != AuthHandler.AuthState.WaitOtherDeviceConfirmation &&
			authState != AuthHandler.AuthState.WaitRegistration &&
			authState != AuthHandler.AuthState.WaitPassword)
		{
			switch (authState)
			{
				case AuthHandler.AuthState.WaitTdLibParams:
					await auth.Handle_WaitTdLibParameters(new TdLib.TdApi.TdlibParameters()
					{
						ApiId = GlobalVars.ApiId,
						ApiHash = GlobalVars.ApiHash,
						SystemLanguageCode = GlobalVars.SystemLanguageCode,
						DeviceModel = GlobalVars.DeviceModel,
						ApplicationVersion = GlobalVars.ApplicationVersion,
						DatabaseDirectory = GlobalVars.TdDir
					});
					break;
				case AuthHandler.AuthState.WaitEncryptionKey:
					await auth.Handle_WaitEncryptionKey();
					break;
				default:
					await Task.Delay(100);
					break;
			}

			while (lastRequest == auth.LastRequestReceivedAt) await Task.Delay(100);

			lastRequest = auth.LastRequestReceivedAt;
			authState = AuthHandler.GetState(auth.CurrentState);

			if (authState == AuthHandler.AuthState.Null)
			{
				RootFrame.Navigate(typeof(Pages.GenericError), "The application encountered an unknown sign-in state. Please restart the app");
			}
		}

		switch (authState)
		{
			case AuthHandler.AuthState.Ready:
				RootFrame.Navigate(typeof(Pages.Home), null, new DrillInNavigationTransitionInfo());
				break;
			default:
				RootFrame.Navigate(typeof(Pages.LoginPages.LoginPhone), auth, new DrillInNavigationTransitionInfo());
				break;
		}
	}

	public static BitmapImage GetBitmapFromPath(string path) => new BitmapImage(new Uri(path));

    public static BitmapImage PackThumbImg(StickerPackThumb thumb) => GetBitmapFromPath(TgApi.GlobalVars.TdDir +
                          (thumb.IsDesignatedThumb ? "thumbnails" : "stickers") +
                          System.IO.Path.DirectorySeparatorChar + thumb.Filename);

    public static BitmapImage StickerFromFilename(string filename) =>
        GetBitmapFromPath($"{TgApi.GlobalVars.TdDir}stickers{System.IO.Path.DirectorySeparatorChar}{filename}");

    public static BitmapImage PackThumb(StickerPackThumb thumb)
    {
        if (thumb.Type == StickerType.STANDARD) return PackThumbImg(thumb);
        return new BitmapImage();
    }

}

