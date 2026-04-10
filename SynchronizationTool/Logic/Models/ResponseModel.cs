namespace SynchronizationTool.Logic.Models
{
    public record ResponseModel
    {
        public int StatusCode { get; init; }

        public string Message { get; init; } = string.Empty;

        public bool IsError => StatusCode >= 200 && StatusCode < 300 && string.IsNullOrEmpty(Message);
    }

    public record ResponseModel<TResponse> : ResponseModel
    {
        public TResponse? Response { get; init; }
    }
}
