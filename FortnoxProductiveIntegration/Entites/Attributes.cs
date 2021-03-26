using Newtonsoft.Json;

namespace FortnoxProductiveIntegration.Entites
{
    public class Attributes
    {
        [JsonProperty("number")]
        public string Number { get; set; }
        [JsonProperty("subject")]
        public string Subject { get; set; }
        [JsonProperty("invoiced_on")]
        public string InvoicedOn { get; set; }
        [JsonProperty("pay_on")]
        public string PayOn { get; set; }
        [JsonProperty("currency")]
        public string Currency { get; set; }
    }
}