using Microsoft.Maui.Controls;
using System;

namespace MomentumMaui
{
    public partial class CalendarPage : ContentPage
    {
        public CalendarPage()
        {
            InitializeComponent();

            // Set initial date in code to avoid x:Static / assembly mapping issues in XAML
            HistoryDatePicker.Date = DateTime.Now;
        }

        private async void OnOpenEntryClicked(object sender, EventArgs e)
        {
            // Placeholder action — hook this into your journal storage to open the selected date's entry.
            await DisplayAlertAsync("History", $"Open entry for {HistoryDatePicker.Date:d}", "OK");
        }

        // Navigate back to the Journal page when the journal image/button is tapped.
        private async void OnJournalClicked(object sender, EventArgs e)
        {
            var journal = new MainPage();
            var navPage = new NavigationPage(journal);

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
                await Navigation.PushAsync(journal);
                return;
            }

            var newWindow = new Window(navPage);
            Application.Current?.OpenWindow(newWindow);
        }
    }
}
