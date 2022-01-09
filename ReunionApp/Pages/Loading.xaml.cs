using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using TgApi.Telegram;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReunionApp.Pages
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class Loading : Page
	{

		private AuthHandler auth;
		private AuthHandler.AuthState authState;

		public Loading()
		{
			this.InitializeComponent();
			auth = new AuthHandler(TgApi.GlobalVars.Client);
			handle_auth();
		}

		async Task handle_auth()
		{
			await Task.Delay(1000);
			if (auth is null) return;
			var lastRequest = DateTimeOffset.MinValue;
			authState = AuthHandler.GetState(auth.CurrentState);
			while ( authState != AuthHandler.AuthState.Ready ||
				authState != AuthHandler.AuthState.WaitPhoneNumber ||
				authState != AuthHandler.AuthState.WaitCode)
			{
				switch (authState)
				{
					case AuthHandler.AuthState.WaitTdLibParams: 
						await auth.Handle_WaitTdLibParameters(TgApi.GlobalVars.TdParams);
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
				LoadingProgressText.Text = authState.ToString();

				if (authState == AuthHandler.AuthState.Null)
				{
					LoadingProgressRing.ShowError = true;
					LoadingProgressText.Text = "The program encountered an unexpected state during authentication";
				}
			}
		}
	}
}
