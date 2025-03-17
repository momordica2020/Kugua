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



namespace Kugua.Mods
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

        /// <summary>
        /// 单语言翻译/多语言翻译（译N次）
        /// 译英 你好啊/译英译日译中 共是风，还是法律的。/译N次 你们别骂我了
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string getRandTrans(MessageContext context, string[] param)
        {
            try
            {
                int num = int.Parse(param[1]);
                string input = param[2].Trim();
                if (num > 10) num = 10;
                List<string> langs = new List<string>();
                var res = "(";
                //langs.Add("zh-CN");
                var ll = GoogleTranslate.Language.ToList();
                
                for (int i = 0; i < num; i++)
                {
                    langs.Add(ll[MyRandom.Next(ll.Count)].Value);
                    res += $"译{ll[MyRandom.Next(ll.Count)].Key}";
                    if (res.EndsWith("语")) res = res.Remove(res.Length - 1);
                }
                langs.Add("zh-CN");
                res += "译中)\n";
                res += getTrans(input, langs);
                return res;
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }

            return "";
        }


        bool HasLanguage(string name)
        {
            return (
                 !string.IsNullOrWhiteSpace(name) &&
                 (  GoogleTranslate.Language.ContainsKey(name)
                 || GoogleTranslate.Language.ContainsKey(name + "语")
                 || GoogleTranslate.Language.ContainsKey(name.Replace("语", "")))
            );
        }

        /// <summary>
        /// 将输入文本中的译文指令切出来，返回待译语言列表和后续待翻译内容
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private (string text, List<string> langs) CutLanguages(string input)
        {
            List<string> langs = new List<string>();
            string lastlang = "";
            int i = 0;
            for (; i < input.Length; i++)
            {
                if ("译 ；:，,\r\n".Contains(input[i]))
                {
                    //input = input.Substring(i).Trim();
                    if (lastlang.Length > 0)
                    {
                        if (HasLanguage(lastlang))
                        {
                            langs.Add(new string(lastlang));
                            lastlang = "";
                        }
                        else
                        {    
                            break;
                        }                     
                    }
                }
                else
                {
                    lastlang +=input[i];
                }

            }
            int lastmax = 0;
            if (lastlang.Length > 0)
            {
                for (int j = lastlang.Length; j > 0; j--)
                {
                    if (HasLanguage(lastlang.Substring(0, j)))
                    {
                        langs.Add(lastlang.Substring(0, j));
                        lastmax = j;
                        break;
                    }
                }
            }
            input = input.Substring(i - lastlang.Length + lastmax);
            //Logger.Log(string.Join(", ", langs));
            return (input, langs);

        }

        public override async Task<bool> HandleMessagesDIY(MessageContext context)
        {
            try
            {
                // 翻译
                var input = context.recvMessages.ToTextString();
                if (!context.IsAskme || !input.Contains('译')) return false;
                (string text, List<string> langs) = CutLanguages(input);
                if (langs.Count > 0 && !string.IsNullOrWhiteSpace(text))
                {
                    var resAll = getTrans(text, langs);
                    if (!string.IsNullOrWhiteSpace(resAll))
                    {
                        context.SendBackText(resAll, true);
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

        public static void handleTransArray(string[] res, string lang)
        {
            try
            {
                var ggt = GoogleTranslate.Get;
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
                        var tmpres = ggt.Translate(s, lang);
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
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }


        public static string getTrans(string input, string lang)
        {
            string[] res = input.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
           
            handleTransArray(res, lang);

            string resAll = string.Join("\r\n", res);
            return resAll;
        }
        public static string getTrans(string input, List<string> langs)
        {
            string[] res = input.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            for (int i = 0; i < langs.Count(); i++)
            {
                handleTransArray(res, langs[i]);
            }
            //for (int j = 0; j < res.Length; j++)
            //{
            //    Logger.Log(res[j].ToString() + " === " + res[j]);

            //}
            string resAll = string.Join("\r\n", res);
            return resAll;
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
            if (langCode.EndsWith("文"))langCode = langCode.Substring(0, langCode.Length - 1);
            if (Language.TryGetValue(langCode, out var language)) return language;
            if (Language.TryGetValue(langCode + "语", out var language2)) return language2;
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

           // Logger.Log("url = " + urlForTranslate);

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


#region language list from google
            { "阿布哈兹语", "ab" },
            { "阿尔巴尼亚语", "sq" },
            { "阿法尔语", "aa" },
            { "阿拉伯语", "ar" },
            { "阿", "ar" },
            { "阿卢尔语", "alz" },
            { "阿姆哈拉语", "am" },
            { "阿乔利语", "ach" },
            { "阿萨姆语", "as" },
            { "阿塞拜疆语", "az" },
            { "阿瓦德语", "awa" },
            { "阿瓦尔语", "av" },
            { "埃维语", "ee" },
            { "艾马拉语", "ay" },
            { "爱尔兰语", "ga" },
            { "爱沙尼亚语", "et" },
            { "奥克语", "oc" },
            { "奥利亚语", "or" },
            { "奥罗莫语", "om" },
            { "奥塞梯语", "os" },
            { "巴布亚皮钦语", "tpi" },
            { "巴达维语", "bew" },
            { "巴厘语", "ban" },
            { "巴什基尔语", "ba" },
            { "巴斯克语", "eu" },
            { "巴塔克卡罗语", "btx" },
            { "巴塔克托巴语", "bbc" },
            { "巴塔克西马隆贡语", "bts" },
            { "巴乌雷语", "bci" },
            { "白俄罗斯语", "be" },
            { "班巴拉语", "bm" },
            { "邦阿西楠语", "pag" },
            { "邦板牙语", "pam" },
            { "保加利亚语", "bg" },
            { "北索托语", "nso" },
            { "奔巴语", "bem" },
            { "比科尔语", "bik" },
            { "俾路支语", "bal" },
            { "冰岛语", "is" },
            { "波兰语", "pl" },
            { "波斯尼亚语", "bs" },
            { "波斯语", "fa" },
            { "博杰普尔语", "bho" },
            { "布里亚特语", "bua" },
            { "布列塔尼语", "br" },
            { "藏语", "bo" },
            { "草原马里语", "chm" },
            { "查莫罗语", "ch" },
            { "车臣语", "ce" },
            { "楚克语", "chk" },
            { "楚瓦什语", "cv" },
            { "茨瓦纳语", "tn" },
            { "聪加语", "ts" },
            { "达里语", "fa-AF" },
            { "鞑靼语", "tt" },
            { "丹麦语", "da" },
            { "掸语", "shn" },
            { "德顿语", "tet" },
            { "德语", "de" },
            { "迪维希语", "dv" },
            { "迪尤拉语", "dyu" },
            { "蒂夫语", "tiv" },
            { "丁卡语", "din" },
            { "多格拉语", "doi" },
            { "俄语", "ru" },
            { "恩道语", "ndc-ZW" },
            { "恩德贝莱语", "nr" },
            { "恩敦贝语", "dov" },
            { "恩科语", "bm-Nkoo" },
            { "法罗语", "fo" },
            { "法语", "fr" },
            //{ "法语", "fr-CA" },
            { "梵语", "sa" },
            { "菲律宾语", "tl" },
            { "斐济语", "fj" },
            { "芬兰语", "fi" },
            { "丰语", "fon" },
            { "弗里西语", "fy" },
            { "弗留利语", "fur" },
            { "富拉尼语", "ff" },
            { "刚果语", "kg" },
            { "高棉语", "km" },
            { "格陵兰语", "kl" },
            { "格鲁吉亚语", "ka" },
            { "贡根语", "gom" },
            { "古吉拉特语", "gu" },
            { "瓜拉尼语", "gn" },
            { "哈卡钦语", "cnh" },
            { "哈萨克语", "kk" },
            { "海地克里奥尔语", "ht" },
            { "韩语", "ko" },
            { "豪萨语", "ha" },
            { "荷兰语", "nl" },
            { "洪斯吕克语", "hrx" },
            { "吉尔吉斯语", "ky" },
            { "吉土巴语", "ktu" },
            { "加利西亚语", "gl" },
            { "加泰罗尼亚语", "ca" },
            { "加语", "gaa" },
            { "捷克语", "cs" },
            { "景颇语", "kac" },
            { "卡纳达语", "kn" },
            { "卡努里语", "kr" },
            { "卡西语", "kha" },
            { "凯克其语", "kek" },
            { "科米语", "kv" },
            { "科萨语", "xh" },
            { "科西嘉语", "co" },
            { "克里米亚鞑靼语", "crh" },
            { "克罗地亚语", "hr" },
            { "克丘亚语", "qu" },
            { "库尔德语", "ku" },
            //{ "库尔德语", "ckb" },
            { "廓克博若克语", "trp" },
            { "拉丁语", "la" },
            { "拉特加莱语", "ltg" },
            { "拉脱维亚语", "lv" },
            { "老挝语", "lo" },
            { "立陶宛语", "lt" },
            { "利古里亚语", "lij" },
            { "林堡语", "li" },
            { "林加拉语", "ln" },
            { "隆迪语", "rn" },
            { "卢奥语", "luo" },
            { "卢干达语", "lg" },
            { "卢森堡语", "lb" },
            { "卢旺达语", "rw" },
            { "伦巴第语", "lmo" },
            { "罗马尼亚语", "ro" },
            { "罗姆语", "rom" },
            { "马都拉语", "mad" },
            { "马恩岛语", "gv" },
            { "马尔加什语", "mg" },
            { "马尔瓦迪语", "mwr" },
            { "马耳他语", "mt" },
            { "马拉地语", "mr" },
            { "马拉雅拉姆语", "ml" },
            { "马来语", "ms" },
            //{ "马来语", "ms-Arab" },
            { "马其顿语", "mk" },
            { "马绍尔语", "mh" },
            { "玛姆语", "mam" },
            { "迈蒂利语", "mai" },
            { "毛里裘斯克里奥耳语", "mfe" },
            { "毛利语", "mi" },
            { "梅泰语", "mni-Mtei" },
            { "蒙古语", "mn" },
            { "蒙语", "mn" },
            { "孟加拉语", "bn" },
            { "米南语", "min" },
            { "米佐语", "lus" },
            { "缅甸语", "my" },
            { "苗语", "hmn" },
            { "纳瓦特尔语", "nhe" },
            { "南非荷兰语", "af" },
            { "南索托语", "st" },
            { "尼泊尔语", "ne" },
            { "尼泊尔语言", "new" },
            { "努尔语", "nus" },
            { "挪威语", "no" },
            { "帕皮阿门托语", "pap" },
            { "旁遮普语", "pa" },
            //{ "旁遮普语", "pa-Arab" },
            { "葡萄牙语", "pt" },
            //{ "葡萄牙语", "pt-PT" },
            { "普什图语", "ps" },
            { "齐切瓦语", "ny" },
            { "奇加语", "cgg" },
            { "奇卢伯语", "lua" },
            { "日语", "ja" },
            { "契维语", "ak" },
            { "瑞典语", "sv" },
            { "萨巴特克语", "zap" },
            { "萨米语", "se" },
            { "萨摩亚语", "sm" },
            { "塞尔维亚语", "sr" },
            { "塞拉利昂克里奥尔语", "kri" },
            { "塞舌尔克里奥尔语", "crs" },
            { "桑戈语", "sg" },
            //{ "桑塔利语", "sat-Latn" },
            { "桑塔利语", "sat" },
            { "僧伽罗语", "si" },
            { "世界语", "eo" },
            { "斯洛伐克语", "sk" },
            { "斯洛文尼亚语", "sl" },
            { "斯瓦特语", "ss" },
            { "斯瓦希里语", "sw" },
            { "苏格兰盖尔语", "gd" },
            { "苏苏语", "sus" },
            { "宿务语", "ceb" },
            { "索马里语", "so" },
            { "塔吉克语", "tg" },
            { "塔马齐格特语", "ber" },
            { "塔马塞特语", "ber-Latn" },
            { "塔希提语", "ty" },
            { "泰卢固语", "te" },
            { "泰米尔语", "ta" },
            { "泰语", "th" },
            { "汤加语", "to" },
            { "提格里尼亚语", "ti" },
            { "图鲁语", "tcy" },
            { "图姆布卡语", "tum" },
            { "图瓦语", "tyv" },
            { "土耳其语", "tr" },
            { "土库曼语", "tk" },
            { "瓦瑞语", "war" },
            { "望加锡语", "mak" },
            { "威尔士语", "cy" },
            { "威尼斯语", "vec" },
            { "维吾尔语", "ug" },
            { "文达语", "ve" },
            { "沃洛夫语", "wo" },
            { "乌德穆尔特语", "udm" },
            { "乌尔都语", "ur" },
            { "乌克兰语", "uk" },
            { "乌兹别克语", "uz" },
            { "西班牙语", "es" },
            { "西里西亚语", "szl" },
            { "希伯来语", "scn" },
            { "希腊语", "el" },
            { "希利盖农语", "hil" },
            { "夏威夷语", "haw" },
            { "信德语", "sd" },
            { "匈牙利语", "hu" },
            { "修纳语", "sn" },
            { "巽他语", "su" },
            { "牙买加土语", "jam" },
            { "雅库特语", "sah" },
            { "亚美尼亚语", "hy" },
            { "亚齐语", "ace" },
            { "伊班语", "iba" },
            { "伊博语", "ig" },
            { "伊洛卡诺语", "ilo" },
            { "意大利语", "it" },
            { "意第绪语", "yi" },
            //{ "因纽特语", "iu-Latn" },
            { "因纽特语", "iu" },
            { "印地语", "hi" },
            { "印语", "hi" },
            { "英语", "id" },
            { "尤卡坦玛雅语", "yua" },
            { "约鲁巴语", "yo" },
            { "粤语", "yue" },
            { "越南语", "vi" },
            { "汉语", "jw" },
            { "宗卡语", "dz" },
            { "祖鲁语", "zu" },

            //{ "自动", "auto" },
            { "中", "zh-CN" },
            { "简", "zh-CN" },
            //{ "简体", "zh-CN" },
            { "繁", "zh-TW" },
            //{ "繁体", "zh-TW" },



#endregion
        };




    }
}
