using BhDream.Application.Abstractions.ExternalServices;
using BhDream.Domain.Entities;
using BhDream.Infrastructure.Mapper.Proto;
using BhDream.Infrastructure.Persistence.Configurations;
using BhDream.Infrastructure.Protobuf;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Infrastructure.ExternalServices.Messaging
{
    public class ZmqOptionPricingDispatcher : IOptionPricingDispatcher, IDisposable
    {
        private readonly PushSocket _pushSocket;
        private readonly ILogger<ZmqOptionPricingDispatcher> _logger;
        public ZmqOptionPricingDispatcher(ILogger<ZmqOptionPricingDispatcher> logger)
        {
           
            _logger = logger;
            _pushSocket = new PushSocket();

            // We BIND here. The C++ Workers will CONNECT to this address.
            _pushSocket.Bind("tcp://*:5555");
            _pushSocket.Options.SendHighWatermark = 20; // Set a high water mark to prevent blocking if workers are slow
            _pushSocket.Options.SendBuffer = 64 * 1024 * 512;
            _pushSocket.Options.Linger = TimeSpan.FromMilliseconds(500);
            _logger.LogInformation("ZMQ Dispatcher bound to tcp://*:5555");
        }
        public async Task Dispatch(IEnumerable<OptionPricingParameterSnapshot> requestSnapshots, Guid batchId)
        {
            await Task.Run(() =>
            {
                try
                {
                    var protoBatch = new OptionBatchRequestProto
                    {
                        BatchId = batchId.ToString()
                    };

                    protoBatch.OptionRequestSnapshots.AddRange(
                        requestSnapshots.Select(OptionPricingParameterSnapshotMapper.ToProto)
                    );

                    byte[] messageBytes = protoBatch.ToByteArray();

                    _pushSocket.SendFrame(messageBytes);

                    //_logger.LogInformation("Dispatched batch {BatchId} with {Count} options.", batchId, protoBatch.OptionRequestSnapshots.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to dispatch batch {BatchId}", batchId);
                    throw;
                }
            });
        }

        public void Dispose()
        {
            _pushSocket.Close();
            _pushSocket.Dispose();
            NetMQConfig.Cleanup();
        }
    }
}
