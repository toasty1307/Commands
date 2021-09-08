using System;

namespace Commands.Utils
{
    public class FriendlyException : Exception
    {
        public FriendlyException(string message) : base(message) { }
    }
}