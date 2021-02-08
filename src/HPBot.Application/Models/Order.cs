using System;
using System.Collections.Generic;
using System.Text;

namespace HPBot.Application.Models
{
    public abstract class Order
    {
        public string Id { get; set; }
        public float PriceBtc { get; set; }
        public DateTimeOffset CanLiveTill { get; set; }
    }
}
