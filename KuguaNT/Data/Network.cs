using HtmlAgilityPack;
using ImageMagick;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Security;

using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace Kugua
{

    /// <summary>
    /// 网络请求处理模块
    /// </summary>
    public class Network
    {
        static string proxyAddress = "http://localhost:7897";
        static string defaultHeaderAgentString = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36";



        static HttpClientHandler handlerWithProxy = new HttpClientHandler
        {
            Proxy = new WebProxy(proxyAddress),
            UseProxy = true,
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; } // 忽略所有证书错误
        };

        public static void Download(string url, string localPath, bool useProxy=false)
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                UseProxy = useProxy,
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; } // 忽略所有证书错误
            };
            if (useProxy)
            {
                handler.Proxy = new WebProxy(proxyAddress);
            }
            using (HttpClient client = new HttpClient(handler))
            {
                try
                {
                    client.Timeout = TimeSpan.FromMinutes(5);  // 设置为5分钟
                    client.DefaultRequestHeaders.Add("User-Agent", defaultHeaderAgentString);

                    // 获取图片的响应
                    using (HttpResponseMessage response = client.GetAsync(url).Result)
                    {
                        response.EnsureSuccessStatusCode(); // 确保请求成功

                        // 读取图片内容
                        using (var stream = response.Content.ReadAsStream())
                        using (var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            stream.CopyTo(fileStream); // 将内容写入本地文件
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }

            }
        }
        public static async void DownloadAsync(string url, string localPath)
        {
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13; // 启用 TLS 1.2 和 TLS 1.3
            HttpClientHandler handler = new HttpClientHandler
            {
                UseProxy = false,
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; } // 忽略所有证书错误
            };
            using (HttpClient client = new HttpClient(handler))
            {
                try
                {
                    client.DefaultRequestHeaders.Add("User-Agent", defaultHeaderAgentString);

                    // 获取图片的响应
                    using (HttpResponseMessage response = await client.GetAsync(url))
                    {
                        response.EnsureSuccessStatusCode(); // 确保请求成功

                        // 读取图片内容
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await stream.CopyToAsync(fileStream); // 将内容写入本地文件
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }

            }
        }


        
        public static MagickImageCollection DownloadImage(string imageUrl)
        {//ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13; // 启用 TLS 1.2 和 TLS 1.3

            if (string.IsNullOrWhiteSpace(imageUrl)) return null;
            HttpClientHandler handler = new HttpClientHandler
            {
                UseProxy = false,
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; } // 忽略所有证书错误
            };


            using (HttpClient client = new HttpClient(handler))
            {
                try
                {
                    client.DefaultRequestHeaders.Add("User-Agent", defaultHeaderAgentString);

                    // 获取图片的响应
                    using (HttpResponseMessage response = client.GetAsync(imageUrl).Result)
                    {
                        response.EnsureSuccessStatusCode(); // 确保请求成功

                        byte[] imageBytes = client.GetByteArrayAsync(imageUrl).Result;
                        //Logger.Log("url=" +imageUrl+ "\r\nByte = " + imageBytes.Length);
                        using (MemoryStream ms = new MemoryStream(imageBytes))
                        {

                            MagickImageCollection images = new MagickImageCollection();
                            images.Read(ms);  // Read the image stream
                            //Logger.Log("?-- "+images.Count);
                            return images;
                            //if (images.Count > 0)
                            //{
                            //    // Return the first frame of the GIF (static or animated)
                            //    return ConvertMagickImageToBitmap(images[0]);
                            //}
                            
                            //return new Bitmap(ms);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }

            }
            return null;
        }

        // 从网络图片 URL 转换为 Base64 编码字符串
        public static async Task<string> DownloadImageUrlToBase64(string imageUrl)
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                UseProxy = false,
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; } // 忽略所有证书错误
            };


            using (HttpClient client = new HttpClient(handler))
            {
                try
                {
                    client.DefaultRequestHeaders.Add("User-Agent", defaultHeaderAgentString);

                    // 获取图片的响应
                    using (HttpResponseMessage response = client.GetAsync(imageUrl).Result)
                    {
                        response.EnsureSuccessStatusCode(); // 确保请求成功

                        byte[] imageBytes = client.GetByteArrayAsync(imageUrl).Result;
                        return Convert.ToBase64String(imageBytes);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }

            }
            return null;
        }



        public static async Task<string> GetHtmlFromUrlAsync(string url)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13; // 启用 TLS 1.2 和 TLS 1.3
            var handler = new HttpClientHandler
            {
                Proxy = new WebProxy(proxyAddress),
                UseProxy = true,
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; } // 忽略所有证书错误
            };

            using (HttpClient client = new HttpClient(handler))
            {
                try
                {
                    client.DefaultRequestHeaders.Add("User-Agent", defaultHeaderAgentString);

                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    var encoding = Encoding.UTF8; // Default encoding
                    var contentType = response.Content.Headers.ContentType;

                    if (contentType != null && !string.IsNullOrEmpty(contentType.CharSet))
                    {
                        // Try to find the correct encoding
                        try
                        {
                            encoding = Encoding.GetEncoding(contentType.CharSet);
                        }
                        catch (ArgumentException)
                        {
                            // Fallback to UTF-8 if the charset is invalid
                            encoding = Encoding.UTF8;
                        }
                    }

                    string htmlContent = encoding.GetString(bytes);
                    Logger.Log("** HTML REQUEST **" + htmlContent.Length);
                    Logger.Log(htmlContent.Length > 100 ? htmlContent.Substring(0, 100) + "..............." + htmlContent.Length : htmlContent);
                    return htmlContent;

                }
                catch (HttpRequestException e)
                {
                    Logger.Log("url=" + url);
                    Logger.Log(e);
                }
            }

            return "";
        }

        // 将 HTML 转换为纯文本
        public static string ConvertHtmlToPlainText(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            // 提取文本
            return Regex.Replace(htmlDocument.DocumentNode.InnerText, @"\s+", "");
        }

        public static string Get(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // 发送 POST 请求
                    HttpResponseMessage response = client.GetAsync(url).Result;

                    // 确认响应状态
                    response.EnsureSuccessStatusCode();

                    // 读取响应内容
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    //Console.WriteLine("Received response: " + responseBody);

                    return responseBody;
                }
                catch (HttpRequestException e)
                {
                    Logger.Log(e);
                }
            }

            return "";
        }
        public static async Task<string> PostAsync(string url, StringContent paramString)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpContent content = paramString;
                try
                {
                    // 发送 POST 请求
                    HttpResponseMessage response = await client.PostAsync(url, content);

                    // 确认响应状态
                    response.EnsureSuccessStatusCode();

                    // 读取响应内容
                    string responseBody = await response.Content.ReadAsStringAsync();
                    //Console.WriteLine("Received response: " + responseBody);

                    return responseBody;
                }
                catch (HttpRequestException e)
                {
                     Logger.Log(e, LogType.Debug);
                }
            }

            return "";
        }
        public static async Task<string> PostAsync(string url, string paramString, bool use_encode = false)
        {
            var sp = paramString;
            if (use_encode) sp = System.Web.HttpUtility.UrlPathEncode(paramString);
           // var sp = System.Web.HttpUtility.UrlPathEncode(paramString);
            return await PostAsync(url, new StringContent(sp, Encoding.UTF8, "application/x-www-form-urlencoded"));

        }


        public static async Task<string> PostJsonAsync(string url, string paramString, bool use_encode = true)
        {
            var sp = paramString;
            if(use_encode)  sp = System.Web.HttpUtility.UrlPathEncode(paramString);
            return await PostAsync(url, new StringContent(sp, Encoding.UTF8, "application/json"));
        }
    }
}
