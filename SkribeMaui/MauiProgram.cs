using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using SkribeMaui;

namespace Skribe
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans_Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans_Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans_Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("OpenSans_Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

#if ANDROID

            // Remove native underline/background for Editor on Android
            Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
                try
                {
                    if (handler?.PlatformView is Android.Widget.EditText editText)
                    {
                        // Remove native background (clears the underline)
                        editText.Background = null;
                    }
                }
                catch (System.Exception ex)
                {
                    Android.Util.Log.Error("SkribeMaui", $"EditorHandler mapping error: {ex}");
                    System.Diagnostics.Debug.WriteLine($"EditorHandler mapping error: {ex}");
                }
            });
#endif

            return builder.Build();
        }
    }
}
