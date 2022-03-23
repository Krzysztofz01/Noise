using System;

namespace Noise.Host.Exceptions
{
    public class CommandHandlerException : Exception
    {
        private const string _defaultMessage = "Invalid command or operation. Type HELP to get further information.";

        private CommandHandlerException() { }
        public CommandHandlerException(string message = null) : base(message ?? _defaultMessage) { }

    }
}
