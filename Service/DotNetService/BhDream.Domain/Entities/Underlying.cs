using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Domain.Entities
{
    public class Underlying
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Symbol { get; set; } = null!;

        public ICollection<OptionContract> Contracts { get; set; } = new List<OptionContract>();
    }
}
