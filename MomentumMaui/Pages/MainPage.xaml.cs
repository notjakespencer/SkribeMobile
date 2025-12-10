using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Momentum.AIAgent.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Graphics;
using Skribe.Shared.Services;
using Skribe.Shared.Models;
using Skribe.Shared.Data;
using SKRIBE.Controls;

namespace MomentumMaui
{
    public partial class MainPage : ContentPage
    {
        private readonly UserStateService _userStats = new();

        // Preference keys
        private const string KEY_PROMPT_TEXT = "CurrentPromptText";
        private const string KEY_PROMPT_DATE = "CurrentPromptDate"; // yyyy-MM-dd
        private const string KEY_COMPLETED_DATE = "PromptCompletedDate"; // yyyy-MM-dd

        // Watches for local midnight so the prompt & completion state reset for the user
        private CancellationTokenSource? _midnightCts;

        public MainPage()
        {
            InitializeComponent();

            // Subscribe to the timer completion event
            MyTimer?.TimerCompleted += OnTimerCompleted;

            // Attach editor TextChanged directly to avoid FindByName misses
            if (PromptResponse != null)
            {
                PromptResponse.TextChanged += PromptResponse_TextChanged;
            }
            else
            {
                // defensive fallback if XAML name changed
                var promptEditor = this.FindByName<Editor>("PromptResponse");
                if (promptEditor != null)
                    promptEditor.TextChanged += PromptResponse_TextChanged;
            }
        }

        protected override void OnAppearing()
        {
            System.Diagnostics.Debug.WriteLine(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT"));
            base.OnAppearing();

            EnsurePromptForToday();
            ApplyCompletionStateToUi();

            _userStats.LoadData();
            UpdateUI();
            UpdateThemeIcon();

            // If user already completed today, show the success card and hide main content.
            if (HasCompletedToday())
            {
                try
                {
                    var fileName = Path.Combine(FileSystem.AppDataDirectory, $"journal_{DateTime.Now:yyyyMMdd}.json");
                    var completedAt = File.Exists(fileName) ? File.GetLastWriteTime(fileName) : DateTime.Now;
                    ShowSuccessOverlay(completedAt);
                }
                catch
                {
                    // ignore filesystem errors here; still show overlay using now
                    ShowSuccessOverlay(DateTime.Now);
                }
            }
            else
            {
                // Ensure main content visible if not completed (in case it was hidden earlier)
                if (this.FindByName<VisualElement>("MainContent") is VisualElement main) main.IsVisible = true;
            }

            // Start watching for local midnight to reset prompt/completion state automatically
            StartMidnightWatcher();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Stop the midnight watcher when page not visible to avoid background work
            StopMidnightWatcher();
        }

        private void StartMidnightWatcher()
        {
            // Cancel any previous watcher
            StopMidnightWatcher();

            _midnightCts = new CancellationTokenSource();
            var ct = _midnightCts.Token;

            // Run fire-and-forget watcher that triggers at local midnight
            _ = Task.Run(async () =>
            {
                try
                {
                    while (!ct.IsCancellationRequested)
                    {
                        var now = DateTime.Now;
                        var nextMidnight = now.Date.AddDays(1);
                        var delay = nextMidnight - now;

                        // If delay is negative or zero for any reason, set to a small positive value
                        if (delay <= TimeSpan.Zero) delay = TimeSpan.FromSeconds(5);

                        await Task.Delay(delay, ct).ConfigureAwait(false);

                        if (ct.IsCancellationRequested) break;

                        // Execute UI updates on main thread
                        Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
                        {
                            // Reset prompt for the new day and clear completion state
                            EnsurePromptForToday();
                            ApplyCompletionStateToUi();
                        });
                    }
                }
                catch (TaskCanceledException) { /* expected on cancel */ }
                catch (Exception ex)
                {
                    // Swallow unexpected exceptions but log for diagnostics
                    System.Diagnostics.Debug.WriteLine($"Midnight watcher error: {ex}");
                }
            }, ct);
        }

        private void StopMidnightWatcher()
        {
            if (_midnightCts != null)
            {
                try { _midnightCts.Cancel(); } catch { }
                _midnightCts.Dispose();
                _midnightCts = null;
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

        private void EnsurePromptForToday()
        {
            // Use LOCAL date to match user expectation and device local midnight
            var today = DateTime.Now.Date;
            var todayKey = today.ToString("yyyy-MM-dd");

            var storedDate = Preferences.Get(KEY_PROMPT_DATE, string.Empty);
            if (storedDate == todayKey)
            {
                // Use stored prompt
                var storedText = Preferences.Get(KEY_PROMPT_TEXT, string.Empty);
                if (!string.IsNullOrEmpty(storedText))
                {
                    PromptLabel.Text = storedText;
                    return;
                }
            }

            // New day or no stored prompt: assign a random prompt and persist it
            var prompt = PromptRepository.GetRandomPrompt();
            PromptLabel.Text = prompt.Text;
            SaveCurrentPrompt(prompt.Text, todayKey);

            // New day: clear completion so user can complete today's prompt
            Preferences.Remove(KEY_COMPLETED_DATE);

            // Ensure the success overlay is hidden and main content visible when a new day starts
            var success = this.FindByName<VisualElement>("SuccessOverlay");
            if (success != null) success.IsVisible = false;
            var main = this.FindByName<VisualElement>("MainContent");
            if (main != null) main.IsVisible = true;
        }

        private void SaveCurrentPrompt(string text, string yyyyMMddDate)
        {
            Preferences.Set(KEY_PROMPT_TEXT, text ?? string.Empty);
            Preferences.Set(KEY_PROMPT_DATE, yyyyMMddDate ?? string.Empty);
        }

        private bool HasCompletedToday()
        {
            // Use LOCAL date for completion check
            var todayKey = DateTime.Now.Date.ToString("yyyy-MM-dd");
            var completedDate = Preferences.Get(KEY_COMPLETED_DATE, string.Empty);
            return completedDate == todayKey;
        }

        private void MarkCompletedToday()
        {
            var todayKey = DateTime.Now.Date.ToString("yyyy-MM-dd");
            Preferences.Set(KEY_COMPLETED_DATE, todayKey);
        }

        private void ApplyCompletionStateToUi()
        {
            // Disable editor and complete button if already completed today
            var completed = HasCompletedToday();

            var editor = this.FindByName<Editor>("PromptResponse");
            if (editor != null)
            {
                editor.IsEnabled = !completed;
                if (completed)
                {
                    // safe resource lookup for CardBrush (supports Brush or Color)
                    var card = GetResourceObject("CardBrush");
                    if (card is Brush b) editor.Background = b;
                    else if (card is Color c) editor.BackgroundColor = c;
                }
            }

            var completeBtn = this.FindByName<Button>("CompleteButton");
            if (completeBtn != null)
            {
                completeBtn.IsEnabled = !completed;
            }
        }

        private object? GetResourceObject(string key)
        {
            // Look up in Application resources then page resources
            if (Application.Current?.Resources?.ContainsKey(key) == true)
                return Application.Current.Resources[key];

            if (this.Resources?.ContainsKey(key) == true)
                return this.Resources[key];

            return null;
        }

        private void UpdateUI()
        {
            StreakLabel.Text = _userStats.Streak.Current.ToString();
            LevelLabel.Text = $"Level {_userStats.Level}";
            var (xpInto, xpNeeded, progress) = _userStats.GetProgress();
            XpProgressBar.Progress = progress;
            XpLabel.Text = $"{xpInto} / {xpNeeded} XP";
            // theme handled globally
        }

        // Event handlers for XAML Clicked must return void. Changed from Task to async void.
        private async void OnSubmitClicked(object sender, EventArgs e)
        {
            // Use shared save logic that requires non-empty text for manual submit
            if (await SaveEntryAsync(requireNonEmpty: true))
            {
                // Show the mood selection card after successful manual submit
                ShowMoodPopup();
            }
        }

        private void ShowMoodPopup()
        {
            var overlay = this.FindByName<VisualElement>("MoodOverlay");
            if (overlay != null)
                overlay.IsVisible = true;
        }

        private void HideMoodPopup()
        {
            var overlay = this.FindByName<VisualElement>("MoodOverlay");
            if (overlay != null)
                overlay.IsVisible = false;
        }

        private async void OnMoodSelected(object? sender, EventArgs e)
        {
            if (sender is not Button btn) return;

            var moodText = btn.Text?.Trim() ?? string.Empty;

            // Map button text to MoodType enum
            var mood = moodText switch
            {
                "🤩 Amazing" or "Amazing" => MoodType.VeryHappy,
                "😊 Good" or "Good" => MoodType.Happy,
                "😐 Okay" or "Okay" => MoodType.Neutral,
                "😔 Tough" or "Tough" => MoodType.Sad,
                "😢 Difficult" or "Difficult" => MoodType.VerySad,
                _ => MoodType.Neutral
            };

            // Update today's journal entry file with the selected mood (if present)
            var fileName = Path.Combine(FileSystem.AppDataDirectory, $"journal_{DateTime.Now:yyyyMMdd}.json");

            try
            {
                if (File.Exists(fileName))
                {
                    var json = await File.ReadAllTextAsync(fileName);
                    var entry = JsonSerializer.Deserialize<JournalEntry>(json);
                    if (entry != null)
                    {
                        entry.Mood = mood;
                        var updated = JsonSerializer.Serialize(entry, new JsonSerializerOptions { WriteIndented = true });
                        await File.WriteAllTextAsync(fileName, updated);
                    }
                }

                HideMoodPopup();
                // Show success overlay; it will read the file and display mood/time
                ShowSuccessOverlay(DateTime.Now);
            }
            catch (Exception ex)
            {
                HideMoodPopup();
                await DisplayAlertAsync("Error", $"Failed to save mood: {ex.Message}", "OK");
            }
        }

        private void OnStopClicked(object sender, EventArgs e)
        {
            MyTimer.IsActive = false;
        }

        // Timer completion should auto-save the entry even if empty
        private async void OnTimerCompleted(object? sender, EventArgs e)
        {
            // Attempt to save without requiring non-empty content
            var saved = await SaveEntryAsync(requireNonEmpty: false);

            // Always inform the user that timer finished and whether we saved
            if (saved)
            {
                await DisplayAlertAsync("Timer Complete", "Time is up — your entry was saved.", "OK");
            }
            else
            {
                // If SaveEntryAsync returned false it either found an existing completion
                // or experienced an error message already shown inside SaveEntryAsync.
                await DisplayAlertAsync("Timer Complete", "Time is up.", "OK");
            }
        }

        /// <summary>
        /// Centralized save logic used by both manual submit and timer expiry.
        /// If <paramref name="requireNonEmpty"/> is true the method will require user text and show a message if empty.
        /// Returns true if an entry was saved or considered completed; false otherwise.
        /// </summary>
        private async Task<bool> SaveEntryAsync(bool requireNonEmpty)
        {
            // Enforce one completion per day
            if (HasCompletedToday())
            {
                await DisplayAlertAsync("Already Completed", "You have already completed today's entry. Come back tomorrow for the next prompt.", "OK");
                return false;
            }

            var response = PromptResponse?.Text?.Trim() ?? string.Empty;

            if (requireNonEmpty && string.IsNullOrEmpty(response))
            {
                await DisplayAlertAsync("No Response", "Please write something before submitting.", "Ok");
                return false;
            }

            try
            {
                var entry = new JournalEntry
                {
                    Id = Guid.NewGuid().ToString(),
                    Date = DateTime.Now.Date,
                    EntryText = response,
                    Mood = MoodType.Neutral,
                    PromptText = PromptLabel?.Text ?? Preferences.Get(KEY_PROMPT_TEXT, string.Empty),
                    CreatedAt = DateTime.UtcNow
                };

                // Persist to a file in the app data directory (one entry per day)
                var fileName = Path.Combine(FileSystem.AppDataDirectory, $"journal_{DateTime.Now:yyyyMMdd}.json");

                // If file exists, treat as completed and do not overwrite
                if (File.Exists(fileName))
                {
                    MarkCompletedToday();
                    ApplyCompletionStateToUi();
                    await DisplayAlertAsync("Already Completed", "An entry for today already exists. You cannot submit additional entries until tomorrow.", "OK");
                    return false;
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

                    // safe resource lookup for CardBrush (supports Brush or Color)
                    var card = GetResourceObject("CardBrush");
                    if (card is Brush b) PromptResponse.Background = b;
                    else if (card is Color c) PromptResponse.BackgroundColor = c;
                }

                // Mark completed so user cannot complete again today
                MarkCompletedToday();
                ApplyCompletionStateToUi();

                // Publish a lightweight message so other pages (Calendar) can refresh immediately
                try
                {
                    NotificationService.NotifyEntrySaved(DateTime.Now);
                }
                catch { /* safe to ignore notification errors */ }

                return true;
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"Failed to save entry: {ex.Message}", "OK");
                return false;
            }
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

        // Remaining navigation and handlers unchanged...
        private async void OnHistoryClicked(object sender, EventArgs e)
        {
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

            var newWindow = new Window(navPage);
            Application.Current?.OpenWindow(newWindow);
        }

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

        private async void OnPromptClicked(object? sender, EventArgs e)
        {
            if (PromptButton == null || PromptLabel == null) return;
            if (!PromptButton.IsEnabled) return;

            PromptButton.IsEnabled = false;
            var previousText = PromptLabel.Text;
            PromptLabel.Text = "Generating...";

            try
            {
                var userContext = PromptResponse?.Text?.Trim();
                var generator = new PromptGeneratorService();

                // Call AI
                var aiPrompt = await generator.GeneratePromptAsync(userContext);

                if (!string.IsNullOrWhiteSpace(aiPrompt))
                {
                    PromptLabel.Text = aiPrompt.Trim();
                    // persist override for today
                    var todayKey = DateTime.Now.Date.ToString("yyyy-MM-dd");
                    SaveCurrentPrompt(aiPrompt.Trim(), todayKey);
                }
                else PromptLabel.Text = previousText ?? "No Prompt returned";
            }

            catch (Exception ex)
            {
                PromptLabel.Text = previousText ?? "Error generating new prompt";
                await DisplayAlertAsync("AI Error", $"Failed to generate prompt: {ex.Message}", "OK");
            }

            finally
            {
                PromptButton.IsEnabled = true;
            }
        }

        // ShowSuccessOverlay now hides main content and shows the success card permanently (until next day).
        private async void ShowSuccessOverlay(DateTime completedAt)
        {
            // Hide mood picker if visible
            var moodOverlay = this.FindByName<VisualElement>("MoodOverlay");
            if (moodOverlay != null) moodOverlay.IsVisible = false;

            // Hide main content so user only sees success card
            var main = this.FindByName<VisualElement>("MainContent");
            if (main != null) main.IsVisible = false;

            // Set timestamp (local user's time)
            var label = this.FindByName<Label>("CompletionTimeLabel");
            if (label != null)
            {
                // Format: e.g., "4/5/2025 3:24 PM" – adjust format to taste
                label.Text = completedAt.ToString("f");
            }

            // Try to read today's journal entry to show the mood if available
            var moodLabel = this.FindByName<Label>("CompletionMoodLabel");
            if (moodLabel != null)
            {
                moodLabel.IsVisible = false;
                try
                {
                    var fileName = Path.Combine(FileSystem.AppDataDirectory, $"journal_{DateTime.Now:yyyyMMdd}.json");
                    if (File.Exists(fileName))
                    {
                        var json = await File.ReadAllTextAsync(fileName);
                        var entry = JsonSerializer.Deserialize<JournalEntry>(json);
                        if (entry != null)
                        {
                            var moodText = entry.Mood switch
                            {
                                MoodType.VeryHappy => "🤩 Amazing",
                                MoodType.Happy => "😊 Good",
                                MoodType.Neutral => "😐 Okay",
                                MoodType.Sad => "😔 Tough",
                                MoodType.VerySad => "😢 Difficult",
                                _ => "—"
                            };
                            moodLabel.Text = moodText;
                            moodLabel.IsVisible = true;
                        }
                    }
                }
                catch
                {
                    // ignore read/parse errors, leave label hidden
                    moodLabel.IsVisible = false;
                }
            }

            var success = this.FindByName<VisualElement>("SuccessOverlay");
            if (success != null) success.IsVisible = true;
        }

        private void HideSuccessOverlay()
        {
            var success = this.FindByName<VisualElement>("SuccessOverlay");
            if (success != null) success.IsVisible = false;
            var main = this.FindByName<VisualElement>("MainContent");
            if (main != null) main.IsVisible = true;
        }

        // OK now does NOT hide the success overlay: overlay remains until next day reset.
        private void OnSuccessOkClicked(object? sender, EventArgs e)
        {
            // Do not call HideSuccessOverlay(); we want the success card to remain visible.
            // Optionally clear the editor text for tidiness while leaving the overlay up.
            var editor = this.FindByName<Editor>("PromptResponse");
            if (editor != null)
            {
                editor.Text = string.Empty;
            }
        }

        private async void OnResetProgressClicked(object? sender, EventArgs e)
        {
            // Confirm destructive action
            var confirm = await DisplayAlertAsync("Reset Progress (Test)",
                "This will permanently delete local journal entries for this device and reset all local user state (streak, level, XP, theme, prompts). Continue?",
                "Reset", "Cancel");

            if (!confirm) return;

            try
            {
                // Remove prompt/preferences keys
                Preferences.Remove(KEY_PROMPT_TEXT);
                Preferences.Remove(KEY_PROMPT_DATE);
                Preferences.Remove(KEY_COMPLETED_DATE);

                // Delete daily journal files: journal_YYYYMMDD.json
                var appDir = FileSystem.AppDataDirectory;
                var files = Directory.GetFiles(appDir, "journal_*.json");
                foreach (var f in files)
                {
                    try { File.Delete(f); } catch { /* ignore per-file errors */ }
                }

                // Remove persisted user state keys used by UserStateService
                var userStateKeys = new[]
                {
                    "CurrentStreak",
                    "LongestStreak",
                    "TotalXp",
                    "Theme",
                    "Level"
                };
                foreach (var k in userStateKeys)
                {
                    try { Preferences.Remove(k); } catch { /* ignore */ }
                }

                // Reload the in-memory user state from preferences (now defaults)
                _userStats.LoadData();

                // Hide overlays if visible
                var mood = this.FindByName<VisualElement>("MoodOverlay");
                if (mood != null) mood.IsVisible = false;
                var success = this.FindByName<VisualElement>("SuccessOverlay");
                if (success != null) success.IsVisible = false;

                // Clear & re-enable the editor
                var editor = this.FindByName<Editor>("PromptResponse");
                if (editor != null)
                {
                    editor.Text = string.Empty;
                    editor.IsEnabled = true;

                    var card = GetResourceObject("CardBrush");
                    if (card is Brush b) editor.Background = b;
                    else if (card is Color c) editor.BackgroundColor = c;
                }

                // Stop timer and reset UI state
                if (MyTimer != null) MyTimer.IsActive = false;

                // Ensure a fresh prompt for today
                EnsurePromptForToday();
                ApplyCompletionStateToUi();
                UpdateUI();

                await DisplayAlertAsync("Reset Complete", "All local progress has been reset for testing.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"Failed to reset progress: {ex.Message}", "OK");
            }
        }

        private async void OnMoodPressed(object? sender, EventArgs e)
        {
            if (sender is VisualElement ve)
            {
                try
                {
                    // Slightly scale down on press for tactile feedback
                    await ve.ScaleToAsync(0.96, 80, Easing.CubicOut);
                }
                catch { }
            }
        }

        private async void OnMoodReleased(object? sender, EventArgs e)
        {
            if (sender is VisualElement ve)
            {
                try
                {
                    // Return to normal scale
                    await ve.ScaleToAsync(1.0, 120, Easing.CubicOut);
                }
                catch { }
            }
        }
    }
}