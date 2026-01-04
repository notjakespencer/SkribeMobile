using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using SkribeMaui.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using System.Reflection;

// Alias to disambiguate the standard MAUI NavigationPage from the iOS-specific one
using MauiNavigation = Microsoft.Maui.Controls.NavigationPage;
// Alias to disambiguate the MAUI Application type from the platform-specific one
using MauiApplication = Microsoft.Maui.Controls.Application;
using Skribe.Shared.Services;
using Skribe.Shared.Models;
using System.Threading.Tasks;

namespace SkribeMaui
{
    public partial class CalendarPage : ContentPage
    {
        private readonly UserStateService _userStats = new();
        private DateTime _currentMonth = DateTime.Today;

        // Swipe gesture state
        private double _panX;
        private bool _isAnimating;
        private const double SwipeThreshold = 50;
        private const double MaxDragDistance = 80;
        private const uint AnimationDuration = 200;

        public CalendarPage()
        {
            InitializeComponent();
            Loaded += CalendarPage_Loaded;

            Shell.SetNavBarIsVisible(this, false);

            try
            {
                var pageType = typeof(Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page);
                var prop = pageType.GetField("SafeAreaEdgesProperty", BindingFlags.Public | BindingFlags.Static);
                if (prop != null)
                {
                    var bp = prop.GetValue(null) as BindableProperty;
                    if (bp != null)
                    {
                        SetValue(bp, SafeAreaEdges.None);
                    }
                }
                else if (DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    Padding = new Thickness(0);
                }
            }
            catch
            {
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    Padding = new Thickness(0);
                }
            }
        }

        private void RefreshCalendarForMonth(DateTime month)
        {
            _currentMonth = month;
            MyCalendar.CurrentMonth = month;
            LoadEntriesForMonth(MyCalendar, month);
            RefreshStats();
        }

        private void CalendarPage_Loaded(object? sender, EventArgs e)
        {
            Loaded -= CalendarPage_Loaded;

            RefreshCalendarForMonth(DateTime.Now);

            MyCalendar.DateSelected -= OnCalendarDateSelected;
            MyCalendar.DateSelected += OnCalendarDateSelected;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            RefreshStats();

            try
            {
                NotificationService.EntrySaved += OnEntrySaved;
            }
            catch
            {
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            try
            {
                NotificationService.EntrySaved -= OnEntrySaved;
            }
            catch
            {
            }
        }

        private void OnEntrySaved(object? sender, DateTime when)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    LoadEntriesForMonth(MyCalendar, MyCalendar.CurrentMonth);
                }
                catch
                {
                }

                RefreshStats();
            });
        }

        private void RefreshStats()
        {
            try
            {
                _userStats.LoadData();

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

                TotalEntriesLabel.Text = totalEntries.ToString();
                CurrentStreakLabel.Text = _userStats.Streak.Current.ToString();
                LongestStreakLabel.Text = _userStats.Streak.Longest.ToString();
                CurrentLevelLabel.Text = _userStats.Level.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RefreshStats error: {ex}");
            }
        }

        // Navigate back to the Journal page when the journal image/button is tapped.
        private async void OnJournalClicked(object sender, EventArgs e)
        {
            var journal = new MainPage();
            var navPage = new MauiNavigation(journal);

            if (Window != null)
            {
                Window.Page = navPage;
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

        private void LoadEntriesForMonth(Controls.CalendarGrid calendar, DateTime month)
        {
            try
            {
                var list = new List<Controls.CalendarGrid.CalendarGridEntry>();

                var start = new DateTime(month.Year, month.Month, 1);
                var end = start.AddMonths(1).AddDays(-1);

                for (var d = start; d <= end; d = d.AddDays(1))
                {
                    var fileName = Path.Combine(FileSystem.AppDataDirectory, $"journal_{d:yyyyMMdd}.json");
                    if (!File.Exists(fileName))
                    {
                        continue;
                    }

                    try
                    {
                        var json = File.ReadAllText(fileName);
                        var entry = System.Text.Json.JsonSerializer.Deserialize<JournalEntry>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (entry == null)
                        {
                            continue;
                        }

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
                            Payload = entry
                        });
                    }
                    catch
                    {
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

            if (entry.Payload is JournalEntry je)
            {
                await Navigation.PushModalAsync(new EntryDetailsPage(je));
                return;
            }

            var fallbackMsg = $"Date: {entry.Date:d}\nMood: {entry.Mood ?? "(unknown)"}\n\nNo further details available.";
            await DisplayAlertAsync("Entry Details", fallbackMsg, "OK");
        }

        #region Swipe Gesture Handling

        // Event handler must be async void, not async Task
        private async void OnCalendarPanUpdated(object? sender, PanUpdatedEventArgs e)
        {
            if (_isAnimating)
                return;

            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    _panX = 0;
                    break;

                case GestureStatus.Running:
                    // Clamp the drag distance for a "rubber band" feel
                    _panX = Math.Clamp(e.TotalX, -MaxDragDistance, MaxDragDistance);
                    
                    // Move calendar with finger (visual feedback)
                    MyCalendar.TranslationX = _panX;
                    
                    // Fade slightly as it moves
                    MyCalendar.Opacity = 1.0 - (Math.Abs(_panX) / MaxDragDistance * 0.3);
                    break;

                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                    await HandleSwipeCompleted();
                    break;
            }
        }

        private async Task HandleSwipeCompleted()
        {
            _isAnimating = true;

            try
            {
                if (_panX < -SwipeThreshold)
                {
                    // Swiped left -> next month
                    await AnimateMonthTransition(isNext: true);
                }
                else if (_panX > SwipeThreshold)
                {
                    // Swiped right -> previous month
                    await AnimateMonthTransition(isNext: false);
                }
                else
                {
                    // Didn't swipe far enough -> snap back
                    await SnapBack();
                }
            }
            finally
            {
                _panX = 0;
                _isAnimating = false;
            }
        }

        private async Task AnimateMonthTransition(bool isNext)
        {
            var slideOutDistance = isNext ? -Width : Width;
            var slideInDistance = isNext ? Width : -Width;

            // Slide out current view
            await Task.WhenAll(
                MyCalendar.TranslateTo(slideOutDistance * 0.5, 0, AnimationDuration / 2, Easing.CubicIn),
                MyCalendar.FadeTo(0, AnimationDuration / 2)
            );

            // Update to new month
            var newMonth = _currentMonth.AddMonths(isNext ? 1 : -1);
            RefreshCalendarForMonth(newMonth);

            // Position off-screen on opposite side
            MyCalendar.TranslationX = slideInDistance * 0.3;
            MyCalendar.Opacity = 0;

            // Slide in new view
            await Task.WhenAll(
                MyCalendar.TranslateTo(0, 0, AnimationDuration / 2, Easing.CubicOut),
                MyCalendar.FadeTo(1, AnimationDuration / 2)
            );
        }

        private async Task SnapBack()
        {
            await Task.WhenAll(
                MyCalendar.TranslateTo(0, 0, AnimationDuration, Easing.CubicOut),
                MyCalendar.FadeTo(1, AnimationDuration)
            );
        }

        #endregion
    }
}
