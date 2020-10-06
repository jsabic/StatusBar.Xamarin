using Xamarin.Forms;

namespace StatusbarXamarin.Interfaces.Services
{
    public interface IStatusBarService
    {
        void RemoveStatusBarModalDialogColor();
        void SetStatusBarColor(Color color);
        void SetStatusBarModalDialogColor(Color color);
    }
}
