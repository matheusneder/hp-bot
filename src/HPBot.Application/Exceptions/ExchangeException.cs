using HPBot.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Text;

namespace HPBot.Application.Exceptions
{
    [Serializable]
    public class ExchangeException : AppException
    {
        public ExchangeException(ExchangeErrorReason reason, string market, float amount)
        {
            Reason = reason;
            Market = market;
            Amount = amount;
        }

        protected ExchangeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ExchangeErrorReason Reason { get; }
        public string Market { get; }
        public float Amount { get; }
        public override string Message => $"Reason: {Reason}; Market: {Market}; Amount: {Amount}";

        public enum ExchangeErrorReason
        {
            QuantityTooSmall = 1219
        }
    }
}
