using Microsoft.Maui.Graphics;
using System.Timers;

namespace SkribeMaui.Controls
{
    public partial class TimerControl : ContentView
    {
        private System.Timers.Timer? _timer;
        private double _timeLeft = 120;
        private bool _isActive;
        private const double TotalDuration = 120.0;
        private const double TickInterval = 50;
        private const double DecrementPerTick = TotalDuration / (TotalDuration * 1000 / TickInterval);
        public double TimeTakenSeconds { get; set; }

        public TimerControl()
        {
            InitializeComponent();
            ProgressCircle.Drawable = new CircularProgressDrawable(1.0);
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                StatusLabel.Text = value ? "ready" : "remaining";

                if (value && _timer == null)
                {
                    StartTimer();
                }
                else if (!value)
                {
                    StopTimer();
                }
            }
        }

        public double TimeLeft
        {
            get => _timeLeft;
            set
            {
                _timeLeft = value;
                UpdateDisplay();
            }
        }

        public event EventHandler? TimerCompleted;

        public void TimeElapsed()
        {
            double elapsed;

            if (_timeLeft <= 0) elapsed = TotalDuration;
            else
            {
                elapsed = TotalDuration - _timeLeft;
                if (elapsed < 0) elapsed = 0;
                if (elapsed > TotalDuration) elapsed = TotalDuration;
            }

            TimeTakenSeconds = elapsed;
            StopTimer();
            TimerCompleted?.Invoke(this, EventArgs.Empty);
        }

        private void StartTimer()
        {
            _timer = new System.Timers.Timer(TickInterval);
            _timer.Elapsed += OnTimerTick;
            _timer.Start();
        }

        private void StopTimer()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;
            _timeLeft = 120;
            UpdateDisplay();
        }

        private void OnTimerTick(object? sender, ElapsedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (_timeLeft > 0)
                {
                    _timeLeft -= DecrementPerTick;
                    if (_timeLeft < 0) _timeLeft = 0;
                    UpdateDisplay();
                }
                else
                {
                    StopTimer();
                    TimerCompleted?.Invoke(this, EventArgs.Empty);
                }
            });
        }

        private void UpdateDisplay()
        {
            int minutes = (int)_timeLeft / 60;
            int seconds = (int)_timeLeft % 60;
            TimeLabel.Text = $"{minutes}:{seconds:D2}";

            double progress = _timeLeft / TotalDuration;
            ((CircularProgressDrawable)ProgressCircle.Drawable).Progress = progress;
            ProgressCircle.Invalidate();
        }

        private class CircularProgressDrawable : IDrawable
        {
            public double Progress { get; set; }

            public CircularProgressDrawable(double progress)
            {
                Progress = progress;
            }

            public void Draw(ICanvas canvas, RectF dirtyRect)
            {
                float centerX = dirtyRect.Width / 2;
                float centerY = dirtyRect.Height / 2;
                float radius = Math.Min(centerX, centerY) - 8;
                // float sweepAngle = (float)(Progress * 360);

                // Background Circle
                canvas.StrokeColor = Colors.LightGray;
                canvas.StrokeSize = 8;
                canvas.DrawCircle(centerX, centerY, radius);

                float startAngle = 90;
                float endAngle = 90 + (float)(Progress * 360);
                
                // Draw progress arc
                if (Progress > 0)
                {
                    canvas.StrokeColor = Color.FromArgb("#8B5CF6"); // Primary color
                    canvas.StrokeSize = 8;
                    canvas.StrokeLineCap = LineCap.Round;

                    canvas.DrawArc(centerX - radius, centerY - radius,
                        radius * 2, radius * 2,
                        startAngle, endAngle, false, closed: false);
                }
            }
        }
    }
}