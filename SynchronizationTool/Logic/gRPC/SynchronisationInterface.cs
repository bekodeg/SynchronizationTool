using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;
using Protos;
using SynchronizationTool.Logic.Models.Commads;
using SynchronizationTool.Logic.Models.Queries;
using static Protos.SynchronisationService;

namespace SynchronizationTool.Logic.gRPC
{
    public class SynchronisationInterface(IMediator mediator) : SynchronisationServiceBase()
    {
        private readonly IMediator _mediator = mediator;

        public override async Task<Empty> SendChange(ChangeBucket request, ServerCallContext context)
        {
            var response =  await _mediator.Send(new SaveChangelogCommand()
            {
                RpcBucket = request
            });

            if (response.IsError)
            {
                throw new InvalidOperationException(response.Message);
            }

            return new Empty();
        }

        public override async Task<ChangeBucket> GetChange(ChangeRequest request, ServerCallContext context)
        {
            var changelogResponse = await _mediator.Send(new ChangeLogQuery()
            {
                ClientId = Guid.Parse(request.ClientId)
            });

            if (!changelogResponse.IsError)
            {
                throw new InvalidOperationException($"{changelogResponse.Message}");
            }

            return changelogResponse.Response!;
        }
    }
}
