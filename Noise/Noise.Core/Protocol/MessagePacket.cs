using Noise.Core.Abstraction;

namespace Noise.Core.Protocol
{
    public class MessagePacket : Packet, IPacket
    {
        private readonly string _digest = "MSG"; 

        protected MessagePacket() : base() { }

        public override string Digest => _digest;

        public static MessagePacket Create(string content)
        {
            var packet = new MessagePacket();

            packet.SetContent(content);

            return packet;
        }
    }
}
