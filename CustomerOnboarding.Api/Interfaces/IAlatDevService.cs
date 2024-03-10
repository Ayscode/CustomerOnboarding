using System.Threading.Tasks;
using CustomerOnboarding.Integrations.WemaTechTest;

namespace CustomerOnboarding.Interfaces
{
    public interface IWemaAPIService
    {
        Task<GetBanksResponse> GetBanks();
    }
}