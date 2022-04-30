using System.Threading.Tasks;

namespace Noise.Host.Abstraction
{
    internal interface IProgramMode
    {
        public Task<bool> Launch(string[] args);
    }
}
