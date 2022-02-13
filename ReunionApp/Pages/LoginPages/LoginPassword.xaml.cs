using System;
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
using TgApi.Telegram;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReunionApp.Pages.LoginPages;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class LoginPassword : Page
{
    private AuthHandler auth;

    public LoginPassword()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        auth = e.Parameter as AuthHandler;
    }

    private void Pwd_PasswordChanged(object sender, RoutedEventArgs e)
    {
        ContinueButton.IsEnabled = Pwd.Password.Length > 0;
    }

    private async void Pwd_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter && ContinueButton.IsEnabled)
            await AttemptLogin();
    }

    private async void ContinueButton_Click(object sender, RoutedEventArgs e)
    {
        await AttemptLogin();
    }

    private async Task AttemptLogin()
    {
        var last = auth.LastRequestReceivedAt;

        try
        {
            await auth.Handle_WaitPassword(Pwd.Password);
        }
        catch (Exception ex)
        {
            await App.GetInstance().ShowExceptionDialog(ex);
            return;
        }

        while (last == auth.LastRequestReceivedAt) await Task.Delay(50);

        if (LoginCode.IsNotSupportedState(AuthHandler.GetState(auth.CurrentState))) return;

        if (AuthHandler.GetState(auth.CurrentState) == AuthHandler.AuthState.Ready)
            App.GetInstance().RootFrame.Navigate(typeof(Home), null, new DrillInNavigationTransitionInfo());
    }
}
