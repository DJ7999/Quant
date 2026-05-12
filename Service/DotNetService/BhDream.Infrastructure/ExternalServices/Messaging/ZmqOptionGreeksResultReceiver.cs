using BhDream.Application.Abstractions.ExternalServices;
using BhDream.Domain.Entities;
using BhDream.Infrastructure.Mapper.Proto;
using BhDream.Infrastructure.Protobuf;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Infrastructure.ExternalServices.Messaging
{
    public class ZmqOptionGreeksResultReceiver : IOptionGreeksResultReceiver, IDisposable
    {
        private readonly PullSocket _pullSocket;
        private readonly ILogger<ZmqOptionPricingDispatcher> _logger;
        public ZmqOptionGreeksResultReceiver(ILogger<ZmqOptionPricingDispatcher> logger)
        {
            _logger = logger;
            _pullSocket = new PullSocket();
            _pullSocket.Bind("tcp://*:5556");
            _pullSocket.Options.ReceiveHighWatermark = 20; // Set a high water mark to prevent blocking if workers are slow
            _pullSocket.Options.ReceiveBuffer = 64 * 1024 * 512;
            _pullSocket.Options.Linger = TimeSpan.FromMilliseconds(500);
            _logger.LogInformation("ZMQ Puller bound to tcp://*:5556");

        }

        
        public async Task<List<OptionGreeksAndIv>> Receive()
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Blocking receive
                    byte[] messageBytes = _pullSocket.ReceiveFrameBytes();

                    if (messageBytes == null || messageBytes.Length == 0)
                        return new List<OptionGreeksAndIv>();
                    var batchResult = OptionGreeksBatchResultProto.Parser.ParseFrom(messageBytes);

                    _logger.LogInformation("Received result batch {BatchId} with {Count} snapshots.",
                        batchResult.BatchId, batchResult.OptionGreeksResultSnapshots.Count);
                    return batchResult.OptionGreeksResultSnapshots.Select(OptionGreeksAndIvMapper.FromProto).ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error receiving option greeks result");
                    throw;
                }
            });
        }

        public void Dispose()
        {
            _pullSocket.Close();
            _pullSocket.Dispose();
            NetMQConfig.Cleanup();
        }

    }
}
