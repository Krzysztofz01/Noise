using System;
using System.Net.Sockets;
using System.Threading;

namespace Noise.Core.Server
{
    public class PeerMetadata : IDisposable
    {
        private readonly TcpClient _tcpClient;
        private readonly NetworkStream _networkStream;
        private readonly string _ipPort;

        public TcpClient TcpClient => _tcpClient;
        public NetworkStream NetworkStream => _networkStream;
        public string IpPort => _ipPort;

        internal CancellationTokenSource TokenSource { get; set; }
        internal CancellationToken Token { get; set; }

        internal SemaphoreSlim SendLock = new(1, 1);
        internal SemaphoreSlim ReceiveLock = new(1, 1);

        public PeerMetadata(TcpClient tcpClient)
        {
            _tcpClient = tcpClient ??
                throw new ArgumentNullException(nameof(tcpClient));

            _networkStream = tcpClient.GetStream();
            _ipPort = tcpClient.Client.RemoteEndPoint.ToString();

            TokenSource = new CancellationTokenSource();
            Token = TokenSource.Token;
        }
        public void Dispose()
        {
            if (TokenSource is not null)
            {
                if (!TokenSource.IsCancellationRequested)
                {
                    TokenSource.Cancel();
                    TokenSource.Dispose();
                }
            }

            if (_networkStream is not null)
            {
                _networkStream.Close();
                //_networkStream.Dispose();
            }

            if (_tcpClient is not null)
            {
                _tcpClient.Close();
                _tcpClient.Dispose();
            }
        }
    }
}
