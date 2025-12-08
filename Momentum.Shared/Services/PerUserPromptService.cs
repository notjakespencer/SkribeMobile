// using Microsoft.Identity.Client;
using Momentum.Shared.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Momentum.Shared;
using Momentum.Shared.Models;

namespace Momentum.Shared.Services
{
    internal class PerUserPromptService
    {
        private readonly Dictionary<string, HashSet<int>> _completedPromptsByUser = new();
        private readonly Dictionary<string, (DateTime Date, Prompt Prompt)> _dailyPromptCache = new();

        public Prompt GetDailyPromptForUser(string userId)
        {
            var today = DateTime.UtcNow.Date;

            // Check if the current day's prompt is already cached for a specific user.
            if (_dailyPromptCache.TryGetValue(userId, out var cached))
            {
                if (cached.Date == today) return cached.Prompt;
            }

            // Get users completed prompts
            if (!_completedPromptsByUser.ContainsKey(userId))
                _completedPromptsByUser[userId] = new HashSet<int>();


            var completedIds = _completedPromptsByUser[userId];
            var allPrompts = PromptRepository.GetAllPrompts();

            var availablePrompts = allPrompts.Where(p => !completedIds.Contains(p.Id)).ToList();

            if (!availablePrompts.Any())
            {
                return new Prompt
                {
                    Id = 0,
                    Category = "Comnplete",
                    Text = "All finished! Come back tomorrow for a new entry!"
                };
            }

            var seed = userId.GetHashCode() ^ today.GetHashCode();
            var random = new Random(seed);
            var randomIndex = random.Next(0, availablePrompts.Count);
            var selectedPrompt = availablePrompts[randomIndex];

            _dailyPromptCache[userId] = (today, selectedPrompt);

            return selectedPrompt;
        }

        public void MarkPromptAsCompleted(string userId, int promptId)
        {
            if (!_completedPromptsByUser.ContainsKey(userId))
                _completedPromptsByUser[userId] = new HashSet<int>();

            _completedPromptsByUser[userId].Add(promptId);

            _dailyPromptCache.Remove(userId);
        }

        public bool HasCompletedTodaysPrompt(string userId)
        {
            var today = DateTime.UtcNow.Date;

            if (_dailyPromptCache.TryGetValue(userId, out var cached))
            {
                if (cached.Date == today)
                {
                    if (_completedPromptsByUser.TryGetValue(userId, out var completed))
                        return completed.Contains(cached.Prompt.Id);
                }
            } return false;
        }

        public int getRemainingPromptsCount(string userId)
        {
            if (!_completedPromptsByUser.ContainsKey(userId))
                return PromptRepository.GetAllPrompts().Count;

            var completedIds = _completedPromptsByUser[userId];
            return PromptRepository.GetAllPrompts().Count - completedIds.Count;
        }

        // Clears today's prompt cache for a user (for testing)
        public void ClearTodaysPrompt(string userId)
        {
            if (_dailyPromptCache.ContainsKey(userId))
            {
                _dailyPromptCache.Remove(userId);
            }
        }

        // Clears completed prompts for a user to reset their progress (for testing)
        public void ResetUserProgress(string userId)
        {
            if (_completedPromptsByUser.ContainsKey(userId))
            {
                _completedPromptsByUser.Remove(userId);
            }

            if (_dailyPromptCache.ContainsKey(userId))
            {
                _dailyPromptCache.Remove(userId);
            }
        }

        // Clears ALL data (for testing)
        public void ResetAll()
        {
            _completedPromptsByUser.Clear();
            _dailyPromptCache.Clear();
        }

    }
}
