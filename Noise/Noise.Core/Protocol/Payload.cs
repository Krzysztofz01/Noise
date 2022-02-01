using Noise.Core.Extensions;
using Noise.Core.Hashing;
using System;
using System.Collections.Generic;

namespace Noise.Core.Protocol
{
    public abstract class Payload<TPayload> where TPayload : Payload<TPayload>, new()
    {
        private Dictionary<string, string> _properties;
        protected IReadOnlyDictionary<string, string> Properties => _properties;

        protected Payload()
        {
            _properties = new Dictionary<string, string>();
        }

        public abstract PacketType Type { get; }

        public byte[] Serialize() => PayloadSerializer.Serialize(Properties);
        public byte[] CalculateChecksum() => SHA1HashingHandler.HashToBytes(Serialize());

        protected void InsertProperty(string key, string value)
        {
            if (key.IsEmpty() || value is null)
                throw new InvalidOperationException("Invalid property require a key-value pair.");

            if (Properties.ContainsKey(key))
                throw new InvalidOperationException("The payload property key must be unique.");

            _properties.Add(key, value);
        }

        protected abstract void Validate();

        public static TPayload Deserialize(byte[] payload)
        {
            return new TPayload
            {
                _properties = PayloadSerializer.Deserialize(payload)
            };
        }
    }
}
