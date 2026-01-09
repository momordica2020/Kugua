using Kugua.Integrations.NTBot;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Text;
using Kugua.Core;
using Kugua.Integrations.AI.Base;
using System.ComponentModel;

namespace Kugua.Integrations.AI
{

    /// <summary>
    /// Ollama
    /// </summary>
    public class Ollama : LLMBase
    {
        OllamaFunctions funcs = new OllamaFunctions();
        public string ModelName;

        static List<dynamic> OllamaTools = new List<dynamic>
        {
            new
            {
                type="function",
                function = new
                {
                    name = "发送图片",
                    description="把指定URL的图片发送给对方",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            url = new
                            {
                                type = "string",
                                description = "图片url地址"
                            },
                        },
                        required = new[] { "url" }
                    }
                }
            },
            new
            {
                type="function",
                function = new
                {
                    name = "发送语音",
                    description="用语音把要说的内容发送给用户",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            words = new
                            {
                                type = "string",
                                description = "要说的内容"
                            },
                        },
                        required = new[] { "words" }
                    }
                }
            },
            new
            {
                type="function",
                function = new
                {
                    name = "执行python语句",
                    description="执行输入的python语句，并返回结果",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            code = new
                            {
                                type = "string",
                                description = "待执行的语句"
                            },
                        },
                        required = new[] { "code" }
                    }
                }
            },
            new
            {
                type = "function",
                function = new
                {
                    name = "在线搜索",
                    description = "根据网址搜索，返回网页的html文本",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            url = new
                            {
                                type = "string",
                                description = "网页url前缀"
                            },
                            keyword = new
                            {
                                type = "string",
                                description = "所需搜索的关键词"
                            }
                        },
                        required = new[] {  "url" ,"keyword" }
                    }
                }
            },
                new
            {
                type = "function",
                function = new
                {
                    name = "获取日期和时间",
                    description = "获取当前某一时区的实时时间，默认是当前时区",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            timezone = new
                            {
                                type = "string",
                                description = "所在城市的时区代码"
                            },
                        },
                        required = new[] { "timezone" }
                    }
                }
            }

        };




        public async Task HandleMessageList(string chatId, List<dynamic> messages)
        {
            var msgRecently = messages.Last();
            if ((string)msgRecently.role == "user" || (string)msgRecently.role == "tool")
            {
                // need ask
                var responseJson = await Send(messages);
                if (responseJson == null) return;

                messages.Add(responseJson.message);
                await HandleMessageList(chatId, messages);
                return;
            }

            if ((string)msgRecently.role == "assistant")
            {
                // need deal tool?
                // 检查模型是否决定使用提供的函数
                var toolCalls = msgRecently.tool_calls;
                var content = msgRecently.content;
                var availableFunctions = new Dictionary<string, Func<string[], Task<string>>>{
                    { "在线搜索", funcs.GetWebContent },
                    {"获取日期和时间",funcs.GetCurrentTimeInTimeZone },
                    {"执行python语句",funcs.GetRunPython },
                    //{ "发送语音", funcs.GetSpeak},
                    //{"发送图片", funcs.GetImage },
                };



                string functionResponse = "";
                if (content != null)
                {
                    // 响应莫名其妙的xml形式的函数调用。好像只有ollama在特定上下文的情况下才会出现这种调用，理论上ollama不调函数来着？
                    Match callStringMatch = new Regex(@"<tool_call>(.*)</tool_call>", RegexOptions.Singleline).Match(content.ToString());
                    if (callStringMatch.Success)
                    {
                        try
                        {
                            string jcc = content.ToString().Replace(callStringMatch.Groups[0].Value, "");
                            content = jcc;
                            var ss = callStringMatch.Groups[1].Value.Trim() + "}";
                            var function = JsonConvert.DeserializeObject<dynamic>(ss);
                            var ps = new List<string>();
                            var functionName = function.name.ToString();
                            foreach (var p in function.arguments)
                            {
                                ps.Add(p.First.ToString());
                            }
                            if (availableFunctions.TryGetValue(functionName, out Func<string[], Task<string>> functionToCall))
                            {
                                ps.Add(chatId); // 即在最后一个元素加入chatId用于函数内寻找上下文
                                functionResponse = await functionToCall(ps.ToArray());
                            }

                        }
                        catch (Exception ex)
                        {
                            Logger.Log(ex);
                        }
                    }
                }


                if (toolCalls != null)
                {
                    foreach (var tool in toolCalls)
                    {
                        var functionName = tool.function.name.ToString();
                        var ps = new List<string>();
                        foreach (var p in tool.function.arguments)
                        {
                            ps.Add(p.First.ToString());
                        }
                        if (availableFunctions.TryGetValue(functionName, out Func<string[], Task<string>> functionToCall))
                        {
                            ps.Add(chatId); // 即在最后一个元素加入chatId用于函数内寻找上下文
                            functionResponse = await functionToCall(ps.ToArray());
                            if (!string.IsNullOrWhiteSpace(functionResponse))
                            {
                                break;
                            }
                        }
                    }
                }
                
                if (!string.IsNullOrWhiteSpace(functionResponse))
                {
                    messages.Add(new { role = "tool", content = functionResponse });
                    await HandleMessageList(chatId, messages);
                }
                return;
            }
        }


        private async Task<dynamic> Send(List<object> _messages)
        {
            string jsonRequestBody = "";
            if (ModelName.Contains("qwen"))
            {
                // 如果运行千问，可支持函数库，就加载tool，否则不加载，以防调用出错
                var requestBody = new
                {
                    model = ModelName,
                    messages = _messages,
                    stream = false,
                    tools = OllamaTools,
                };
                jsonRequestBody = JsonConvert.SerializeObject(requestBody);
            }
            else
            {
                var requestBody = new
                {
                    model = ModelName,
                    messages = _messages,
                    stream = false,
                };
                jsonRequestBody = JsonConvert.SerializeObject(requestBody);
            }
            var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");
            Logger.Log("=>" + (jsonRequestBody.Length < 12000 ? jsonRequestBody : jsonRequestBody.Substring(jsonRequestBody.Length - 12000)), LogType.Debug);
            var jsonResponse = await Network.PostAsync(Config.Instance.App.Net.OllamaUri, content);
            Logger.Log("<=" + (jsonResponse.Length < 12000 ? jsonResponse : jsonResponse.Substring(jsonResponse.Length - 12000)), LogType.Debug);

            return JsonConvert.DeserializeObject<dynamic>(jsonResponse);
        }



        //private static async Task<string> SendChatImageRequest(string msg, string[] imgs)
        //{
        //    var requestBody = new
        //    {
        //        model = ModelName,
        //        prompt = msg,
        //        images = imgs,
        //        stream = false,

        //    };
        //    var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
        //    var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");
        //    Logger.Log("=>" + (jsonRequestBody.Length < 200 ? jsonRequestBody : jsonRequestBody.Substring(-200)), LogType.Debug);
        //    var jsonResponse = await Network.PostAsync(Config.Instance.App.Net.OllamaUriG, content);
        //    Logger.Log("<=" + (jsonResponse.Length < 200 ? jsonResponse : jsonResponse.Substring(-200)), LogType.Debug);
        //    var responseJson = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

        //    if (responseJson == null) return null;


        //    string res = responseJson.response;
        //    return res;
        //}


        //public void removeUndealedMessages(List<dynamic> messages)
        //{
        //    int l = messages.Count;
        //    for (int i = 0; i < l; i++)
        //    {
        //        if (messages.Last().role.ToString() == "assistant" || messages.Last().role.ToString() == "system")
        //        {
        //            return;

        //        }
        //        messages.RemoveAt(messages.Count - 1);
        //    }
        //}













        //public async void OllamaReplyWithImage(MessageContext context, string[] imgUrls)
        //{
        //    if (string.IsNullOrWhiteSpace(Config.Instance.App.Net.OllamaUriG)) return;
        //    if (imgUrls == null || imgUrls.Length <= 0)
        //    {
        //        return;
        //    }
        //    else
        //    {
        //        try
        //        {
        //            // 带图消息
        //            var imagesArray = new object[imgUrls.Length];

        //            //// 构造图像对象数组
        //            //for (int i = 0; i < imgUrl.Length; i++)
        //            //{
        //            //    imagesArray[i] = new
        //            //    {
        //            //        url = imgUrl[i], // 图像的 URL 地址
        //            //        description = $"Image {i + 1}" // 可选描述信息
        //            //    };
        //            //}
        //            var res = await SendChatImageRequest(context.recvMessages.ToTextString(), imgUrls);
        //            //UserMessageList[chatid].Add(new { role = "assistant", content = res });

        //            res = Filter.Instance.FiltingBySentense(res, FilterType.Normal);

        //            if (string.IsNullOrWhiteSpace(res)) return;
        //            context.SendBackPlain(res);
        //        }
        //        catch (Exception ex)
        //        {
        //            Logger.Log(ex);
        //        }


        //    }
        //}





        //public override async Task<string> Chat(ChatContext context, string input)
        //{
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(Config.Instance.App.Net.OllamaUri)) return;
        //        if (string.IsNullOrWhiteSpace(context.recvMessages.ToTextString())) return;
        //        string chatId = LLM.Instance.GetChatContext(context).Id;
        //        //string chatId = GetChatId(context.groupId, context.userId);
        //        if (string.IsNullOrWhiteSpace(chatId)) return;
        //        if (!ChatMessageList.ContainsKey(chatId))
        //            ChatMessageList[chatId] = new List<dynamic> { new { role = "system", content = DefaultPrompt } };


        //        //// if filtered pass
        //        //if (!IOFilter.Instance.IsPass(input, FilterType.Normal))
        //        //{
        //        //    // filtered
        //        //    return;
        //        //}
        //        ChatMessageContext[chatId] = context;
        //        ChatMessageList[chatId].Add(new { role = "user", content = context.recvMessages.ToTextString() });

        //        await OllamaHandleMessageList(chatId, ChatMessageList[chatId]);

        //        string res = (string)ChatMessageList[chatId].Last().content;


        //        res = res.Replace("\r\n\r\n", "\n").Replace("\n\n", "\n");
        //        res = Filter.Instance.FiltingBySentense(res, FilterType.Normal);

        //        if (string.IsNullOrWhiteSpace(res)) return;
        //        context.SendBackText(res,true, true);

        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Log(ex);
        //    }
        //}


        public override async Task<string> ChatAsync(ChatContext context)
        {
             return String.Empty;
        }
    }
}
