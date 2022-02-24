using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using TgApi.Telegram;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReunionApp.Pages.LoginPages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class LoginCode : Page
{

    private AuthHandler auth;
    private string phone;

    public record LoginCodeParams(AuthHandler auth, string phone);

    public LoginCode()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        var (authHandler, s) = (LoginCodeParams) e.Parameter;
        auth = authHandler;
        phone = s;
    }

    private void ChangeButton_Click(object sender, RoutedEventArgs e) => App.GetInstance().RootFrame.GoBack();

    private async void ContinueButton_Click(object sender, RoutedEventArgs e) => await TgSubmitCode(CodeBox.Text.Trim());

    private async Task TgSubmitCode(string code)
    {
        var last = auth.LastRequestReceivedAt;

        try
        {
            await auth.Handle_WaitCode(code);
        }
        catch (Exception ex)
        {
            await App.GetInstance().ShowExceptionDialog(ex);
            return;
        }

        while (last == auth.LastRequestReceivedAt) await Task.Delay(50);

        if (IsNotSupportedState(AuthHandler.GetState(auth.CurrentState))) return;

        if (AuthHandler.GetState(auth.CurrentState) == AuthHandler.AuthState.WaitPassword)
            App.GetInstance().RootFrame.Navigate(typeof(LoginPassword), auth, new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight });

        if (AuthHandler.GetState(auth.CurrentState) == AuthHandler.AuthState.Ready)
            App.GetInstance().RootFrame.Navigate(typeof(Home), true, new DrillInNavigationTransitionInfo());
    }

    private void CodeBox_TextChanged(object sender, TextChangedEventArgs e) => ContinueButton.IsEnabled = CodeBox.Text.Length > 0;
    

    private async void CodeBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter && ContinueButton.IsEnabled)
            await TgSubmitCode(CodeBox.Text.Trim());
    }

    public static bool IsNotSupportedState(AuthHandler.AuthState state)
    {
        switch (state)
        {
            case AuthHandler.AuthState.WaitRegistration:
                App.GetInstance().RootFrame.Navigate(typeof(GenericError), "Registration is not supported in this application. Please register using the Telegram app.");
                return true;
            case AuthHandler.AuthState.WaitOtherDeviceConfirmation:
                App.GetInstance().RootFrame.Navigate(typeof(GenericError), "State WaitOtherDeviceConfirmation is not supported.");
                return true;
            default: return false;
        }
    }
}
