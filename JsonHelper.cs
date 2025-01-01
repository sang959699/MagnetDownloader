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
        readonly static string DownloadedFilePath = "Config/DownloadedFile.json";
        readonly static string FailedDownloadedFilePath = "Config/FailedDownloadedFile.json";
        readonly static string ConfigPath = "Config/Config.json";
        readonly static string VideoRegexPath = "Config/VideoRegex.json";

        public static void Print(string msg) {
            Console.WriteLine($"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")} {msg}");
        }

        public static void JsonInit() {
            if (!File.Exists(DownloadedFilePath)) {
                File.WriteAllText(DownloadedFilePath, "[]");
            }
            if (!File.Exists(FailedDownloadedFilePath)) {
                File.WriteAllText(FailedDownloadedFilePath, "[]");
            }
            if (!File.Exists(ConfigPath)) {
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(new Config()));
            }
            if (!File.Exists(VideoRegexPath)) {
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
    
        public static bool SaveDownloadedFile(string fileName, string source, string downloadPath) {
            var temp = DownloadedFileList;
            temp.Insert(0, new DownloadedFile(fileName, DateTime.Now, source, downloadPath));

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

        public static bool DisableSearchAll() {
            var model = Config;
            model.SearchAll = false;
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(model));
            return true;
        }

        public static DateTime GetLatestSuccessfulRunDt() {
            var result = Config.LatestSuccessfulRunDt;
            if (result == null) Print($"No Latest Download Config");
            else Print($"Latest Download DT: {Config.LatestSuccessfulRunDt:dd/MM/yyyy HH:mm:ss}");
            return Config.LatestSuccessfulRunDt ?? DateTime.Now;
        }

        public static bool SaveLatestSuccessfulRunDt() {
            var model = Config;
            model.LatestSuccessfulRunDt = DateTime.Now;
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(model));
            Print($"Latest Download DT Saved: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            return true;
        }
        #endregion

        #region VideoRegex
        public static List<VideoRegex> VideoRegexList {
            get {
                var result = new List<VideoRegex>();
                try {
                    var json = File.ReadAllText(VideoRegexPath);

                    var tempResult = JsonConvert.DeserializeObject<List<string>>(json);
                    if (tempResult != null) {
                        tempResult.ForEach(f => {
                            if (f.Contains(Config.DownloadPathDelimiter)) {
                                var splitted = f.Split(Config.DownloadPathDelimiter);
                                result.Add(new VideoRegex(splitted[0], splitted[1]));
                            } else {
                                result.Add(new VideoRegex(f));
                            }
                        });
                    }
                }
                catch (Exception ex) {
                    Print($"Error when extracting VideoRegex, Exception: {ex}");
                }
                return result;
            }
        }
        #endregion

        #region FailedDownloadedFile
        public static List<FailedDownloadedFile> FailedDownloadedFileList {
            get {
                var result =  new List<FailedDownloadedFile>();
                try {
                    var json = File.ReadAllText(FailedDownloadedFilePath);

                    result = JsonConvert.DeserializeObject<List<FailedDownloadedFile>>(json);
                    if (result == null)  return new List<FailedDownloadedFile>();
                } catch (Exception ex) {
                    Print($"Error when extracting FailedDownloadedFileList, Exception: {ex}");
                }
                return result;
            }
        }
    
        public static bool SaveFailedDownloadedFile(string fileName, string magnetLink, string downloadPath, string source) {
            var temp = FailedDownloadedFileList;
            if (!temp.Exists(e => e.FileName == fileName)) temp.Add(new FailedDownloadedFile(fileName, magnetLink, DateTime.Now, downloadPath, source));

            File.WriteAllText(FailedDownloadedFilePath, JsonConvert.SerializeObject(temp));
            return true;
        }

        public static bool DeleteFailedDownloadedFile(string fileName) {
            var temp = FailedDownloadedFileList;
            temp.RemoveAll(r => r.FileName == fileName);

            File.WriteAllText(FailedDownloadedFilePath, JsonConvert.SerializeObject(temp));
            return true;
        }
        #endregion
    }
}
