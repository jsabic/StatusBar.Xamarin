# StatusBar.Xamarin

### Introduction

Setting StatusBar and NavigationBar color is common requirement of mobile application design.
NavigationBar is shared Xamarin component for iOS and Android so its color can be changed easily from shared project. On the other hand, StatusBar must be handled natively on both platforms.

### NavigationBar color

To set NavigationBar color we are going to use Xamarin xaml styles and dynamic resource binding. Since NavigationBar background color is property of navigation page all we need to do is define style for navigation page and dinamicaly bind its BarBackgroundColor to NavigationBarColor resource.
Using DynamicResource binding every time NavigationBarColor is changed, all resources bound to its value will be updated.

```csharp
<Application.Resources>

    <Color x:Key="NavigationBarColor">#2196F3</Color>

    <Style TargetType="NavigationPage">
        <Setter Property="BarBackgroundColor" Value="{DynamicResource Key=NavigationBarColor}" />
    </Style>

</Application.Resources>
```

Navigation bar color can be set by changing value of NavigationBarColor property in ResourceDictionary. So, we can define INavigationBarService that will do that for us.

```csharp
public interface INavigationBarService
{
    void SetNavigationBarColor(Color color);
}

public class NavigationBarService : INavigationBarService
{
    public const string NavigationBarColorKey = "NavigationBarColor";

    public void SetNavigationBarColor(Color color)
    {
        App.Current.Resources[NavigationBarColorKey] = color;
    }
}
```

### StatusBar color Android

Setting status bar color in Android is relatively simple since Android has native functionality to change StatusBar color. We only need to create StatusBarService in Android and register it to IStatusBarService in dependency injection container.

```csharp
private readonly MainActivity _mainActivity;

public StatusBarService(MainActivity mainActivity)
{
    _mainActivity = mainActivity;
}

public void SetStatusBarColor(Color color)
    => _mainActivity.Window.SetStatusBarColor(color.ToAndroid());
```

Bacuse SetStatusBarColor method can be only accessed via MainActivity when registering IStatusBarService we need to pass an instance of StatusBarService that has reference to MainActivitiy.
Since in android dependency injection is done in AndroidInitalizer we need a way to pass MainActivity reference to initializer, in this example it is done via constructor.

```csharp
public class AndroidInitializer : IPlatformInitializer
{
    private readonly MainActivity _mainActivity;

    public AndroidInitializer(MainActivity mainActivity)
    {
        _mainActivity = mainActivity;
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterInstance<IStatusBarService>(new StatusBarService(_mainActivity));
    }
}
```

Lastly, we need to update OnCreate method to use our new AndoridInitalizer with MainActivity reference

```csharp
protected override void OnCreate(Bundle savedInstanceState)
{
    TabLayoutResource = Resource.Layout.Tabbar;
    ToolbarResource = Resource.Layout.Toolbar;

    base.OnCreate(savedInstanceState);

    Xamarin.Essentials.Platform.Init(this, savedInstanceState);
    global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
    LoadApplication(new App(new AndroidInitializer(this)));
}
```

### StatusBar color iOS

In iOS 13.0 and greater StatusBar color is set to color of NavigationBar color if there is a navigation page present. If not StatusBar color is set to system light or dark depending on current theme.
To get the same functionality as in Android we need to create a new iOS frame and put it in StatusBar location, then we can change its background color, hence we can have different color for StatusBar and NavigationBar. Also, we can have StatusBar color if there is no Navigation page present.
On lesser iOS versions StatusBar is set by simply setting background color of StatusBar UIView.
Just like in Android we need to create StatusBarService that will be registered to IStatusBarService in iOS dependency injection.

```csharp
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
}
```

Registering status bar service

```csharp
public class iOSInitializer : IPlatformInitializer
{
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<IStatusBarService, StatusBarService>();
    }
}
```

### PopupPage iOS

With current implementation using INavigationBarServie and IStatusBarService we can independently change background color of NavigationBar and StatusBar on iOS and Android.
But on iOS there is a problem with PopupPages. In this example we are using [Rg.Plugins.Popup](https://github.com/rotorgames/Rg.Plugins.Popup). When navigating on PopupPage its background color tints the page behind it.
On Android when on PopupPage, StatusBar and NavigationBar color is tinted by PopupPage.
On iOS if on version 13.0 and up, we are creating a frame that is above PopupPage therefor it will not be tinted by PopupPage backgroun color. To fix this we need to use current StatusBar color and PopupPage background color and calculate new color by overlaying latter on former.
When new StatusBar color is calculated we set it to StatusBar color and save previous status bar color so that we can set it back to original color after navigating from PopupPage.
Now we can update StatusBarService with two new methods. SetStatusBarModalDialogColor that has argument corresponding to PopupPage backgound color and RemoveStatusBarModalDialogColor that will set background color to original. Also, there is a private method AddOverlayColorOnStatusBar for calculating overlayed color.

```csharp
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
```

### Final IStatusBarService

After fixing PopupPage behavior on iOS we need to update Andoird implementation as well since it implements same IStatusBarService as iOS service.

Final IStatusBarService:

```csharp
public interface IStatusBarService
{
    void RemoveStatusBarModalDialogColor();
    void SetStatusBarColor(Color color);
    void SetStatusBarModalDialogColor(Color color);
}
```

Final android implementation:

```csharp
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
```

### Conclusion

With this implementation we can have same StatusBar and NavigationBar functionality on both Android and iOS. For further information you can check out example project that fully demonstrates how to setup and implement given functionalities.
