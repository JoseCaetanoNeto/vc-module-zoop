using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zoop.ModelApi
{
    public class BuyerIn
    {
        public class Address
        {
            [JsonProperty("line1")]
            public string Line1 { get; set; }

            [JsonProperty("line2")]
            public string Line2 { get; set; }

            [JsonProperty("line3")]
            public string Line3 { get; set; }

            [JsonProperty("neighborhood")]
            public string Neighborhood { get; set; }

            [JsonProperty("city")]
            public string City { get; set; }

            [JsonProperty("state")]
            public string State { get; set; }

            [JsonProperty("postal_code")]
            public string PostalCode { get; set; }

            [JsonProperty("country_code")]
            public string CountryCode { get; set; }
        }

        public class AdditionalProp
        {
        }

        public class Metadata
        {
            [JsonProperty("additionalProp")]
            public AdditionalProp AdditionalProp { get; set; }
        }


        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("resource")]
        public string Resource { get; set; }

        [JsonProperty("account_balance")]
        public int AccountBalance { get; set; }

        [JsonProperty("current_balance")]
        public int CurrentBalance { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phone_number")]
        public string PhoneNumber { get; set; }

        [JsonProperty("taxpayer_id")]
        public string TaxpayerId { get; set; }

        [JsonProperty("birthdate")]
        public string Birthdate { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("address")]
        public Address address { get; set; }

        [JsonProperty("delinquent")]
        public bool Delinquent { get; set; }

        [JsonProperty("default_debit")]
        public string DefaultDebit { get; set; }

        [JsonProperty("default_credit")]
        public string DefaultCredit { get; set; }

        [JsonProperty("metadata")]
        public Metadata metadata { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }
    }

    public class BuyerOut: Generic
    {
        public class Address
        {
            [JsonProperty("line1")]
            public string Line1 { get; set; }

            [JsonProperty("line2")]
            public string Line2 { get; set; }

            [JsonProperty("line3")]
            public string Line3 { get; set; }

            [JsonProperty("neighborhood")]
            public string Neighborhood { get; set; }

            [JsonProperty("city")]
            public string City { get; set; }

            [JsonProperty("state")]
            public string State { get; set; }

            [JsonProperty("postal_code")]
            public string PostalCode { get; set; }

            [JsonProperty("country_code")]
            public string CountryCode { get; set; }
        }

        public class AdditionalProp
        {
        }

        public class Metadata
        {
            [JsonProperty("additionalProp")]
            public AdditionalProp AdditionalProp { get; set; }
        }


        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("resource")]
        public string Resource { get; set; }

        [JsonProperty("account_balance")]
        public int AccountBalance { get; set; }

        [JsonProperty("current_balance")]
        public int CurrentBalance { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phone_number")]
        public string PhoneNumber { get; set; }

        [JsonProperty("taxpayer_id")]
        public string TaxpayerId { get; set; }

        [JsonProperty("birthdate")]
        public string Birthdate { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("address")]
        public Address address { get; set; }

        [JsonProperty("delinquent")]
        public bool Delinquent { get; set; }

        [JsonProperty("default_debit")]
        public string DefaultDebit { get; set; }

        [JsonProperty("default_credit")]
        public string DefaultCredit { get; set; }

        [JsonProperty("metadata")]
        public Metadata metadata { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }
    }
}
