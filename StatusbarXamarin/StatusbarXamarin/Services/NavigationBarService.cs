using StatusbarXamarin.Interfaces.Services;
using System.Drawing;

namespace StatusbarXamarin.Services
{
    public class NavigationBarService : INavigationBarService
    {
        public const string NavigationBarColorKey = "NavigationBarColor";

        public void SetNavigationBarColor(Color color)
        {
            App.Current.Resources[NavigationBarColorKey] = color;
        }
    }
}
