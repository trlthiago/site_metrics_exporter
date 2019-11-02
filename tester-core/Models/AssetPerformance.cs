using System.Collections.Generic;

namespace tester_core.Models
{
    /*
          "{
          "name": "https://www.google.com/?gws_rd=ssl",
          "entryType": "navigation",
          "startTime": 0,
          "duration": 1603.8800000678748,
          "initiatorType": "navigation",
          "nextHopProtocol": "h2",
          "workerStart": 0,
          "redirectStart": 0,
          "redirectEnd": 0,
          "fetchStart": 545.3250000718981,
          "domainLookupStart": 546.6749999905005,
          "domainLookupEnd": 546.6899999883026,
          "connectStart": 546.6899999883026,
          "connectEnd": 703.6150000058115,
          "secureConnectionStart": 594.2400000058115,
          "requestStart": 703.745000064373,
          "responseStart": 876.5650000423193,
          "responseEnd": 958.1349999643862,
          "transferSize": 64306,
          "encodedBodySize": 63628,
          "decodedBodySize": 217361,
          "serverTiming": [],
          "unloadEventStart": 0,
          "unloadEventEnd": 0,
          "domInteractive": 984.0049999766052,
          "domContentLoadedEventStart": 984.1850000666454,
          "domContentLoadedEventEnd": 988.8349999673665,
          "domComplete": 1598.8999999826774,
          "loadEventStart": 1599.4150000624359,
          "loadEventEnd": 1603.8800000678748,
          "type": "navigate",
          "redirectCount": 0
          }"
    */

    public class AssetPerformance
    {
        public string name { get; set; }
        public string entryType { get; set; }
        public long startTime { get; set; }
        public string initiatorType { get; set; }
        public string nextHopProtocol { get; set; }
        public int workerStart { get; set; }
        public double redirectStart { get; set; }
        public double redirectEnd { get; set; }
        public double fetchStart { get; set; }
        public double domainLookupStart { get; set; }
        public double domainLookupEnd { get; set; }
        public double connectStart { get; set; }
        public double connectEnd { get; set; }
        public double secureConnectionStart { get; set; }
        public double requestStart { get; set; }
        public double responseStart { get; set; }
        public double responseEnd { get; set; }
        [Prometheus(Name = "site_transfer_size")]
        public double transferSize { get; set; }
        public double encodedBodySize { get; set; }
        public double decodedBodySize { get; set; }
        public List<object> serverTiming { get; set; }
        public double unloadEventStart { get; set; }
        public double unloadEventEnd { get; set; }
        public double domInteractive { get; set; }
        public double domContentLoadedEventStart { get; set; }
        public double domContentLoadedEventEnd { get; set; }
        public double domComplete { get; set; }
        public double loadEventStart { get; set; }
        public double loadEventEnd { get; set; }
        public string type { get; set; }
        public int redirectCount { get; set; }
        
        [Prometheus(Name = "site_duration")]
        public double duration { get; set; }

        [Prometheus(Name = "site_response_time")]
        public double GetResponseTime => responseEnd - startTime;

        [Prometheus(Name = "site_loaded")]
        public double GetLoadEventEnd => loadEventEnd - startTime;

        public double GetDomInteractive => domInteractive - startTime;

        [Prometheus(Name = "site_dom_content_loaded")]
        public double GetDomContentLoadedEventEnd => domContentLoadedEventEnd - startTime;

        [Prometheus(Name = "site_dns_time")]
        public double GetDnsTime => domainLookupEnd - domainLookupStart;

        public double GetTcpTime => connectEnd - connectStart;

        [Prometheus(Name = "site_waiting_time")]
        public double GetWaitingTime => responseStart - requestStart;

        [Prometheus(Name = "site_content_time")]
        public double GetContentTime => responseEnd - responseStart;

        [Prometheus(Name = "site_ttfb")]
        public double GetTTFB => responseStart - requestStart;

        [Prometheus(Name = "site_network_time")]
        public double GetNetworkTime => GetDnsTime + GetTcpTime + GetWaitingTime + GetContentTime;
    }
}
