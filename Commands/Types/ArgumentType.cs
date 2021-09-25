using System;
using DSharpPlus;

namespace Commands.Types
{
    public abstract class ArgumentType : CommandsBase
    {
        protected ArgumentType(DiscordClient client) : base(client) { }
        public abstract bool Validate(string argString);
    }
    
    public abstract class ArgumentType<T> : ArgumentType
    {
        protected ArgumentType(DiscordClient client) : base(client) { }
        public abstract T Parse(string argString);
        public abstract bool IsEmpty(T arg);
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class ArgumentTypeAttribute : Attribute { }
}