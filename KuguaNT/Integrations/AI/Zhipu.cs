


using System.Text;

using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;

using Newtonsoft.Json;
using System.Web;
using Kugua.Integrations.NTBot;
using System.Reflection;
using System.Text.Json.Nodes;
using ZhipuApi.Models.RequestModels.FunctionModels;
using ZhipuApi.Models.RequestModels;
using ZhipuApi;
using NvAPIWrapper.Native.GPU;
using ZhipuApi.Models.RequestModels.ImageToTextModels;
using ZhipuApi.Modules;
using ImageMagick;
using System.Buffers.Text;



namespace Kugua
{
    /// <summary>
    ///  对接智谱API
    /// </summary>
    public partial class GPT
    {
        const string apikey = "f2517ff9b5aaab14138d95b14810a779.FoEa7zFIfqn2VfAb";
        ClientV4 clientV4;

        static async Task<T?> InvokeInstanceMethodAsync<T>(string chatid, object instance, string methodName, string parameters = null)
        {
            // 获取实例对象的类型
            Type type = instance.GetType();

            // 获取指定名称的方法
            MethodInfo method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);

            if (method != null)
            {
                // 调用方法并获取返回值
                var returnValue = method.Invoke(instance, new object[] { chatid, parameters });
                // 检查返回值是否为 Task 类型
                if (returnValue is Task task)
                {
                    // 等待异步任务完成并返回结果
                    await task.ConfigureAwait(false);

                    // 如果返回的 Task 是泛型，获取其 Result
                    if (task.GetType().IsGenericType)
                    {
                        return (T?)task.GetType().GetProperty("Result")?.GetValue(task);
                    }

                    return default;
                }
                else
                {
                    // 同步方法直接返回结果
                    return (T?)returnValue;
                }
            }
            else
            {
                return default;
            }
        }

        //Dictionary<string, string> ZPtoolname = new Dictionary<string, string>
        //{
        //    {"get_speak","GetSpeak" },
        //};
        List<FunctionTool> ZPTools = new List<FunctionTool>
        {
            new FunctionTool()
                .SetName("speak_sentense")
                .SetDescription("发送语音给用户")
                .SetParameters(new FunctionParameters()
                .AddParameter("sentense", ParameterType.String, "要说的内容")
                //.AddParameter("days", ParameterType.Integer, "要查询的未来的天数，默认为0")
                .SetRequiredParameter(["sentense"])),
        };

        private void ZPHandleList(string chatId)
        {
            var messages = ChatMessageList[chatId];
            List<MessageItem> items = new List<MessageItem>();
            foreach (var item in messages) { items.Add(new MessageItem((string)item.role.ToString(), (string)item.content.ToString())); }
            if (items.Count > 0)
            {
                int trynum = 5;
                while (items.Last().role == "tool" || items.Last().role == "user")
                {
                    // send tool callback
                    ZPSend(chatId, items);

                    if (trynum-- < 0) break;
                }
            }
            messages.Clear();
            foreach (var item in items) { messages.Add(new MessageItem(item.role, item.content)); }
        }


        private string ZPHandleV(List<MessageItem> msgs)
        {
            string res = "";
            var resp = clientV4.chat.Completion(
                new TextRequestBase()
                    .SetModel("glm-4v")//"glm-4")
                    .SetMessages(msgs.ToArray())
                    .SetTemperature(0.5)
                    .SetTopP(0.7)
            );


            var data = JsonConvert.SerializeObject(resp);
            Logger.Log(data);


            if (resp != null && resp.choices?.Length > 0)
            {
                var rspi = resp.choices.FirstOrDefault().message;
                res = rspi.content;
            }
            else
            {
                if (resp?.error != null)
                {
                    foreach (var err in resp.error)
                    {
                        Logger.Log($"ERROR:({err.Key}){err.Value}");
                        return $"ERROR{err.Key}:{err.Value}";
                    }

                }
                //msgs.RemoveAt(msgs.Count - 1);
            }
            return res;

        }
        private void ZPSend(string chatid, List<MessageItem> msgs)
        {
            var resp = clientV4.chat.Completion(
                new TextRequestBase()
                    .SetModel("glm-4")//"glm-4")
                    .SetMessages(msgs.ToArray())
                    .SetTools(ZPTools.ToArray())
                    .SetTemperature(0.5)
                    .SetTopP(0.7)
            );


            var data = JsonConvert.SerializeObject(resp);

            Logger.Log(data);


            if (resp != null && resp.choices?.Length > 0)
            {
                var rspi = resp.choices.FirstOrDefault().message;
                if (rspi.tool_calls?.Length > 0)
                {
                    // need toolcall
                    string fname = rspi.tool_calls.FirstOrDefault().function.name;
                    string fparams = rspi.tool_calls.FirstOrDefault().function.arguments;
                    string toolCallstr = $"{fname},{fparams}";

                    string toolCallResponse = InvokeInstanceMethodAsync<string>(chatid, this, fname, fparams).Result;

                    //msgs.Add(new MessageItem(rspi.role, toolCallstr));
                    msgs.Add(new MessageItem("tool", toolCallResponse));
                }
                else
                {

                    msgs.Add(new MessageItem(rspi.role, rspi.content));
                }

            }
            else
            {
                if (resp?.error != null)
                {
                    foreach (var err in resp.error)
                    {
                        Logger.Log($"ERROR:({err.Key}){err.Value}");
                    }

                }
                msgs.RemoveAt(msgs.Count - 1);
            }
        }

        public void ZPImage(MessageContext context, string desc)
        {
            var resp = clientV4.images.Generation(
                new ImageRequestBase()
                    .SetModel("cogview-3-plus")//"glm-4")
                    .SetPrompt(desc.Trim())
            );


            var data = JsonConvert.SerializeObject(resp);
            Logger.Log(data);


            if (resp != null && resp.data?.Count > 0)
            {
                var imgurl = resp.data.FirstOrDefault().url;
                var base64 = Network.DownloadImage(imgurl).ToBase64();
                context.SendBack([new Image($"base64://{base64}")]);
            }
            else
            {
                if (resp?.error != null)
                {
                    foreach (var err in resp.error)
                    {
                        Logger.Log($"ERROR:({err.Key}){err.Value}");
                        context.SendBackPlain($"ERROR{err.Key}:{err.Value}");
                    }

                }
                //msgs.RemoveAt(msgs.Count - 1);
            }
        }
        public string ZPGetImgDesc(string imgBase64, string imgDesc)
        {
            string res = "";

            if (string.IsNullOrWhiteSpace(imgDesc)) imgDesc = "描述这张图片的内容";
            if (imgBase64 != null)
            {
                res = ZPHandleV([ new ImageToTextMessageItem("user")
                    .setText(imgDesc)
                    .setImageUrl(imgBase64)]);
            }

            return res;
        }
        public string ZPChat(MessageContext context)
        {
            try
            {
                string chatId = GetChatId(context.groupId, context.userId);
                if (string.IsNullOrWhiteSpace(chatId)) return "";




                if (!ChatMessageList.ContainsKey(chatId))
                    ChatMessageList[chatId] = new List<dynamic> { new { role = "system", content = GetDefaultPrompt() } };
                ChatMessageContext[chatId] = context;


                if (context.isImage)
                {
                    // image!
                    string imgBase64 = context.PNG1Base64;
                    string imgDesc = context.recvMessages.ToTextString();
                    var imgdesc = ZPGetImgDesc(imgBase64, imgDesc);
                    ChatMessageList[chatId].Add(new MessageItem("assistant", imgdesc));
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(context.recvMessages.ToTextString())) return "";
                    ChatMessageList[chatId].Add(new { role = "user", content = context.recvMessages.ToTextString() });
                    ZPHandleList(chatId);
                }
                //// if filtered pass
                //if (!IOFilter.Instance.IsPass(input, FilterType.Normal))
                //{
                //    // filtered
                //    return;
                //}




                string res = (string)ChatMessageList[chatId].Last().content;
                if (res.Contains("对话区:"))
                {
                    // remove unnessary
                    res = res.Substring(res.IndexOf("对话区:") + 4);
                }

                res = res.Replace("\r\n\r\n", "\r\n").Replace("\n\n", "\n");
                res = Filter.Instance.FiltingBySentense(res, FilterType.Normal);

                if (string.IsNullOrWhiteSpace(res)) return "";
                return res;

            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }


            // 
            //new ImageToTextMessageItem("user")
            //    .setText("她的脚在女生里算大吗？对她的打扮有什么建议谢谢")
            //    .setImageUrl("https://img.soufunimg.com/news/2008_08/25/news/1219645619902_000.jpg")
            return "";
        }
    }
}
