using System.Threading;
using System.Threading.Tasks;

namespace Noise.Host.Abstraction
{
    public interface ICommandHandler
    {
        void Prefix();
        Task Execute(string command, CancellationTokenSource cancellationTokenSource);
        void Config(string[] args);
    }
}
