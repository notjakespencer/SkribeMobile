using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using System;
using System.Collections.Generic;
using MomentumMaui.Controls;

// Alias to disambiguate the standard MAUI NavigationPage from the iOS-specific one
using MauiNavigation = Microsoft.Maui.Controls.NavigationPage;
// Alias to disambiguate the MAUI Application type from the platform-specific one
using MauiApplication = Microsoft.Maui.Controls.Application;

namespace MomentumMaui
{
    public partial class CalendarPage : ContentPage
    {
        public CalendarPage()
        {
            InitializeComponent();
            this.Loaded += CalendarPage_Loaded;

            Shell.SetNavBarIsVisible(this, false);
            this.On<iOS>().SetUseSafeArea(false);
            // PlatformConfiguration.iOSSpecific.Page.SetSafeAreaEdges(this, SafeAreaEdges.None);
        }

        private void CalendarPage_Loaded(object? sender, EventArgs e)
        {
            this.Loaded -= CalendarPage_Loaded;
            var myCalendar = this.FindByName<Controls.CalendarGrid>("MyCalendar");
            if (myCalendar == null) return;

            myCalendar.CurrentMonth = DateTime.Now;

            // Do not populate demo entries here. Leave Entries empty so days render neutral when no entry exists.
            myCalendar.Entries = new List<CalendarGrid.CalendarGridEntry>();
        }

        private async void OnOpenEntryClicked(object sender, EventArgs e)
        {
            // DatePicker was removed from the UI. Show a neutral message or open the selected entry when a date is chosen from the calendar.
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
