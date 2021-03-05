using HPBot.Application.Dtos;
using HPBot.Application.Exceptions;
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
        public NiceHashConfiguration Configuration { get; set; }
        private readonly HttpClient httpClient;
        private readonly ILogger logger;

        public NiceHashApiClient(HttpClient httpClient, NiceHashConfiguration configuration, ILoggerFactory loggerFactory)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            logger = loggerFactory?.CreateLogger<NiceHashApiClient>() ?? 
                throw new ArgumentNullException(nameof(loggerFactory));
        }

        public virtual void ConfigureRequestMessage(HttpRequestMessage message, string requestId, HttpMethod method, 
            string path, string queryString, string nonce, string time, string bodyText)
        {
        }

        public async Task<HttpResponseMessage> SendAsync(string requestId, HttpMethod method, string path, Dictionary<string, object> query, object body)
        {
            var queryString = string.Join("&", query
                .Select(i => $"{HttpUtility.UrlEncode(i.Key)}=" +
                    $"{HttpUtility.UrlEncode(Convert.ToString(i.Value, CultureInfo.InvariantCulture))}")
                    .ToArray());

            string requestUri = $"https://{Configuration.ApiHost}{path}";

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

            ConfigureRequestMessage(message, requestId, method, path, queryString, nonce, time, bodyText);

            logger.LogDebug("Initiating HTTP request {RequestId}; " +
                "HttpMethod: '{HttpMethod}' ::" +
                "RequestUri: '{RequestUri}' :: " +
                "Body: '{Body}'",
                requestId, method.ToString(), requestUri, bodyText);

            try
            {
                return await httpClient.SendAsync(message);
            }
            catch (Exception e)
            {
                throw new NiceHashApiSendRequestException(e);
            }
        }

        /// <summary>
        /// You should take care only of <see cref="NiceHashApiTechnicalIssueException"/> for <see cref="NiceHashApiSendRequestException"/>, 
        /// <see cref="NiceHashApiReadResponseException"/> and <see cref="NiceHashApiServerException"/>.
        /// For API client errors (400 &lt;= StatusCode &lt; 500), use <see cref="NiceHashApiClientException" />.
        /// </summary>
        /// <exception cref="NiceHashApiSendRequestException"></exception>
        /// <exception cref="NiceHashApiReadResponseException"></exception>
        /// <exception cref="NiceHashApiClientException"></exception>
        /// <exception cref="NiceHashApiServerException"></exception>
        public async Task<T> SendAsync<T>(HttpMethod method, string path, Dictionary<string, object> query, object body)
        {
            string requestId = Guid.NewGuid().ToString();
            string responseText;
            var httpResponse = await SendAsync(requestId, method, path, query, body);

            try
            {
                responseText = await httpResponse.Content.ReadAsStringAsync();
            }
            catch(Exception e)
            {
                throw new NiceHashApiReadResponseException(e);
            }

            if (httpResponse.IsSuccessStatusCode)
            {
                logger.LogDebug("HTTP request {RequestId} success. Status: {HttpStatus}",
                    requestId,
                    (int)httpResponse.StatusCode);

                try
                {
                    return JsonSerializer.Deserialize<T>(responseText);
                }
                catch(Exception e)
                {
                    throw new InvalidOperationException(
                        $"Could not deserialize NiceHash API response (Status {httpResponse.StatusCode}). " +
                        $"Response text: '{responseText}'.", e);
                }
            }

            NiceHashApiErrorDto errorDto = null;

            if (httpResponse.StatusCode >= HttpStatusCode.BadRequest && 
                httpResponse.StatusCode < HttpStatusCode.InternalServerError)
            {
                logger.LogInformation("HTTP request {RequestId} client error. Status: {HttpStatus}",
                    requestId,
                    (int)httpResponse.StatusCode);

                try
                {
                    errorDto = JsonSerializer.Deserialize<NiceHashApiErrorDto>(responseText);

                    logger.LogInformation("ErrorId: {ErrorId}. ResponseText: '{ResponseText}'.", 
                        errorDto.error_id,
                        responseText);
                }
                catch(Exception e)
                {
                    throw new InvalidOperationException(
                        $"Could not deserialize NiceHash API response (Status {httpResponse.StatusCode}). " +
                        $"Response text: '{responseText}'.", e);
                }

                throw new NiceHashApiClientException(httpResponse.StatusCode, errorDto);
            }

            logger.LogWarning("HTTP request {RequestId} server error. Status: {HttpStatus}",
                requestId,
                (int)httpResponse.StatusCode);

            try
            {
                errorDto = JsonSerializer.Deserialize<NiceHashApiErrorDto>(responseText);

                logger.LogInformation("ErrorId: {ErrorId}. ResponseText: '{ResponseText}'.",
                    errorDto.error_id,
                    responseText);
            }
            catch
            {
                // Despite catch all, the raw responseText is being included in
                // NiceHashApiServerException for further analysis.
                errorDto = null;

                logger.LogInformation("ErrorId: #NA. ResponseText: '{ResponseText}'.",
                    responseText);
            }

            // assume status code >= 500 (in fact, if status range 300-399 occur 
            // will also throw this exception, but it's an unexpected kind of status for this scenario)
            throw new NiceHashApiServerException(httpResponse.StatusCode,
                errorDto, responseText);
        }

        /// <inheritdoc cref="SendAsync{T}(HttpMethod, string, Dictionary{string, object}, object)"/>
        public Task<T> PostAsync<T>(string path, Dictionary<string, object> query, object body)
        {
            return SendAsync<T>(HttpMethod.Post, path, query, body);
        }

        /// <inheritdoc cref="SendAsync{T}(HttpMethod, string, Dictionary{string, object}, object)"/>
        public Task<T> PostAsync<T>(string path, object body)
        {
            return PostAsync<T>(path, new Dictionary<string, object>() { }, body);
        }

        /// <inheritdoc cref="SendAsync{T}(HttpMethod, string, Dictionary{string, object}, object)"/>
        public Task PostAsync(string path, object body)
        {
            return PostAsync<object>(path, body);
        }

        /// <inheritdoc cref="SendAsync{T}(HttpMethod, string, Dictionary{string, object}, object)"/>
        public Task<T> GetAsync<T>(string path, Dictionary<string, object> query)
        {
            return SendAsync<T>(HttpMethod.Get, path, query, null);
        }

        /// <inheritdoc cref="SendAsync{T}(HttpMethod, string, Dictionary{string, object}, object)"/>
        public Task<T> GetAsync<T>(string path)
        {
            return GetAsync<T>(path, new Dictionary<string, object>() { });
        }

        /// <inheritdoc cref="SendAsync{T}(HttpMethod, string, Dictionary{string, object}, object)"/>
        public Task DeleteAsync(string path)
        {
            return SendAsync<object>(HttpMethod.Delete, path, new Dictionary<string, object>() { }, null);
        }
    }
}
