using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Domain.Entities
{
    public class RiskFreeRate
    {
        public required DateTime Date { get; set; }
        public required decimal Rate { get; set; }
        public required string Tenor { get; set; }
        public string Market { get; set; } = "India";
    }
}
