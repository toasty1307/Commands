
using System;

namespace Commands.Types
{
    public class ArgumentType : CommandsExtensionBase
    {
    }
    
    public abstract class ArgumentType<T> : ArgumentType
    {
        public abstract bool Validate(string argString);
        public abstract T Parse(string argString);
        public abstract bool IsEmpty(T arg);
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class ArgumentTypeAttribute : Attribute { }
}