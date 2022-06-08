using System;

namespace Noise.Core.Server
{
    public class NoiseServerConfiguration
    {
        private bool _verboseMode = false;
        public bool VerboseMode
        {
            get => _verboseMode;
            set => _verboseMode = value;
        }

        private int _streamBufferSize = 16384;
        public int StreamBufferSize
        {
            get => _streamBufferSize;
            set
            {
                if (value < 1 || value > 65536) throw new ArgumentException("Invalid StreamBufferSize.");
                _streamBufferSize = value;
            }
        }

        private bool _enableKeepAlive = false;
        public bool EnableKeepAlive
        {
            get => _enableKeepAlive;
            set => _enableKeepAlive = value;
        }

        private int _keepAliveInterval = 2;
        public int KeepAliveInterval
        {
            get => _keepAliveInterval;
            set
            {
                if (value < 1) throw new ArgumentException("Invalid KeepAliveInterval.");
                _keepAliveInterval = value;
            }
        }

        private int _keepAliveTime = 2;
        public int KeepAliveTime
        {
            get => _keepAliveTime;
            set
            {
                if (value < 1) throw new ArgumentException("Invalid KeepAliveTime.");
                _keepAliveTime = value;
            }
        }

        private int _keepAliveRetryCount = 2;
        public int KeepAliveRetryCount
        {
            get => _keepAliveRetryCount;
            set
            {
                if (value < 1) throw new ArgumentException("Invalid KeepAliveRetryCount.");
                _keepAliveRetryCount = value;
            }
        }

        private bool _enableNatTraversal = false;
        public bool EnableNatTraversal
        {
            get => _enableNatTraversal;
            set => _enableNatTraversal = value;
        }

        private bool _relayMode = false;
        public bool RelayMode
        {
            get => _relayMode;
            set => _relayMode = value;
        }
    }
}
