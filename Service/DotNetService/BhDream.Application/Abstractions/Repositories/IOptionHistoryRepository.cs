using BhDream.Application.Dtos;
using BhDream.Domain.Entities;

namespace BhDream.Application.Abstractions.Repositories
{
    public interface IOptionHistoryRepository
    {
        Task AddAsync(OptionHistory entity);
        Task AddRangeAsync(List<OptionHistory> entity);
        Task UpdateAsync(OptionHistory entity);
        Task UpdateRangeAsync(List<OptionHistory> entity);
        Task<OptionHistory?> GetOptionHistoryAsync(OptionHistory entity);
        Task<List<OptionHistory>> GetOptionHistoryForContractAsync(OptionContractDto entity);
    }
}
