using System;
using System.Collections.Generic;
using System.Text;

namespace HPBot.Application.Dtos
{
    public class NiceHashExchangeApiErrorDto
    {
        public ErrorContainerDto error { get; set; }

        public class ErrorContainerDto
        {
            public int status { get; set; }
            public string category { get; set; }
            public string message { get; set; }
        }
    }
}
