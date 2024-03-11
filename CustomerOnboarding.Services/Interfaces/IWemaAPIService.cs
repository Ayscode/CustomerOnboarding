using System.Threading.Tasks;
using CustomerOnboarding.Integrations.WemaAPI;

namespace CustomerOnboarding.Interfaces
{
    public interface IWemaAPIService
    {
        Task<GetBanksResponse> GetBanks();
    }
}