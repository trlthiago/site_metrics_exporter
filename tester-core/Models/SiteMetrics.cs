using System.Collections.Generic;

namespace tester_core.Models
{
    public class SiteMetrics
    {
        public string SiteStatus { get; set; }
        
        public List<AssetPerformance> Assets { get; set; }

        public int IsSiteUp => SiteStatus == "OK" ? 1 : 0;

        public string ScreenShotPath { get; internal set; }
    }
}
