using Noise.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Noise.Core.Protocol
{
    public class DiscoveryPayload : Payload<DiscoveryPayload>
    {
        private const string _propPublicKeyCollection = "p";
        private const string _propEndpointCollection = "e";

        [Obsolete("Use the DiscoveryPayload.Factory.Create method create a new payload instance.")]
        public DiscoveryPayload() : base() { }

        public override PacketType Type => PacketType.DISCOVERY;

        public IEnumerable<string> PublicKeys =>
            JsonSerializer.Deserialize<IEnumerable<string>>(Properties[_propPublicKeyCollection]);

        public IEnumerable<string> Endpoints =>
            JsonSerializer.Deserialize<IEnumerable<string>>(Properties[_propEndpointCollection]);

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
            public static DiscoveryPayload Create(IEnumerable<string> publicKeys, IEnumerable<string> endpoints)
            {
                if (publicKeys is null) throw new ArgumentNullException(nameof(publicKeys));
                if (endpoints is null) throw new ArgumentNullException(nameof(endpoints));

                var serializedPublicKeys = JsonSerializer.Serialize(publicKeys);
                var serializedEndpoints = JsonSerializer.Serialize(endpoints);

                var payload = new DiscoveryPayload();

                payload.InsertProperty(_propPublicKeyCollection, serializedPublicKeys);
                payload.InsertProperty(_propEndpointCollection, serializedEndpoints);

                payload.Validate();
                return payload;
            }
        }
        #pragma warning restore CS0618
    }
}
