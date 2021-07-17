using System.Collections.Generic;
using Newtonsoft.Json;

namespace MagnetDownloader.Model.Json {
    public class DownloadedFile{
        public string FileName { get; set; }
        public DownloadedFile(string FileName) {
            this.FileName = FileName;
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
        public bool RunImmediately { get; set; }
        public string[] RssUrl { get; set; }
    }
}