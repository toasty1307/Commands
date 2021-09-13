using System;
using DSharpPlus;

namespace Commands.Types
{
    public class ArgumentType : CommandsBase
    {
        public ArgumentType(DiscordClient client) : base(client)
        {
        }
    }
    
    public abstract class ArgumentType<T> : ArgumentType
    {
        protected ArgumentType(DiscordClient client) : base(client)
        {
        }

        public abstract bool Validate(string argString);
        public abstract T Parse(string argString);
        public abstract bool IsEmpty(T arg);
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class ArgumentTypeAttribute : Attribute { }
}