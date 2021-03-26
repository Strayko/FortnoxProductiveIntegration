using Newtonsoft.Json;

namespace FortnoxProductiveIntegration.Entites
{
    public class Data
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}