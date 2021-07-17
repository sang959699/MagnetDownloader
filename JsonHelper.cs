using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MagnetDownloader.Model.Json;
using Newtonsoft.Json;

namespace MagnetDownloader.Helper
{
    class JsonHelper
    {
        readonly static string DownloadedFilePath = "DownloadedFile.json";
        readonly static string ConfigPath = "Config.json";
        readonly static string VideoRegexPath = "VideoRegex.json";

        public static void Print(string msg) {
            Console.WriteLine($"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")} {msg}");
        }

        public static void JsonInit() {
            if (!File.Exists(DownloadedFilePath)) {
                File.Create(DownloadedFilePath);
                File.WriteAllText(DownloadedFilePath, "[]");
            }
            if (!File.Exists(ConfigPath)) {
                File.Create(ConfigPath);
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(new Config()));
            }
            if (!File.Exists(VideoRegexPath)) {
                File.Create(VideoRegexPath);
                File.WriteAllText(VideoRegexPath, "[]");
            }
        }

        #region DownloadedFile
        public static List<DownloadedFile> DownloadedFileList {
            get {
                var result =  new List<DownloadedFile>();
                try {
                    var json = File.ReadAllText(DownloadedFilePath);

                    result = JsonConvert.DeserializeObject<List<DownloadedFile>>(json);
                    if (result == null)  return new List<DownloadedFile>();
                } catch (Exception ex) {
                    Print($"Error when extracting DownloadedFileList, Exception: {ex}");
                }
                return result;
            }
        }
    
        public static bool SaveDownloadedFile(string fileName) {
            var temp = DownloadedFileList;
            temp.Add(new DownloadedFile(fileName, DateTime.Now));

            File.WriteAllText(DownloadedFilePath, JsonConvert.SerializeObject(temp));
            return true;
        }
        #endregion
    
        #region Config
        public static Config Config {
            get {
                var result = new Config();
                try {
                    var json = File.ReadAllText(ConfigPath);

                    result = JsonConvert.DeserializeObject<Config>(json);
                    if (result == null)  return new Config();
                } catch (Exception ex) {
                    Print($"Error when extracting Config, Exception: {ex}");
                }
                return result;
            }
        }
    
        public static bool SaveConfig(Config model) {
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(model));
            return true;
        }
        
        public static bool DisableRunImmediately() {
            var model = Config;
            model.RunImmediately = false;
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(model));
            return true;
        }
        #endregion

        #region VideoRegex
        public static List<string> VideoRegex {
            get {
                var result = new List<string>();
                try {
                    var json = File.ReadAllText(VideoRegexPath);

                    result = JsonConvert.DeserializeObject<List<string>>(json);
                    if (result == null)  return new List<string>();
                }
                catch (Exception ex) {
                    Print($"Error when extracting VideoRegex, Exception: {ex}");
                }
                return result;
            }
        }
        #endregion
    }
}
