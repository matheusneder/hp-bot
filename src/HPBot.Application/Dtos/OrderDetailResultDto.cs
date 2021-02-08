using System;
using System.Collections.Generic;
using System.Text;

namespace HPBot.Application.Dtos
{
    public class OrderDetailResultDto
    {
        public string id { get; set; }
        public string amount { get; set; }
        public string availableAmount { get; set; }
        public string payedAmount { get; set; }
        public DateTimeOffset endTs { get; set; } // order life limit
        public int estimateDurationInSeconds { get; set; }
        public bool alive { get; set; }
        public DateTimeOffset startTs { get; set; } // order created
        public string price { get; set; }
        public OrderStatusDto status { get; set; }

        private string _marketFactor;

        public string marketFactor
        {
            get => _marketFactor;
            set
            {
                _marketFactor = value;

                if (_marketFactor != "1000000000000")
                {
                    throw new InvalidOperationException($"Expected marketFactor = 1000000000000 but {_marketFactor}");
                }
            }
        }
    }
}
