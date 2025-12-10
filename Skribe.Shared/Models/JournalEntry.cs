using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skribe.Shared.Models
{
    public class JournalEntry
    {
        public string? Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int PromptID { get; set; }
        public string PromptText { get; set; } = string.Empty;
        public string EntryText { get; set; } = string.Empty;

        public MoodType Mood { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum MoodType
    {
        VeryHappy = 1,  // 😁
        Happy = 2,      // 🙂
        Neutral = 3,    // 😐
        Sad = 4,        // 😔
        VerySad = 5     // 😢
    };
}
