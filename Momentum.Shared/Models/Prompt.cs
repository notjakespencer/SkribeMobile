using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skribe.Shared.Models
{
    public class Prompt
    {
        public int Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}
