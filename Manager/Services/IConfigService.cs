using System.Threading.Tasks;

namespace Manager.Services;

public interface IConfigService
{
    Task LoadConfigAsync();
}