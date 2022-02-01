using Noise.Core.Extensions;
using System;

namespace Noise.Core.Protocol
{
    public class MessagePayload : Payload<MessagePayload>
    {
        private const string _propMessageCipher = "c";
        private const string _propIdentityProve = "s";

        [Obsolete("Use the MessagePayload.Factory.Create method create a new payload instance.")]
        public MessagePayload() : base() { }

        public override PacketType Type => PacketType.MESSAGE;

        public string MessageCipher => Properties[_propMessageCipher];
        public string IdentityProve => Properties[_propIdentityProve];

        public override void Validate()
        {
            if (!Properties.ContainsKey(_propMessageCipher))
                throw new InvalidOperationException("The payload does not include required property.");

            if (!Properties.ContainsKey(_propIdentityProve))
                throw new InvalidOperationException("The payload does not include required property.");

            if (Properties[_propMessageCipher].IsEmpty())
                throw new InvalidOperationException("The required payload property has a invalid value.");

            if (Properties[_propIdentityProve].IsEmpty())
                throw new InvalidOperationException("The required payload property has a invalid value.");
        }

        #pragma warning disable CS0618
        public static class Factory
        {
            public static MessagePayload Create(string messageCipher, string identityProveSignature)
            {
                var payload = new MessagePayload();
                payload.InsertProperty(_propMessageCipher, messageCipher);
                payload.InsertProperty(_propIdentityProve, identityProveSignature);

                payload.Validate();
                return payload;
            }
        }
        #pragma warning restore CS0618
    }
}
