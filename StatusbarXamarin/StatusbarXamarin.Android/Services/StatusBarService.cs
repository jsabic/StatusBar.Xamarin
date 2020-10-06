using Android.Content;
using StatusbarXamarin.Interfaces.Services;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace StatusbarXamarin.Droid.Services
{
    public class StatusBarService : IStatusBarService
    {
        private readonly MainActivity _mainActivity;

        public StatusBarService(MainActivity mainActivity)
        {
            _mainActivity = mainActivity;
        }

        public void SetStatusBarColor(Color color)
            => _mainActivity.Window.SetStatusBarColor(color.ToAndroid());

        public void RemoveStatusBarModalDialogColor()
        {
            // Not needed on Android
        }

        public void SetStatusBarModalDialogColor(Color color)
        {
            // Not needed on Android
        }
    }
}