using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace HPBot.Application

{
    public class NiceHashConfiguration
    {
        public string ApiKey { set; get; }
        public string OrganizationId { set; get; }
        public string ApiSecret { set; get; }
        public string ApiHost { set; get; }
        public string EUPoolId { get; set; }
        public string UsaPoolId { get; set; }

        public static NiceHashConfiguration ReadFromNiceHashConfigJsonFile(string environment)
        {
            var configurationSet = JsonSerializer
                .Deserialize<Dictionary<string, NiceHashConfiguration>>(
                    File.ReadAllText("niceHashConfig.json"));

            var configuration = configurationSet[environment];
            return configuration;
        }
    }
}
