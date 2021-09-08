using System.Collections.Generic;

namespace Commands.CommandsStuff
{
    public class ArgumentCollector
    {
        public Dictionary<string, object> Args { get; set; } = new();

        public object this[string key]
        {
            get => Args.ContainsKey(key) ? Args[key] : null;
            set { if (Args.ContainsKey(key)) Args[key] = value; else Args.Add(key, value); }
        }

        public T Get<T>(string key)  => (T)this[key];
    }
}