using System;
using System.Collections.Generic;
using System.Text;

namespace HPBot.Application

{
    public class NiceHashConfiguration
    {
        public string ApiKey { set; get; }
        public string OrganizationId { set; get; }
        public string ApiSecret { set; get; }
        public string ApiHost { set; get; }

        //public NiceHashConfiguration(string apiKey, string organizationId, string apiSecret, string apiHost)
        //{
        //    ApiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        //    OrganizationId = organizationId ?? throw new ArgumentNullException(nameof(organizationId));
        //    ApiSecret = apiSecret ?? throw new ArgumentNullException(nameof(apiSecret));
        //    ApiHost = apiHost ?? throw new ArgumentNullException(nameof(apiHost));
        //}
    }
}
