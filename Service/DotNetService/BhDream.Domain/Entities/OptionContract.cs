using BhDream.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Domain.Entities
{
    public class OptionContract
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UnderlyingId { get; set; }
        public Underlying Underlying { get; set; }

        public DateTime Expiry { get; set; }
        public decimal StrikePrice { get; set; }
        public OptionRightType OptionType { get; set; }

        public ICollection<OptionHistory> Histories { get; set; } = new List<OptionHistory>();
    }
}
