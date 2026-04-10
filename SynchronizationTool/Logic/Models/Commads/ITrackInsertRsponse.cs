namespace SynchronizationTool.Logic.Models.Commads
{
    public interface ITrackInsertRsponse : ResponseModel
    {
        List<object> Entities { get; }

        public DateTime DateTime { get; }
    }
}
