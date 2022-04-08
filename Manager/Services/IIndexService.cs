using System.Threading.Tasks;

namespace Manager.Services;

public interface IIndexService
{
    Task GenIndexAsync(string indexId);
}