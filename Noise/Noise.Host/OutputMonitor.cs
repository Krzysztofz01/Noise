using Noise.Core.Abstraction;
using Noise.Core.Extensions;
using System;

namespace Noise.Host
{
    internal class OutputMonitor : IOutputMonitor
    {
        private const int _publicKeyStripLength = 10;
        private const ConsoleColor _errorColor = ConsoleColor.Red;
        private const ConsoleColor _warningColor = ConsoleColor.Yellow;
        private const ConsoleColor _informationColor = ConsoleColor.Green;
        private const ConsoleColor _messageColor = ConsoleColor.Green;
        private const ConsoleColor _pingColor = ConsoleColor.DarkGreen;
        private const ConsoleColor _signatureColor = ConsoleColor.DarkGreen;

        public void LogError(string message)
        {
            Console.ForegroundColor = _errorColor;
            Console.WriteLine("[e] {0}",
                message.Trim());
            Console.ResetColor();
        }

        public void LogError(Exception exception)
        {
            Console.ForegroundColor = _errorColor;
            Console.WriteLine("[e] {0}{1}{2}",
                exception.Message.Trim(),
                Environment.NewLine,
                exception.StackTrace.ToString().Trim());
            Console.ResetColor();
        }

        public void LogError(string message, Exception exception)
        {
            Console.ForegroundColor = _errorColor;
            Console.WriteLine("[e] {0}{1}{2}",
                message.Trim(),
                Environment.NewLine,
                exception.StackTrace.ToString().Trim());
            Console.ResetColor();
        }

        public void LogInformation(string message)
        {
            Console.ForegroundColor = _informationColor;
            Console.WriteLine("[i] {0}",
                message.Trim());
            Console.ResetColor();
        }

        public void LogWarning(string message)
        {
            Console.ForegroundColor = _warningColor;
            Console.WriteLine("[e] {0}",
                message.Trim());
            Console.ResetColor();
        }

        public void LogWarning(Exception exception)
        {
            Console.ForegroundColor = _warningColor;
            Console.WriteLine("[e] {0}{1}{2}",
                exception.Message.Trim(),
                Environment.NewLine,
                exception.StackTrace.ToString().Trim());
            Console.ResetColor();
        }

        public void LogWarning(string message, Exception exception)
        {
            Console.ForegroundColor = _warningColor;
            Console.WriteLine("[e] {0}{1}{2}",
                message.Trim(),
                Environment.NewLine,
                exception.StackTrace.ToString().Trim());
            Console.ResetColor();
        }

        public void WriteIncomingMessage(string senderPublicKey, string senderAlias, string senderEndpoint, string message)
        {
            string displayName = senderAlias.IsEmpty() || senderAlias == "Anonymous"
                ? senderPublicKey[.._publicKeyStripLength]
                : senderAlias;

            Console.ForegroundColor = _messageColor;
            Console.WriteLine("(@{0}): {1}//{2}",
                displayName,
                message,
                senderEndpoint);
            Console.ResetColor();
        }

        public void WriteIncomingPing(string receiverEndpoint)
        {
            Console.ForegroundColor = _pingColor;
            Console.WriteLine("(Ping!@): //{0}",
                receiverEndpoint);
            Console.ResetColor();
        }

        public void WriteIncomingSignature(string senderPublicKey, string senderEndpoint)
        {
            Console.ForegroundColor = _signatureColor;
            Console.WriteLine("(Signature!@{0}): Signature received!//{1}",
                senderPublicKey[.._publicKeyStripLength],
                senderEndpoint);
            Console.ResetColor();
        }

        public void WriteOutgoingMessage(string message)
        {
            Console.ForegroundColor = _messageColor;
            Console.WriteLine("(@You): {1}//127.0.0.1",
                message);
            Console.ResetColor();
        }

        public void WriteOutgoingPing(string senderEndpoint)
        {
            Console.ForegroundColor = _pingColor;
            Console.WriteLine("(Ping!@): Pinged: {0}//127.0.0.1",
                senderEndpoint);
            Console.ResetColor();
        }

        public void WriteOutgoingSignature(string receiverPublicKey)
        {
            Console.ForegroundColor = _signatureColor;
            Console.WriteLine("(Signature!@You): Signature for {0} has been sent.//127.0.0.1",
                receiverPublicKey[.._publicKeyStripLength]);
            Console.ResetColor();
        }

        public void WriteRaw(string content, bool newLine = true)
        {
            if (newLine)
            {
                Console.WriteLine(content);
                return;
            }
            Console.Write(content);
        }

        public void WriteRaw(string content, ConsoleColor consoleColor, bool newLine = true)
        {
            Console.ForegroundColor = consoleColor;
            WriteRaw(content, newLine);
            Console.ResetColor();
        }

        public void Clear()
        {
            Console.Clear();
        }
    }
}
