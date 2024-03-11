using System;
using System.Threading.Tasks;

namespace CustomerOnboarding.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IStateRepository StateRepository { get; }
        ILgaRepository LgaRepository { get; }
        Task Save();
    }
}
