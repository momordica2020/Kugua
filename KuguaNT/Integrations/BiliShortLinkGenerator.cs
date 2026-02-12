using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kugua.Integrations
{


    public class BiliShortLinkGenerator
    {
        private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(9) };
        private const string ApiUrl = "http://api.bilibili.com/x/share/click";

        /// <summary>
        /// 生成随机的 buvid
        /// </summary>
        public static string RandomBuvid()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var result = new string(Enumerable.Repeat(chars, 32)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return result + "infoc";
        }

        /// <summary>
        /// 获取 b23.tv 短链接
        /// </summary>
        public static async Task<string> GetB23OfAsync(string longUrl)
        {
            try
            {
                var formData = new Dictionary<string, string>
            {
                { "build", "6500300" },
                { "buvid", RandomBuvid() },
                { "oid", longUrl },
                { "platform", "android" },
                { "share_channel", "COPY" },
                { "share_id", "public.webview.0.0.pv" },
                { "share_mode", "3" }
            };

                var content = new FormUrlEncodedContent(formData);
                var response = await _httpClient.PostAsync(ApiUrl, content);

                if (!response.IsSuccessStatusCode) return null;

                var responseBody = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseBody);

                // 检查 data 节点及其下的 content 属性
                if (doc.RootElement.TryGetProperty("data", out JsonElement dataElement) &&
                    dataElement.TryGetProperty("content", out JsonElement contentElement))
                {
                    return contentElement.GetString();
                }
                Logger.Log($"b32 error:{responseBody}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return string.Empty;
            }
        }
    }


}