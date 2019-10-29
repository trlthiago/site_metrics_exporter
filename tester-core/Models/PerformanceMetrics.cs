using System;
using System.Text;

namespace tester_core.Models
{
    public class PerformanceMetrics
    {
        public long navigationStart { get; set; }
        public int unloadEventStart { get; set; }
        public int unloadEventEnd { get; set; }
        public int redirectStart { get; set; }
        public int redirectEnd { get; set; }
        public long fetchStart { get; set; }
        public long domainLookupStart { get; set; }
        public long domainLookupEnd { get; set; }
        public long connectStart { get; set; }
        public long connectEnd { get; set; }
        public long secureConnectionStart { get; set; }
        public long requestStart { get; set; }
        public long responseStart { get; set; }
        public long responseEnd { get; set; }
        public long domLoading { get; set; }
        public long domInteractive { get; set; }
        public long domContentLoadedEventStart { get; set; }
        public long domContentLoadedEventEnd { get; set; }
        public long domComplete { get; set; }
        public long loadEventStart { get; set; }
        public long loadEventEnd { get; set; }

        [Prometheus] public long GetResponseTime => responseEnd - navigationStart;
        [Prometheus] public long GetLoadEventEnd => loadEventEnd - navigationStart;
        [Prometheus] public long GetDomInteractive => domInteractive - navigationStart;
        [Prometheus] public long GetDomContentLoadedEventEnd => domContentLoadedEventEnd - navigationStart;
        [Prometheus] public long GetDnsTime => domainLookupEnd - domainLookupStart;
        [Prometheus] public long GetTcpTime => connectEnd - connectStart;
        [Prometheus] public long GetWaitingTime => responseStart - requestStart;
        [Prometheus] public long GetContentTime => responseEnd - responseStart;
        [Prometheus] public long GetTTFB => responseStart - requestStart;
        [Prometheus] public long GetNetworkTime => GetDnsTime + GetTcpTime + GetWaitingTime + GetContentTime;

        /*
            metrics.dnsTime = timing.domainLookupEnd - timing.domainLookupStart;
            metrics.tcpTime = timing.connectEnd - timing.connectStart;
            metrics.waitingTime = timing.responseStart - timing.requestStart;
            metrics.contentTime = timing.responseEnd - timing.responseStart;
            metrics.networkTime = (metrics.dnsTime + metrics.tcpTime + metrics.waitingTime + metrics.contentTime);
        */
    }
}
