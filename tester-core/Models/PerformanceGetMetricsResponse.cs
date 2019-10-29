using System.Collections.Generic;

namespace tester_core.Models
{
    public class PerformanceGetMetricsResponse
    {
        [Newtonsoft.Json.JsonProperty("metrics")]
        internal List<KeyValue> Metrics { get; set; }
    }
}
