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

        [ConfigurablePreference]
        public bool FixedPublicKeyValidationLength { get; private set; }

        [ConfigurablePreference]
        public int ServerStreamBufferSize { get; private set; }

        [ConfigurablePreference]
        public bool ServerEnableKeepAlive { get; private set; }

        [ConfigurablePreference]
        public int ServerKeepAliveInterval { get; private set; }

        [ConfigurablePreference]
        public int ServerKeepAliveTime { get; private set; }

        [ConfigurablePreference]
        public int ServerKeepAliveRetryCount { get; private set; }

        [ConfigurablePreference]
        public int ClientStreamBufferSize { get; private set; }

        [ConfigurablePreference]
        public int ClientConnectTimeoutMs { get; private set; }

        [ConfigurablePreference]
        public int ClientReadTimeoutMs { get; private set; }

        [ConfigurablePreference]
        public int ClientMaxConnectRetryCount { get; private set; }

        [Dangerous]
        [ConfigurablePreference]
        public bool AllowHostVersionMismatch { get; private set; }

        [ConfigurablePreference]
        public bool BroadcastDiscoveryOnStartup { get; private set; }

        [ConfigurablePreference]
        public bool SharePublicKeysViaDiscovery { get; private set; }

        [ConfigurablePreference]
        public bool AcceptPublicKeysViaDiscovery { get; private set; }

        [Dangerous]
        [ConfigurablePreference]
        public bool AcceptUnpromptedConnectionEndpoints { get; private set; }

        [Dangerous]
        [ConfigurablePreference]
        public bool EnableWindowsSpecificNatTraversal { get; private set; }

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

        public static bool IsDangerous(string name)
        {
            return typeof(PeerPreferences)
               .GetProperties()
               .Where(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(ConfigurablePreferenceAttribute)))
               .Where(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(DangerousAttribute)))
               .Any(p => p.Name.ToLower() == name.ToLower());
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
                EndpointAttemptIntervalSeconds = EndpointAttemptIntervalSeconds,
                FixedPublicKeyValidationLength = FixedPublicKeyValidationLength,
                ServerStreamBufferSize = ServerStreamBufferSize,
                ServerEnableKeepAlive = ServerEnableKeepAlive,
                ServerKeepAliveInterval = ServerKeepAliveInterval,
                ServerKeepAliveTime = ServerKeepAliveTime,
                ServerKeepAliveRetryCount = ServerKeepAliveRetryCount,
                ClientStreamBufferSize = ClientStreamBufferSize,
                ClientConnectTimeoutMs = ClientConnectTimeoutMs,
                ClientReadTimeoutMs = ClientReadTimeoutMs,
                ClientMaxConnectRetryCount = ClientMaxConnectRetryCount,
                AllowHostVersionMismatch = AllowHostVersionMismatch,
                BroadcastDiscoveryOnStartup = BroadcastDiscoveryOnStartup,
                SharePublicKeysViaDiscovery = SharePublicKeysViaDiscovery,
                AcceptPublicKeysViaDiscovery = AcceptPublicKeysViaDiscovery,
                AcceptUnpromptedConnectionEndpoints = AcceptUnpromptedConnectionEndpoints,
                EnableWindowsSpecificNatTraversal = EnableWindowsSpecificNatTraversal
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
                    EndpointAttemptIntervalSeconds = 60 * 5,
                    FixedPublicKeyValidationLength = true,
                    ServerStreamBufferSize = 16384,
                    ServerEnableKeepAlive = false,
                    ServerKeepAliveInterval = 2,
                    ServerKeepAliveTime = 2,
                    ServerKeepAliveRetryCount = 2,
                    ClientStreamBufferSize = 16384,
                    ClientConnectTimeoutMs = 5000,
                    ClientReadTimeoutMs = 1000,
                    ClientMaxConnectRetryCount = 3,
                    AllowHostVersionMismatch = false,
                    BroadcastDiscoveryOnStartup = true,
                    SharePublicKeysViaDiscovery = false,
                    AcceptPublicKeysViaDiscovery = false,
                    AcceptUnpromptedConnectionEndpoints = true,
                    EnableWindowsSpecificNatTraversal = false
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
                    EndpointAttemptIntervalSeconds = peerPreferences.EndpointAttemptIntervalSeconds ?? defaultPreferences.EndpointAttemptIntervalSeconds,
                    FixedPublicKeyValidationLength = peerPreferences.FixedPublicKeyValidationLength ?? defaultPreferences.FixedPublicKeyValidationLength,
                    ServerStreamBufferSize = peerPreferences.ServerStreamBufferSize ?? defaultPreferences.ServerStreamBufferSize,
                    ServerEnableKeepAlive = peerPreferences.ServerEnableKeepAlive ?? defaultPreferences.ServerEnableKeepAlive,
                    ServerKeepAliveInterval = peerPreferences.ServerKeepAliveInterval ?? defaultPreferences.ServerKeepAliveInterval,
                    ServerKeepAliveTime = peerPreferences.ServerKeepAliveTime ?? defaultPreferences.ServerKeepAliveTime,
                    ServerKeepAliveRetryCount = peerPreferences.ServerKeepAliveRetryCount ?? defaultPreferences.ServerKeepAliveRetryCount,
                    ClientStreamBufferSize = peerPreferences.ClientStreamBufferSize ?? defaultPreferences.ClientStreamBufferSize,
                    ClientConnectTimeoutMs = peerPreferences.ClientConnectTimeoutMs ?? defaultPreferences.ClientConnectTimeoutMs,
                    ClientReadTimeoutMs = peerPreferences.ClientReadTimeoutMs ?? defaultPreferences.ClientReadTimeoutMs,
                    ClientMaxConnectRetryCount = peerPreferences.ClientMaxConnectRetryCount ?? defaultPreferences.ClientMaxConnectRetryCount,
                    AllowHostVersionMismatch = peerPreferences.AllowHostVersionMismatch ?? defaultPreferences.AllowHostVersionMismatch,
                    BroadcastDiscoveryOnStartup = peerPreferences.BroadcastDiscoveryOnStartup ?? defaultPreferences.BroadcastDiscoveryOnStartup,
                    SharePublicKeysViaDiscovery = peerPreferences.SharePublicKeysViaDiscovery ?? defaultPreferences.SharePublicKeysViaDiscovery,
                    AcceptPublicKeysViaDiscovery = peerPreferences.AcceptPublicKeysViaDiscovery ?? defaultPreferences.AcceptPublicKeysViaDiscovery,
                    AcceptUnpromptedConnectionEndpoints = peerPreferences.AcceptUnpromptedConnectionEndpoints ?? defaultPreferences.AcceptUnpromptedConnectionEndpoints,
                    EnableWindowsSpecificNatTraversal = peerPreferences.EnableWindowsSpecificNatTraversal ?? defaultPreferences.EnableWindowsSpecificNatTraversal
                };
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    internal class ConfigurablePreferenceAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property)]
    internal class DangerousAttribute : Attribute { }
}
