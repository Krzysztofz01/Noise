using System;
using System.Threading.Tasks;

namespace Noise.Core.Abstraction
{
    public interface INoiseServer : IDisposable, IAsyncDisposable
    {
        Task StartAsync();
    }
}
