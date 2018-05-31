using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace WorkPanel.Services
{
    public class FileLoggerService : ILogger
    {
        private string filePath;
        private object _lock = new object();

        public FileLoggerService(string path)
        {
            filePath = path;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (formatter != null)
            {
                lock (_lock)
                {
                    File.AppendAllText(filePath, formatter(state, exception) + Environment.NewLine);
                }
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}