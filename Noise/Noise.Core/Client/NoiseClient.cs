using Noise.Core.Abstraction;
using Noise.Core.Peer;
using Noise.Core.Protocol;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Noise.Core.Client
{
    public class NoiseClient : INoiseClient
    {
        private readonly IOutputMonitor<NoiseClient> _outputMonitor;
        private readonly PeerConfiguration _peerConfiguration;
        private readonly NoiseClientConfiguration _noiseClientConfiguration;

        private readonly string _peerIp = null;
        private readonly IPAddress _ipAddress = null;
        private TcpClient _tcpClient = null;
        private NetworkStream _networkStream = null;

        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
        private bool _isConnected = false;

        private CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private CancellationToken _token;

        private NoiseClient() { }
        public NoiseClient(string endpoint, IOutputMonitor<NoiseClient> outputMonitor, PeerConfiguration peerConfiguration, NoiseClientConfiguration noiseClientConfiguration)
        {
            _outputMonitor = outputMonitor ??
                throw new ArgumentNullException(nameof(outputMonitor));

            _peerConfiguration = peerConfiguration ??
                throw new ArgumentNullException(nameof(peerConfiguration));

            _noiseClientConfiguration = noiseClientConfiguration ??
                throw new ArgumentNullException(nameof(noiseClientConfiguration));

            if (!IPAddress.TryParse(endpoint, out var parsedEndpoint))
                throw new ArgumentException("Invalid endpoint provided.", nameof(endpoint));

            _ipAddress = Dns.GetHostEntry(parsedEndpoint).AddressList.First();
            _peerIp = _ipAddress.ToString();
        }

        public async Task SendMessage(string receiverPublicKey, string message, CancellationToken cancellationToken = default)
        {
            try
            {
                var packetHandlingService = new PacketHandlingService();

                var signature = _peerConfiguration.GetPeerByPublicKey(receiverPublicKey).SendingSignature;

                var (keyPacket, messagePacket) = packetHandlingService.CreateMessagePackets(signature, receiverPublicKey, message);

                var bufferStream = PacketBufferStreamBuilder
                    .Create()
                    .InsertPacket(keyPacket)
                    .InsertPacket(messagePacket)
                    .Build();

                _outputMonitor.WriteOutgoingMessage(message);

                Connect();
                await SendAsync(bufferStream, cancellationToken);
            }
            catch (Exception ex)
            {
                _outputMonitor.LogError(ex);
            }
            finally
            {
                Disconnect();
            }
        }

        public async Task SendSignature(string receiverPublicKey, CancellationToken cancellationToken = default)
        {
            try
            {
                var packetHandlingService = new PacketHandlingService();

                var certification = _peerConfiguration.IndependentMediumCertification ?? null;

                var (keyPacket, signaturePacket, receiverIdentityProve) = packetHandlingService.CreateSignaturePacket(
                    receiverPublicKey,
                    _peerConfiguration.PublicKey,
                    _peerConfiguration.PrivateKey,
                    certification);

                _peerConfiguration.GetPeerByPublicKey(receiverPublicKey).SetSendingSignature(receiverIdentityProve);

                var bufferStream = PacketBufferStreamBuilder
                    .Create()
                    .InsertPacket(keyPacket)
                    .InsertPacket(signaturePacket)
                    .Build();

                _outputMonitor.WriteOutgoingSignature(receiverPublicKey);

                Connect();
                await SendAsync(bufferStream, cancellationToken);
            }
            catch (Exception ex)
            {
                _outputMonitor.LogError(ex);
            }
            finally
            {
                Disconnect();
            }
        }

        public Task SendDiscovery(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task SendPing(CancellationToken cancellationToken = default)
        {
            try
            {
                var packetHandlingService = new PacketHandlingService();

                var pingPacket = packetHandlingService.CreatePingPacket();

                var pingPacketBuffer = pingPacket.GetBytes();

                _outputMonitor.WriteOutgoingPing(_peerIp);

                Connect();
                await SendAsync(pingPacketBuffer, cancellationToken);
            }
            catch (Exception ex)
            {
                _outputMonitor.LogError(ex);
            }
            finally
            {
                Disconnect();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                LogVerbose("Disposing the noise client.");

                try
                {
                    _isConnected = false;

                    if (_tokenSource is not null)
                    {
                        if (!_tokenSource.IsCancellationRequested)
                        {
                            _tokenSource.Cancel();
                            _tokenSource.Dispose();
                        }
                    }

                    if (_networkStream is not null)
                    {
                        _networkStream.Close();
                        _networkStream.Dispose();
                    }

                    if (_tcpClient is not null)
                    {
                        _tcpClient.Close();
                        _tcpClient.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    _outputMonitor.LogError("Some errors occured while disposing the client.", ex);
                }

                LogVerbose("Noise client disposed.");
            }
        }

        private void Connect()
        {
            if (_isConnected)
            {
                LogVerbose("The noise client is already connected to a peer.");
                return;
            }

            LogVerbose("Initializing the noise client.");
            _tcpClient = new TcpClient();
            LogVerbose("Noise client initialized.");

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
                        LogVerbose($"Attempting connection to {_peerIp}. Tries: {retryCount}.");

                        _tcpClient.Dispose();
                        _tcpClient = new TcpClient();
                        _tcpClient.ConnectAsync(_peerIp, Constants.ProtocolPort).Wait(1000, connectToken);

                        if (_tcpClient.Connected)
                        {
                            LogVerbose($"Connected successful to {_peerIp}.");
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
                    catch (Exception)
                    {
                        LogVerbose($"Connection to {_peerIp} failed.");
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
            catch (Exception)
            {
                throw;
            }

            _isConnected = true;
        }

        private void Disconnect()
        {
            if (!_isConnected)
            {
                LogVerbose("The noise client has already disconnected from the peer.");
                return;
            }

            LogVerbose($"Disconnecting from the peer ${_peerIp}");

            _tokenSource.Cancel();
            _tcpClient.Close();
            _isConnected = false;
        }

        private async Task SendAsync(byte[] dataBuffer, CancellationToken token = default)
        {
            if (dataBuffer is null || dataBuffer.Length < Constants.PacketBaseSize)
                throw new ArgumentException("Invalid sending buffer data.", nameof(dataBuffer));

            if (!_isConnected)
                throw new IOException("Connection with a peer is not established.");

            if (token == default) token = _token;

            using var memoryStream = new MemoryStream();
            await memoryStream.WriteAsync(dataBuffer, 0, dataBuffer.Length, token).ConfigureAwait(false);
            memoryStream.Seek(0, SeekOrigin.Begin);
            
            try
            {
                long bytesRemaining = dataBuffer.Length;
                int bytesRead = 0;
                byte[] sendingBuffer = new byte[_noiseClientConfiguration.StreamBufferSize];

                await _sendLock.WaitAsync(token).ConfigureAwait(false);

                while (bytesRemaining > 0)
                {
                    bytesRead = await memoryStream.ReadAsync(sendingBuffer, 0, sendingBuffer.Length, token).ConfigureAwait(false);
                    if (bytesRead > 0)
                    {
                        await _networkStream.WriteAsync(sendingBuffer, 0, bytesRead, token).ConfigureAwait(false);

                        bytesRemaining -= bytesRead;
                    }
                }

                await _networkStream.FlushAsync(token).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                LogVerbose("Sending task canceled.");
            }
            catch (OperationCanceledException)
            {
                LogVerbose("Sending operation canceled.");
            }
            catch (Exception)
            {
                LogVerbose("Unexpected sending failure.");
                throw;
            }
            finally
            {
                _sendLock.Release();
            }
        }

        private void LogVerbose(string message)
        {
            if (_noiseClientConfiguration.VerboseMode)
                _outputMonitor.LogInformation(message);
        }
    }
}
