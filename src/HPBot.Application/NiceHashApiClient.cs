using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace HPBot.Application
{
    public class NiceHashApiClient
    {
        private readonly NiceHashConfiguration configuration;

        private HttpClient HttpClient => new HttpClient(new HttpClientHandler()
        {
            Proxy = new WebProxy()
            {
                Address = new Uri("http://localhost:8888")
            }
        });

        private readonly ILogger logger;

        public NiceHashApiClient(NiceHashConfiguration configuration, ILoggerFactory loggerFactory)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            logger = loggerFactory?.CreateLogger<NiceHashApiClient>() ?? 
                throw new ArgumentNullException(nameof(loggerFactory));
        }

        public async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, Dictionary<string, object> query, object body)
        {
            var queryString = string.Join("&", query
                .Select(i => $"{HttpUtility.UrlEncode(i.Key)}=" +
                $"{HttpUtility.UrlEncode(Convert.ToString(i.Value, CultureInfo.InvariantCulture))}")
                .ToArray());
            
            string requestUri = $"https://{configuration.ApiHost}{path}";
            string requestId = Guid.NewGuid().ToString();

            if (!string.IsNullOrWhiteSpace(queryString))
            {
                requestUri += $"?{queryString}";
            }

            HttpRequestMessage message = new HttpRequestMessage(method, requestUri);
            string nonce = Guid.NewGuid().ToString();
            string time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            string bodyText = string.Empty;

            if (body != null)
            {
                bodyText = JsonSerializer.Serialize(body);
                message.Content = new StringContent(bodyText, Encoding.UTF8, "application/json");
            }

            message.Headers.Add("X-Time", time);
            message.Headers.Add("X-Nonce", nonce);
            message.Headers.Add("X-Organization-Id", configuration.OrganizationId);
            message.Headers.Add("X-Request-Id", requestId);
            
            Encoding iso = Encoding.GetEncoding("ISO-8859-1");

            var authInput = ImmutableList.Create<byte>()
                .AddRange(iso.GetBytes(configuration.ApiKey))
                .Add(0)
                .AddRange(iso.GetBytes(time))
                .Add(0)
                .AddRange(iso.GetBytes(nonce))
                .Add(0)
                // empty field
                .Add(0)
                .AddRange(iso.GetBytes(configuration.OrganizationId))
                .Add(0)
                // empty field
                .Add(0)
                .AddRange(iso.GetBytes(method.ToString()))
                .Add(0)
                .AddRange(iso.GetBytes(path))
                .Add(0)
                .AddRange(iso.GetBytes(queryString))
                ;

            if (body != null)
            {
                authInput = authInput
                    .Add(0)
                    .AddRange(Encoding.UTF8.GetBytes(bodyText));
            }

            var authKey = Encoding.UTF8.GetBytes(configuration.ApiSecret);

            string auth = configuration.ApiKey + ":" +
                BitConverter.ToString(
                    new HMACSHA256(authKey)
                        .ComputeHash(authInput.ToArray()))
                        .Replace("-", string.Empty)
                        .ToLower();

            message.Headers.Add("X-Auth", auth);
            
            logger.LogInformation("Initiating HTTP request; " +
                "HttpMethod: {HttpMethod};" +
                "RequestUri: {RequestUri}; " +
                "X-Time: {Time};" +
                "X-Nonce: {Nonce};" +
                "X-Organization-Id: {OrganizationId};" +
                "X-Request-Id: {RequestId};" +
                "X-Auth: {Auth};" +
                "Body: {Body};",
                method.ToString(), requestUri, time, nonce, configuration.OrganizationId, requestId, auth, bodyText);

            var result = await HttpClient.SendAsync(message);
            
            if (result.IsSuccessStatusCode)
            {
                logger.LogInformation("HTTP request {RequestId} success. Status: {HttpStatus}",
                    requestId,
                    (int)result.StatusCode);
            }
            else
            {
                logger.LogWarning("HTTP request {RequestId} error. Status: {HttpStatus}",
                    requestId,
                    (int)result.StatusCode);
            }

            return result;
        }

        public Task<HttpResponseMessage> PostAsync(string path, object body)
        {
            return SendAsync(HttpMethod.Post, path, new Dictionary<string, object>() { }, body);
        }
    }
}
