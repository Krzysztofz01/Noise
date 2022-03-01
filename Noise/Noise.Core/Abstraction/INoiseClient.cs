using System;

namespace Noise.Core.Abstraction
{
    public interface INoiseClient : IDisposable
    {
        //Implement/expose sending for all kind of packet types to avoid exposing low-level byte buffer sending
    }
}
