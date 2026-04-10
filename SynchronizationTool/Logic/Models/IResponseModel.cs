namespace SynchronizationTool.Logic.Models
{
    public interface IResponseModel
    {
        int StatusCode { get; }

        string Message { get; }

        bool IsError { get; }
    }

    public interface IResponseModel<TResponse> : IResponseModel
    {
        public TResponse? Response { get; }
    }
}
