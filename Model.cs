using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MagnetDownloader.Model.Json {
    public class VideoRegex {
        public string FileRegex { get; set; }
        public string DownloadPath { get; set; }
        public VideoRegex(string fileRegex, string downloadPath) {
            FileRegex = fileRegex;
            DownloadPath = downloadPath;
        }
        public VideoRegex(string fileRegex){
            FileRegex = fileRegex;
        }
    }
    public class DownloadedFile{
        public string FileName { get; set; }
        public DateTime DownloadedDT { get; set; }
        public string Source { get; set; }
        public string DownloadPath { get; set; }
        public DownloadedFile(string fileName, DateTime downloadedDT, string source, string downloadPath)
        {
            FileName = fileName;
            DownloadedDT = downloadedDT;
            Source = source;
            DownloadPath = downloadPath;
        }
    }
    public class FailedDownloadedFile{
        public string FileName { get; set; }
        public string MagnetLink { get; set; }
        public DateTime AttemptDT { get; set; }
        public string DownloadPath { get; set; }
        public string Source { get; set; }
        public FailedDownloadedFile(string fileName, string magnetLink, DateTime attemptDT, string downloadPath, string source) {
            FileName = fileName;
            MagnetLink = magnetLink;
            AttemptDT = attemptDT;
            DownloadPath = downloadPath;
            Source = source;
        }
    }

    public class AriaAddUri {
        [JsonProperty("jsonrpc")]
        public string JsonRpc { get => "2.0"; }

        [JsonProperty("method")]
        public string Method { get => "aria2.addUri"; }

        [JsonProperty("id")]
        public string Id { get => string.Empty; }

        [JsonProperty("params")]
        public List<dynamic> Params { get; set; }

        public AriaAddUri(){
            this.Params = new List<dynamic>();
        }
    }

    public class Config {
        public string AriaJsonRpcUrl { get; set; }
        public string AriaJsonRpcToken { get; set; }
        public bool RunImmediately { get; set; }
        public string[] RssUrl { get; set; }
        public string DownloadPathPrefix { get; set; }
        public string DownloadPathDelimiter { get; set; }
    }
}