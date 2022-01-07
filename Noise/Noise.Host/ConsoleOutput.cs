using Noise.Core.Abstraction;
using Noise.Core.Extensions;
using System;

namespace Noise.Host
{
    public class ConsoleOutput : IOutput
    {
        public void WriteException(string message, Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[Error] {0} - {1}", message, ex.Message);
            Console.ResetColor();
        }

        public void WriteLog(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("[Log] {0}", message);
            Console.ResetColor();
        }

        public void WriteMessage(string senderPublicKey, string message, string senderIpAddress, string senderAlias)
        {
            string displayName = (senderAlias.IsEmpty()) ? senderPublicKey.Substring(0, 9) : senderAlias;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("(@{0}): {1}//{2}", displayName, message, senderIpAddress);
            Console.ResetColor();
        }

        public void WritePing(string senderIpAddress)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("[Ping] {0}", senderIpAddress);
            Console.ResetColor();
        }
    }
}
