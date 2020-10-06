using Prism.Navigation;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace StatusbarXamarin.ViewModels
{
    public class NoNavigationBarViewModel : ViewModelBase
    {
        public NoNavigationBarViewModel(INavigationService navigationService) : base(navigationService)
        {
            GoBackCommand = new Command(async () => await ExecuteGoBack());
        }

        public ICommand GoBackCommand { get; }

        private async Task ExecuteGoBack()
        {
            await NavigationService.GoBackAsync();
        }
    }
}
