using System;

namespace Noise.Core.Abstraction
{
    public interface IOutputMonitor
    {
        void LogError(string message);
        void LogError(Exception exception);
        void LogError(string message, Exception exception);

        void LogWarning(string message);
        void LogWarning(Exception exception);
        void LogWarning(string message, Exception exception);

        void LogInformation(string message);

        void WriteIncomingMessage(string senderPublicKey, string senderAlias, string senderEndpoint, string message);
        void WriteOutgoingMessage(string message);
        void WriteIncomingSignature(string senderPublicKey, string senderEndpoint);
        void WriteOutgoingSignature(string receiverPublicKey);
        void WriteIncomingPing(string senderEndpoint);
        void WriteOutgoingPing(string receiverEndpoint);
        void WriteIncomingDiscovery(string senderPublicKey, string senderAlias, string senderEndpoint);
        void WriteOutgoinDiscovery(string receiverEndpoint);

        void WriteRaw(string content, bool newLine = true);

        void Clear();
    }
}
