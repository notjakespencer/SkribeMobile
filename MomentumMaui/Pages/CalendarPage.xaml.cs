using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using MomentumMaui.Services;
using MomentumMaui.Controls;
using Momentum.Shared.Services;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel;

// Alias to disambiguate the standard MAUI NavigationPage from the iOS-specific one
using MauiNavigation = Microsoft.Maui.Controls.NavigationPage;
// Alias to disambiguate the MAUI Application type from the platform-specific one
using MauiApplication = Microsoft.Maui.Controls.Application;

namespace MomentumMaui
{
    public partial class CalendarPage : ContentPage
    {
        private readonly UserStateService _userStats = new();

        public CalendarPage()
        {
            InitializeComponent();
            this.Loaded += CalendarPage_Loaded;

            Shell.SetNavBarIsVisible(this, false);
            this.On<iOS>().SetUseSafeArea(false);
        }

        private void CalendarPage_Loaded(object? sender, EventArgs e)
        {
            this.Loaded -= CalendarPage_Loaded;
            var myCalendar = this.FindByName<Controls.CalendarGrid>("MyCalendar");
            if (myCalendar == null) return;

            myCalendar.CurrentMonth = DateTime.Now;

            // Leave Entries empty so days render neutral when no entry exists.
            myCalendar.Entries = new List<CalendarGrid.CalendarGridEntry>();

            // Refresh UI counters once calendar is initialized
            RefreshStats();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Always refresh when page becomes visible so counters reflect latest entries
            RefreshStats();

            // Subscribe to entry-saved notifications for real-time updates
            try
            {
                NotificationService.EntrySaved += OnEntrySaved;
            }
            catch { /* ignore registration errors */ }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Unsubscribe to avoid leaks and duplicate handlers
            try
            {
                NotificationService.EntrySaved -= OnEntrySaved;
            }
            catch { }
        }

        private void OnEntrySaved(object? sender, DateTime when)
        {
            // Ensure UI updates run on the main thread
            MainThread.BeginInvokeOnMainThread(RefreshStats);
        }

        private void RefreshStats()
        {
            try
            {
                // Load persisted user state (streaks, xp, level)
                _userStats.LoadData();

                // Count journal files in the app data directory
                var appDir = FileSystem.AppDataDirectory;
                int totalEntries = 0;
                try
                {
                    if (Directory.Exists(appDir))
                    {
                        totalEntries = Directory.GetFiles(appDir, "journal_*.json").Length;
                    }
                }
                catch
                {
                    totalEntries = 0;
                }

                // Update labels (safe FindByName in case XAML names change)
                var totalLabel = this.FindByName<Label>("TotalEntriesLabel");
                var currentStreakLabel = this.FindByName<Label>("CurrentStreakLabel");
                var longestStreakLabel = this.FindByName<Label>("LongestStreakLabel");
                var levelLabel = this.FindByName<Label>("CurrentLevelLabel");

                if (totalLabel != null) totalLabel.Text = totalEntries.ToString();
                if (currentStreakLabel != null) currentStreakLabel.Text = _userStats.Streak.Current.ToString();
                if (longestStreakLabel != null) longestStreakLabel.Text = _userStats.Streak.Longest.ToString();
                if (levelLabel != null) levelLabel.Text = _userStats.Level.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RefreshStats error: {ex}");
            }
        }

        private async void OnOpenEntryClicked(object sender, EventArgs e)
        {
            await DisplayAlertAsync("History", "No date picker available. Select a date from the calendar to open an entry.", "OK");
        }

        // Navigate back to the Journal page when the journal image/button is tapped.
        private async void OnJournalClicked(object sender, EventArgs e)
        {
            var journal = new MainPage();
            var navPage = new MauiNavigation(journal);

            if (this.Window != null)
            {
                this.Window.Page = navPage;
                return;
            }

            var windows = MauiApplication.Current?.Windows;
            if (windows != null && windows.Count > 0)
            {
                windows[0].Page = navPage;
                return;
            }

            if (Navigation?.NavigationStack != null)
            {
                await Navigation.PushAsync(journal);
                return;
            }

            var newWindow = new Window(navPage);
            MauiApplication.Current?.OpenWindow(newWindow);
        }
    }
}
