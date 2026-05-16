using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.Dtos
{
    using global::BhDream.Domain.Entities;
    using System;
    using System.Collections.Generic;
    using System.Text;

    namespace BhDream.Domain.Entities
    {
        public class OptionGreeksAndIvDto
        {
            public DateTime? OptionHistoryDate { get; set; }
            public string? Underlying { get; set; }
            public string? OptionType { get; set; }
            public decimal? StrikePrice { get; set; }
            public DateTime? ExpirationDate { get; set; }
            public string RfrTenor { get; set; }
            public double Delta { get; set; }
            public double Theta { get; set; }
            public double Gamma { get; set; }
            public double Vega { get; set; }
            public double Rho { get; set; }
            public double Vomma { get; set; }
            public double ImpliedVolatility { get; set; }

            public static OptionGreeksAndIvDto FromEntity(OptionGreeksAndIv entity)
                {
                    return new OptionGreeksAndIvDto
                    {
                        OptionHistoryDate = entity.OptionHistory?.Date,
                        Underlying = entity.Contract?.Underlying?.Symbol,
                        OptionType = entity.Contract?.OptionType.ToString(),
                        StrikePrice = entity.Contract?.StrikePrice,
                        ExpirationDate = entity.Contract?.Expiry,
                        RfrTenor = entity.RfrTenor,
                        Delta = entity.Delta,
                        Theta = entity.Theta,
                        Gamma = entity.Gamma,
                        Vega = entity.Vega,
                        Rho = entity.Rho,
                        Vomma = entity.Vomma,
                        ImpliedVolatility = entity.ImpliedVolatility
                    };
            }
        }
    }

}
