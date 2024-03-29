﻿using Noise.Core.Extensions;
using System;

namespace Noise.Core.Protocol
{
    public class SignaturePayload : Payload<SignaturePayload>
    {
        private const string _propSignature = "s";
        private const string _propSenderPublicKey = "p";
        private const string _propIndependentMediumCertificationSecret = "c";
        private const string _propSenderAsymmetricSugnature = "r";

        [Obsolete("Use the KeyPayload.Factory.Create method create a new payload instance.")]
        public SignaturePayload() : base() { }

        public override PacketType Type => PacketType.SIGNATURE;

        public string Signature => Properties[_propSignature];
        public string SenderPublicKey => Properties[_propSenderPublicKey];
        public string Certification => Properties[_propIndependentMediumCertificationSecret];
        public string SenderAsymmetricSignature => Properties[_propSenderAsymmetricSugnature];

        public override void Validate()
        {
            if (!Properties.ContainsKey(_propSignature))
                throw new InvalidOperationException("The payload does not include required property.");

            if (Properties[_propSignature].IsEmpty())
                throw new InvalidOperationException("The required payload property has a invalid value.");

            if (!Properties.ContainsKey(_propSenderPublicKey))
                throw new InvalidOperationException("The payload does not include required property.");

            if (Properties[_propSenderPublicKey].IsEmpty())
                throw new InvalidOperationException("The required payload property has a invalid value.");

            if (!Properties.ContainsKey(_propSenderAsymmetricSugnature))
                throw new InvalidOperationException("The payload does not include required property.");

            if (Properties[_propSenderAsymmetricSugnature].IsEmpty())
                throw new InvalidOperationException("The required payload property has a invalid value.");

            if (!Properties.ContainsKey(_propIndependentMediumCertificationSecret))
                throw new InvalidOperationException("The payload does not include required property.");
        }

        #pragma warning disable CS0618
        public static class Factory
        {
            public static SignaturePayload Create(string signature, string senderPublicKey, string senderAsymmetricSignature, string certification = null)
            {
                var payload = new SignaturePayload();
                payload.InsertProperty(_propSignature, signature);
                payload.InsertProperty(_propSenderPublicKey, senderPublicKey);
                payload.InsertProperty(_propSenderAsymmetricSugnature, senderAsymmetricSignature);
                payload.InsertProperty(_propIndependentMediumCertificationSecret, certification ?? string.Empty);

                payload.Validate();
                return payload;
            }
        }
        #pragma warning restore CS0618
    }
}
