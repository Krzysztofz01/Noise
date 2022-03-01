using System;

namespace Noise.Core.Client
{
    public class NoiseClientConfiguration
    {
        [Obsolete("Configuration validation not implemented.")]
        public bool VerboseMode { get; set; }

        [Obsolete("Configuration validation not implemented.")]
        public int StreamBufferSize { get; set; }

        [Obsolete("Configuration validation not implemented.")]
        public int ConnectTimeoutMs { get; set; }

        [Obsolete("Configuration validation not implemented.")]
        public int ReadTimeoutMs { get; set; }

        [Obsolete("Configuration validation not implemented.")]
        public int IdleServerTimeoutMs { get; set; }

        [Obsolete("Configuration validation not implemented.")]
        public int IdleServerEvalIntervalMs { get; set; }

        [Obsolete("Configuration validation not implemented.")]
        public int MaxConnectRetryCount { get; set; }
    }
}
