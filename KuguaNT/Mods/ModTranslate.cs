using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Kugua.Integrations.NTBot;
using Microsoft.Extensions.FileSystemGlobbing;
using System.Net;



namespace Kugua
{
    class ModTranslate : Mod
    {
        public override bool Init(string[] args)
        {
            try
            {
                ModCommands.Add(new ModCommand(new Regex(@"^译(\d+)次(.+)", RegexOptions.Singleline), getRandTrans));
            }
            catch (Exception e)
            {
                Logger.Log(e.Message + "\r\n" + e.StackTrace);
            }
            return true;
        }

        private string getRandTrans(MessageContext context, string[] param)
        {
            try
            {
                int num = int.Parse(param[1]);
                string input = param[2].Trim();
                if (num > 10) num = 10;
                List<string> langs = new List<string>();
                var res = "(中";
                langs.Add("zh-CN");
                var ll = GoogleTranslate.Language.ToList();
                
                for (int i = 0; i < num; i++)
                {
                    langs.Add(ll[MyRandom.Next(ll.Count)].Value);
                    res += $"译{ll[MyRandom.Next(ll.Count)].Key}";
                    if (res.EndsWith("文") || res.EndsWith("语")) res = res.Remove(res.Length - 1);
                }
                langs.Add("zh-CN");
                res += "译中)\n";
                res += getMultiTrans(input, langs);
                return res;
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }

            return "";
        }

        public override async Task<bool> HandleMessagesDIY(MessageContext context)
        {
            try
            {
                // 翻译
                var input = context.recvMessages.ToTextString();
                if (!context.isAskme || !input.Contains('译')) return false;
                List<string> langs = new List<string>();
                StringBuilder lastlang = new StringBuilder();
                for (int i = 0; i < input.Length; i++)
                {
                    var c = input[i];
                    if (new char[] { ' ', ':', ',' }.Contains(c))
                    {
                        input = input.Substring(i).Trim();
                        if (lastlang.Length > 0) langs.Add(lastlang.ToString());
                        break;
                    }
                    else if (c == '译')
                    {
                        if (lastlang.Length <= 0) lastlang = new StringBuilder("auto");
                        langs.Add(lastlang.ToString());
                        lastlang = new StringBuilder();
                    }
                    else
                    {
                        lastlang.Append(c);
                         
                        //if (lastlang.Length > 6)
                        //{
                        //    // cut
                        //    input = input.Substring(i).Trim();
                        //    if (lastlang.Length > 0) langs.Add(lastlang.ToString());
                        //    break;
                        //}
                    }
                }
                if (langs.Count >= 1 && input.Length>0)
                {
                    var resAll = getMultiTrans(input,langs);
                    if (!string.IsNullOrWhiteSpace(resAll))
                    {
                        context.SendBackPlain(resAll, true);
                        return true;
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return false;
        }


        public static string getMultiTrans(string input, List<string> langs)
        {
            try
            {
                var ggt = GoogleTranslate.Get;
                string[] res = input.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                for (int i = 1; i < langs.Count(); i++)
                {
                    for (int j = 0; j < res.Length; j++)
                    {
                        //Logger.Log(res[j].ToString() + " === " + res[j]);
                        StringBuilder thisres = new StringBuilder();

                        List<string> ss = new List<string>();
                        int beginindex = 0;
                        int maxlen = 100;
                        while (beginindex < res[j].Length)
                        {
                            ss.Add(res[j].Substring(beginindex, Math.Min(res[j].Length - beginindex, maxlen)));
                            beginindex += maxlen;
                        }
                        //Logger.Log(langs[i].ToString() + " === " + res[j]);
                        foreach (var s in ss)
                        {
                            // Logger.Log(langs[i].ToString() + " === " + s);
                            var tmpres = ggt.Translate(s, langs[i]);
                            if (!string.IsNullOrWhiteSpace(tmpres))
                            {
                                thisres.Append(tmpres);
                            }
                            else
                            {
                                thisres.Append(s);
                            }
                        }

                        if (thisres.Length <= 0)
                        {
                            // failure.
                        }
                        else
                        {
                            res[j] = thisres.ToString();
                        }
                        // Logger.Log(langs[i] + " !!! " + res[j]);
                    }

                }
                //for (int j = 0; j < res.Length; j++)
                //{
                //    Logger.Log(res[j].ToString() + " === " + res[j]);

                //}
                string resAll = string.Join("\r\n", res);

                return resAll;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return input;
        }


    }


    




    public static class HttpClientExtensions
    {


        public static string GetUserAgent()
        {
            List<string> UserAgentCollection = new List<string>()
            {
                #region user-agents
		        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.60 Safari/537.36",
                "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.60 Safari/537.36",
                "Mozilla/5.0 (Windows NT 10.0) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.60 Safari/537.36",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 12_3_1) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.60 Safari/537.36",
                "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.60 Safari/537.36",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:98.0) Gecko/20100101 Firefox/98.0",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 12.3; rv:98.0) Gecko/20100101 Firefox/98.0",
                "Mozilla/5.0 (X11; Linux i686; rv:98.0) Gecko/20100101 Firefox/98.0",
                "Mozilla/5.0 (Linux x86_64; rv:98.0) Gecko/20100101 Firefox/98.0",
                "Mozilla/5.0 (X11; Ubuntu; Linux i686; rv:98.0) Gecko/20100101 Firefox/98.0",
                "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:98.0) Gecko/20100101 Firefox/98.0",
                "Mozilla/5.0 (X11; Fedora; Linux x86_64; rv:98.0) Gecko/20100101 Firefox/98.0",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 12_3_1) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.3 Safari/605.1.15",
                "Mozilla/4.0 (coMpatible; MSIE 8.0; Windows NT 5.1; Trident/4.0)",
                "Mozilla/4.0 (coMpatible; MSIE 8.0; Windows NT 6.0; Trident/4.0)",
                "Mozilla/4.0 (coMpatible; MSIE 8.0; Windows NT 6.1; Trident/4.0)",
                "Mozilla/4.0 (coMpatible; MSIE 9.0; Windows NT 6.0; Trident/5.0)",
                "Mozilla/4.0 (coMpatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)",
                "Mozilla/5.0 (coMpatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)",
                "Mozilla/5.0 (coMpatible; MSIE 10.0; Windows NT 6.2; Trident/6.0)",
                "Mozilla/5.0 (Windows NT 6.1; Trident/7.0; rv:11.0) like Gecko",
                "Mozilla/5.0 (Windows NT 6.2; Trident/7.0; rv:11.0) like Gecko",
                "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko",
                "Mozilla/5.0 (Windows NT 10.0; Trident/7.0; rv:11.0) like Gecko",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.60 Safari/537.36 Edg/99.0.1150.36",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 12_3_1) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.60 Safari/537.36 Edg/99.0.1150.36",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.60 Safari/537.36 OPR/85.0.4341.18",
                "Mozilla/5.0 (Windows NT 10.0; WOW64; x64) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.60 Safari/537.36 OPR/85.0.4341.18",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 12_3_1) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.60 Safari/537.36 OPR/85.0.4341.18",
                "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.60 Safari/537.36 OPR/85.0.4341.18",
                "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.60 Safari/537.36 Vivaldi/4.3",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.60 Safari/537.36 Vivaldi/4.3",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 12_3_1) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.60 Safari/537.36 Vivaldi/4.3",
                "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.60 Safari/537.36 Vivaldi/4.3",
                "Mozilla/5.0 (X11; Linux i686) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.60 Safari/537.36 Vivaldi/4.3",
                "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.60 YaBrowser/22.3.0 Yowser/2.5 Safari/537.36",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.60 YaBrowser/22.3.0 Yowser/2.5 Safari/537.36",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 12_3_1) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.60 YaBrowser/22.3.0 Yowser/2.5 Safari/537.36",
                "Mozilla/5.0 (iPhone; CPU iPhone OS 15_4 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) CriOS/100.0.4896.56 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (iPad; CPU OS 15_4 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) CriOS/100.0.4896.56 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (iPod; CPU iPhone OS 15_4 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) CriOS/100.0.4896.56 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (Linux; Android 10) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.58 Mobile Safari/537.36",
                "Mozilla/5.0 (Linux; Android 10; SM-A205U) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.58 Mobile Safari/537.36",
                "Mozilla/5.0 (Linux; Android 10; SM-A102U) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.58 Mobile Safari/537.36",
                "Mozilla/5.0 (Linux; Android 10; SM-G960U) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.58 Mobile Safari/537.36",
                "Mozilla/5.0 (Linux; Android 10; SM-N960U) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.58 Mobile Safari/537.36",
                "Mozilla/5.0 (Linux; Android 10; LM-Q720) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.58 Mobile Safari/537.36",
                "Mozilla/5.0 (Linux; Android 10; LM-X420) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.58 Mobile Safari/537.36",
                "Mozilla/5.0 (Linux; Android 10; LM-Q710(FGN)) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.58 Mobile Safari/537.36",
                "Mozilla/5.0 (iPhone; CPU iPhone OS 12_3_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) FxiOS/98.0 Mobile/15E148 Safari/605.1.15",
                "Mozilla/5.0 (iPad; CPU OS 12_3_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) FxiOS/98.0 Mobile/15E148 Safari/605.1.15",
                "Mozilla/5.0 (iPod touch; CPU iPhone OS 12_3_1 like Mac OS X) AppleWebKit/604.5.6 (KHTML, like Gecko) FxiOS/98.0 Mobile/15E148 Safari/605.1.15",
                "Mozilla/5.0 (Android 12; Mobile; rv:68.0) Gecko/68.0 Firefox/98.0",
                "Mozilla/5.0 (Android 12; Mobile; LG-M255; rv:98.0) Gecko/98.0 Firefox/98.0",
                "Mozilla/5.0 (iPhone; CPU iPhone OS 15_4_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.3 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (iPad; CPU OS 15_4_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.3 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (iPod touch; CPU iPhone 15_4_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.3 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (Linux; Android 10; HD1913) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.58 Mobile Safari/537.36 EdgA/97.0.1072.69",
                "Mozilla/5.0 (Linux; Android 10; SM-G973F) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.58 Mobile Safari/537.36 EdgA/97.0.1072.69",
                "Mozilla/5.0 (Linux; Android 10; Pixel 3 XL) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.58 Mobile Safari/537.36 EdgA/97.0.1072.69",
                "Mozilla/5.0 (Linux; Android 10; ONEPLUS A6003) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.58 Mobile Safari/537.36 EdgA/97.0.1072.69",
                "Mozilla/5.0 (iPhone; CPU iPhone OS 15_4_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.0 EdgiOS/97.1072.69 Mobile/15E148 Safari/605.1.15",
                "Mozilla/5.0 (Windows Mobile 10; Android 10.0; Microsoft; LuMia 950XL) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.60 Mobile Safari/537.36 Edge/40.15254.603",
                "Mozilla/5.0 (Linux; Android 10; VOG-L29) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.58 Mobile Safari/537.36 OPR/63.3.3216.58675",
                "Mozilla/5.0 (Linux; Android 10; SM-G970F) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.58 Mobile Safari/537.36 OPR/63.3.3216.58675",
                "Mozilla/5.0 (Linux; Android 10; SM-N975F) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.58 Mobile Safari/537.36 OPR/63.3.3216.58675",
                "Mozilla/5.0 (iPhone; CPU iPhone OS 15_4_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.3 YaBrowser/22.3.4.566 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (iPad; CPU OS 15_4_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.3 YaBrowser/22.3.4.566 Mobile/15E148 Safari/605.1",
                "Mozilla/5.0 (iPod touch; CPU iPhone 15_4_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.3 YaBrowser/22.3.4.566 Mobile/15E148 Safari/605.1",
                "Mozilla/5.0 (Linux; arM_64; Android 12; SM-G965F) AppleWebKit/537.36 (KHTML, like Gecko) ChroMe/100.0.4896.58 YaBrowser/21.3.4.59 Mobile Safari/537.36" 
	    #endregion
            };
            int randomIndex = new Random().Next(0, UserAgentCollection.Count - 1);
            return UserAgentCollection[randomIndex];
        }
        public static void AddUserAgentToHeader(this System.Net.Http.HttpClient httpClient)
        {
            if (httpClient.DefaultRequestHeaders.Contains("User-Agent"))
                httpClient.DefaultRequestHeaders.Remove("User-Agent");

            httpClient.DefaultRequestHeaders.Add("User-Agent", GetUserAgent());
        }
    }




    public class GoogleTranslate
    {
        private static class Nested
        {
            internal static readonly GoogleTranslate Instance = new();
        }

        private readonly HttpClient _httpClient;
        public static GoogleTranslate Get => Nested.Instance;

        private GoogleTranslate() => _httpClient = new HttpClient();


        /// <summary> enum  </summary>
        /// <param name="langCode"> fit ISO 639-1</param>
        public static string GetLanguage(string langCode)
        {
            langCode = langCode.Trim();
            if(langCode.EndsWith("语"))langCode=langCode.Substring(langCode.Length - 2);
            if (Language.TryGetValue(langCode, out var language)) return language;
            if (Language.TryGetValue(langCode + "文", out var language2)) return language2;
            return langCode;
        }

        /// <summary>  DIV  </summary>
        /// <param name="htmlPageResult">html </param>
        private string ParseTranslatedText(string htmlPageResult)
        {
            var resultContainer = Regex.Matches(htmlPageResult, @"div[^""]*?""result-container"".*?>(.+?)</div>");
            return resultContainer.Count > 0 ? resultContainer[0].Groups[1].Value : null;
        }

        public string TranslateRandom(string inputText)
        {
            string rr = "en";
            var vlist = Language.Values.ToList();
            do
            {
                var v = vlist[MyRandom.Next(vlist.Count)];
                if (!v.Contains("cn")) { rr = v; break; }
            } while (true);

            return Translate(inputText, rr);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="to"></param>
        /// <param name="from"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string Translate(string inputText, string to, string? from = null)
        {
            _httpClient.AddUserAgentToHeader();

            var fromLang = from == null ? "auto" : GetLanguage(from);
            var toLang = GetLanguage(to);

            string urlForTranslate = $"https://translate.google.com/m?" +
                                     $"sl={fromLang}&tl={toLang}" +
                                     $"&ie=UTF-8&prev=_m&q={Uri.EscapeDataString(inputText)}";
            try
            {
                var response = _httpClient.GetAsync(urlForTranslate).Result;
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    // fail
                    //throw new Exception($"Translate fault with http status code {response.StatusCode}");
                }
                return WebUtility.HtmlDecode(ParseTranslatedText(response.Content.ReadAsStringAsync().Result));
            }
            catch(Exception ex)
            {
                    Logger.Log("url = " + urlForTranslate);
            }
                return "";
        }






        public static Dictionary<string, string> Language = new Dictionary<string, string>
        {
            #region Languages dict

            { "不丹文", "dz" },
            { "世界文", "eo" },
            { "丹麦文", "da" },
            { "乌克兰文", "uk" },
            { "乌兹别克文", "uz" },
            { "乌尔都文", "ur" },
            { "亚美尼亚文", "hy" },
            { "依地文", "yi" },
            { "俄文", "ru" },
            { "保加利亚文", "bg" },
            { "信德文", "sd" },
            { "僧伽罗文", "si" },
            { "克罗地亚文", "hr" },
            { "冰岛文", "is" },
            { "加利西亚文", "gl" },
            { "加泰罗尼亚文", "ca" },
            { "匈牙利文", "hu" },
            { "南非荷兰文", "af" },
            { "卡纳达文", "kn" },
            { "卢森堡文", "lb" },
            { "印", "hi" },
            { "印地文", "hi" },
            { "印度尼西亚文", "id" },
            { "古吉拉特文", "gu" },
            { "吉尔吉斯文", "ky" },
            { "吉普赛文", "rom" },
            { "哈萨克文", "kk" },
            { "因纽特文", "iu" },
            { "土耳其文", "tr" },
            { "塔吉克文", "tg" },
            { "塔塔尔文", "tt" },
            { "塞内卡文", "see" },
            { "塞尔维亚文", "sr" },
            { "夏威夷文", "haw" },
            { "奥里亚文", "or" },
            { "威尔士文", "cy" },
            { "孟加拉文", "bn" },
            { "宿务文", "ceb" },
            { "尼扬扎文", "ny" },
            { "尼泊尔文", "ne" },
            { "巴斯克文", "eu" },
            { "巽他文", "su" },
            { "希伯来文", "he" },
            { "希腊文", "el" },
            { "库尔德文", "ku" },
            { "彻罗基文", "chr" },
            { "德文", "de" },
            { "意大利文", "it" },
            { "意", "it" },
            { "拉丁文", "la" },
            { "拉脱维亚文", "lv" },
            { "挪威尼诺斯克文", "nn" },
            { "挪威文", "nb" },
            { "捷克文", "cs" },
            { "提格里尼亚文", "ti" },
            { "斯洛伐克文", "sk" },
            { "斯洛文尼亚文", "sl" },
            { "斯瓦希里文", "sw" },
            { "旁遮普文", "pa" },
            { "日文", "ja" },
            { "普什图文", "ps" },
            { "曼尼普里文", "mni-Mtei" },
            { "曼尼普尔文", "mni-Mtei" },
            { "格鲁吉亚文", "ka" },
            { "梵文", "sa" },
            { "毛利文", "mi" },
            { "法文", "fr" },
            { "波兰文", "pl" },
            { "波斯尼亚文", "bs" },
            { "波斯文", "fa" },
            { "泰卢固文", "te" },
            { "泰文", "th" },
            { "泰米尔文", "ta" },
            { "海地文", "ht" },
            { "爪哇文", "jv" },
            { "爱尔兰文", "ga" },
            { "爱沙尼亚文", "et" },
            { "瑞典文", "sv" },
            { "白俄罗斯文", "be" },
            { "祖鲁文", "zu" },
            { "科萨文", "xh" },
            { "科西嘉文", "co" },
            { "立陶宛文", "lt" },
            //{ "简体中文", "zh-CN" },
            //{ "简体中文（中国）", "zh-Hans" },
            { "索拉尼库尔德文", "ckb" },
            { "索马里文", "so" },
            //{ "繁体中文", "zh-TW" },
            //{ "繁体中文（台湾）", "zh-Hant" },
            { "纳瓦霍文", "nv" },
            { "绍纳文", "sn" },
            //{ "维吾尔文", "ug" },
            { "缅甸文", "my" },
            { "罗马尼亚文", "ro" },
            { "老挝文", "lo" },
            { "芬兰文", "fi" },
            { "苏格兰盖尔文", "gd" },
            { "英文", "en" },
            { "荷兰文", "nl" },
            { "菲律宾文", "fil" },
            { "萨摩亚文", "sm" },
            //{ "葡萄牙文（巴西）", "pt-BR" },
            { "葡萄牙文", "pt-PT" },
            { "蒙古文", "mn" },
            { "阿", "ar" },
            { "西弗里西亚文", "fy" },
            { "西班牙文", "es" },
            { "赫蒙文", "hmn" },
            { "越南文", "vi" },
            { "阿塞拜疆文", "az" },
            { "阿姆哈拉文", "am" },
            { "阿尔巴尼亚文", "sq" },
            { "阿拉伯文", "ar" },
            { "韩文", "ko" },
            { "马其顿文", "mk" },
            { "马尔加什文", "mg" },
            { "马拉地文", "mr" },
            { "马拉雅拉姆文", "ml" },
            { "马来文", "ms" },
            { "马耳他文", "mt" },
            { "高棉文", "km" },
            //{ "自动", "auto" },
            { "汉", "zh-CN" },
            { "中", "zh-CN" },
            { "简", "zh-CN" },
            { "繁", "zh-Hant" }

             #endregion
        };




    }
}
