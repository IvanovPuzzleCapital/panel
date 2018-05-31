using Microsoft.Extensions.Logging;
using WorkPanel.Services;

namespace WorkPanel.Providers
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private string path;

        public FileLoggerProvider(string _path)
        {
            path = _path;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLoggerService(path);
        }

        public void Dispose()
        {
        }
    }
}
