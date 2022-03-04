using System;

namespace Noise.Core.Abstraction
{
    public interface IOutput
    {
        [Obsolete]
        void WriteMessage(string senderPublicKey, string message, string senderIpAddress, string senderAlias);
        [Obsolete]
        void WritePing(string senderIpAddress);
        [Obsolete]
        void WriteException(string message, Exception ex);
        [Obsolete]
        void WriteLog(string message);
        [Obsolete]
        void WriteRaw(string value, bool newLine = true);
    }
}
