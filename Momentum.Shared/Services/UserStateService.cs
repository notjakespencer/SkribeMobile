using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Skribe.Shared.Models;
using System;

namespace Skribe.Shared.Services
{
    public class UserStateService
    {
        public Streak Streak { get; private set; } = new();
        public int TotalXp { get; private set; }
        public int Level { get; private set; }
        public string Theme { get; private set; }

        public void LoadData()
        {
            Streak.Current = Preferences.Get("CurrentStreak", 0);
            Streak.Longest = Preferences.Get("LongestStreak", 0);
            TotalXp = Preferences.Get("TotalXp", 0);
            Theme = Preferences.Get("Theme", "light");
            Level = CalculateLevel(TotalXp);
            Preferences.Set("Level", Level);
        }

        public void SaveData()
        {
            Preferences.Set("CurrentStreak", Streak.Current);
            Preferences.Set("LongestStreak", Streak.Longest);
            Preferences.Set("TotalXp", TotalXp);
            Preferences.Set("Theme", Theme);
            Preferences.Set("Level", Level);
        }

        public void UpdateTheme(string newTheme)
        {
            Theme = newTheme;
            Preferences.Set("Theme", Theme);
        }

        public (int xpIntoLevel, int xpNeeded, double progress) GetProgress()
        {
            int xpAtStartOfLevel = 100 * (Level - 1) * Level / 2;
            int xpIntoCurrentLevel = TotalXp - xpAtStartOfLevel;
            int xpNeededForNextLevel = Level * 100;
            double progress = (double)xpIntoCurrentLevel / xpNeededForNextLevel;
            return (xpIntoCurrentLevel, xpNeededForNextLevel, Math.Clamp(progress, 0, 1));
        }

        private int CalculateLevel(int xp)
        {
            if (xp < 100) return 1;
            double level = 0.5 * (1 + Math.Sqrt(1 + (8.0 * xp) / 100));
            return (int)Math.Floor(level);
        }

        public void AddEntry()
        {
            int baseXP = 50;
            double multiplier = Math.Pow(1.1, Streak.Current);
            int earnedXP = (int)Math.Round(baseXP * multiplier);
            TotalXp += earnedXP;
            Streak.Increment();
            Level = CalculateLevel(TotalXp);
            SaveData();
        }
    }
}
