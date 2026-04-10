using MediatR;

namespace SynchronizationTool.Logic.Models
{
    public record Command : IRequest<ResponseModel>
    {
    }
}
