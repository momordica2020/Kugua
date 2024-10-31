using ChatGPT.Net;
using ChatGPT.Net.DTO.ChatGPT;
using MeowMiraiLib.Msg.Type;
using MeowMiraiLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeowMiraiLib.Msg;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace MMDK.Util
{
    internal class GPT
    {
        private static readonly Lazy<GPT> instance = new Lazy<GPT>(() => new GPT());
        ChatGpt gptAgent;

        MeowMiraiLib.Client client;

        private GPT()
        {

            
        }

        public void Init(MeowMiraiLib.Client _client)
        {
            client = _client;

            var opt2 = new ChatGptOptions();
            opt2.BaseUrl = "http://127.0.0.1:8000";
            opt2.Model = "rwkv";//"chatgal-RWKV-4-World-CHNtuned-7B-v1-20230709-ctx32k-1000";
            opt2.Temperature = 0.5; // Default: 0.9;
            opt2.TopP = 0.3; // Default: 1.0;
            opt2.MaxTokens = 300; // Default: 64;
            //opt2.Stop = ["。"]; // Default: null;
            opt2.PresencePenalty = 0.0; // Default: 0.0;
            opt2.FrequencyPenalty = 1.0; // Default: 0.0;

            gptAgent = new ChatGpt("", opt2);
        }


        // 公共静态属性获取实例
        public static GPT Instance => instance.Value;

        public async void AIReply(long groupId, long userId, string userName, string input)
        {
            try
            {
                //string prompt = "从这一片段开始续写一段奇幻冒险故事：";
                string[] emos = ["快乐", "耐心", "伤心", "痛苦", "绝望", "激动", "轻松写意"];
                string emotion = emos[MyRandom.Next(emos.Length)];


                input = input.Replace("“", "").Replace("”", "");
                string text = $"最近本公主的朋友\"{userName}\"写信来问我：“{input}”我只好{emotion}地写了内容极为详细的回答：\n";
                JObject json = new JObject();
                json.Add("frequency_penalty", 1);
                json.Add("max_tokens", 200);
                //json.Add("model", "rwkv");
                //json.Add("presence_penalty", 0);
                json.Add("prompt", text);
                json.Add("stream", false);
                //json.Add("temperature", 1);
                //json.Add("top_p", 0.3);
                //string json = @"{
                //      'frequency_penalty': 1,
                //      'max_tokens': 100,
                //      'model': 'rwkv',
                //      'presence_penalty': 0,
                //      'prompt': text,
                //      'stream': false,
                //      'temperature': 1,
                //      'top_p': 0.3
                //    }";

                string url = "http://127.0.0.1:8000/completions";

                var responseBody = await WebLinker.PostJsonAsync(url, json.ToString());
                JObject jsonResponse = JObject.Parse(responseBody);
                string res = jsonResponse["choices"].First()["text"].ToString();

                // 后处理
                res = res.Replace("\"", "").Replace("“","").Replace("”","");

                Regex regex = new Regex(@"(User|Assistant|Answer|Question)", RegexOptions.Singleline);
                Match match = regex.Match(res);               
                if (match.Success)
                {
                    // 截取从开始到匹配位置的字符串
                    res = res.Substring(0, match.Index);
                }

                regex = new Regex(@"[、…，。？！](?!.*[、…，。？！])", RegexOptions.Singleline);
                match = regex.Match(res);
                if (match.Success)
                {
                    // 截取从字符串开始到最后一个标点符号的位置
                    res = res.Substring(0, match.Index + 1);
                }
                if (res.EndsWith('、') || res.EndsWith('，')) res = res.Substring(0, res.Length - 1) + "……";
                

                // 回复
                //var res = await gptAgent.Ask($"{prompt}{input}");
                if (groupId > 0)
                {
                    new GroupMessage(groupId, [
                         new At(userId, ""),
                        new Plain($"{res}")
                                ]).Send(client);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
            }

        }

        static async Task<string> PostJsonAsync(string url, string jsonContent)
        {
            using (HttpClient client = new HttpClient())
            {
                // 设置请求体为 JSON 格式
                HttpContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                try
                {
                    // 发送 POST 请求
                    HttpResponseMessage response = await client.PostAsync(url, content);

                    // 确认响应状态
                    response.EnsureSuccessStatusCode();

                    // 读取响应内容
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Received response: " + responseBody);

                    // 解析 JSON 
                    
                    JObject jsonResponse = JObject.Parse(responseBody);
                    string res = jsonResponse["choices"].First()["text"].ToString();
                    return res;
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
            }

            return "";
        }
    }
}
