using System;
using System.Runtime.Serialization;

namespace Noise.Host.Exceptions
{
    public class CommandHandlerException : Exception
    {
        public CommandHandlerException()
        {
        }

        public CommandHandlerException(string message) : base(message)
        {
        }

        public CommandHandlerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CommandHandlerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
