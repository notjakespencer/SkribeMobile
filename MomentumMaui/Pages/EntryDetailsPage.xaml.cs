using Microsoft.Maui.Controls;
using System;
using Microsoft.Maui.Graphics;
using System.Linq;
using System.Globalization;
using System.IO;
using Microsoft.Maui.Storage;
using Skribe.Shared.Models;

namespace MomentumMaui
{
    public partial class EntryDetailsPage : ContentPage
    {
        public EntryDetailsPage(JournalEntry entry)
        {
            InitializeComponent();

            // Format: "Month Day, Year" e.g. "December 9, 2025"
            DateLabel.Text = entry.Date.ToString("MMMM d, yyyy", CultureInfo.CurrentCulture);

            MoodLabel.Text = entry.Mood switch
            {
                MoodType.VeryHappy => "🤩 Amazing",
                MoodType.Happy => "😊 Good",
                MoodType.Neutral => "😐 Okay",
                MoodType.Sad => "😔 Tough",
                MoodType.VerySad => "😢 Difficult",
                _ => "—"
            };

            // Apply mood color to the capsule background and adjust label contrast
            ApplyMoodBrush(entry.Mood);

            PromptLabel.Text = string.IsNullOrEmpty(entry.PromptText) ? "(no prompt)" : entry.PromptText;
            ResponseLabel.Text = string.IsNullOrEmpty(entry.EntryText) ? "(no response)" : entry.EntryText;

            // Compute and display elapsed time for this entry
            SetElapsedTime(entry);
        }

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        // Close when tapping outside the card (backdrop)
        private async void OnBackgroundTapped(object? sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        // Public helper (kept for programmatic updates)
        public void SetMood(string moodText, Color moodColor)
        {
            MoodLabel.Text = moodText;

            // Set capsule background to a solid color
            MoodBorder.Background = new SolidColorBrush(moodColor);

            // Simple contrast: compute luminance and pick black/white text
            var luminance = 0.2126 * moodColor.Red + 0.7152 * moodColor.Green + 0.0722 * moodColor.Blue;
            MoodLabel.TextColor = luminance > 0.6 ? Colors.Black : Colors.White;
        }

        // Apply brush from Resources or fallback to a gradient brush representative for the MoodType
        private void ApplyMoodBrush(MoodType mood)
        {
            // resource keys defined in Resources/Styles/Colors.xaml
            string key = mood switch
            {
                MoodType.VeryHappy => "MoodAmazingBrush",
                MoodType.Happy => "MoodGoodBrush",
                MoodType.Neutral => "MoodOkayBrush",
                MoodType.Sad => "MoodToughBrush",
                MoodType.VerySad => "MoodDifficultBrush",
                _ => null
            };

            Brush? resolvedBrush = null;

            if (!string.IsNullOrEmpty(key) && Application.Current != null)
            {
                // Try direct resources
                if (Application.Current.Resources.ContainsKey(key))
                {
                    resolvedBrush = Application.Current.Resources[key] as Brush;
                }

                // Search merged dictionaries as well
                if (resolvedBrush == null && Application.Current.Resources.MergedDictionaries != null)
                {
                    foreach (var rd in Application.Current.Resources.MergedDictionaries)
                    {
                        if (rd.ContainsKey(key))
                        {
                            resolvedBrush = rd[key] as Brush;
                            break;
                        }
                    }
                }
            }

            // Create gradient fallback matching the colors in Resources/Styles/Colors.xaml
            LinearGradientBrush fallbackGradient = mood switch
            {
                MoodType.VeryHappy => new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 1),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromArgb("#4ADE80"), 0.0f),
                        new GradientStop(Color.FromArgb("#10B981"), 1.0f),
                    }
                },
                MoodType.Happy => new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 1),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromArgb("#A3E635"), 0.0f),
                        new GradientStop(Color.FromArgb("#22C55E"), 1.0f),
                    }
                },
                MoodType.Neutral => new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 1),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromArgb("#FACC15"), 0.0f),
                        new GradientStop(Color.FromArgb("#FB923C"), 1.0f),
                    }
                },
                MoodType.Sad => new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 1),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromArgb("#FB923C"), 0.0f),
                        new GradientStop(Color.FromArgb("#F87171"), 1.0f),
                    }
                },
                MoodType.VerySad => new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 1),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromArgb("#F87171"), 0.0f),
                        new GradientStop(Color.FromArgb("#F43F5E"), 1.0f),
                    }
                },
                _ => new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 1),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Colors.Transparent, 0.0f),
                        new GradientStop(Colors.Transparent, 1.0f),
                    }
                }
            };

            // Apply the resolved resource brush if available, otherwise use the fallback gradient
            MoodBorder.Background = resolvedBrush ?? fallbackGradient;

            // Determine a representative color for contrast (use midpoint of the two gradient stops)
            Color repColor;
            var stops = fallbackGradient.GradientStops;
            if (stops != null && stops.Count >= 2)
            {
                var c1 = stops[0].Color;
                var c2 = stops[stops.Count - 1].Color;
                repColor = new Color(
                    (c1.Red + c2.Red) / 2f,
                    (c1.Green + c2.Green) / 2f,
                    (c1.Blue + c2.Blue) / 2f,
                    (c1.Alpha + c2.Alpha) / 2f
                );
            }
            else
            {
                // fallback representative
                repColor = Color.FromArgb("#22C55E");
            }

            // Contrast check using luminance of representative color
            var luminance = 0.2126 * repColor.Red + 0.7152 * repColor.Green + 0.0722 * repColor.Blue;
            MoodLabel.TextColor = luminance > 0.6 ? Colors.Black : Colors.White;
        }

        // Compute and set the ElapsedTime label text.
        // Strategy:
        // 1) Try to find the journal file for entry.Date and use its LastWriteTimeUtc as completion time.
        // 2) If file missing but entry.CreatedAt exists, approximate using now (UTC).
        // 3) Clamp to timer max (120s) and format as mm:ss or "Xs" for short durations.
        private void SetElapsedTime(JournalEntry entry)
        {
            try
            {
                TimeSpan elapsed;
                var fileName = Path.Combine(FileSystem.AppDataDirectory, $"journal_{entry.Date:yyyyMMdd}.json");

                if (File.Exists(fileName))
                {
                    var lastWriteUtc = File.GetLastWriteTimeUtc(fileName);

                    // Ensure CreatedAt is treated as UTC (JournalEntry.CreatedAt stored as UTC in save code)
                    var createdUtc = entry.CreatedAt;
                    if (createdUtc.Kind == DateTimeKind.Unspecified)
                        createdUtc = DateTime.SpecifyKind(createdUtc, DateTimeKind.Utc);

                    elapsed = lastWriteUtc - createdUtc;
                }
                else if (entry.CreatedAt != default)
                {
                    var createdUtc = entry.CreatedAt;
                    if (createdUtc.Kind == DateTimeKind.Unspecified)
                        createdUtc = DateTime.SpecifyKind(createdUtc, DateTimeKind.Utc);

                    elapsed = DateTime.UtcNow - createdUtc;
                }
                else
                {
                    ElapsedTime.Text = "(unknown)";
                    return;
                }

                if (elapsed.TotalSeconds <= 0) elapsed = TimeSpan.Zero;

                // If the user ran out of time, show the full duration (120s)
                var maxDuration = TimeSpan.FromSeconds(120);
                if (elapsed > maxDuration) elapsed = maxDuration;

                ElapsedTime.Text = FormatElapsed(elapsed);
            }
            catch
            {
                ElapsedTime.Text = "(unknown)";
            }
        }

        private static string FormatElapsed(TimeSpan ts)
        {
            if (ts.TotalMinutes >= 1)
            {
                return $"{(int)ts.TotalMinutes}:{ts.Seconds:D2}";
            }

            // show seconds for < 1 minute
            return $"{(int)ts.TotalSeconds}s";
        }
    }
}