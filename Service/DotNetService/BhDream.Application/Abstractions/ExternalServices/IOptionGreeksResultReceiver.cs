using BhDream.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Application.Abstractions.ExternalServices
{
    public interface IOptionGreeksResultReceiver
    {
        Task<List<OptionGreeksAndIv>> Receive();
    }
}
