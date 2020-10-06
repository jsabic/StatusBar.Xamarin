using Prism.Navigation;
using Rg.Plugins.Popup.Contracts;
using StatusbarXamarin.Interfaces.Services;
using StatusbarXamarin.Views;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace StatusbarXamarin.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private readonly IStatusBarService _statusBarService;
        private readonly INavigationBarService _navigationBarService;
        private readonly IPopupNavigation _popupNavigation;

        private bool _fixStatusBar;

        public MainPageViewModel(
            INavigationService navigationService,
            IStatusBarService statusBarService,
            INavigationBarService navigationBarService,
            IPopupNavigation popupNavigation)
            : base(navigationService)
        {
            Title = "Main Page";
            _statusBarService = statusBarService;
            _navigationBarService = navigationBarService;
            _popupNavigation = popupNavigation;

            SetStatusBarColorCommand = new Command<Color>(x => ExecuteSetStatusBarColor(x));
            SetNavBarColorCommand = new Command<Color>(x => ExecuteSetNavBarColor(x));
            OpenPopupPageCommand = new Command(async () => await ExecuteOpenPopup());
            OpenNoNavigationBarPageCommand = new Command(async () => await ExecuteOpenNoNavigationBarPage());

            _popupNavigation.Popped += _popupNavigation_Popped;
        }
        public bool FixStatusBar
        {
            get => _fixStatusBar;
            set => SetProperty(ref _fixStatusBar, value);
        }

        public ICommand SetStatusBarColorCommand { get; }
        public ICommand SetNavBarColorCommand { get; }
        public ICommand OpenPopupPageCommand { get; }
        public ICommand OpenNoNavigationBarPageCommand { get; }

        private Task ExecuteOpenNoNavigationBarPage()
        {
            return NavigationService.NavigateAsync(nameof(NoNavigationBarPage));
        }

        private void _popupNavigation_Popped(object sender, Rg.Plugins.Popup.Events.PopupNavigationEventArgs e)
        {
            _statusBarService.RemoveStatusBarModalDialogColor();
        }

        private async Task ExecuteOpenPopup()
        {
            var page = new ExamplePopupPage();
            if (FixStatusBar)
                _statusBarService.SetStatusBarModalDialogColor(page.BackgroundColor);

            await _popupNavigation.PushAsync(page);
        }

        private void ExecuteSetStatusBarColor(Color color)
        {
            _statusBarService.SetStatusBarColor(color);
        }

        private void ExecuteSetNavBarColor(Color color)
        {
            _navigationBarService.SetNavigationBarColor(color);
        }

    }
}
