using Microsoft.Extensions.Logging;

namespace HPBot.Application
{
    public static class LoggerFactoryExtensions
    {
        public static ILogger CreateNotifier<T>(this ILoggerFactory loggerFactory)
        {
            return loggerFactory.CreateLogger($"{TelegramLogProvider.NotifierLogCategoryName}.{typeof(T).FullName}");
        }
    }
}
