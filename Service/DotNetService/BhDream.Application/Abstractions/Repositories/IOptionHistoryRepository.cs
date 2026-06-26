using BhDream.Application.Dtos;
using BhDream.Domain.Entities;

namespace BhDream.Application.Abstractions.Repositories
{
    public interface IOptionHistoryRepository
    {
        Task<List<OptionHistory>> GetOptionHistoryForContractAsync(OptionContractDto entity);
        Task UpsertRangeAsync(List<OptionHistory> entities);
        Task<(DateTime, DateTime)> GetFirstAndLastDate();
    }
}
