using BhDream.Domain.Entities;
using BhDream.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.Dtos
{
    public class OptionContractDto
    {
        public string? Underlying { get; set; }
        public string? OptionType { get; set; }
        public decimal? StrikePrice { get; set; }
        public DateTime? ExpirationDate { get; set; }

        public static OptionContractDto FromEntity(OptionContract contract)
        {
            return new OptionContractDto
            {
                Underlying = contract.Underlying?.Symbol,
                OptionType = contract.OptionType.ToString(),
                StrikePrice = contract.StrikePrice,
                ExpirationDate = contract.Expiry
            };
        }
    }
}
