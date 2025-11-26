using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using MomentumMaui.Controls;
using Momentum.Shared.Data;
using Momentum.Shared.Services;
using System;
using System.Threading.Tasks;

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

        private void OnStartClicked(object sender, EventArgs e)
        {
            MyTimer.IsActive = true;
        }

        private void OnStopClicked(object sender, EventArgs e)
        {
            MyTimer.IsActive = false;
        }

        private void OnTimerCompleted(object sender, EventArgs e)
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

            var historyPage = new HistoryPage();
            var navPage = new NavigationPage(historyPage);

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
                await Navigation.PushAsync(historyPage);
                return;
            }

            // Final fallback: create and open a new Window containing the navigation page
            var newWindow = new Window(navPage);
            Application.Current?.OpenWindow(newWindow);
        }
    }
}