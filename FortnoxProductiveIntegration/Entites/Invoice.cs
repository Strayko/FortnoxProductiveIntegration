using Newtonsoft.Json;

namespace FortnoxProductiveIntegration.Entites
{
    public class Invoice
    {
        [JsonProperty("data")] 
        public GeneralData GeneralData { get; set; }
        [JsonProperty("relationships")]
        public Relationships Relationships { get; set; }
    }
}