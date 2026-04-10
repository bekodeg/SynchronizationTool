using MediatR;

namespace SynchronizationTool.Logic.Models
{
    public record Query<TResponse> : IRequest<ResponseModel<TResponse>>
    {
    }
}
