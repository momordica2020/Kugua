using Kugua.Integrations.NTBot;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;

namespace Kugua.Integrations.AI
{
    /// <summary>
    ///  函数
    /// </summary>
    public partial class LLM
    {
        #region 功能函数


        static bool IsValidUrl(string url)
        {
            string pattern = @"^(https?:\/\/)?(www\.)?([a-zA-Z0-9_-]+(\.[a-zA-Z0-9_-]+)+)(:[0-9]{1,5})?(\/.*)?$";
            return Regex.IsMatch(url, pattern);
        }

        public async Task<string> GetUrlCode(string[] param)
        {
            return HttpUtility.UrlEncode(param[0]);
        }
        public async Task<string> GetWebContent(string[] param)
        {
            string url = "";
            string keyword = "";


            if (param.Length == 2)
            {
                var val = param[0].Trim();
                if (IsValidUrl(val))
                {
                    // input url
                    url = val;
                    if (url.Contains("example.com"))
                    {
                        // fake url
                        url = $"https://www.google.com";
                    }
                    keyword = "";
                }
                else
                {
                    keyword = val;
                    url = $"https://www.google.com";
                }
            }
            else
            {
                url = param[1].Trim();
                keyword = param[0].Trim();
                if (IsValidUrl(keyword))
                {
                    // 颠倒
                    var tmp = keyword;
                    keyword = url;
                    url = tmp;
                }
            }

            var urlHeader = new Dictionary<string, string>
        {
            {"google","https://www.google.com.hk/search?q=" },
            {"baike.baidu","https://baike.baidu.com/search?enc=utf8&word=" },
            {"image.baidu","https://image.baidu.com/search/index?tn=baiduimage&word=" },
            {"tieba.baidu","https://tieba.baidu.com/f/search/res?ie=utf-8&qw=" },
            {"wikipedia","https://zh.wikipedia.org/w/index.php?search=" },
            {"www.baidu","https://www.baidu.com/s?ie=UTF-8&wd=" },
            {"soso.com","https://www.sogou.com/tx?ie=utf8&pid=&query=" },
            {"bilibili","https://search.bilibili.com/all?keyword=" },
            {"bing.com","https://www.bing.com/search?q=" },
        };

            string completeUrl = "";
            foreach (var uh in urlHeader)
            {
                if (url.Contains(uh.Key))
                {
                    completeUrl = $"{uh.Value}{keyword}";
                    //header = uh.Value;
                    //if (header.Contains("//")) url = "";  // 用值替换掉原有url
                    break;
                }
            }
            if (string.IsNullOrWhiteSpace(completeUrl)) completeUrl = $"{url}{keyword}";
            //var url2 = $"{url}{header}{keyword}";




            Logger.Log($"***>>>{completeUrl}", LogType.Debug);

            string htmlContent = await Network.GetHtmlFromUrlAsync($"{completeUrl}");
            string plainText = Network.ConvertHtmlToPlainText(htmlContent);


            if (string.IsNullOrWhiteSpace(plainText))
            {
                // url link may be error.  try raw url
                Logger.Log($"***>>>{url}", LogType.Debug);
                htmlContent = await Network.GetHtmlFromUrlAsync($"{url}");
                plainText = Network.ConvertHtmlToPlainText(htmlContent);
            }


            return string.IsNullOrWhiteSpace(plainText) ? $"访问{url}出错" : plainText;
        }

        public static async Task<string> GetCurrentTimeInTimeZone(string[] param)
        {
            var timeZoneName = param[0].Trim();
            try
            {
                // 获取指定时区的时间信息
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneName);
                // 获取 UTC 当前时间
                DateTime utcNow = DateTime.UtcNow;
                // 转换为指定时区的时间
                DateTime currentTimeInTimeZone = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone);
                return $"{timeZoneName}时间：{currentTimeInTimeZone.ToString("F")}";
            }
            catch (TimeZoneNotFoundException ex)
            {
                Logger.Log(ex);
            }
            catch (InvalidTimeZoneException ex)
            {
                Logger.Log(ex);
            }

            return $"北京时间：{DateTime.Now.ToString("F")}";
        }

        private async Task<string> GetRunPython(string[] param)
        {
            string pythonCode = param[0];

            // 创建进程信息
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "python",
                RedirectStandardInput = true, // 重定向标准输入
                RedirectStandardOutput = true, // 重定向标准输出
                UseShellExecute = false, // 不使用操作系统外壳
                CreateNoWindow = true // 不显示窗口
            };

            // 启动进程
            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start(); // 启动进程

                // 将 Python 代码写入标准输入
                using (var writer = process.StandardInput)
                {
                    if (writer.BaseStream.CanWrite)
                    {
                        writer.WriteLine(pythonCode);
                    }
                }
                Logger.Log("_____run python____");
                var beginTime = DateTime.Now;
                Logger.Log(pythonCode, LogType.Debug);
                // 读取输出
                string output = process.StandardOutput.ReadToEnd();

                // 等待进程结束
                process.WaitForExit();
                Logger.Log($"_____over run python____{(DateTime.Now - beginTime).TotalMilliseconds}ms");
                // 输出结果
                return output;
            }
        }

        /// <summary>
        /// 发送语音给用户
        /// 参数：
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        private async Task<string> GetSpeak(string[] param)
        {
            string speakWords = param[0];
            string chatid = param[1];


            try
            {

                if (ChatMessageContext[chatid] != null)
                {
                    Talk(ChatMessageContext[chatid], speakWords);
                    return "语音发送成功";
                }

            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }




            return "语音发送失败";
        }

        private async Task<string> GetImage(string[] param)
        {
            string imageUrl = param[0];
            string chatid = param[1];
            try
            {
                if (ChatMessageContext[chatid] != null)
                {
                    if (IsValidUrl(imageUrl))
                    {
                        Logger.Log(imageUrl);
                        var base64data = await Network.DownloadImageUrlToBase64(imageUrl);
                        if (base64data.Length > 0)
                        {
                            ChatMessageContext[chatid].SendBackImageBase64(base64data);
                            return "图片发送成功！";
                        }
                    }
                    else
                    {
                        return "图片发送失败，这不是一个可解析的URL";
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }




            return "图片发送失败";
        }

        public bool IsUniKey(string param)
        {
            return new Regex(@"\d+_\d+").IsMatch(param);
        }


        #endregion
    }
}
