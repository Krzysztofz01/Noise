using System;

namespace Noise.Core.Protocol
{
    public class PingPayload : Payload<PingPayload>
    {
        public PingPayload() : base() { }

        public override PacketType Type => PacketType.PING;

        protected override void Validate()
        {
            if (Properties.Count != 0)
                throw new InvalidOperationException("The ping packet must not contain any data inside the payload.");
        }

        public static class Factory
        {
            public static PingPayload Create()
            {
                var payload = new PingPayload();

                payload.Validate();
                return payload;
            }
        }
    }
}
