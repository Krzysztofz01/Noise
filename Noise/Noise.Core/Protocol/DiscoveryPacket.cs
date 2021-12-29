using Noise.Core.Abstraction;

namespace Noise.Core.Protocol
{
    public class DiscoveryPacket : Packet, IPacket
    {
        private readonly string _digest = "DIS";

        protected DiscoveryPacket() : base() { }

        public override string Digest => _digest;

        public static DiscoveryPacket Create()
        {
            var packet = new DiscoveryPacket();

            return packet;
        }
    }
}
