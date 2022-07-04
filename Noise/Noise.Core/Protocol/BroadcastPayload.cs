using Noise.Core.Extensions;
using System;

namespace Noise.Core.Protocol
{
    public class BroadcastPayload : Payload<BroadcastPayload>
    {
        private const string _propMessageContent = "c";

        [Obsolete("Use the BroadcastPayload.Factory.Create method create a new payload instance.")]
        public BroadcastPayload() : base() { }

        public override PacketType Type => PacketType.BROADCAST;

        public string MessageContent => Properties[_propMessageContent];

        public override void Validate()
        {
            if (!Properties.ContainsKey(_propMessageContent))
                throw new InvalidOperationException("The payload does not include required property.");

            if (Properties[_propMessageContent].IsEmpty())
                throw new InvalidOperationException("The required payload property has a invalid value.");
        }

        #pragma warning disable CS0618
        public static class Factory
        {
            public static BroadcastPayload Create(string messageContent)
            {
                var payload = new BroadcastPayload();
                payload.InsertProperty(_propMessageContent, messageContent);

                payload.Validate();
                return payload;
            }
        }
        #pragma warning restore CS0618
    }
}
