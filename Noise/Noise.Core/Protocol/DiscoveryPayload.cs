using Noise.Core.Extensions;
using System;

namespace Noise.Core.Protocol
{
    public class DiscoveryPayload : Payload<DiscoveryPayload>
    {
        private const string _propPublicKeyCollection = "p";
        private const string _propEndpointCollection = "e";
        private const string _propIdentityProve = "s";

        [Obsolete("Use the DiscoveryPayload.Factory.Create method create a new payload instance.")]
        public DiscoveryPayload() : base() { }

        public override PacketType Type => PacketType.DISCOVERY;

        public string PublicKeys => Properties[_propPublicKeyCollection];
        public string Endpoints => Properties[_propEndpointCollection];
        public string IdentityProve => Properties[_propIdentityProve];

        public override void Validate()
        {
            if (!Properties.ContainsKey(_propPublicKeyCollection))
                throw new InvalidOperationException("The payload does not include required property.");

            if (!Properties.ContainsKey(_propEndpointCollection))
                throw new InvalidOperationException("The payload does not include required property.");

            if (Properties[_propPublicKeyCollection].IsEmpty())
                throw new InvalidOperationException("The required payload property has a invalid value.");

            if (Properties[_propEndpointCollection].IsEmpty())
                throw new InvalidOperationException("The required payload property has a invalid value.");
        }

        #pragma warning disable CS0618
        public static class Factory
        {
            public static DiscoveryPayload Create(string serializedPublicKeys, string serializedEndpoints, string identityProveSignature)
            {
                var payload = new DiscoveryPayload();

                payload.InsertProperty(_propPublicKeyCollection, serializedPublicKeys);
                payload.InsertProperty(_propEndpointCollection, serializedEndpoints);
                payload.InsertProperty(_propIdentityProve, identityProveSignature);

                payload.Validate();
                return payload;
            }
        }
        #pragma warning restore CS0618
    }
}
