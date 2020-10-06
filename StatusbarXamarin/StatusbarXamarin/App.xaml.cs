using Prism;
using Prism.Ioc;
using StatusbarXamarin.ViewModels;
using StatusbarXamarin.Views;
using Xamarin.Essentials.Interfaces;
using Xamarin.Essentials.Implementation;
using Xamarin.Forms;
using System.Net.Http.Headers;
using StatusbarXamarin.Interfaces.Services;
using StatusbarXamarin.Services;
using Prism.Plugin.Popups;

namespace StatusbarXamarin
{
    public partial class App
    {
        public App(IPlatformInitializer initializer)
            : base(initializer)
        {
        }

        protected override async void OnInitialized()
        {
            InitializeComponent();

            await NavigationService.NavigateAsync("NavigationPage/MainPage");
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterPopupNavigationService();

            containerRegistry.RegisterSingleton<IAppInfo, AppInfoImplementation>();
            containerRegistry.RegisterSingleton<INavigationBarService, NavigationBarService>();

            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<MainPage, MainPageViewModel>();
            containerRegistry.RegisterForNavigation<NoNavigationBarPage, NoNavigationBarViewModel>();
        }
    }
}
