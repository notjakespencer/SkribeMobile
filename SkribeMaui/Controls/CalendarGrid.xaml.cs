using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkribeMaui.Controls
{
    public partial class CalendarGrid : ContentView
    {

        Brush GetResourceBrush(string key, Color fallback)
        {
            if (Application.Current?.Resources?.TryGetValue(key, out var value) == true)
            {
                if (value is Brush b) return b;
                if (value is Color c) return new SolidColorBrush(c);
            } return new SolidColorBrush(fallback);
        }

        Color GetResourceColorfromBrushOrColor(string key, Color fallback)
        {
            if (Application.Current?.Resources?.TryGetValue(key, out var value) == true)
            {
                if (value is Color c) return c;
                if (value is SolidColorBrush sb) return sb.Color;
                if (value is Brush)
                {
                    try
                    {
                        var brush = value as SolidColorBrush;
                        if (brush != null) return brush.Color;
                    }
                    catch { }
                }
            } return fallback;
        }

        // Lightweight entry model used by this control.
        // You can map your domain model to this type when setting Entries.
        public class CalendarGridEntry
        {
            public DateTime Date { get; set; }
            public string? Mood { get; set; }
            public object? Payload { get; set; }
        }

        public static readonly BindableProperty CurrentMonthProperty = BindableProperty.Create(
            nameof(CurrentMonth),
            typeof(DateTime),
            typeof(CalendarGrid),
            DateTime.Now,
            propertyChanged: (b, _, _) => ScheduleBuild((CalendarGrid)b));

        public static readonly BindableProperty EntriesProperty = BindableProperty.Create(
            nameof(Entries),
            typeof(IList<CalendarGridEntry>),
            typeof(CalendarGrid),
            new List<CalendarGridEntry>(),
            propertyChanged: (b, _, _) => ScheduleBuild((CalendarGrid)b));

        /// <summary>
        /// Month to render. Only year/month are used.
        /// </summary>
        public DateTime CurrentMonth
        {
            get => (DateTime)GetValue(CurrentMonthProperty);
            set => SetValue(CurrentMonthProperty, value);
        }

        /// <summary>
        /// List of entries (date + mood). Dates should be local dates (time component ignored).
        /// </summary>
        public IList<CalendarGridEntry> Entries
        {
            get => (IList<CalendarGridEntry>)GetValue(EntriesProperty);
            set => SetValue(EntriesProperty, value);
        }

        /// <summary>
        /// Fired when a calendar date with an entry is tapped.
        /// Payload contains whatever the host set (e.g. JournalEntry).
        /// </summary>
        public event EventHandler<CalendarGridEntry?>? DateSelected;

        readonly Dictionary<string, Brush> _moodBrushes = new()
        {
            { "amazing", new SolidColorBrush(Color.FromRgb(16, 185, 129)) },   // #10B981
            { "good",    new SolidColorBrush(Color.FromRgb(132, 204, 22)) },   // #84CC16
            { "okay",    new SolidColorBrush(Color.FromRgb(245, 158, 11)) },   // #F59E0B
            { "tough",   new SolidColorBrush(Color.FromRgb(249, 115, 22)) },   // #F97316
            { "difficult", new SolidColorBrush(Color.FromRgb(239, 68, 68)) },  // #EF4444
        };

        List<Button> _dayButtons = new List<Button>(42); // enough for 6 rows x 7 days

        public CalendarGrid()
        {
            InitializeComponent();
            // Build weekday header immediately (safe, small work)
            try
            {
                BuildWeekdayHeader();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CalendarGrid weekday header error: {ex}");
            }

            // Schedule calendar build on the UI thread and guard against early creation
            ScheduleBuild(this);

            // Also rebuild when handler becomes available (control fully attached)
            this.HandlerChanged += (s, e) => ScheduleBuild(this);
            this.Loaded += (s, e) => ScheduleBuild(this);

            try
            {
                Application.Current!.RequestedThemeChanged += (s, e) => ScheduleBuild(this);
            }
            catch { }
        }

        static void ScheduleBuild(CalendarGrid grid)
        {
            if (grid == null) return;

            void action()
            {
                try
                {
                    grid.BuildCalendar();
                }
                catch (Exception ex)
                {
                    // Log safely so Android/JavaProxyThrowable doesn't hide the real exception
                    System.Diagnostics.Debug.WriteLine($"CalendarGrid build error: {ex}");
                }
            }

            if (MainThread.IsMainThread)
            {
                action();
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(action);
            }
        }

        void BuildWeekdayHeader()
        {
            WeekdayHeader.Children.Clear();
            var days = new[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };

            for (var i = 0; i < 7; i++)
            {
                var lbl = new Label
                {
                    Text = days[i],
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 12,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = GetResourceColor("MutedForegroundBrush", Colors.Gray)
                };
                WeekdayHeader.Add(lbl, i, 0);
            }
        }

        void BuildCalendar()
        {
            System.Diagnostics.Debug.WriteLine("CalendarGrid: BuildCalendar start");

            // Do not clear Children here - preserve pooled buttons so they stay attached to the visual tree.
            // Clearing Children would detach pooled views and subsequent builds might not re-add them.
            // Only clear row definitions; pooled buttons are added when created below.
            DaysGrid.RowDefinitions.Clear();

            var monthStart = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
            var daysInMonth = DateTime.DaysInMonth(CurrentMonth.Year, CurrentMonth.Month);
            var startDay = (int)monthStart.DayOfWeek; // 0 = Sunday

            var totalCells = startDay + daysInMonth;
            var rows = (int)Math.Ceiling(totalCells / 7.0);

            // ensure Rows
            DaysGrid.RowDefinitions.Clear();
            for (var r = 0; r < rows; r++)
                DaysGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));

            // ensure enough buttons in pool and add new ones to the grid
            while (_dayButtons.Count < totalCells)
            {
                var btn = new Button
                {
                    Style = null,

                    CornerRadius = 12,
                    Padding = new Thickness(0),
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 14,
                    HeightRequest = 48,
                    Background = GetResourceBrush("CalendarBrush", Color.FromRgb(243, 244, 246)),
                    TextColor = GetResourceColorfromBrushOrColor("TextBrush", Colors.Gray)
                };
                _dayButtons.Add(btn);
                DaysGrid.Add(btn, 0, 0); // initial position; we'll set row/col below
            }

            // If there are pooled buttons that weren't previously added (edge case), ensure they're attached
            if (DaysGrid.Children.Count == 0 && _dayButtons.Count > 0)
            {
                foreach (var btn in _dayButtons)
                {
                    if (btn.Parent == null)
                        DaysGrid.Add(btn, 0, 0);
                }
            }

            // update each cell
            for (int i = 0; i < totalCells; i++)
            {
                var btn = _dayButtons[i];
                var row = i / 7;
                var col = i % 7;
                Grid.SetRow(btn, row);
                Grid.SetColumn(btn, col);

                // Clear any previously assigned command to avoid stale handlers/state.
                btn.Command = null;
                btn.CommandParameter = null;

                if (i < startDay)
                {
                    btn.IsVisible = false; // placeholder cell
                    btn.IsEnabled = false;
                    btn.Text = string.Empty;
                    btn.Background = new SolidColorBrush(Colors.Transparent);
                }
                else
                {
                    var day = i - startDay + 1;
                    var date = new DateTime(CurrentMonth.Year, CurrentMonth.Month, day);
                    btn.IsVisible = true;
                    btn.Text = day.ToString();

                    var entry = Entries?.FirstOrDefault(e => e.Date.Date == date.Date);

                    var isToday = date.Date == DateTime.Now.Date;
                    var isFuture = date.Date > DateTime.Now.Date;

                    if (entry != null)
                    {
                        if (!string.IsNullOrWhiteSpace(entry.Mood) && _moodBrushes.TryGetValue(entry.Mood.ToLowerInvariant(), out var brush))
                        {
                            btn.Background = brush;
                        }
                        else
                        {
                            btn.Background = GetResourceBrush("CalendarBrush", Color.FromRgb(229, 231, 235));
                        }

                        btn.TextColor = Colors.White;
                        // make sure entries in the future (shouldn't happen) are not interactable
                        btn.IsEnabled = !isFuture;

                        // Raise DateSelected with the pooled entry; host (CalendarPage) will show details.
                        btn.Command = new Command(() => DateSelected?.Invoke(this, entry));
                        btn.CommandParameter = entry;
                    }
                    else
                    {
                        // day without entry
                        var calendarBrush = GetResourceBrush("CalendarBrush", Color.FromRgb(243, 244, 246));
                        btn.Background = calendarBrush;

                        btn.IsEnabled = false;
                        btn.Opacity = isFuture ? 0.5 : 1.0;
                    }

                    if (isToday)
                    {
                        // ring effect: slightly thicker border
                        btn.BorderWidth = 2;
                        btn.BorderColor = GetResourceColor("PrimaryColor", Colors.Purple);
                    }
                    else
                    {
                        btn.BorderWidth = 0;
                    }
                }
            }

            // hide any extra buttons if grid shrank
            for (int i = totalCells; i < _dayButtons.Count; i++)
                _dayButtons[i].IsVisible = false;

            System.Diagnostics.Debug.WriteLine("CalendarGrid: BuildCalendar end");
        }

        // Helper to allow updating entries from domain objects (optional)
        public void SetEntriesFromObjects<T>(IEnumerable<T> items, Func<T, DateTime> dateSelector, Func<T, string?> moodSelector, Func<T, object?> payloadSelector)
        {
            Entries = items.Select(i => new CalendarGridEntry
            {
                Date = dateSelector(i).Date,
                Mood = moodSelector(i),
                Payload = payloadSelector(i)
            }).ToList();
        }

        Color GetResourceColor(string key, Color fallback)
        {
            if (Application.Current?.Resources?.TryGetValue(key, out var value) == true && value is Color c)
                return c;
            return fallback;
        }
    }
}