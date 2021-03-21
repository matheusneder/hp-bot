using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace HPBot.Application
{
    public interface INiceHashApiClient
    {
        NiceHashConfiguration Configuration { get; set; }

        void ConfigureRequestMessage(HttpRequestMessage message, string requestId, HttpMethod method, string path, string queryString, string nonce, string time, string bodyText);
        Task DeleteAsync(string path);
        Task<T> GetAsync<T>(string path);
        Task<T> GetAsync<T>(string path, Dictionary<string, object> query);
        Task PostAsync(string path, object body);
        Task<T> PostAsync<T>(string path, Dictionary<string, object> query, object body);
        Task<T> PostAsync<T>(string path, object body);
        Task<HttpResponseMessage> SendAsync(string requestId, HttpMethod method, string path, Dictionary<string, object> query, object body);
        Task<T> SendAsync<T>(HttpMethod method, string path, Dictionary<string, object> query, object body);
    }
}