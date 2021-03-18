using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Web;

namespace HPBot.Application
{
    public class TelegramLogProvider : ILoggerProvider
    {
        public const string NotifierLogCategoryName = "Notifier";

        public class TelegramLogger : ILogger
        {
            private readonly HttpClient httpClient = new HttpClient();
            private readonly string categoryName;
            private readonly string apiKey;
            private readonly string chatId;

            public TelegramLogger(string categoryName)
            {
                this.categoryName = categoryName;
                
                apiKey = Environment.GetEnvironmentVariable("TELEGRAM_APIKEY");

                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new Exception("Missing telegram apiKey");
                }

                chatId = Environment.GetEnvironmentVariable("TELEGRAM_CHATID");

                if (string.IsNullOrWhiteSpace(chatId))
                {
                    throw new Exception("Missing telegram chatId");
                }
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                throw new NotImplementedException();
            }

            public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (categoryName.StartsWith($"{NotifierLogCategoryName}."))
                {
                    string text = $"{categoryName}: {state}";

                    httpClient.GetAsync(
                        $"https://api.telegram.org/bot{apiKey}/" +
                        $"sendMessage?chat_id={chatId}&text={HttpUtility.UrlEncode(text)}");

                }
            }
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new TelegramLogger(categoryName);
        }

        public void Dispose()
        {
            
        }
    }
}