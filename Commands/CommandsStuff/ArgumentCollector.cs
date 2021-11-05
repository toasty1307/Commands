using System.Collections.Generic;

namespace Commands.CommandsStuff
{
    public class ArgumentCollector
    {
        public Dictionary<string, object> Args { get; } = new();

        public object this[string key]
        {
            get => Args.TryGetValue(key, out var arg) ? arg : null;
            set { if (!Args.TryAdd(key, value)) Args[key] = value; }
        }

        public T Get<T>(string key) => this[key] is T t ? t : default;

        public bool Get<T>(string key, out T t)
        {
            var flag = this[key] is T;
            t = this[key] is T t1 ? t1 : default;
            return flag;
        }
    }
}