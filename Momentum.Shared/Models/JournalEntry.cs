using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Momentum.Shared.Models
{
    public class JournalEntry
    {
        public DateTime Date { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Mood { get; set; } = string.Empty;

    }
}
