using Newtonsoft.Json;

namespace FortnoxProductiveIntegration.Entites
{
    public class Company
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }
}