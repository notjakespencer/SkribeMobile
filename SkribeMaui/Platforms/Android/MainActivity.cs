using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace SkribeMaui.Platforms.Android
{
    [Activity(
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleTop,
        ConfigurationChanges =
            ConfigChanges.ScreenSize |
            ConfigChanges.Orientation |
            ConfigChanges.UiMode |
            ConfigChanges.ScreenLayout |
            ConfigChanges.SmallestScreenSize |
            ConfigChanges.Density)]


    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (OperatingSystem.IsAndroidVersionAtLeast(30) &&
                !OperatingSystem.IsAndroidVersionAtLeast(35))
            {
                Window?.SetDecorFitsSystemWindows(false);
            }

            if (OperatingSystem.IsAndroidVersionAtLeast(30))
            {
                var controller = Window?.InsetsController;
                if (controller != null)
                {
                    controller.Hide(WindowInsets.Type.StatusBars());
                    controller.SystemBarsBehavior =
                        (int)WindowInsetsControllerBehavior.ShowTransientBarsBySwipe;
                }
            }
            else
            {
                var decor = Window?.DecorView;
                if (decor != null)
                {
                    decor.SystemUiFlags = SystemUiFlags.LayoutStable |
                                          SystemUiFlags.LayoutFullscreen |
                                          SystemUiFlags.Fullscreen;
                }
            }
        }
    }
}
