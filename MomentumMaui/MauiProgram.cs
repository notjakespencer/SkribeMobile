using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace MomentumMaui
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
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

#if ANDROID
            // Remove native underline/background for Editor on Android
            Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
                if (handler.PlatformView is Android.Widget.EditText editText)
                {
                    // Remove native background (clears the underline)
                    editText.Background = null;
                    // Ensure padding stays correct if you want:
                    // editText.SetPadding(0, editText.PaddingTop, 0, editText.PaddingBottom);
                }
            });
#endif

            return builder.Build();
        }
    }
}
