﻿using Noise.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Noise.Core.Protocol
{
    public class SignaturePayload : Payload<SignaturePayload>
    {
        private const string _propSignature = "s";

        [Obsolete("Use the KeyPayload.Factory.Create method create a new payload instance.")]
        public SignaturePayload() : base() { }

        public override PacketType Type => PacketType.SIGNATURE;

        public string Signature => Properties[_propSignature];

        public override void Validate()
        {
            if (!Properties.ContainsKey(_propSignature))
                throw new InvalidOperationException("The payload does not include required property.");

            if (Properties[_propSignature].IsEmpty())
                throw new InvalidOperationException("The required payload property has a invalid value.");
        }

        #pragma warning disable CS0618
        public static class Factory
        {
            public static SignaturePayload Create(string signature)
            {
                var payload = new SignaturePayload();
                payload.InsertProperty(_propSignature, signature);

                payload.Validate();
                return payload;
            }
        }
        #pragma warning restore CS0618
    }
}
