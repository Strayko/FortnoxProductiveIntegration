using Newtonsoft.Json;

namespace FortnoxProductiveIntegration.Entites
{
    public class Budget
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }
}