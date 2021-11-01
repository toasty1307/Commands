using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Commands.Utils
{
    public class Logger : ILogger
    {
        private readonly ConsoleTheme _theme;
        private readonly string _name;

        public Logger(string name, ConsoleTheme theme)
        {
            _name = name;
            _theme = theme;
        }

        public IDisposable BeginScope<TState>(TState state) => default;

        public bool IsEnabled(LogLevel logLevel) =>
            _theme.LogLevels.ContainsKey(logLevel);

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            if (_theme.EventId != 0 && _theme.EventId != eventId.Id) return;
            var originalColor = Console.ForegroundColor;
            var stackTrace = new StackTrace();
            var frame = stackTrace.GetFrames()[5];
            var method = frame.GetMethod();
            var fullName = method!.DeclaringType!.FullName;
            var source = fullName!.Contains('+') ? fullName![fullName.LastIndexOf(".", StringComparison.Ordinal)..fullName.IndexOf("+", StringComparison.Ordinal)] : method.DeclaringType.Name;
            if (source.Replace(".", "") == "LoggerExtensions")
            {
                frame = stackTrace.GetFrames()[6];
                method = frame.GetMethod();
                fullName = method!.DeclaringType!.FullName;
                source = fullName!.Contains('+') ? fullName![fullName.LastIndexOf(".", StringComparison.Ordinal)..fullName.IndexOf("+", StringComparison.Ordinal)] : method.DeclaringType.Name; 
            }
            var methodName = method.Name == "MoveNext" ? method.DeclaringType.Name[2..^1] : method.Name;
            methodName = methodName == ".ctor" ? fullName + "_ctor" : methodName;
            methodName = methodName.Contains('>') ? "(some local method somewhere idk)" : methodName; 
            Console.ForegroundColor = _theme.LogLevels[logLevel];
            Console.WriteLine($"[{string.Join(" : ", _name, eventId.Id, string.IsNullOrEmpty(eventId.Name) ? methodName : eventId.Name, logLevel, DateTime.Now.ToString("hh:mm:ss t z"))}] {(string.IsNullOrEmpty(source) ? "" : $"[{source.Replace(".", "")}] ")}{formatter(state, exception)}");
            Console.ForegroundColor = originalColor;
        }
    }

    public class LoggerFactory : ILoggerFactory
    {
        public void Dispose() { }

        public ILogger CreateLogger(string categoryName) => new Logger(categoryName, new ConsoleTheme());

        public void AddProvider(ILoggerProvider provider) { }
    }
    
    public class ConsoleTheme
    {
        public int EventId { get; set; }
        
        public Dictionary<LogLevel, ConsoleColor> LogLevels { get; } = new()
        {
            [LogLevel.Information] = ConsoleColor.Green,
            [LogLevel.Critical] = ConsoleColor.DarkRed,
            [LogLevel.Debug] = ConsoleColor.Blue,
            [LogLevel.Error] = ConsoleColor.Red,
            [LogLevel.Warning] = ConsoleColor.Yellow,
        };
    }
}