using System;

namespace TwitchChatConnect.HLAPI
{
    public class TwitchCommandLogger
    {
        public Action<string> InfoHandler;
        public Action<string> ErrorHandler;
        public Action<string> WarningHandler;
        public Action<Exception> ExceptionHandler;

        internal void Info(string message) => InfoHandler?.Invoke(message);

        internal void Error(string message) => ErrorHandler?.Invoke(message);

        internal void Warning(string message) => WarningHandler?.Invoke(message);

        internal void Exception(Exception exception) => ExceptionHandler?.Invoke(exception);
    }
}