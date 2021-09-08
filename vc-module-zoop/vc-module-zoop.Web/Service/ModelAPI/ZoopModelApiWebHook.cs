using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Zoop.ModelApi
{
    public class WebhookIn
    {
        public class Payload
        {
            public JObject @object { get; set; }
        }
        public string id { get; set; }
        public string type { get; set; }
        public string resource { get; set; }
        public Payload payload { get; set; }
        public object source { get; set; }
        public object name { get; set; }
        public string uri { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime? UpdatedAt { get; set; }

    }

    public class CreateWebHookIn
    {
        [JsonProperty("method")]
        public string Method;

        [JsonProperty("url")]
        public string Url;

        [JsonProperty("description")]
        public string Description;

        [JsonProperty("event")]
        public List<string> Event;
    }

    public class CreateWebHookOut
    {
        public class AdditionalProp
        {
        }

        public class Metadata
        {
            [JsonProperty("additionalProp")]
            public AdditionalProp AdditionalProp;
        }

        [JsonProperty("method")]
        public string Method;

        [JsonProperty("url")]
        public string Url;

        [JsonProperty("description")]
        public string Description;

        [JsonProperty("event")]
        public List<string> Event;

        [JsonProperty("status")]
        public string Status;

        [JsonProperty("last_error")]
        public string LastError;

        [JsonProperty("retries")]
        public int Retries;

        [JsonProperty("events_sent")]
        public int EventsSent;

        [JsonProperty("batches_sent")]
        public int BatchesSent;

        [JsonProperty("metadata")]
        public Metadata metadata;

        [JsonProperty("created_at")]
        public string CreatedAt;

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt;

        [JsonProperty("last_sent_at")]
        public DateTime LastSentAt;

        [JsonProperty("uri")]
        public string Uri;

        [JsonProperty("id")]
        public string Id;

        [JsonProperty("resource")]
        public string Resource;
    }

    public class ListWebHookOut
    {
        public class AdditionalProp
        {
        }

        public class Metadata
        {
            [JsonProperty("additionalProp")]
            public AdditionalProp AdditionalProp;
        }

        public class Item
        {
            [JsonProperty("method")]
            public string Method;

            [JsonProperty("url")]
            public string Url;

            [JsonProperty("description")]
            public string Description;

            [JsonProperty("events")]
            public List<string> Events;

            [JsonProperty("status")]
            public string Status;

            [JsonProperty("last_error")]
            public string LastError;

            [JsonProperty("retries")]
            public int Retries;

            [JsonProperty("events_sent")]
            public int EventsSent;

            [JsonProperty("batches_sent")]
            public int BatchesSent;

            [JsonProperty("metadata")]
            public Metadata Metadata;

            [JsonProperty("created_at")]
            public string CreatedAt;

            [JsonProperty("updated_at")]
            public string UpdatedAt;

            [JsonProperty("last_sent_at")]
            public string LastSentAt;

            [JsonProperty("uri")]
            public string Uri;

            [JsonProperty("id")]
            public string Id;

            [JsonProperty("resource")]
            public string Resource;
        }

        [JsonProperty("resource")]
        public string Resource;

        [JsonProperty("uri")]
        public string Uri;

        [JsonProperty("limit")]
        public int Limit;

        [JsonProperty("offset")]
        public int Offset;

        [JsonProperty("has_more")]
        public bool HasMore;

        [JsonProperty("total")]
        public int Total;

        [JsonProperty("items")]
        public List<Item> Items;
    }
}
