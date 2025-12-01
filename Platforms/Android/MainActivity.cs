using Android.App;
using Android.OS;
using Android.Views;

namespace MomentumMaui
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = Android.Content.PM.ConfigChanges.ScreenSize | Android.Content.PM.ConfigChanges.Orientation)]
    public class MainActivity : Microsoft.Maui.ApplicationModel.Platform.MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // allow layout under the status bar
            Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(SystemUiFlags.LayoutStable | SystemUiFlags.LayoutFullscreen);
            // make the status bar transparent so content shows through
            Window.SetStatusBarColor(Android.Graphics.Color.Transparent);
        }
    }
}