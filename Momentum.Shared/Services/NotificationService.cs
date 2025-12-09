namespace MomentumMaui.Services
{
    public static class NotificationService
    {
        public static event EventHandler<DateTime>? EntrySaved;

        public static void NotifyEntrySaved(DateTime when)
            => EntrySaved?.Invoke(null, when);
    }
}