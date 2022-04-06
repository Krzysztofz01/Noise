using Noise.Core.Abstraction;
using Noise.Core.Peer;
using System;
using Xunit;

namespace Noise.Core.Test
{
    [Collection("Sequential")]
    public class IntegrationTestsUtility
    {
        public static int Timeout => 8000;

        public static PeerConfiguration MockupPeerConfiguration() =>
            PeerConfiguration.Factory.Initialize(Guid.NewGuid().ToString());

        public static IOutputMonitor MockupOutputMonitor() => new MockupOutputMonitor();
    }

    internal class MockupOutputMonitor : IOutputMonitor
    {
        void IOutputMonitor.Clear() { }
        void IOutputMonitor.LogError(string message) { }
        void IOutputMonitor.LogError(Exception exception) { }
        void IOutputMonitor.LogError(string message, Exception exception) { }
        void IOutputMonitor.LogInformation(string message) { }
        void IOutputMonitor.LogWarning(string message) { }
        void IOutputMonitor.LogWarning(Exception exception) { }
        void IOutputMonitor.LogWarning(string message, Exception exception) { }
        void IOutputMonitor.WriteIncomingMessage(string senderPublicKey, string senderAlias, string senderEndpoint, string message) { }
        void IOutputMonitor.WriteIncomingPing(string receiverEndpoint) { }
        void IOutputMonitor.WriteIncomingSignature(string senderPublicKey, string senderEndpoint) { }
        void IOutputMonitor.WriteOutgoingMessage(string message) { }
        void IOutputMonitor.WriteOutgoingPing(string senderEndpoint) { }
        void IOutputMonitor.WriteOutgoingSignature(string receiverPublicKey) { }
        void IOutputMonitor.WriteRaw(string content, bool newLine) { }
    }
}
