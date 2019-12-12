using System.Collections.Generic;

namespace tester_core.Models
{
    public class SiteMetrics
    {
        public int Status { get; set; }

        public string SiteStatus { get; set; }

        public List<AssetPerformance> Assets { get; set; }

        public int IsSiteUp => SiteStatus == "OK" ? 1 : 0;

        public int IsSiteStatus5xx
        {
            get
            {
                if (Title != null && Title.StartsWith("503"))
                    return 1;

                if (Status >= 500)
                    return 1;

                return 0;
            }
        }

        public string ScreenShotPath { get; internal set; }

        public string Title { get; internal set; }
    }
}
