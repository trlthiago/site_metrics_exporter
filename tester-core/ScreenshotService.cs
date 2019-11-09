using System;
using System.Collections.Generic;
using System.Text;

namespace tester_core
{
    public class ScreenshotService
    {
        public ScreenshotService(string directory)
        {
            if (!System.IO.Directory.Exists("screens"))
                System.IO.Directory.CreateDirectory("screens");
        }

        public string GetScreenshotPath(string url)
        {
            return System.IO.Path.Combine("screens", new Uri(url).Host + DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss") + ".png");
        }
    }
}
