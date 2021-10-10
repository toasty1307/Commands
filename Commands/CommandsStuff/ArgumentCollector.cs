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

        public T Get<T>(string key) => this[key] is T t ? t : default;

        public bool Get<T>(string key, out T t)
        {
            var flag = this[key] is T;
            t = this[key] is T t1 ? t1 : default;
            return flag;
        }
    }
}