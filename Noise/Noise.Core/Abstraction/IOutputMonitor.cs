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

        void WriteMessage(string senderPublicKey, string senderAlias, string senderEndpoint, string message);
        void WritePing(string senderEndpoint);
    }
}
