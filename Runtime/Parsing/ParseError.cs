using System;

namespace Runtime.Parsing
{
    public class ParseErrorException : Exception
    {
        public ParseErrorException()
        {
        }

        public ParseErrorException(string message)
            : base(message)
        {
        }

        public ParseErrorException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}