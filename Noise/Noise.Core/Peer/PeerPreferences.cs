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

        [ConfigurablePreference]
        public bool UseEndpointAttemptFilter { get; private set; }

        [ConfigurablePreference]
        public int EndpointAttemptIntervalSeconds { get; private set; }

        public bool ApplyPreference(string name, string value)
        {
            try
            {
                var property = typeof(PeerPreferences)
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
            return typeof(PeerPreferences)
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
                IndependentMediumCertification = IndependentMediumCertification,
                UseEndpointAttemptFilter = UseEndpointAttemptFilter,
                EndpointAttemptIntervalSeconds = EndpointAttemptIntervalSeconds
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
                    IndependentMediumCertification = string.Empty,
                    UseEndpointAttemptFilter = true,
                    EndpointAttemptIntervalSeconds = 60 * 5
                };
            }

            public static PeerPreferences Deserialize(PeerPreferencesPersistence peerPreferences)
            {
                var defaultPreferences = Initialize();

                return new PeerPreferences
                {
                    VerboseMode = peerPreferences.VerboseMode ?? defaultPreferences.VerboseMode,
                    UseTracker = peerPreferences.UseTracker ?? defaultPreferences.UseTracker,
                    IndependentMediumCertification = peerPreferences.IndependentMediumCertification ?? defaultPreferences.IndependentMediumCertification,
                    UseEndpointAttemptFilter = peerPreferences.UseEndpointAttemptFilter ?? defaultPreferences.UseEndpointAttemptFilter,
                    EndpointAttemptIntervalSeconds = peerPreferences.EndpointAttemptIntervalSeconds ?? defaultPreferences.EndpointAttemptIntervalSeconds
                };
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    internal class ConfigurablePreferenceAttribute : Attribute { }
}
