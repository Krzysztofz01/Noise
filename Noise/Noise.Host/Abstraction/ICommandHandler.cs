using System.Threading.Tasks;

namespace Noise.Host.Abstraction
{
    public interface ICommandHandler
    {
        void Prefix();
        Task Execute(string command);
    }
}
