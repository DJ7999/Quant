using BhDream.Domain.Entities;
using BhDream.Infrastructure.Protobuf;

namespace BhDream.Infrastructure.Mapper.Proto
{
    public class OptionGreeksAndIvMapper
    {
        public static OptionGreeksAndIv FromProto(OptionGreeksResultSnapshotProto proto)
        {
            return new OptionGreeksAndIv
            {
                OptionHistoryId = Guid.Parse(proto.OptionHistoryId),
                ContractId = Guid.Parse(proto.ContractId),
                RfrMarket = proto.RfrMarket,
                RfrTenor = proto.RfrTenor,
                Delta = proto.Delta,
                Theta = proto.Theta,
                Gamma = proto.Gamma,
                Vega = proto.Vega,
                Rho = proto.Rho,
                Vomma = proto.Vomma,
                ImpliedVolatility = proto.ImpliedVolatility,
                BenchMarkDelta = proto.BenchmarkResult.Delta,
                BenchMarkTheta = proto.BenchmarkResult.Theta,
                BenchMarkGamma = proto.BenchmarkResult.Gamma,
                BenchMarkVega = proto.BenchmarkResult.Vega,
                BenchMarkRho = proto.BenchmarkResult.Rho,
                BenchMarkVomma = proto.BenchmarkResult.Vomma,
                BenchMarkImpliedVolatility = proto.BenchmarkResult.ImpliedVolatility
            };
        }
    }
}
