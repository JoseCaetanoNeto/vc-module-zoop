using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zoop.ModelApi
{
    public class TransactionBoletoIn
    {
        public class LateFee
        {
            [JsonProperty("mode")]
            public string Mode { get; set; }

            [JsonProperty("amount")]
            public int Amount { get; set; }
        }

        public class Interest
        {
            [JsonProperty("mode")]
            public string Mode { get; set; }

            [JsonProperty("amount")]
            public int Amount { get; set; }
        }

        public class Discount
        {
            [JsonProperty("mode")]
            public string Mode { get; set; }

            [JsonProperty("limit_date")]
            public string LimitDate { get; set; }

            [JsonProperty("amount")]
            public int Amount { get; set; }
        }

        public class BillingInstructions
        {
            [JsonProperty("late_fee")]
            public LateFee LateFee { get; set; }

            [JsonProperty("interest")]
            public Interest Interest { get; set; }

            [JsonProperty("discount")]
            public List<Discount> Discount { get; set; }
        }

        public class PaymentMethod
        {
            [JsonProperty("expiration_date")]
            public string ExpirationDate { get; set; }

            [JsonProperty("payment_limit_date")]
            public string PaymentLimitDate { get; set; }

            [JsonProperty("body_instructions")]
            public List<string> BodyInstructions { get; set; }

            [JsonProperty("billing_instructions")]
            public BillingInstructions BillingInstructions { get; set; }
        }


        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("reference_id")]
        public string ReferenceId { get; set; }

        [JsonProperty("on_behalf_of")]
        public string OnBehalfOf { get; set; }

        [JsonProperty("customer")]
        public string Customer { get; set; }

        [JsonProperty("payment_type")]
        public string PaymentType { get; set; }

        [JsonProperty("payment_method")]
        public PaymentMethod paymentMethod { get; set; }
    }
}
