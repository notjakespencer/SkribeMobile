using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Skribe.Shared.Data;
using Skribe.Shared.Models;

namespace Skribe.Shared.Services
{
    internal class PerUserPromptService
    {
        private readonly Dictionary<string, HashSet<int>> _completedPromptsByUser = new();
        private readonly Dictionary<string, (DateTime Date, Prompt Prompt)> _dailyPromptCache = new();
        private readonly object _lock = new();

        public Prompt GetDailyPromptForUser(string userId)
        {
            var today = DateTime.UtcNow.Date;

            lock (_lock)
            {
                // Check if the current day's prompt is already cached for a specific user.
                if (_dailyPromptCache.TryGetValue(userId, out var cached))
                {
                    if (cached.Date == today) return cached.Prompt;
                }

                // Ensure completed set exists
                if (!_completedPromptsByUser.TryGetValue(userId, out var completedIds))
                {
                    completedIds = new HashSet<int>();
                    _completedPromptsByUser[userId] = completedIds;
                }

                var allPrompts = PromptRepository.GetAllPrompts();
                // Avoid enumerating allPrompts multiple times
                var availablePrompts = allPrompts.Where(p => !completedIds.Contains(p.Id)).ToList();

                if (!availablePrompts.Any())
                {
                    return new Prompt
                    {
                        Id = 0,
                        Category = "Complete",
                        Text = "All finished! Come back tomorrow for a new entry!"
                    };
                }

                // Use a deterministic stable hash for seed so selection is repeatable
                var seed = GetStableHash(userId + today.ToString("yyyy-MM-dd"));
                var random = new Random(seed);
                var randomIndex = random.Next(0, availablePrompts.Count);
                var selectedPrompt = availablePrompts[randomIndex];

                _dailyPromptCache[userId] = (today, selectedPrompt);

                return selectedPrompt;
            }
        }

        public void MarkPromptAsCompleted(string userId, int promptId)
        {
            lock (_lock)
            {
                if (!_completedPromptsByUser.TryGetValue(userId, out var set))
                {
                    set = new HashSet<int>();
                    _completedPromptsByUser[userId] = set;
                }

                set.Add(promptId);
                _dailyPromptCache.Remove(userId);
            }
        }

        public bool HasCompletedTodaysPrompt(string userId)
        {
            var today = DateTime.UtcNow.Date;

            lock (_lock)
            {
                if (_dailyPromptCache.TryGetValue(userId, out var cached))
                {
                    if (cached.Date == today)
                    {
                        if (_completedPromptsByUser.TryGetValue(userId, out var completed))
                            return completed.Contains(cached.Prompt.Id);
                    }
                }

                return false;
            }
        }

        public int GetRemainingPromptsCount(string userId)
        {
            lock (_lock)
            {
                var allCount = PromptRepository.GetAllPrompts().Count;
                if (!_completedPromptsByUser.TryGetValue(userId, out var completedIds))
                    return allCount;

                return Math.Max(0, allCount - completedIds.Count);
            }
        }

        // Clears today's prompt cache for a user (for testing)
        public void ClearTodaysPrompt(string userId)
        {
            lock (_lock)
            {
                _dailyPromptCache.Remove(userId);
            }
        }

        // Clears completed prompts for a user to reset their progress (for testing)
        public void ResetUserProgress(string userId)
        {
            lock (_lock)
            {
                _completedPromptsByUser.Remove(userId);
                _dailyPromptCache.Remove(userId);
            }
        }

        // Clears ALL data (for testing)
        public void ResetAll()
        {
            lock (_lock)
            {
                _completedPromptsByUser.Clear();
                _dailyPromptCache.Clear();
            }
        }

        // Simple stable hash (deterministic across runs)
        private static int GetStableHash(string s)
        {
            unchecked
            {
                int hash = 23;
                foreach (var ch in s)
                {
                    hash = (hash * 31) + ch;
                }
                return hash & 0x7FFFFFFF;
            }
        }
    }
}
