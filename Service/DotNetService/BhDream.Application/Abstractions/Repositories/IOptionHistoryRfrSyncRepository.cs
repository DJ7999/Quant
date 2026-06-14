using BhDream.Domain.Entities;
using BhDream.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace BhDream.Application.Abstractions.Repositories
{
    public interface IOptionHistoryRfrSyncRepository
    {
        Task UpdateStatus(List<OptionHistoryRfrSync> optionHistoryRfrSyncs, ProcessingStatus status);
        Task UpdateSyncTableAsync();
    }
}
