using Grpc.Net.Client;
using Grpc.Net.Compression;
using SynchronizationTool.Database.Models;
using System.Collections.Concurrent;

namespace SynchronizationTool.Logic.gRPC
{
    public class ClientChannelStorage : IClientChannelStorage
    {
        private readonly ConcurrentDictionary<Guid, GrpcChannel> _clientChannels = [];

        public GrpcChannel GetGrpcChannel(SynchClient synchClient)
        {
            var channel = _clientChannels.GetOrAdd(synchClient.Id, address =>
            {
                return GrpcChannel.ForAddress(synchClient.Address, new GrpcChannelOptions
                {
                    CompressionProviders = new List<ICompressionProvider>
                    {
                        new GzipCompressionProvider(System.IO.Compression.CompressionLevel.Fastest)
                    }
                });
            });

            return channel;
        }
    }
}
