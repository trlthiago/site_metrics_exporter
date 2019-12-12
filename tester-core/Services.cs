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
        private readonly ScreenshotService _screenshotService;

        //https://automationrhapsody.com/performance-testing-in-the-browser/

        private Browser _browser { get; set; }

        private Browser Browser
        {
            get
            {
                if (_browser != null)
                {
                    try
                    {
                        if (_browser.IsClosed || !_browser.IsConnected)
                        {
                            _browser.Dispose();
                            _browser.CloseAsync().ConfigureAwait(false);
                            _browser = null;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }

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

        public Services(ScreenshotService screenshotService)
        {
            _screenshotService = screenshotService;

            if (string.IsNullOrWhiteSpace(ChromePath))
                DownloadChromeAsync().Wait();
        }

        public async Task<Page[]> GetTabsAsync()
        {
            if (_browser == null)
                return new Page[0];
            return await _browser.PagesAsync();
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

            //if (title.StartsWith("503"))
            //    throw new Exception("Page title starts with 503");

            //if ((int)response.Status >= 300)
            //    throw new Exception("Status >= 300; " + (int)response.Status);
            EnsurePageLooksOkUnsafe(title, (int)response.Status);
        }

        private void EnsurePageLooksOkUnsafe(string title, int status)
        {
            if (title.StartsWith("503"))
                throw new Exception("Page title starts with 503");

            if (status >= 300)
                throw new Exception("Status >= 300; " + status);
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


        private static async Task<List<AssetPerformance>> GetEntriesMetrics(Page page)
        {
            return await GetMetrics(page, "performance.getEntries().map(e=> JSON.stringify(e, null, 2))");
        }

        private static async Task<List<AssetPerformance>> GetNavigationMetrics(Page page)
        {
            return await GetMetrics(page, "performance.getEntries().filter(e => e.entryType === 'navigation').map(e=> JSON.stringify(e, null, 2))");
        }

        private static async Task<List<AssetPerformance>> GetMetrics(Page page, string query)
        {
            //https://github.com/GoogleChrome/puppeteer/issues/417
            var entries1 = await page.EvaluateExpressionAsync<string[]>(query);

            List<AssetPerformance> assetsPerf = new List<AssetPerformance>();

            foreach (var entry in entries1)
            {
                var b = Newtonsoft.Json.JsonConvert.DeserializeObject<AssetPerformance>(entry);
                assetsPerf.Add(b);
            }

            return assetsPerf;
        }

        public async Task<SiteMetrics> AccessPageAndTakeScreenshot(string url, bool takeScreenshotOnError = false)
        {
            var siteMetrics = new SiteMetrics();
            var page = await Browser.NewPageAsync();
            await page.SetCacheEnabledAsync(false);

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

                var havestatusRedirect = (int)response.Status >= 300 && (int)response.Status <= 399;
                var tesste = await page.GetContentAsync();
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
            catch (PuppeteerSharp.NavigationException e) when (e.Message.StartsWith("net::"))
            {
                // when (e.Message.Contains("net::ERR_NAME_NOT_RESOLVED") || e.Message.Contains("net::ERR_CONNECTION_REFUSED") || e.Message.Contains("net::ERR_CONNECTION_TIMED_OUT") || e.Message.Contains("net::ERR_CERT_DATE_INVALID"))
                //Console.WriteLine(e.Message);
                siteMetrics.SiteStatus += e.Message.Split(" ")[0];
                return siteMetrics;
            }
            catch (Exception e)
            {
                if (takeScreenshotOnError)
                {
                    siteMetrics.ScreenShotPath = _screenshotService.GetScreenshotPath(url);
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

        private async Task<Response> GetRedirectionResponseIfNeed(Page page, Response response, bool waitNetworkIdle)
        {
            try
            {
                //var havestatusRedirect = (int)response.Status >= 300 && (int)response.Status <= 399; //ñ deveria-se esperar aqui tb?
                var haveRedirect = await page.QuerySelectorAsync("meta[http-equiv=refresh]");
                if (haveRedirect != null)
                {
                    //rsp += "META-REDIRECT!; ";
                    return await page.WaitForNavigationAsync(new NavigationOptions { WaitUntil = new WaitUntilNavigation[] { waitNetworkIdle ? WaitUntilNavigation.Networkidle0 : WaitUntilNavigation.Load }, Timeout = 30000 });
                }
                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return response;
                //return await page.WaitForNavigationAsync(new NavigationOptions { WaitUntil = new WaitUntilNavigation[] { WaitUntilNavigation.Networkidle0 }, Timeout = 60000 });
            }
        }

        public async Task<SiteMetrics> AccessPageAndGetResourcesFull(string url, bool waitNetworkIdle, bool getEntriesMetrics, bool getNavigationMetrics, bool takeScreenshotOnError, bool takeScreenshoOnSuccess)
        {
            var siteMetrics = new SiteMetrics();
            var page = await Browser.NewPageAsync();
            await page.SetCacheEnabledAsync(false);
            string rsp = string.Empty;

            try
            {
                Response response;
                response = waitNetworkIdle
                            ? await page.GoToAsync(url, WaitUntilNavigation.Networkidle0)
                            : await page.GoToAsync(url);
                string text;
                if (response == null /*|| response.Request.Url != url*/)
                {
                    try
                    {
                        rsp += "response null; ";
                        response = await page.WaitForNavigationAsync(new NavigationOptions { WaitUntil = new WaitUntilNavigation[] { waitNetworkIdle ? WaitUntilNavigation.Networkidle0 : WaitUntilNavigation.Load }, Timeout = 30000 });
                        rsp += "get content after waiting time; ";
                        text = await page.GetContentAsync();
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }

                response = await GetRedirectionResponseIfNeed(page, response, waitNetworkIdle);

                if (response == null)
                    throw new Exception("Null response for " + url);

                siteMetrics.Title = await page.GetTitleAsync();
                siteMetrics.Status = (int)response.Status;

                EnsurePageLooksOkUnsafe(siteMetrics.Title, siteMetrics.Status);

                siteMetrics.SiteStatus = /*rsp +*/ response.Status.ToString();

                if (getEntriesMetrics) //precendence coz NavigationMetrics is a subgroup of this one.
                    siteMetrics.Assets = await GetEntriesMetrics(page);
                else if (getNavigationMetrics)
                    siteMetrics.Assets = await GetNavigationMetrics(page);

                return siteMetrics;
            }
            catch (NavigationException e) when (e.Message.StartsWith("net::"))
            {
                siteMetrics.SiteStatus += e.Message.Split(" ")[0];
                siteMetrics.Status = 999;
                return siteMetrics;
            }
            catch (Exception e)
            {
                if (takeScreenshotOnError)
                {
                    siteMetrics.ScreenShotPath = _screenshotService.GetScreenshotPath(url);
                    await page.ScreenshotAsync(siteMetrics.ScreenShotPath);
                }

                siteMetrics.SiteStatus += e.Message;
                siteMetrics.Status = 999;
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
                _browser.Closed += BrowserClosedEvent;
                await _browser.CloseAsync();
                _browser.Dispose();
            }
        }

        private void BrowserClosedEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Browser Closed!");
            Console.WriteLine(e);
        }
    }
}
