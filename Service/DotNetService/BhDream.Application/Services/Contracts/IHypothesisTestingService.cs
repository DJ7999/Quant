using System.Threading.Tasks;
using BhDream.Application.Hypothesis.DTO;

namespace BhDream.Application.Services.Contracts
{
    public interface IHypothesisTestingService
    {
        Task<StrategyMetadataDto> GetHypothesisMetadataAsync();
        Task<string> RunBacktestAsync(HypothesisBacktestRequestDto request);
    }
}
