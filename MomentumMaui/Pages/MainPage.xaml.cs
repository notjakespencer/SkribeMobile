using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using MomentumMaui.Controls;
using Momentum.Shared.Data;
using Momentum.Shared.Services;
using System;
using System.Threading.Tasks;
using Momentum.Shared.Models;
using System.IO;
using System.Text.Json;
using Microsoft.Maui.Storage;

namespace MomentumMaui
{
    public partial class MainPage : ContentPage
    {
        private readonly UserStateService _userStats = new();

        public MainPage()
        {
            InitializeComponent();

            // Subscribe to the timer completion event
            MyTimer?.TimerCompleted += OnTimerCompleted;

            // Start MyTimer when the user begins typing in the PromptResponse editor
            var promptEditor = this.FindByName<Editor>("PromptResponse");
            if (promptEditor != null)
            {
                promptEditor.TextChanged += PromptResponse_TextChanged;
            }
        }
        
        void UpdateThemeIcon()
        {
            bool isLight = Application.Current?.UserAppTheme == AppTheme.Light;

            var animated = this.FindByName<AnimatedThemeIcon>("ThemeButton");
            if (animated != null)
            {
                animated.LightSource = "moon.svg";
                animated.DarkSource = "sun.svg";
                animated.ThemeKey = isLight ? "light" : "dark";
                _ = animated.PlayAnimationAsync();
                return;
            }
            var imageButton = this.FindByName<ImageButton>("ThemeButton");
            if (imageButton != null)
            {
                imageButton.Source = isLight ? "moon.svg" : "sun.svg";
                return;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            var prompt = PromptRepository.GetRandomPrompt();
            PromptLabel.Text = prompt.Text;

            _userStats.LoadData();
            UpdateUI();
            UpdateThemeIcon();
        }

        private void UpdateUI()
        {
            StreakLabel.Text = _userStats.Streak.Current.ToString();
            LevelLabel.Text = $"Level {_userStats.Level}";
            var (xpInto, xpNeeded, progress) = _userStats.GetProgress();
            XpProgressBar.Progress = progress;
            XpLabel.Text = $"{xpInto} / {xpNeeded} XP";
            var newTheme = Application.Current?.UserAppTheme == AppTheme.Light ? "moon.svg" : "sun.svg";
            Application.Current?.UserAppTheme = _userStats.Theme == "light" ? AppTheme.Light : AppTheme.Dark;
        }

        // Event handlers for XAML Clicked must return void. Changed from Task to async void.
        private async void OnSubmitClicked(object sender, EventArgs e)
        {
            var response = PromptResponse?.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(response))
            {
                await DisplayAlertAsync("No Response", "Please write something before submitting.", "Ok");
                return;
            }

            try
            {
                var entry = new JournalEntry
                {
                    Date = DateTime.UtcNow.Date,
                    Text = response,
                    Mood = string.Empty
                };

                // Persist to a file in the app data directory (one entry per day)
                var fileName = Path.Combine(FileSystem.AppDataDirectory, $"journal_{DateTime.UtcNow:yyyyMMdd}.json");

                if (File.Exists(fileName))
                {
                    // Confirm overwrite
                    var overwrite = await DisplayAlertAsync("Entry exists",
                        "An entry for today already exists. Overwrite it?",
                        "Overwrite", "Cancel");
                    if (!overwrite) return;
                }

                var json = JsonSerializer.Serialize(entry, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(fileName, json);

                // Update user stats (XP, streak) and refresh UI
                _userStats.AddEntry();
                UpdateUI();

                // Stop timer and disable editing to indicate submission
                if (MyTimer != null) MyTimer.IsActive = false;
                if (PromptResponse != null)
                {
                    PromptResponse.IsEnabled = false;
                    PromptResponse.BackgroundColor = (Color)Resources["CardBrush"];
                }

                await DisplayAlertAsync("Saved", "Your entry was saved successfully.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"Failed to save entry: {ex.Message}", "OK");
            }
        }

        private void OnStopClicked(object sender, EventArgs e)
        {
            MyTimer.IsActive = false;
        }

        private void OnTimerCompleted(object? sender, EventArgs e)
        {
            DisplayAlertAsync("Timer Complete", "The 2-minute timer has finished!", "OK");
        }

        private async void OnThemeToggleClicked(object sender, EventArgs e)
        {
            var newTheme = Application.Current?.UserAppTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light;
            Application.Current!.UserAppTheme = newTheme;

            var animated = this.FindByName<AnimatedThemeIcon>("ThemeButton");
            if (animated != null)
            {
                animated.LightSource = "moon.svg";
                animated.DarkSource = "sun.svg";
                animated.ThemeKey = newTheme == AppTheme.Light ? "light" : "dark";
                await animated.PlayAnimationAsync();
                return;
            }

            var imageBtn = this.FindByName<ImageButton>("ThemeButton");
            if (imageBtn != null)
            {
                imageBtn.Source = newTheme == AppTheme.Light ? "moon.svg" : "sun.svg";
                return;
            }

            // Final fallback: call UpdateThemeIcon (safe no-op if nothing exists)
            UpdateThemeIcon();
        }

        void UpdateIconsForTheme()
        {
            bool isLight = Application.Current?.UserAppTheme == AppTheme.Light;
            StreakIcon.Source = isLight ? "flame.svg" : "flame_dark.svg";            
        }

        private void OnJournalClicked(object sender, EventArgs e)
        {
            // Start on the Journal Page -> Add a visual to the tab to show the page
        }

        // Signature changed to match XAML event handler requirements (return type must be void).
        // Also keep async so we can await navigation operations.
        private async void OnHistoryClicked(object sender, EventArgs e)
        {
            // Try to push navigation stack.  If this page isn't wrapped in a NavigationPage,
            // Fall back to updating the active Window.Page or opening a new Window instead
            // of setting Application.Current.MainPage (deprecated).

            var calendarPage = new CalendarPage();
            var navPage = new NavigationPage(calendarPage);

            if (this.Window != null)
            {
                this.Window.Page = navPage;
                return;
            }

            var windows = Application.Current?.Windows;
            if (windows != null && windows.Count > 0)
            {
                windows[0].Page = navPage;
                return;
            }

            if (Navigation?.NavigationStack != null)
            {
                await Navigation.PushAsync(calendarPage);
                return;
            }

            // Final fallback: create and open a new Window containing the navigation page
            var newWindow = new Window(navPage);
            Application.Current?.OpenWindow(newWindow);
        }

        // Start the existing MyTimer when the user begins typing (first non-empty change).
        private void PromptResponse_TextChanged(object? sender, TextChangedEventArgs e)
        {
            if (MyTimer == null) return;

            // If timer is already active, do nothing.
            if (MyTimer.IsActive) return;

            var oldText = e?.OldTextValue ?? string.Empty;
            var newText = e?.NewTextValue ?? string.Empty;

            // Start only on the transition from empty -> non-empty (first typed character)
            if (string.IsNullOrEmpty(oldText) && !string.IsNullOrEmpty(newText))
            {
                MyTimer.IsActive = true;
            }
        }
    }
}