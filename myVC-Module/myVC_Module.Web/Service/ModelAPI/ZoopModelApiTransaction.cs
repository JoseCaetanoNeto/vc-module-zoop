using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Zoop.ModelApi
{

    public class TransactionOut
    {
        public class PaymentMethod
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("resource")]
            public string Resource { get; set; }

            [JsonProperty("uri")]
            public string Uri { get; set; }
        }

        public class PointOfSale
        {
            [JsonProperty("entry_mode")]
            public string EntryMode { get; set; }

            [JsonProperty("identification_number")]
            public object IdentificationNumber { get; set; }
        }

        public class InstallmentPlan
        {
            [JsonProperty("number_installments")]
            public string NumberInstallments { get; set; }

            [JsonProperty("mode")]
            public string Mode { get; set; }
        }

        public class Detail
        {
            [JsonProperty("amount")]
            public string Amount { get; set; }

            [JsonProperty("prepaid")]
            public bool Prepaid { get; set; }

            [JsonProperty("currency")]
            public string Currency { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("is_gateway_fee")]
            public bool IsGatewayFee { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }
        }

        public class Fees
        {
            [JsonProperty("total")]
            public string Total { get; set; }

            [JsonProperty("uri")]
            public string Uri { get; set; }

            [JsonProperty("details")]
            public List<Detail> Details { get; set; }
        }

        public class History
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("transaction")]
            public string Transaction { get; set; }

            [JsonProperty("authorizer")]
            public string Authorizer { get; set; }

            [JsonProperty("amount")]
            public decimal Amount { get; set; }

            [JsonProperty("operation_type")]
            public string OperationType { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("response_code")]
            public string ResponseCode { get; set; }

            [JsonProperty("authorization_code")]
            public string AuthorizationCode { get; set; }

            [JsonProperty("authorizer_id")]
            public string AuthorizerId { get; set; }

            [JsonProperty("authorization_nsu")]
            public string AuthorizationNsu { get; set; }

            [JsonProperty("gatewayResponseTime")]
            public string GatewayResponseTime { get; set; }

            [JsonProperty("created_at")]
            public DateTime? CreatedAt { get; set; }

            [JsonProperty("updated_at")]
            public DateTime? UpdatedAt { get; set; }
        }


        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("resource")]
        public string Resource { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("original_amount")]
        public string OriginalAmount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("description")]
        public object Description { get; set; }

        [JsonProperty("payment_type")]
        public string PaymentType { get; set; }

        [JsonProperty("transaction_number")]
        public string TransactionNumber { get; set; }

        [JsonProperty("sales_receipt")]
        public string SalesReceipt { get; set; }

        [JsonProperty("on_behalf_of")]
        public string OnBehalfOf { get; set; }

        [JsonProperty("customer")]
        public object Customer { get; set; }

        [JsonProperty("statement_descriptor")]
        public string StatementDescriptor { get; set; }

        [JsonProperty("payment_method")]
        public PaymentMethod paymentMethod { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("point_of_sale")]
        public PointOfSale pointOfSale { get; set; }

        [JsonProperty("installment_plan")]
        public InstallmentPlan installmentPlan { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("reference_id")]
        public string ReferenceId { get; set; }

        [JsonProperty("metadata")]
        public object Metadata { get; set; }

        [JsonProperty("fees")]
        public Fees fees { get; set; }

        [JsonProperty("split_rules")]
        public object SplitRules { get; set; }

        [JsonProperty("history")]
        public List<History> history { get; set; }

        public Error error;
    }

    public class TransactionIn
    {
        public class Card
        {
            [JsonProperty("holder_name")]
            public string HolderName;

            [JsonProperty("expiration_month")]
            public string ExpirationMonth;

            [JsonProperty("expiration_year")]
            public string ExpirationYear;

            [JsonProperty("card_number")]
            public string CardNumber;

            [JsonProperty("security_code")]
            public string SecurityCode;
        }

        public class Source
        {
            [JsonProperty("type")]
            public string Type;

            [JsonProperty("card")]
            public Card Card;
        }

        public class InstallmentPlan
        {
            [JsonProperty("number_installments")]
            public int NumberInstallments;

            [JsonProperty("mode")]
            public string Mode;
        }

        [JsonProperty("source")]
        public Source source;

        [JsonProperty("installment_plan")]
        public InstallmentPlan installmentPlan;

        [JsonProperty("on_behalf_of")]
        public string OnBehalfOf;

        [JsonProperty("amount")]
        public int Amount;

        [JsonProperty("capture")]
        public bool Capture;

        [JsonProperty("description")]
        public string Description;

        [JsonProperty("reference_id")]
        public string ReferenceId;

        [JsonProperty("statement_descriptor")]
        public string StatementDescriptor;
    }

    public class VoidCaptureTransactionIn
    {
        [JsonProperty("on_behalf_of")]
        public string OnBehalfOf;

        [JsonProperty("amount")]
        public int Amount;
    }
}
