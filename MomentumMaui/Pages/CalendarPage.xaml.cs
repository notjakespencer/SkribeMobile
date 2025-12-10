using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using MomentumMaui.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel;

// Alias to disambiguate the standard MAUI NavigationPage from the iOS-specific one
using MauiNavigation = Microsoft.Maui.Controls.NavigationPage;
// Alias to disambiguate the MAUI Application type from the platform-specific one
using MauiApplication = Microsoft.Maui.Controls.Application;
using Skribe.Shared.Services;
using Skribe.Shared.Models;

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

            // Load persisted journal files for the current month and map to calendar entries
            LoadEntriesForMonth(myCalendar, myCalendar.CurrentMonth);

            // Subscribe to calendar DateSelected so the page can show the full journal details popup
            myCalendar.DateSelected -= OnCalendarDateSelected;
            myCalendar.DateSelected += OnCalendarDateSelected;

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
            // Reload calendar entries for the currently displayed month
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    var myCalendar = this.FindByName<Controls.CalendarGrid>("MyCalendar");
                    if (myCalendar != null)
                        LoadEntriesForMonth(myCalendar, myCalendar.CurrentMonth);
                }
                catch { /* ignore errors in refresh path */ }

                // Also refresh stats
                RefreshStats();
            });
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

        // Load journal files for the provided month and populate the calendar control's Entries.
        void LoadEntriesForMonth(Controls.CalendarGrid calendar, DateTime month)
        {
            try
            {
                var list = new List<Controls.CalendarGrid.CalendarGridEntry>();

                var start = new DateTime(month.Year, month.Month, 1);
                var end = start.AddMonths(1).AddDays(-1);

                for (var d = start; d <= end; d = d.AddDays(1))
                {
                    var fileName = Path.Combine(FileSystem.AppDataDirectory, $"journal_{d:yyyyMMdd}.json");
                    if (!File.Exists(fileName)) continue;

                    try
                    {
                        var json = File.ReadAllText(fileName);
                        var entry = System.Text.Json.JsonSerializer.Deserialize<JournalEntry>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (entry == null) continue;

                        // Map the stored Mood enum to the string keys used by the calendar's brush map.
                        var moodKey = entry.Mood switch
                        {
                            MoodType.VeryHappy => "amazing",
                            MoodType.Happy => "good",
                            MoodType.Neutral => "okay",
                            MoodType.Sad => "tough",
                            MoodType.VerySad => "difficult",
                            _ => null
                        };

                        list.Add(new Controls.CalendarGrid.CalendarGridEntry
                        {
                            Date = entry.Date.Date,
                            Mood = moodKey,
                            Payload = entry // pass full JournalEntry so the page can show prompt/response/mood
                        });
                    }
                    catch
                    {
                        // ignore individual file parse errors to avoid breaking UI for others
                    }
                }

                calendar.Entries = list;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadEntriesForMonth error: {ex}");
            }
        }

        // Calendar date clicked -> show the journal details (prompt, response, mood)
        private async void OnCalendarDateSelected(object? sender, CalendarGrid.CalendarGridEntry? entry)
        {
            if (entry == null)
            {
                await DisplayAlertAsync("No Entry", "No journal entry exists for that date.", "OK");
                return;
            }

            // If payload contains a JournalEntry, show its details.
            if (entry.Payload is JournalEntry je)
            {
                await Navigation.PushModalAsync(new EntryDetailsPage(je));
                return;
            }

            // If payload is not a JournalEntry, fall back to showing whatever is present
            var fallbackMsg = $"Date: {entry.Date:d}\nMood: {entry.Mood ?? "(unknown)"}\n\nNo further details available.";
            await DisplayAlertAsync("Entry Details", fallbackMsg, "OK");
        }
    }
}
