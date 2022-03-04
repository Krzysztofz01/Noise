using System;

namespace Noise.Core.Client
{
    public class NoiseClientConfiguration
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

        private int _connectTimeoutMs = 5000;
        public int ConnectTimeoutMs
        {
            get => _connectTimeoutMs;
            set
            {
                if (value < 0 || value > int.MaxValue) throw new ArgumentException("Invalid ConnectTimeoutMs.");
                _connectTimeoutMs = value;
            }
        }

        private int _readTimeoutMs = 1000;
        public int ReadTimeoutMs
        {
            get => _readTimeoutMs;
            set
            {
                if (value < 0 || value > int.MaxValue) throw new ArgumentException("Invalid ReadTimeoutMs.");
                _readTimeoutMs = value;
            }
        }

        private int _maxConnectRetryCount = 3;
        public int MaxConnectRetryCount
        {
            get => _maxConnectRetryCount;
            set
            {
                if (value < 0 || value > 15) throw new ArgumentException("Invalid MaxConnectRetryCount.");
                _maxConnectRetryCount = value;
            }
        }
    }
}
