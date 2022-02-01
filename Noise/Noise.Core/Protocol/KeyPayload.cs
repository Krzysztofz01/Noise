using Noise.Core.Extensions;
using System;

namespace Noise.Core.Protocol
{
    public class KeyPayload : Payload<KeyPayload>
    {
        private const string _propMessageKey = "k";
        private const string _propIdentityProve = "s";

        [Obsolete("Use the KeyPayload.Factory.Create method create a new payload instance.")]
        public KeyPayload() : base() { }

        public override PacketType Type => PacketType.KEY;

        public string MessageKey => Properties[_propMessageKey];
        public string IdentityProve => Properties[_propIdentityProve];

        public override void Validate()
        {
            if (!Properties.ContainsKey(_propMessageKey))
                throw new InvalidOperationException("The payload does not include required property.");

            if (!Properties.ContainsKey(_propIdentityProve))
                throw new InvalidOperationException("The payload does not include required property.");

            if (Properties[_propMessageKey].IsEmpty())
                throw new InvalidOperationException("The required payload property has a invalid value.");

            if (Properties[_propIdentityProve].IsEmpty())
                throw new InvalidOperationException("The required payload property has a invalid value.");
        }

        #pragma warning disable CS0618
        public static class Factory
        {
            public static KeyPayload Create(string messageKey, string identityProveSignature)
            {
            
                var payload = new KeyPayload();

                payload.InsertProperty(_propMessageKey, messageKey);
                payload.InsertProperty(_propIdentityProve, identityProveSignature);

                payload.Validate();
                return payload;
            }
        }
        #pragma warning restore CS0618
    }
}
