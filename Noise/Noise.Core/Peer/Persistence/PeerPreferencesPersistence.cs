namespace Noise.Core.Peer.Persistence
{
    public class PeerPreferencesPersistence
    {
        public bool? UseTracker { get; set; }
        public bool? VerboseMode { get; set; }
        public string IndependentMediumCertification { get; set; }
        public bool? UseEndpointAttemptFilter { get; set; }
        public int? EndpointAttemptIntervalSeconds { get; set; }
    }
}
