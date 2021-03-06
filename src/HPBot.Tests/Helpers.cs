using HPBot.Application;
using HPBot.Application.Adapters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace HPBot.Tests
{
    public static class Helpers
    {
        public static HttpClient HttpClient => new HttpClient(new HttpClientHandler()
        {
            Proxy = new WebProxy()
            {
                Address = new Uri("http://localhost:8888")
            }
        })
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        public static ILoggerFactory LoggerFactory => Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole());

        public static NiceHashConfiguration Configuration => NiceHashConfiguration
                .ReadFromNiceHashConfigJsonFile("test");

        public static NiceHashApiPersonedClient NiceHashApiPersonedClient => 
            new NiceHashApiPersonedClient(HttpClient, Configuration, LoggerFactory);

        public static NiceHashApiClient NiceHashApiClient =>
            new NiceHashApiClient(HttpClient, Configuration, LoggerFactory);
    }
}
