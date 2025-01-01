using System;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using MagnetDownloader.Helper;
using MagnetDownloader.Model.Json;
using Newtonsoft.Json;
using System.Web;
using System.Collections.Generic;

namespace MagnetDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            JsonHelper.JsonInit();
            Task.Run(async () =>
            {
                await Process();
            }).GetAwaiter().GetResult();
        }
        public static async Task Process()
        {
            var playTime = DateTime.Now;
            while (true)
            {
                try
                {
                    var runImmediately = JsonHelper.Config.RunImmediately;
                    if (!runImmediately && DateTime.Now < playTime) {
                        await Task.Delay(10000);
                        continue;
                    }
                    JsonHelper.Print("runImmediately: " + runImmediately);
                    JsonHelper.Print("DownloadMagnet Start");
                    DownloadMagnet();
                    JsonHelper.Print("DownloadMagnet End");
                    playTime = DateTime.Now.AddMinutes(15);
                    if (runImmediately) JsonHelper.DisableRunImmediately();
                    JsonHelper.Print($"Next Run {playTime.ToString("dd/MM/yyyy HH:mm:ss")}");
                } catch (Exception) {
                    playTime = DateTime.Now.AddMinutes(15);
                    JsonHelper.Print($"Network down");
                    JsonHelper.Print($"Retry on {playTime:dd/MM/yyyy HH:mm:ss}");
                }

                await Task.Delay(10000);
            }
        }

        static void DownloadMagnet() {
            var urlList = JsonHelper.Config.RssUrl;
            var failedUrlList = new List<string>();
            var latestDownloadedDt = JsonHelper.GetLatestSuccessfulRunDt();
            var searchAll = JsonHelper.Config.SearchAll;
            JsonHelper.Print("searchAll: " + searchAll);

            foreach(var url in urlList) {
                SyndicationFeed feed = null;
                try {
                    JsonHelper.Print($"Accessing {url}");
                    using var reader = XmlReader.Create(url);
                    feed = SyndicationFeed.Load(reader);
                    if (!feed.Items.Any()) throw new Exception("0 feed in this url");
                } catch (Exception ex) {
                    JsonHelper.Print($"Error occured, Exception {ex}");
                    failedUrlList.Add(url);
                    continue;
                }

                if (!searchAll) feed.Items = feed.Items.Where(w => w.PublishDate >= latestDownloadedDt);
                JsonHelper.Print($"Feed Items Count: {feed.Items.Count()}");
                foreach(var item in feed.Items) {
                    foreach(var regexData in JsonHelper.VideoRegexList) {
                        var regex = new Regex(regexData.FileRegex);
                        string titleText = HttpUtility.HtmlDecode(item.Title.Text);
                        if (regex.IsMatch(titleText) && !JsonHelper.DownloadedFileList.Any(a => a.FileName == titleText)) {
                            var magnetLink = GetMagnetLink(item.Links.ToArray()).ToString();
                            var isResponseSuccess = AddDownload(magnetLink, regexData.DownloadPath);
                            if (isResponseSuccess) {
                                JsonHelper.DeleteFailedDownloadedFile(titleText);
                                SaveDownloadedName(HttpUtility.HtmlDecode(titleText), url, regexData.DownloadPath);
                                JsonHelper.Print($"Downloading: {titleText}");
                            }
                            else {
                                JsonHelper.SaveFailedDownloadedFile(titleText, magnetLink, regexData.DownloadPath, url);
                                JsonHelper.Print($"AddDownload Failed: {titleText}");
                            }
                            break;
                        }
                    }
                }
            }
            var FailedDownloadedFileList = JsonHelper.FailedDownloadedFileList;
            JsonHelper.Print($"Scan Failed Download File List, Count: {FailedDownloadedFileList.Count}");
            foreach(var item in FailedDownloadedFileList) {
                var isResponseSuccess = AddDownload(item.MagnetLink, item.DownloadPath);
                if (isResponseSuccess) {
                    JsonHelper.DeleteFailedDownloadedFile(item.FileName);
                    SaveDownloadedName(item.FileName, item.Source, item.DownloadPath);
                    JsonHelper.Print($"Downloading: {item.FileName}");
                }
                else JsonHelper.Print($"AddDownload Failed: {item.FileName}");
            }
            if (failedUrlList.Count != urlList.Length) JsonHelper.SaveLatestSuccessfulRunDt();
            if (searchAll) JsonHelper.DisableSearchAll();
        }

        static Uri GetMagnetLink(SyndicationLink[] links){
            return links.Where(w => w.MediaType == "application/x-bittorrent").Select(s => s.Uri).FirstOrDefault();
        }

        static bool SaveDownloadedName(string name, string source, string downloadPath){
            JsonHelper.SaveDownloadedFile(name, source, downloadPath);
            return true;
        }

        static bool AddDownload(string url, string downloadPath) {
            var result = false;
            try {
                using (HttpClient http = new HttpClient()){
                    var temp = new AriaAddUri();
                    temp.Params.Add($"token:{JsonHelper.Config.AriaJsonRpcToken}");
                    temp.Params.Add(new string[] {
                        url
                    });
                    if (downloadPath != null) temp.Params.Add(new Dictionary<string, string> {{ "dir", JsonHelper.Config.DownloadPathPrefix + downloadPath }});
                    StringContent httpContent = new StringContent(JsonConvert.SerializeObject(temp), System.Text.Encoding.UTF8, "application/json");
                    var response = http.PostAsync(JsonHelper.Config.AriaJsonRpcUrl, httpContent).Result;
                    result = response.IsSuccessStatusCode;
                }
            } catch (Exception ex) {
                Console.WriteLine(ex);
                Console.WriteLine(ex.InnerException);
            }
            return result;
        }
    }
}
