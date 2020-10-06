using Foundation;
using StatusbarXamarin.Interfaces.Services;
using System.Linq;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace StatusbarXamarin.iOS.Services
{
    public class StatusBarService : IStatusBarService
    {
        private int _statusBarFrameHash = 0;

        private Color _lastStatusBarColor = Color.Default;

        public void SetStatusBarColor(Color color)
        {
            _lastStatusBarColor = color;

            var uiColor = color.ToUIColor();

            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                CreateOrUpdateStatusBarBackgroundFrame(uiColor);
            }
            else if (UIApplication.SharedApplication.ValueForKey(new NSString("statusBar")) is UIView statusBar && statusBar.RespondsToSelector(new ObjCRuntime.Selector("setBackgroundColor:")))
            {
                statusBar.BackgroundColor = uiColor;
            }
        }

        public void SetStatusBarModalDialogColor(Color color)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                CreateOrUpdateStatusBarBackgroundFrame(AddOverlayColorOnStatusBar(color).ToUIColor());
            }
        }

        public void RemoveStatusBarModalDialogColor()
            => SetStatusBarColor(_lastStatusBarColor);

        private void CreateOrUpdateStatusBarBackgroundFrame(UIColor uiColor)
        {
            if (_statusBarFrameHash is 0)
            {
                var window = UIApplication.SharedApplication.Windows.OrderBy(x => x.WindowLevel).LastOrDefault();
                if (window is null)
                    return;

                var statusBarView = new UIView(UIApplication.SharedApplication.StatusBarFrame)
                {
                    BackgroundColor = uiColor
                };

                _statusBarFrameHash = statusBarView.GetHashCode();

                window.AddSubview(statusBarView);
            }
            else
            {
                var status = UIApplication.SharedApplication.Windows.SelectMany(x => x.Subviews).FirstOrDefault(x => x.GetHashCode() == _statusBarFrameHash);
                if (status != null)
                {
                    status.BackgroundColor = uiColor;
                }
            }
        }

        private Color AddOverlayColorOnStatusBar(Color overlay)
        {
            var a0 = overlay.A;
            var r0 = overlay.R;
            var g0 = overlay.G;
            var b0 = overlay.B;

            var a1 = _lastStatusBarColor.A;
            var r1 = _lastStatusBarColor.R;
            var g1 = _lastStatusBarColor.G;
            var b1 = _lastStatusBarColor.B;

            var a01 = (1 - a0) * a1 + a0;

            var r01 = ((1 - a0) * a1 * r1 + a0 * r0) / a01;

            var g01 = ((1 - a0) * a1 * g1 + a0 * g0) / a01;

            var b01 = ((1 - a0) * a1 * b1 + a0 * b0) / a01;

            return new Color(r01, g01, b01, a01);
        }
    }
}