using Grpc.Net.Client;
using SynchronizationTool.Database.Models;

namespace SynchronizationTool.Logic.gRPC
{
    public interface IClientChannelStorage
    {
        GrpcChannel GetGrpcChannel(SynchClient synchClient);
    }
}
