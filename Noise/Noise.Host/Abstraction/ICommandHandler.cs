using System.Threading;
using System.Threading.Tasks;

namespace Noise.Host.Abstraction
{
    internal interface ICommandHandler
    {
        void Prefix();
        Task Execute(string command, CancellationTokenSource cancellationTokenSource);
        void Config(string[] args);
    }
}
