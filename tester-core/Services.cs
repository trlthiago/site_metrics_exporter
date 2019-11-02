using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using tester_core.Models;

namespace tester_core
{
    public class Services : IDisposable
    {

        public static string ChromePath = string.Empty;

        //https://automationrhapsody.com/performance-testing-in-the-browser/

        private Browser _browser { get; set; }

        private Browser Browser
        {
            get
            {
                if (_browser == null)
                {
                    var options = new LaunchOptions { Headless = true, Devtools = false, ExecutablePath = ChromePath };
                    if (string.IsNullOrWhiteSpace(ChromePath))
                        options.Args = new string[] { "--no-sandbox" };
                    _browser = Puppeteer.LaunchAsync(options).Result;
                }
                return _browser;
            }
        }

        public Services()
        {
            if (string.IsNullOrWhiteSpace(ChromePath))
                DownloadChromeAsync().Wait();
        }

        private async Task DownloadChromeAsync()
        {
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
        }

        private async Task EnsurePageLooksOkUnsafe(Page page, Response response)
        {
            //Console.WriteLine("a");
            string title = await page.GetTitleAsync();
            //Console.WriteLine("b");

            if (title.StartsWith("503"))
                throw new Exception("The page title starts with 503.");

            if ((int)response.Status >= 300)
                throw new Exception("The Status Code is greater than 300.");
        }

        public async Task<SiteMetrics> AccessPage(string url)
        {
            var siteMetrics = new SiteMetrics();
            var page = await Browser.NewPageAsync();

            try
            {
                string rsp = "";
                Response response;
                response = await page.GoToAsync(url); //, WaitUntilNavigation.Networkidle0
                string text;
                if (response == null)
                {
                    try
                    {
                        rsp += "response null; ";
                        response = await page.WaitForNavigationAsync(new NavigationOptions { WaitUntil = new WaitUntilNavigation[] { WaitUntilNavigation.Networkidle0 }, Timeout = 60000 });
                        rsp += "get content after waiting time; ";
                        text = await page.GetContentAsync();
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }

                var havestatusRedirect = (int)response.Status >= 300;

                var haveRedirect = await page.QuerySelectorAsync("meta[http-equiv=refresh]");
                if (haveRedirect != null)
                {
                    rsp += "REDIRECT!; ";
                    response = await page.WaitForNavigationAsync(new NavigationOptions { WaitUntil = new WaitUntilNavigation[] { WaitUntilNavigation.Networkidle0 }, Timeout = 60000 });
                }

                if (response == null)
                    throw new Exception("Null Response for " + url);

                await EnsurePageLooksOkUnsafe(page, response);

                siteMetrics.SiteStatus += rsp + response.Status.ToString();
                return siteMetrics;
            }
            catch (Exception e)
            {
                siteMetrics.SiteStatus += e.Message;
                return siteMetrics;
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        public async Task<Dictionary<string, decimal>> AccessPageAndGetMetrics(string url)
        {
            var page = await Browser.NewPageAsync();

            Response response;

            try
            {
                response = await page.GoToAsync(url);
                if (response == null)
                    throw new Exception("Null Response for " + url);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await page.CloseAsync();
                throw e;
            }

            var metrics2 = await page.MetricsAsync();

            Console.WriteLine("Status Code: " + response.Status);

            //await page.ScreenshotAsync("D:\\screens\\" + new Uri(url).Host + ".png");

            await page.CloseAsync();

            return metrics2;
        }

        public async Task<Dictionary<string, decimal>> AccessPageAndGetMetricsByTrl(string url)
        {
            var page = await Browser.NewPageAsync();

            CDPSession cpd = await page.Target.CreateCDPSessionAsync();

            await cpd.SendAsync("Performance.enable");

            var metrics1 = await MetricsAsync(cpd);

            Response response;

            try
            {
                response = await page.GoToAsync(url);
                if (response == null)
                    throw new Exception("Null Response for " + url);
                var metrics2 = await MetricsAsync(cpd);
                System.Threading.Thread.Sleep(10000);
                var metrics3 = await MetricsAsync(cpd);

                foreach (var metric1 in metrics1)
                {
                    var m2 = metrics2[metric1.Key];
                    var m3 = metrics3[metric1.Key];
                    Console.WriteLine($"{metric1.Key,30} | {metric1.Value,15} | {m2,15} | {m3,15}");
                }
                return metrics2;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        public async void AccessPageAndTrace(string url)
        {
            var page = await Browser.NewPageAsync();

            await page.Tracing.StartAsync(new TracingOptions { Path = "D:\\screens\\trace.json" });

            Response response;

            try
            {
                response = await page.GoToAsync(url);
                if (response == null)
                    throw new Exception("Null Response for " + url);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await page.CloseAsync();
                return;
            }

            await page.Tracing.StopAsync();

            Console.WriteLine("Trying close the " + url);
            await page.CloseAsync();
        }

        public async Task<List<AssetPerformance>> AccessPageAndGetTiming(string url)
        {
            var page = await Browser.NewPageAsync();

            try
            {
                Response response;

                response = await page.GoToAsync(url, WaitUntilNavigation.Networkidle0);

                if (response == null)
                    throw new Exception("Null Response for " + url);

                //https://github.com/GoogleChrome/puppeteer/issues/417
                //var entries2 = await page.EvaluateExpressionAsync<object>("performance.getEntries().filter(e => e.entryType === 'resource').map(e => {e.name})");
                //var entries2 = await page.EvaluateExpressionAsync<object>("performance.getEntries().filter(e => e.entryType !== 'resource').map(e => e.name)");
                //var entries1 = await page.EvaluateExpressionAsync<string[]>("performance.getEntries().map(e=> JSON.stringify(e, null, 2))");
                var entries2 = await page.EvaluateExpressionAsync<string>("JSON.stringify(performance.timing)");

                var metric = Newtonsoft.Json.JsonConvert.DeserializeObject<PerformanceMetrics>(entries2);


                //var entries2 = await page.EvaluateExpressionAsync<object>("performance.getEntries().filter(e => e.entryType !== 'resource').map(e => e.name)");
                var entries1 = await page.EvaluateExpressionAsync<string[]>("performance.getEntries().filter(e => e.entryType === 'navigation').map(e=> JSON.stringify(e, null, 2))");

                List<AssetPerformance> assetsPerf = new List<AssetPerformance>();

                foreach (var entry in entries1)
                {
                    var b = Newtonsoft.Json.JsonConvert.DeserializeObject<AssetPerformance>(entry);
                    assetsPerf.Add(b);
                }


                //List<AssetPerformance> assetsPerf = new List<AssetPerformance>();

                //foreach (var entry in entries1)
                //{
                //    var b = Newtonsoft.Json.JsonConvert.DeserializeObject<AssetPerformance>(entry);
                //    assetsPerf.Add(b);
                //}

                return assetsPerf;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        public async Task<List<AssetPerformance>> AccessPageAndGetResources(string url)
        {
            var page = await Browser.NewPageAsync();

            try
            {
                Response response;

                response = await page.GoToAsync(url, WaitUntilNavigation.Networkidle0);

                // await page.WaitForNavigationAsync(new NavigationOptions { WaitUntil = new WaitUntilNavigation[] { WaitUntilNavigation.Networkidle0 }, Timeout = 60000 });

                if (response == null)
                    throw new Exception("Null Response for " + url);

                return await GetEntriesMetrics(page);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                await page.CloseAsync();
            }
        }
      
        private static async Task<List<AssetPerformance>> GetNavigationMetrics(Page page)
        {
            //https://github.com/GoogleChrome/puppeteer/issues/417
            var entries1 = await page.EvaluateExpressionAsync<string[]>("performance.getEntries().filter(e => e.entryType === 'navigation').map(e=> JSON.stringify(e, null, 2))");

            List<AssetPerformance> assetsPerf = new List<AssetPerformance>();

            foreach (var entry in entries1)
            {
                var b = Newtonsoft.Json.JsonConvert.DeserializeObject<AssetPerformance>(entry);
                assetsPerf.Add(b);
            }

            return assetsPerf;
        }

        private static async Task<List<AssetPerformance>> GetEntriesMetrics(Page page)
        {
            //https://github.com/GoogleChrome/puppeteer/issues/417
            var entries1 = await page.EvaluateExpressionAsync<string[]>("performance.getEntries().map(e=> JSON.stringify(e, null, 2))");

            List<AssetPerformance> assetsPerf = new List<AssetPerformance>();

            foreach (var entry in entries1)
            {
                var b = Newtonsoft.Json.JsonConvert.DeserializeObject<AssetPerformance>(entry);
                assetsPerf.Add(b);
            }

            return assetsPerf;
        }
        public async Task<SiteMetrics> AccessPageAndTakeScreenshot(string url, bool takeScreenShot = false)
        {
            var siteMetrics = new SiteMetrics();
            var page = await Browser.NewPageAsync();

            try
            {
                string rsp = "";
                Response response;
                response = await page.GoToAsync(url); //, WaitUntilNavigation.Networkidle0
                string text;
                if (response == null /*|| response.Request.Url != url*/)
                {
                    try
                    {
                        rsp += "response null; ";
                        response = await page.WaitForNavigationAsync(new NavigationOptions { WaitUntil = new WaitUntilNavigation[] { WaitUntilNavigation.Networkidle0 }, Timeout = 60000 });
                        rsp += "get content after waiting time; ";
                        text = await page.GetContentAsync();
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }

                var havestatusRedirect = (int)response.Status >= 300;

                var haveRedirect = await page.QuerySelectorAsync("meta[http-equiv=refresh]");
                if (haveRedirect != null)
                {
                    rsp += "META-REDIRECT!; ";
                    response = await page.WaitForNavigationAsync(new NavigationOptions { WaitUntil = new WaitUntilNavigation[] { WaitUntilNavigation.Networkidle0 }, Timeout = 60000 });
                }

                if (response == null)
                    throw new Exception("Null Response for " + url);

                await EnsurePageLooksOkUnsafe(page, response);
                siteMetrics.SiteStatus += response.Status.ToString();
                siteMetrics.Assets = await GetNavigationMetrics(page);

                return siteMetrics;
            }
            catch (PuppeteerSharp.NavigationException e) when (e.Message.Contains("net::ERR_NAME_NOT_RESOLVED") || e.Message.Contains("net::ERR_CONNECTION_REFUSED") || e.Message.Contains("net::ERR_CONNECTION_TIMED_OUT") || e.Message.Contains("net::ERR_CERT_DATE_INVALID"))
            {
                Console.WriteLine(e.Message);
                siteMetrics.SiteStatus += e.Message;
                return siteMetrics;
            }
            catch (Exception e) when (!e.Message.Contains("net::ERR_NAME_NOT_RESOLVED"))
            {
                if (takeScreenShot)
                {
                    if (!System.IO.Directory.Exists("screens"))
                        System.IO.Directory.CreateDirectory("screens");

                    siteMetrics.ScreenShotPath = System.IO.Path.Combine("screens", new Uri(url).Host + DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss") + ".png");
                    await page.ScreenshotAsync(siteMetrics.ScreenShotPath);
                }

                siteMetrics.SiteStatus += e.Message;
                return siteMetrics;
            }

            finally
            {
                await page.CloseAsync();
            }
        }

        public async Task<SiteMetrics> AccessPageAndGetResourcesFull(string url, bool takeScreenShot = false)
        {
            var siteMetrics = new SiteMetrics();
            var page = await Browser.NewPageAsync();

            try
            {
                string rsp = "";
                Response response;
                response = await page.GoToAsync(url); //, WaitUntilNavigation.Networkidle0
                string text;
                if (response == null /*|| response.Request.Url != url*/)
                {
                    try
                    {
                        rsp += "response null; ";
                        response = await page.WaitForNavigationAsync(new NavigationOptions { WaitUntil = new WaitUntilNavigation[] { WaitUntilNavigation.Networkidle0 }, Timeout = 60000 });
                        rsp += "get content after waiting time; ";
                        text = await page.GetContentAsync();
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }

                if (response == null)
                    throw new Exception("Null Response for " + url);

                var havestatusRedirect = (int)response.Status >= 300;

                var haveRedirect = await page.QuerySelectorAsync("meta[http-equiv=refresh]");
                if (haveRedirect != null)
                {
                    rsp += "META-REDIRECT!; ";
                    response = await page.WaitForNavigationAsync(new NavigationOptions { WaitUntil = new WaitUntilNavigation[] { WaitUntilNavigation.Networkidle0 }, Timeout = 60000 });
                }

                await EnsurePageLooksOkUnsafe(page, response);
                siteMetrics.SiteStatus += response.Status.ToString();
                siteMetrics.Assets = await GetEntriesMetrics(page);

                return siteMetrics;
            }
            catch (PuppeteerSharp.NavigationException e) when (e.Message.Contains("net::ERR_NAME_NOT_RESOLVED") || e.Message.Contains("net::ERR_CONNECTION_REFUSED") || e.Message.Contains("net::ERR_CONNECTION_TIMED_OUT"))
            {
                Console.WriteLine(e.Message);
                siteMetrics.SiteStatus += e.Message;
                return siteMetrics;
            }
            catch (Exception e) when (!e.Message.Contains("net::ERR_NAME_NOT_RESOLVED"))
            {
                if (takeScreenShot)
                {
                    if (!System.IO.Directory.Exists("screens"))
                        System.IO.Directory.CreateDirectory("screens");

                    siteMetrics.ScreenShotPath = System.IO.Path.Combine("screens", new Uri(url).Host + DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss") + ".png");
                    await page.ScreenshotAsync(siteMetrics.ScreenShotPath);
                }

                siteMetrics.SiteStatus += e.Message;
                return siteMetrics;
            }
           
            finally
            {
                await page.CloseAsync();
            }
        }

        private bool IsInFailureSafe(Page page, Response response)
        {
            if (page.GetTitleAsync().Result.StartsWith("503"))
                return true;

            if ((int)response.Status >= 300)
                return true;

            return false;
        }

        private async Task<Dictionary<string, decimal>> MetricsAsync(CDPSession cdp)
        {
            var response = await cdp.SendAsync<PerformanceGetMetricsResponse>("Performance.getMetrics");
            return BuildMetricsObject(response.Metrics);
        }

        private Dictionary<string, decimal> BuildMetricsObject(List<KeyValue> metrics)
        {
            var result = new Dictionary<string, decimal>();

            foreach (var item in metrics)
            {
                result.Add(item.Name, item.Value);
            }

            return result;
        }

        public async void Dispose()
        {
            if (_browser != null)
            {
                await _browser.CloseAsync();
                _browser.Dispose();
            }
        }
    }
}
