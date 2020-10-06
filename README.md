# StatusBar.Xamarin

### Introduction

Setting statusbar and navigation bar color is common requrement of mobile application design.
Navigation bar is shared xamarin component for iOS and Android so its color can be changed easily from shared project. On the other hand status bar must be handled nativly on both platforms.

### Navigation bar color

To set navigation bar color we are going to use xamarin xaml styles and dynamc resource binding. Since navigation bar color is property of navigation page, so we need to define style for navigation page and dinamicaly bind its BarBackgroundColor to NavigationBarColor resource. Using DynamicResource binding every time NavigationBarColor is changed, all resources bindied to its value will be updated.

```csharp
<Application.Resources>

    <Color x:Key="NavigationBarColor">#2196F3</Color>

    <Style TargetType="NavigationPage">
        <Setter Property="BarBackgroundColor" Value="{DynamicResource Key=NavigationBarColor}" />
    </Style>

</Application.Resources>
```

Navigation bar color can be set by changing value of NavigationBarColor property in ResourceDictionary. So we can define INavigationBarService that will do that for us.

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

Setting status bar color on android is relativily simple since android has native functionality to change statusbar color. We only need to create StatusBarService in android and register it to IStatusBarService in dependency injection container.

```csharp
private readonly MainActivity _mainActivity;

public StatusBarService(MainActivity mainActivity)
{
    _mainActivity = mainActivity;
}

public void SetStatusBarColor(Color color)
    => _mainActivity.Window.SetStatusBarColor(color.ToAndroid());

```

Bacuse SetStatusBarColor method can be only accessed via MainActivity when registering IStatusBarService we need to pass an instance of StatusBarService that has reference to MainActivitiy. Since in android dependency injection is done in AndroidInitalizer we need have a way to pass mainactivity reference to initalizer, in this exapmle it is done via constructor.

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

Lastly we need to change update OnCreate method to use our new AndoridInitalizer with MainActivity reference

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

### PopupPage iOS

###
