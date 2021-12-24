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

            foreach(var url in urlList) 
            {
                SyndicationFeed feed = null;
                try {
                    Console.WriteLine($"Accessing {url}");
                    using var reader = XmlReader.Create(url);
                    feed = SyndicationFeed.Load(reader);
                } catch (Exception ex) {
                    Console.WriteLine($"Error occured, Exception {ex}");
                    continue;
                }

                var post = feed.Items.FirstOrDefault();
                var downloadedFileList = JsonHelper.DownloadedFileList;
                foreach(var item in feed.Items) {
                    foreach(var regexString in JsonHelper.VideoRegex) {
                        var regex = new Regex(regexString);
                        if (!downloadedFileList.Any(a => a.FileName == item.Title.Text) && regex.IsMatch(item.Title.Text)) {
                            var magnetLink = GetMagnetLink(item.Links.ToArray());
                            var isResponseSuccess = AddDownload(magnetLink.ToString());
                            if (isResponseSuccess) {
                                SaveDownloadedName(item.Title.Text);
                                JsonHelper.Print($"Downloading: {item.Title.Text}");
                            }
                            else { JsonHelper.Print($"AddDownload Failed: {item.Title.Text}"); }
                        }
                    }
                }
            }
        }

        static Uri GetMagnetLink(SyndicationLink[] links){
            return links.Where(w => w.MediaType == "application/x-bittorrent").Select(s => s.Uri).FirstOrDefault();
        }

        static bool SaveDownloadedName(string name){
            JsonHelper.SaveDownloadedFile(name);
            return true;
        }

        static bool AddDownload(string url) {
            var result = false;
            using (HttpClient http = new HttpClient()){
                var temp = new AriaAddUri();
                temp.Params.Add($"token:{JsonHelper.Config.AriaJsonRpcToken}");
                temp.Params.Add(new string[] {
                    url
                });
                StringContent httpContent = new StringContent(JsonConvert.SerializeObject(temp), System.Text.Encoding.UTF8, "application/json");
                var response = http.PostAsync(JsonHelper.Config.AriaJsonRpcUrl, httpContent).Result;
                result = response.IsSuccessStatusCode;
            }
            return result;
        }
    }
}
