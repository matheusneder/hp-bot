using System;
using System.Collections.Generic;
using System.Text;

namespace HPBot.Application.Dtos
{
    public class NiceHashApiErrorDto
    {
        //{"error_id":"f17cdf12-6fb4-4418-906a-ad4d702b2685","errors":[{"code":5056,"message":"Error creating fixed order, price changed"}]}

        public string error_id { get; set; }
        public IEnumerable<ErrorItemDto> errors { get; set; }

        public class ErrorItemDto
        {
            public int code { get; set; }
            public string message { get; set; }
        }
    }
}
