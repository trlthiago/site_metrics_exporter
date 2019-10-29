namespace tester_core.Models
{
    public class KeyValue
    {
        [Newtonsoft.Json.JsonProperty("name")]
        public string Name { get; set; }
        [Newtonsoft.Json.JsonProperty("value")]
        public decimal Value { get; set; }
    }
}
