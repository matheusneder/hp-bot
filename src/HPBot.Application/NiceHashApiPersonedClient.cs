using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace HPBot.Application
{
    public class NiceHashApiPersonedClient : NiceHashApiClient
    {
        private readonly ILogger logger;

        public NiceHashApiPersonedClient(HttpClient httpClient, NiceHashConfiguration configuration, ILoggerFactory loggerFactory) : 
            base(httpClient, configuration, loggerFactory)
        {
            logger = loggerFactory?.CreateLogger<NiceHashApiPersonedClient>() ??
                throw new ArgumentNullException(nameof(loggerFactory));
        }

        public override void ConfigureRequestMessage(HttpRequestMessage message, string requestId, HttpMethod method,
            string path, string queryString, string nonce, string time, string bodyText)
        {
            message.Headers.Add("X-Time", time);
            message.Headers.Add("X-Nonce", nonce);
            message.Headers.Add("X-Organization-Id", Configuration.OrganizationId);
            message.Headers.Add("X-Request-Id", requestId);
            string auth = CreateAuth(method, path, queryString, nonce, time, bodyText);
            message.Headers.Add("X-Auth", auth);

            logger.LogDebug("Configured message headers {RequestId}; " +
                "X-Time: '{Time}' :: " +
                "X-Nonce: '{Nonce}' :: " +
                "X-Organization-Id: '{OrganizationId}' :: " +
                "X-Request-Id: '{RequestId}' :: " +
                "X-Auth: '{Auth}' :: ",
                requestId, time, nonce, Configuration.OrganizationId, requestId, auth);
        }

        private string CreateAuth(HttpMethod method, string path, string queryString, string nonce, string time, string bodyText)
        {
            Encoding iso = Encoding.GetEncoding("ISO-8859-1");

            var authInput = ImmutableList.Create<byte>()
                .AddRange(iso.GetBytes(Configuration.ApiKey))
                .Add(0)
                .AddRange(iso.GetBytes(time))
                .Add(0)
                .AddRange(iso.GetBytes(nonce))
                .Add(0)
                // empty field
                .Add(0)
                .AddRange(iso.GetBytes(Configuration.OrganizationId))
                .Add(0)
                // empty field
                .Add(0)
                .AddRange(iso.GetBytes(method.ToString()))
                .Add(0)
                .AddRange(iso.GetBytes(path))
                .Add(0)
                .AddRange(iso.GetBytes(queryString))
                ;

            if (!string.IsNullOrEmpty(bodyText))
            {
                authInput = authInput
                    .Add(0)
                    .AddRange(Encoding.UTF8.GetBytes(bodyText));
            }

            var authKey = Encoding.UTF8.GetBytes(Configuration.ApiSecret);

            string auth = Configuration.ApiKey + ":" +
                BitConverter.ToString(
                    new HMACSHA256(authKey)
                        .ComputeHash(authInput.ToArray()))
                        .Replace("-", string.Empty)
                        .ToLower();

            return auth;
        }
    }
}
