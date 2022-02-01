using System;

namespace Noise.Core.Protocol
{
    public class PingPayload : Payload<PingPayload>
    {
        [Obsolete("Use the PingPayload.Factory.Create method create a new payload instance.")]
        public PingPayload() : base() { }

        public override PacketType Type => PacketType.PING;

        public override void Validate()
        {
            if (Properties.Count != 0)
                throw new InvalidOperationException("The ping packet must not contain any data inside the payload.");
        }

        #pragma warning disable CS0618
        public static class Factory
        {
            public static PingPayload Create()
            {
                var payload = new PingPayload();

                payload.Validate();
                return payload;
            }
        }
        #pragma warning restore CS0618
    }
}
