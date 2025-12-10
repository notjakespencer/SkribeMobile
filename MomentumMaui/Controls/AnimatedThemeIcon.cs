using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using static Microsoft.Maui.Easing;

namespace SKRIBE.Controls
{
    /// <summary>
    /// A small reusable ContentView that swaps an icon when the ThemeKey changes
    /// and runs a rotate+scale animation (initial: rotate=0, scale=0 -> rotate=360, scale=1).
    /// Use from XAML and bind ThemeKey to a string (e.g. "light" / "dark").
    /// </summary>
    public sealed class AnimatedThemeIcon : ContentView
    {
        readonly Image _image;
        string? _lastAnimatedKey;
        CancellationTokenSource? _cts;

        public AnimatedThemeIcon()
        {
            _image = new Image
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Aspect = Aspect.AspectFit,
            };

            Content = _image;

            // Ensure initial image source corresponds to provided ThemeKey (if any)
            this.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ThemeKey))
                {
                    _ = OnThemeKeyChangedAsync(ThemeKey);
                }

                if (e.PropertyName == nameof(LightSource) || e.PropertyName == nameof(DarkSource))
                {
                    // Refresh displayed source when underlying assets change
                    _image.Source = GetSourceForKey(ThemeKey);
                }
            };
        }

        // Theme key: any string that acts as the "key" for the animation (e.g. "light" / "dark").
        public static readonly BindableProperty ThemeKeyProperty =
            BindableProperty.Create(
                nameof(ThemeKey),
                typeof(string),
                typeof(AnimatedThemeIcon),
                default(string),
                propertyChanged: null);

        public string? ThemeKey
        {
            get => (string?)GetValue(ThemeKeyProperty);
            set => SetValue(ThemeKeyProperty, value);
        }

        // Image source to use when theme key indicates light
        public static readonly BindableProperty LightSourceProperty =
            BindableProperty.Create(
                nameof(LightSource),
                typeof(string),
                typeof(AnimatedThemeIcon),
                default(string));

        public string? LightSource
        {
            get => (string?)GetValue(LightSourceProperty);
            set => SetValue(LightSourceProperty, value);
        }

        // Image source to use when theme key indicates dark
        public static readonly BindableProperty DarkSourceProperty =
            BindableProperty.Create(
                nameof(DarkSource),
                typeof(string),
                typeof(AnimatedThemeIcon),
                default(string));

        public string? DarkSource
        {
            get => (string?)GetValue(DarkSourceProperty);
            set => SetValue(DarkSourceProperty, value);
        }

        // Duration in milliseconds
        public static readonly BindableProperty DurationProperty =
            BindableProperty.Create(
                nameof(Duration),
                typeof(int),
                typeof(AnimatedThemeIcon),
                300);

        public int Duration
        {
            get => (int)GetValue(DurationProperty);
            set => SetValue(DurationProperty, value);
        }

        // If true, animate when ThemeKey is first set; otherwise animation runs only on changes.
        public static readonly BindableProperty AnimateOnInitialProperty =
            BindableProperty.Create(
                nameof(AnimateOnInitial),
                typeof(bool),
                typeof(AnimatedThemeIcon),
                true);

        public bool AnimateOnInitial
        {
            get => (bool)GetValue(AnimateOnInitialProperty);
            set => SetValue(AnimateOnInitialProperty, value);
        }

        // Public API to force animation for current ThemeKey
        public Task PlayAnimationAsync() => OnThemeKeyChangedAsync(ThemeKey);

        async Task OnThemeKeyChangedAsync(string? key)
        {
            // If this is the first assignment and AnimateOnInitial is false, just update source and return
            if (_lastAnimatedKey is null && !AnimateOnInitial)
            {
                _image.Source = GetSourceForKey(key);
                _lastAnimatedKey = key;
                return;
            }

            if (string.Equals(_lastAnimatedKey, key, StringComparison.Ordinal))
            {
                // same key -> no animation
                return;
            }

            _lastAnimatedKey = key;

            // Cancel any running animation
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;

            // Choose source now so the incoming icon is revealed by the animation
            _image.Source = GetSourceForKey(key);

            // Prepare initial animation state (match initial={{ rotate:0, scale:0 }})
            try
            {
                _image.Rotation = 0;
                _image.Scale = 0;

                // Use the non-obsolete async animation APIs
                var rotateTask = _image.RotateToAsync(360, (uint)Duration, CubicOut);
                var scaleTask = _image.ScaleToAsync(1, (uint)Duration, CubicOut);

                var all = Task.WhenAll(rotateTask, scaleTask);

                // Cancel support: if cancelled, swallow exceptions and reset state
                using (ct.Register(() => { /* visual animations can't be force-cancelled, token used to short-circuit awaiting */ }))
                {
                    await all;
                }
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
            catch (Exception)
            {
                // ignore unexpected animation errors; ensure control remains usable
            }
            finally
            {
                // Reset rotation so future animations start from 0.
                _image.Rotation = 0;
                _cts?.Dispose();
                _cts = null;
            }
        }

        // Very small helper - maps keys to provided sources.
        // By default treats "light" as light; else uses dark source.
        ImageSource? GetSourceForKey(string? key)
        {
            var isLight = string.Equals(key, "light", StringComparison.OrdinalIgnoreCase);
            var source = isLight ? LightSource : DarkSource;

            if (string.IsNullOrEmpty(source))
                return _image.Source;

            return ImageSource.FromFile(source);
        }
    }
}
