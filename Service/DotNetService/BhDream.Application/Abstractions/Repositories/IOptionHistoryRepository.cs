using BhDream.Domain.Entities;

namespace BhDream.Application.Abstractions.Repositories
{
    public interface IOptionHistoryRepository
    {
        Task AddAsync(OptionHistory entity);
        Task UpdateAsync(OptionHistory entity);
        Task<OptionHistory?> GetOptionHistoryAsync(OptionHistory entity);
    }
}
