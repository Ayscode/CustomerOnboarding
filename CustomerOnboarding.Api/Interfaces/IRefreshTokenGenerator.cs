using System.Threading.Tasks;

namespace CustomerOnboarding.Interfaces
{
    public interface IRefreshTokenGenerator
    {
        Task<string> GenerateToken();
    }
}
