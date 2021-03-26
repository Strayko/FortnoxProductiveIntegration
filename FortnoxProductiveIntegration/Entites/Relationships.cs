using Newtonsoft.Json;

namespace FortnoxProductiveIntegration.Entites
{
    public class Relationships
    {
        [JsonProperty("document_type")]
        public DocumentType DocumentType { get; set; }
        [JsonProperty("budget")]
        public Budget Budget { get; set; }
        [JsonProperty("project")]
        public Project Project { get; set; }
        [JsonProperty("company")]
        public Company Company { get; set; }
    }
}