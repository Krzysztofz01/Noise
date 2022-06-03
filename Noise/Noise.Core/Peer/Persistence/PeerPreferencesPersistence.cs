namespace Noise.Core.Peer.Persistence
{
    public class PeerPreferencesPersistence
    {
        public bool? UseTracker { get; set; }
        public bool? VerboseMode { get; set; }
        public string IndependentMediumCertification { get; set; }
        public bool? UseEndpointAttemptFilter { get; set; }
        public int? EndpointAttemptIntervalSeconds { get; set; }
        public bool? FixedPublicKeyValidationLength { get; set; }
        public int? ServerStreamBufferSize { get; set; }
        public bool? ServerEnableKeepAlive { get; set; }
        public int? ServerKeepAliveInterval { get; set; }
        public int? ServerKeepAliveTime { get; set; }
        public int? ServerKeepAliveRetryCount { get; set; }
        public int? ClientStreamBufferSize { get; set; }
        public int? ClientConnectTimeoutMs { get; set; }
        public int? ClientReadTimeoutMs { get; set; }
        public int? ClientMaxConnectRetryCount { get; set; }
        public bool? AllowHostVersionMismatch { get; set; }
        public bool? BroadcastDiscoveryOnStartup { get; set; }
        public bool? SharePublicKeysViaDiscovery { get; set; }
        public bool? AcceptPublicKeysViaDiscovery { get; set; }
        public bool? AcceptUnpromptedConnectionEndpoints { get; set; }
        public bool? EnableWindowsSpecificNatTraversal { get; set; }
        public bool? ForceUpdate { get; set; }
    }
}
