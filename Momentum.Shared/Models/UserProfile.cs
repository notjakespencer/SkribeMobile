using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skribe.Shared.Models
{
    internal class UserProfile
    {
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public int Level { get; set; }
        public int TotalXP {get; set;}
        public string Theme { get; set; } = "light";
    }
}
