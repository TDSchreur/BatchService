using System.Threading.Tasks;

namespace Worker
{
    public interface IMakeSnapshotServiceAgent
    {
        Task MakeSnapshot();
    }
}