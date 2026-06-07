using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Protos;
using SynchronizationTool.Database.Context;
using SynchronizationTool.Logic.gRPC;
using SynchronizationTool.Logic.Models;
using SynchronizationTool.Logic.Models.Commads;
using SynchronizationTool.Logic.Models.Queries;
using static Protos.SynchronisationService;

namespace SynchronizationTool.Logic.Handlers.Commads
{
    public class SynchChangeLogCommandHandler(
        ILogger<SynchChangeLogCommandHandler> logger,
        ISynchronizationToolContext synchronizationToolContext,
        IClientChannelStorage clientChannelStorage,
        IMediator mediator
        ) : AbstractCommandHandler<SynchChangeLogCommand>(logger)
    {
        private readonly ISynchronizationToolContext _synchronizationToolContext = synchronizationToolContext;
        private readonly IClientChannelStorage _clientChannelStorage = clientChannelStorage;
        private readonly IMediator _mediator = mediator;

        public override async Task<ResponseModel> HandleAsync(SynchChangeLogCommand request, CancellationToken cancellationToken)
        {
            var client = await _synchronizationToolContext.SynchClients
                .FirstOrDefaultAsync(c => c.Id == request.ClientId, cancellationToken);

            if (client == null)
            {
                throw new ArgumentException();
            }

            var serviceClient = new SynchronisationServiceClient(_clientChannelStorage.GetGrpcChannel(client));

            var inputchangeLog = await serviceClient.GetChangeAsync(new ChangeRequest() 
            {
                ClientId = request.ClientId.ToString()
            }, cancellationToken: cancellationToken);

            await _mediator.Send(new SaveChangelogCommand()
            {
                RpcBucket = inputchangeLog
            });

            var rpcRequestResponse = await _mediator.Send(new ChangeLogQuery()
            {
                ClientId = request.ClientId,
            });

            if (rpcRequestResponse.IsError)
            {
                throw new InvalidOperationException();
            }
            
            var response = await serviceClient.SendChangeAsync(rpcRequestResponse.Response, cancellationToken: cancellationToken);

            var sentIds = rpcRequestResponse.Response!.Changes
                .Select(c => Guid.Parse(c.Id))
                .ToList();

            var lastSentLog = await _synchronizationToolContext.ChangeLogs
                .Where(cl => sentIds.Contains(cl.Id))
                .OrderByDescending(cl => cl.DateTime)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastSentLog != null)
                client.LastChangeLogId = lastSentLog.Id;

            await _synchronizationToolContext.SaveChangesAsync(cancellationToken);

            return new();
        }
    }
}
