using Noise.Core.Abstraction;
using Noise.Core.Peer;
using Noise.Core.Protocol;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Noise.Core.Client
{
    public class NoiseClient : INoiseClient
    {
        private readonly IOutput _output;
        private readonly PeerConfiguration _peerConfiguration;
        private readonly NoiseClientConfiguration _noiseClientConfiguration;

        private string _peerIp = null;
        private readonly IPAddress _ipAddress = null;
        private TcpClient _tcpClient = null;
        private NetworkStream _networkStream = null;

        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
        private bool _isConnected = false;

        private CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private CancellationToken _token;

        private DateTime _lastActivity = DateTime.Now;
        private bool _isTimeout = false;

        private NoiseClient() { }
        public NoiseClient(string endpoint, IOutput output, PeerConfiguration peerConfiguration, NoiseClientConfiguration noiseClientConfiguration)
        {
            _output = output ??
                throw new ArgumentNullException(nameof(output));

            _peerConfiguration = peerConfiguration ??
                throw new ArgumentNullException(nameof(peerConfiguration));

            _noiseClientConfiguration = noiseClientConfiguration ??
                throw new ArgumentNullException(nameof(noiseClientConfiguration));

            if (!IPAddress.TryParse(endpoint, out var parsedEndpoint))
                throw new ArgumentException("Invalid endpoint provided.", nameof(endpoint));

            _ipAddress = Dns.GetHostEntry(parsedEndpoint).AddressList.First();
            _peerIp = _ipAddress.ToString();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            throw new NotImplementedException();
        }

        private void Connect()
        {
            if (_isConnected)
            {
                _output.WriteLog("The noise client is already connected to a peer.");
                return;
            }

            _output.WriteLog("Initializing the noise client.");
            _tcpClient = new TcpClient();
            _output.WriteLog("Noise client initialized.");

            _tokenSource = new CancellationTokenSource();
            
            _token = _tokenSource.Token;
            _token.Register(() =>
            {
                if (_networkStream is not null)
                    _networkStream.Close();
            });

            using var connectTokenSource = new CancellationTokenSource();
            var connectToken = connectTokenSource.Token;

            var cancelTask = Task.Delay(_noiseClientConfiguration.ConnectTimeoutMs, _token);
            var connectTask = Task.Run(() =>
            {
                int retryCount = 1;

                while (true)
                {
                    try
                    {
                        _output.WriteLog($"Attempting connection to {_peerIp}. Tries: {retryCount}.");

                        _tcpClient.Dispose();
                        _tcpClient = new TcpClient();
                        _tcpClient.ConnectAsync(_peerIp, Constants.ProtocolPort).Wait(1000, connectToken);

                        if (_tcpClient.Connected)
                        {
                            _output.WriteLog($"Connected successful to {_peerIp}.");
                            break;
                        }

                        if (retryCount > _noiseClientConfiguration.MaxConnectRetryCount) break;
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception _)
                    {
                        _output.WriteLog($"Connection to {_peerIp} failed.");
                    }
                    finally
                    {
                        retryCount++;
                    }
                }
            }, connectToken);

            Task.WhenAny(cancelTask, connectTask).Wait();

            if (cancelTask.IsCompleted)
            {
                connectTokenSource.Cancel();
                _tcpClient.Close();
                throw new TimeoutException($"Timeout connecting to {_peerIp}.");
            }

            try
            {
                _networkStream = _tcpClient.GetStream();
                _networkStream.ReadTimeout = _noiseClientConfiguration.ReadTimeoutMs;
            }
            catch (Exception _)
            {
                throw;
            }

            _isConnected = true;
            _lastActivity = DateTime.Now;
            _isTimeout = false;
        }

        private void Disconnect()
        {
            if (!_isConnected)
            {
                _output.WriteLog("The noise client has already disconnected from the peer.");
                return;
            }

            _output.WriteLog($"Disconnecting from the peer ${_peerIp}");

            _tokenSource.Cancel();
            _tcpClient.Close();
            _isConnected = false;
        }

        private async Task SendAsync(byte[] data, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }
    }
}
