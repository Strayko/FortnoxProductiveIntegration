using Newtonsoft.Json;

namespace FortnoxProductiveIntegration.Entites
{
    public class DocumentType
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }
}