using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Noise.Core.Protocol
{
    public static class PayloadSerializer
    {
        public static byte[] Serialize(IReadOnlyDictionary<string, string> payload)
        {
            var payloadBuilder = new StringBuilder();

            foreach (var property in payload)
            {
                payloadBuilder.Append(property.Key);
                payloadBuilder.Append('@');
                payloadBuilder.Append(property.Value);
                payloadBuilder.Append(';');
            }

            return Encoding.UTF8.GetBytes(payloadBuilder.ToString());
        }

        public static Dictionary<string, string> Deserialize(byte[] serializedPayload)
        {
            string decodedPayload = Encoding.UTF8.GetString(serializedPayload);

            return decodedPayload
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Split('@', StringSplitOptions.None))
                .ToDictionary(k => k.First(), v => v.Last());
        }
    }
}
