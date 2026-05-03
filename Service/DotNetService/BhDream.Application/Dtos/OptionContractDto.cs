using BhDream.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.Dtos
{
    public class OptionContractDto
    {
        public string? Underlying { get; set; }
        public OptionRightType? OptionType { get; set; }
        public decimal? StrikePrice { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}
