using Microsoft.Extensions.Logging;

namespace Noise.Core.Abstraction
{
    public interface IOutputMonitor<TContext> : ILogger<TContext> where TContext : class
    {
        void WriteMessage(string senderPublicKey, string senderAlias, string senderEndpoint, string message);
        void WritePing(string senderEndpoint);
    }
}
