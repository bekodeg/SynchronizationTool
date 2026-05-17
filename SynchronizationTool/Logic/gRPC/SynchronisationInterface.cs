using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;
using Protos;
using SynchronizationTool.Database.Context;
using SynchronizationTool.Database.Models;
using SynchronizationTool.Database.Models.Enums;
using static Protos.SynchronisationService;

namespace SynchronizationTool.Logic.gRPC
{
    public class SynchronisationInterface(IDbSynchronizationContext synchronizationContext) : SynchronisationServiceBase()
    {
        private readonly IDbSynchronizationContext _synchronizationContext = synchronizationContext;

        public override Task<Empty> SendChange(ChangeBucket request, ServerCallContext context)
        {
            List<ChangeLog> changes = request.Changes.Select(c => new ChangeLog
            {
                Id = Guid.Parse(c.Id),
                DateTime = c.DateTime.ToDateTime(),
                Type = (ChangeType)c.Type,
                EntityId = Guid.Parse(c.EntityId),
                RowId = Guid.NewGuid(),
                ClientId = Guid.Parse(c.ClientId),
                ClientVersion = c.ClientVersion,
                Changes = c.Changes.Select(ch => new Change
                {
                    Id = Guid.NewGuid(),
                    ColumnName = ch.ColumnName,
                    Value = ch.ColumnCase.HasFlag(ChangeModel.ColumnOneofCase.Value) ? ch.Value : null,
                }).ToList()
            }).ToList();
            return Task.FromResult(new Empty());
        }
    }
}
