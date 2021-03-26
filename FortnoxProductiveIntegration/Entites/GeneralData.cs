using Newtonsoft.Json;

namespace FortnoxProductiveIntegration.Entites
{
    public class GeneralData
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("attributes")] 
        public Attributes Attributes { get; set; }
    }
}