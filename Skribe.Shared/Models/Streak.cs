using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skribe.Shared.Models
{
    public class Streak
    {
        public int Current { get; set; }
        public int Longest { get; set; }

        public void Increment()
        {
            Current++;
            if (Current > Longest) Longest = Current;
        }

        public void Reset()
        {
            Current = 0;
        }
    }
}
