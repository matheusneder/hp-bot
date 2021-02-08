using System;
using System.Collections.Generic;
using System.Text;

namespace HPBot.Application.Dtos
{
    public class EthAverageRewardResultDto
    {
        public class DataDto
        {
            public ContentDto btc { get; set; }
            public ContentDto time { get; set; }
            public ContentDto currency { get; set; }
            public ContentDto usd { get; set; }
        }

        public class ContentDto
        {
            public string html { get; set; }
        }

        public IEnumerable<DataDto> data { get; set; }
    }
}
