using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

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

            [JsonProperty("percentage")]
            public int? Percentage { get; set; }

            [JsonProperty("start_date")]
            public DateTime? StartDate { get; set; }
        }

        public class Interest
        {
            [JsonProperty("mode")]
            public string Mode { get; set; }

            [JsonProperty("amount")]
            public int? Amount { get; set; }

            [JsonProperty("percentage")]
            public int? Percentage { get; set; }

            [JsonProperty("start_date")]
            public DateTime? StartDate { get; set; }
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

        public class ESDateTimeConverter : IsoDateTimeConverter
        {
            public ESDateTimeConverter()
            {
                base.DateTimeFormat = "yyyy-MM-dd";
            }
        }

        public class PaymentMethod
        {
            [JsonProperty("expiration_date")]
            [JsonConverter(typeof(ESDateTimeConverter))]
            public DateTime? ExpirationDate { get; set; }

            [JsonProperty("payment_limit_date")]
            [JsonConverter(typeof(ESDateTimeConverter))]
            public DateTime? PaymentLimitDate { get; set; }

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

        [JsonProperty("logo")]
        public string UrlLogo { get; set; }

    }


    public class TransactionBoletoOut
    {
        public class Metadata
        {
        }

        public class PaymentMethod
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("zoop_boleto_id")]
            public string ZoopBoletoId { get; set; }

            [JsonProperty("resource")]
            public string Resource { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("reference_number")]
            public string ReferenceNumber { get; set; }

            [JsonProperty("document_number")]
            public string DocumentNumber { get; set; }

            [JsonProperty("expiration_date")]
            public DateTime ExpirationDate { get; set; }

            [JsonProperty("payment_limit_date")]
            public DateTime PaymentLimitDate { get; set; }

            [JsonProperty("recipient")]
            public string Recipient { get; set; }

            [JsonProperty("bank_code")]
            public string BankCode { get; set; }

            [JsonProperty("customer")]
            public string Customer { get; set; }

            [JsonProperty("address")]
            public object Address { get; set; }

            [JsonProperty("sequence")]
            public string Sequence { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }

            [JsonProperty("accepted")]
            public bool Accepted { get; set; }

            [JsonProperty("printed")]
            public bool Printed { get; set; }

            [JsonProperty("downloaded")]
            public bool Downloaded { get; set; }

            [JsonProperty("fingerprint")]
            public object Fingerprint { get; set; }

            [JsonProperty("paid_at")]
            public object PaidAt { get; set; }

            [JsonProperty("uri")]
            public string Uri { get; set; }

            [JsonProperty("barcode")]
            public string Barcode { get; set; }

            [JsonProperty("metadata")]
            public Metadata Metadata { get; set; }

            [JsonProperty("created_at")]
            public DateTime CreatedAt { get; set; }

            [JsonProperty("updated_at")]
            public DateTime UpdatedAt { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }
        }

        public class PointOfSale
        {
            [JsonProperty("entry_mode")]
            public string EntryMode { get; set; }

            [JsonProperty("identification_number")]
            public object IdentificationNumber { get; set; }
        }

        public class History
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("transaction")]
            public string Transaction { get; set; }

            [JsonProperty("amount")]
            public decimal Amount { get; set; }

            [JsonProperty("operation_type")]
            public string OperationType { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("response_code")]
            public string ResponseCode { get; set; }

            [JsonProperty("response_message")]
            public string ResponseMessage { get; set; }

            [JsonProperty("authorization_code")]
            public string AuthorizationCode { get; set; }

            [JsonProperty("authorizer_id")]
            public string AuthorizerId { get; set; }

            [JsonProperty("authorization_nsu")]
            public string AuthorizationNsu { get; set; }

            [JsonProperty("gatewayResponseTime")]
            public string GatewayResponseTime { get; set; }

            [JsonProperty("authorizer")]
            public string Authorizer { get; set; }

            [JsonProperty("created_at")]
            public DateTime CreatedAt { get; set; }
        }

        public class InstallmentPlan
        {
            [JsonProperty("number_installments")]
            public string NumberInstallments { get; set; }

            [JsonProperty("mode")]
            public string Mode { get; set; }
        }


        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("resource")]
        public string Resource { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("original_amount")]
        public decimal OriginalAmount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("payment_type")]
        public string PaymentType { get; set; }

        [JsonProperty("transaction_number")]
        public string TransactionNumber { get; set; }

        [JsonProperty("gateway_authorizer")]
        public string GatewayAuthorizer { get; set; }

        [JsonProperty("app_transaction_uid")]
        public string AppTransactionUid { get; set; }

        [JsonProperty("refunds")]
        public string Refunds { get; set; }

        [JsonProperty("rewards")]
        public string Rewards { get; set; }

        [JsonProperty("discounts")]
        public string Discounts { get; set; }

        [JsonProperty("pre_authorization")]
        public string PreAuthorization { get; set; }

        [JsonProperty("sales_receipt")]
        public string SalesReceipt { get; set; }

        [JsonProperty("on_behalf_of")]
        public string OnBehalfOf { get; set; }

        [JsonProperty("customer")]
        public string Customer { get; set; }

        [JsonProperty("statement_descriptor")]
        public string StatementDescriptor { get; set; }

        [JsonProperty("payment_method")]
        public PaymentMethod paymentMethod { get; set; }

        [JsonProperty("source")]
        public object Source { get; set; }

        [JsonProperty("point_of_sale")]
        public PointOfSale pointOfSale { get; set; }

        [JsonProperty("installment_plan")]
        public InstallmentPlan installmentPlan { get; set; }

        [JsonProperty("refunded")]
        public bool Refunded { get; set; }

        [JsonProperty("voided")]
        public bool Voided { get; set; }

        [JsonProperty("captured")]
        public bool Captured { get; set; }

        [JsonProperty("fees")]
        public decimal Fees { get; set; }

        [JsonProperty("fee_details")]
        public object FeeDetails { get; set; }

        [JsonProperty("location_latitude")]
        public object LocationLatitude { get; set; }

        [JsonProperty("location_longitude")]
        public object LocationLongitude { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("metadata")]
        public Metadata metadata { get; set; }

        [JsonProperty("expected_on")]
        public DateTime? ExpectedOn { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("reference_id")]
        public string ReferenceId { get; set; }

        [JsonProperty("payment_authorization")]
        public object PaymentAuthorization { get; set; }

        [JsonProperty("history")]
        public List<History> history { get; set; }

        public Error error;
    }
}
