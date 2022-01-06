using System;

namespace Noise.Core.Abstraction
{
    public interface IOutput
    {
        void WriteMessage(string senderPublicKey, string message, string senderIpAddress, string senderAlias);
        void WritePing(string senderPublicKey, string senderIpAddress);
        void WriteException(string message, Exception ex);
        void WriteLog(string message);
    }
}
