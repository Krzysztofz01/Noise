using System;

namespace Noise.Core.Exceptions
{
    public class PacketRejectedException : Exception
    {
        public const string _defaultMessage = "Decryption failed. The package was not directed to a given peer.";

        public PacketRejectedException(string message = _defaultMessage) : base(message) { }
    }
}
