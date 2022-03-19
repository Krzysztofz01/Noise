using System;

namespace Noise.Core.Abstraction
{
    public interface IOutputMonitor<TContext> where TContext : class
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
        void WriteIncomingSignature(string senderPublicKey);
        void WriteOutgoingSignature(string receiverPublicKey);
        void WriteIncomingPing(string receiverEndpoint);
        void WriteOutgoingPing(string senderEndpoint);
    }
}
