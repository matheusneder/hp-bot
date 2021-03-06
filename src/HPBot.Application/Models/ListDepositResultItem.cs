using System;
using System.Collections.Generic;
using System.Text;

namespace HPBot.Application.Models
{
    public class ListDepositResultItem
    {
        public string Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public float Amount { get; set; }
        public string Currency { get; set; }
    }
}
