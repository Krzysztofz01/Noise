using Noise.Core.Abstraction;
using Noise.Core.Exceptions;
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
        private readonly IOutputMonitor _outputMonitor;
        private readonly PeerConfiguration _peerConfiguration;
        private readonly NoiseClientConfiguration _noiseClientConfiguration;

        private readonly IPacketHandlingService _packetHandlingService;

        private readonly string _peerIp = null;
        private TcpClient _tcpClient = null;
        private NetworkStream _networkStream = null;

        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
        private bool _isConnected = false;

        private CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private CancellationToken _token;

        private NoiseClient() { }
        public NoiseClient(string endpoint, IOutputMonitor outputMonitor, PeerConfiguration peerConfiguration, NoiseClientConfiguration noiseClientConfiguration = null)
        {
            _outputMonitor = outputMonitor ??
                throw new ArgumentNullException(nameof(outputMonitor));

            _peerConfiguration = peerConfiguration ??
                throw new ArgumentNullException(nameof(peerConfiguration));

            _noiseClientConfiguration = noiseClientConfiguration ??
                new NoiseClientConfiguration();

            if (!IPAddress.TryParse(endpoint, out var parsedEndpoint))
                throw new ArgumentException("Invalid endpoint provided.", nameof(endpoint));

            _peerIp = parsedEndpoint.ToString();

            _packetHandlingService = new PacketHandlingService();
        }

        public async Task SendMessage(string receiverPublicKey, string message, CancellationToken cancellationToken = default)
        {
            try
            {
                var signature = _peerConfiguration.GetSendingSignatureForPeer(receiverPublicKey);

                //TODO: Debug purpopses, remove before merge
                _outputMonitor.LogInformation($"Signature to sign message: {signature}");
                
                if (signature is null)
                {
                    _outputMonitor.LogWarning("The target peer has not provided any certification. Unable to send the message.");
                    return;
                }

                var (keyPacket, messagePacket) = _packetHandlingService.CreateMessagePackets(signature, receiverPublicKey, message);

                var bufferStream = PacketBufferStreamBuilder
                    .Create()
                    .InsertPacket(keyPacket)
                    .InsertPacket(messagePacket)
                    .Build();

                _outputMonitor.WriteOutgoingMessage(message);

                await HandleTransaction(bufferStream, cancellationToken);
            }
            catch (PeerDataException ex)
            {
                LogVerbose(ex.Message);
            }
            catch (Exception ex)
            {
                _outputMonitor.LogError(ex);
            }
        }

        public async Task SendSignature(string receiverPublicKey, CancellationToken cancellationToken = default)
        {
            try
            {
                var certification = _peerConfiguration.Preferences.IndependentMediumCertification ?? null;

                var (keyPacket, signaturePacket, receiverIdentityProve) = _packetHandlingService.CreateSignaturePacket(
                    receiverPublicKey,
                    _peerConfiguration.Secrets.PublicKey,
                    _peerConfiguration.Secrets.PrivateKey,
                    certification);

                var bufferStream = PacketBufferStreamBuilder
                    .Create()
                    .InsertPacket(keyPacket)
                    .InsertPacket(signaturePacket)
                    .Build();

                _outputMonitor.WriteOutgoingSignature(receiverPublicKey);

                await HandleTransaction(bufferStream, cancellationToken);

                _peerConfiguration.SetSendingSignatureForPeer(receiverPublicKey, receiverIdentityProve);

                //TODO: Debug purpopses, remove before merge
                _outputMonitor.LogInformation($"Currently sent signature: {receiverIdentityProve}");
            }
            catch (PeerDataException ex)
            {
                LogVerbose(ex.Message);
            }
            catch (Exception ex)
            {
                _outputMonitor.LogError(ex);
            }
        }

        public async Task SendDiscovery(string receiverPublicKey, CancellationToken cancellationToken = default)
        {
            try
            {
                var signature = _peerConfiguration.GetSendingSignatureForPeer(receiverPublicKey);
                if (signature is null)
                {
                    LogVerbose("The target peer has not provided any certification. Unable to send the discovery packet. Peer skipped.");
                    return;
                }

                var endpoints = _peerConfiguration.GetEndpoints(false).Select(e => e.Endpoint);

                var publicKeys = _peerConfiguration.Preferences.SharePublicKeysViaDiscovery ?
                    _peerConfiguration.GetPeers().Select(p => p.PublicKey) :
                    Array.Empty<string>();

                var (keyPacket, discoveryPacket) = _packetHandlingService.CreateDiscoveryPackets(signature, receiverPublicKey, endpoints, publicKeys);

                var bufferStream = PacketBufferStreamBuilder
                    .Create()
                    .InsertPacket(keyPacket)
                    .InsertPacket(discoveryPacket)
                    .Build();

                await HandleTransaction(bufferStream, cancellationToken);

                _outputMonitor.WriteOutgoinDiscovery(_peerIp);
            }
            catch (PeerDataException ex)
            {
                LogVerbose(ex.Message);
            }
            catch (Exception ex)
            {
                _outputMonitor.LogError(ex);
            }
        }

        public async Task SendPing(CancellationToken cancellationToken = default)
        {
            try
            {
                var pingPacket = _packetHandlingService.CreatePingPacket();

                var pingPacketBuffer = pingPacket.GetBytes();

                _outputMonitor.WriteOutgoingPing(_peerIp);

                await HandleTransaction(pingPacketBuffer, cancellationToken);
            }
            catch (PeerDataException ex)
            {
                LogVerbose(ex.Message);
            }
            catch (Exception ex)
            {
                _outputMonitor.LogError(ex);
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

        private async Task HandleTransaction(byte[] dataBuffer, CancellationToken token = default)
        {
            try
            {
                Connect();

                await SendAsync(dataBuffer, token);

                LogVerbose("Transaction successful.");
            }
            catch (TimeoutException ex)
            {
                LogVerbose("Transaction failed.");
                LogVerbose(ex.Message);
            }
            catch (Exception)
            {
                LogVerbose("Transaction failed.");
                throw;
            }
            finally
            {
                Disconnect();
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

                if (_peerConfiguration.Preferences.UseEndpointAttemptFilter && _peerConfiguration.Preferences.TreatConnectionTimeoutAsOffline)
                {
                    LogVerbose($"Endpoint: {_peerIp} will be marked as a disconnected endpoint.");
                    _peerConfiguration.SetEndpointAsDisconnected(_peerIp);
                }

                throw new TimeoutException($"Timeout connecting to {_peerIp}.");
            }

            if (!_tcpClient.Connected)
            {
                if (_peerConfiguration.Preferences.UseEndpointAttemptFilter)
                {
                    LogVerbose($"Endpoint: {_peerIp} will be marked as a disconnected endpoint.");
                    _peerConfiguration.SetEndpointAsDisconnected(_peerIp);
                }

                throw new TimeoutException($"Endpoint {_peerIp} is offline.");
            }

            try
            {
                _networkStream = _tcpClient.GetStream();
                _networkStream.ReadTimeout = _noiseClientConfiguration.ReadTimeoutMs;
            }
            catch (Exception ex)
            {
                LogVerbose($"Unexpected network stream error. {ex.Message}");
                throw;
            }

            _isConnected = true;
        }

        private void Disconnect()
        {
            if (!_isConnected)
            {
                LogVerbose("The noise client has already disconnected from the peer.");
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
