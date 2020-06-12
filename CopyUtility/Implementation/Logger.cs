using CopyUtility.Interfaces;
using System;
using System.IO;

namespace CopyUtility.Implementation
{
    public class Logger : ILogger
    {
        string _logFilePath;
        public Logger(string logPath)
        {
            _logFilePath = logPath ?? throw new ArgumentNullException(nameof(logPath));

        }

        public void Log(string message)
        {
            if (Directory.Exists(Path.GetDirectoryName(_logFilePath)))
                File.AppendAllText(_logFilePath, $"\n{DateTime.Now} {message}");
        }
    }
}
