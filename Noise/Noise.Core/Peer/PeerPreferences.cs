using Noise.Core.Peer.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Noise.Core.Peer
{
    public class PeerPreferences
    {
        [ConfigurablePreference]
        public bool UseTracker { get; private set; }

        [ConfigurablePreference]
        public bool VerboseMode { get; private set; }

        [ConfigurablePreference]
        public string IndependentMediumCertification { get; private set; }

        public bool ApplyPreference(string name, string value)
        {
            try
            {
                var property = typeof(PeerConfiguration)
                    .GetProperties()
                    .Where(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(ConfigurablePreferenceAttribute)))
                    .Single(p => p.Name.ToLower() == name.ToLower());

                switch (property.PropertyType)
                {
                    case Type _ when property.PropertyType == typeof(bool):
                        property.SetValue(this, bool.Parse(value)); break;

                    case Type _ when property.PropertyType == typeof(int):
                        property.SetValue(this, Convert.ToInt32(value)); break;

                    case Type _ when property.PropertyType == typeof(string):
                        property.SetValue(this, value); break;

                    default:
                        throw new InvalidOperationException();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public IDictionary<string, string> GetPreferences()
        {
            return typeof(PeerConfiguration)
                .GetProperties()
                .Where(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(ConfigurablePreferenceAttribute)))
                .ToDictionary(k => k.Name, v => (v.GetValue(this, null) is null) ? "" : v.GetValue(this, null).ToString());
        }

        public PeerPreferencesPersistence Serialize()
        {
            return new PeerPreferencesPersistence
            {
                VerboseMode = VerboseMode,
                UseTracker = UseTracker,
                IndependentMediumCertification = IndependentMediumCertification
            };
        }

        private PeerPreferences() { }
        public static class Factory
        {
            public static PeerPreferences Initialize()
            {
                return new PeerPreferences
                {
                    VerboseMode = false,
                    UseTracker = false,
                    IndependentMediumCertification = string.Empty
                };
            }

            public static PeerPreferences Deserialize(PeerPreferencesPersistence peerPreferences)
            {
                return new PeerPreferences
                {
                    VerboseMode = peerPreferences.VerboseMode,
                    UseTracker = peerPreferences.UseTracker,
                    IndependentMediumCertification = peerPreferences.IndependentMediumCertification
                };
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    internal class ConfigurablePreferenceAttribute : Attribute { }
}
