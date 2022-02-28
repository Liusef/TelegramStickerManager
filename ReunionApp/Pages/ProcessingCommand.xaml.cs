using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using ReunionApp.Runners;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReunionApp.Pages;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ProcessingCommand : Page
{
    CommandRunner runner;
    private Task refreshTimer;

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
        refreshTimer = Task.Run(async () => await Task.Delay(2500));
        Continue.IsEnabled = true;
    }

    private async void Outputs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        await Task.Run(async () => await Task.Delay(20));
        Scroll.ChangeView(null, Scroll.ScrollableHeight, null);
    }

    private async void Continue_Click(object sender, RoutedEventArgs e)
    {
        Load.IsIndeterminate = true;
        Continue.IsEnabled = false;

        await refreshTimer;
        await Task.Run(async () => await runner.PostTasksAsync());

        App.GetInstance().RootFrame.Navigate(typeof(Home), true, new DrillInNavigationTransitionInfo());
    }
}
