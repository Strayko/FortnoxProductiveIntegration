using Newtonsoft.Json;

namespace FortnoxProductiveIntegration.Entites
{
    public class Project
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }
}