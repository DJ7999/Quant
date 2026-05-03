using BhDream.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.Services.Contracts
{
    public interface IRfrCsvImportService
    {
        Task ImportAsync(Stream csvStream);
    }
}
