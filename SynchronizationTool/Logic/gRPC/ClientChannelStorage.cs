using Grpc.Net.Client;
using Grpc.Net.Compression;
using SynchronizationTool.Database.Models;
using System.Collections.Concurrent;
using System.Net.Security;

namespace SynchronizationTool.Logic.gRPC
{
    public class ClientChannelStorage : IClientChannelStorage
    {
        private readonly ConcurrentDictionary<Guid, GrpcChannel> _clientChannels = [];

        public GrpcChannel GetGrpcChannel(SynchClient synchClient)
        {
            var channel = _clientChannels.GetOrAdd(synchClient.Id, address =>
            {
                var handler = new SocketsHttpHandler
                {
                    SslOptions = new SslClientAuthenticationOptions
                    {
                        RemoteCertificateValidationCallback =
                    (sender, certificate, chain, errors) => true
                    }
                };

                return GrpcChannel.ForAddress(synchClient.Address, new GrpcChannelOptions
                {
                    HttpHandler = handler,
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
