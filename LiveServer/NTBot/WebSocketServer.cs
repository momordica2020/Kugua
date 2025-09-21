using ImageMagick;
using Kugua.Integrations.NTBot;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenBLive.Runtime.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace LiveServer.NTBot
{


    public class LBotResponse
    {
        public long userId;
        public string userName;
        public List<MessageInfo> messages = new List<MessageInfo>();
    }
    public class LBotRequest
    {
        public long userId;
        public string userName;
        public string type;
        public List<MessageInfo> messages = new List<MessageInfo>();
    }

    public class WebSocketServer
    {
        public delegate void MessageReceivedHandler(string message);
        public static event MessageReceivedHandler OnMessageReceived;
        private static HttpListener listener;
        private static WebSocket clientSocket;
        private static CancellationTokenSource cancellationTokenSource;

        public static async void Start(string uri)
        {
            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add(uri);
                listener.Start();

                FormMonitor.SendMsgEvent("WebSocket Server started at " + uri);
                cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;

                while (!cancellationToken.IsCancellationRequested)
                {

                    var context = await listener.GetContextAsync();
                    if (context.Request.IsWebSocketRequest)
                    {
                        await ProcessData(context, cancellationToken);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                FormMonitor.SendMsgEvent("HttpListener has been disposed.");
            }
            catch (Exception ex)
            {
                FormMonitor.SendMsgEvent("An error occurred: " + ex.Message);
            }
        }

        private static async Task ProcessData(HttpListenerContext context, CancellationToken cancellationToken)
        {
            var wsContext = await context.AcceptWebSocketAsync(null);
            clientSocket = wsContext.WebSocket;
            FormMonitor.SendMsgEvent("*本地bot已接入*");

            byte[] buffer = new byte[1024 * 1024 * 5];
            try
            {
                while (clientSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                {
                    var result = await clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await clientSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        FormMonitor.SendMsgEvent("*本地bot已断开*");
                    }
                    else
                    {
                        string jsonReceived = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        OnMessageReceived?.Invoke(jsonReceived);
                    }
                }
            }
            catch (Exception ex)
            {
                FormMonitor.SendMsgEvent($"* {ex.Message}\r\n{ex.StackTrace}");
                FormMonitor.SendMsgEvent("*本地bot已断开*");
            }

        }



        // 向客户端发送消息的公共方法
        public static async Task SendMessageAsync(object message)
        {
            if (clientSocket?.State == WebSocketState.Open)
            {
                string json = JObject.FromObject(message).ToString();
                byte[] buffer = Encoding.UTF8.GetBytes(json);
                await clientSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                //FormMonitor.SendMsgEvent("Sent JSON: " + json);

            }
            else
            {
                FormMonitor.SendMsgEvent("No connected client to send message to.");
            }
        }

        public static void Stop()
        {
            try
            {
                cancellationTokenSource?.Cancel();
                if (listener!=null && listener.IsListening)
                {
                    listener.Stop();
                    listener.Close();
                    FormMonitor.SendMsgEvent("WebSocket Server stopped.");
                }
            }catch(Exception ex)
            {
                FormMonitor.SendMsgEvent($"{ex}");
            }
            
        }
        public static void Parse(string messagestr, List<Message> msgs)
        {
            try
            {
                if (msgs == null) return;
                foreach (JToken item in JArray.Parse(messagestr))
                {
                    var data = item["data"].ToString();
                    var type = item["type"].ToString();
                    switch (type)
                    {
                        case "text": msgs.Add(JsonConvert.DeserializeObject<Text>(data)); break;
                        case "image":
                            //var imageBasic = JsonConvert.DeserializeObject<ImageRecvBasic>(data);
                            if (item["data"]["emoji_id"] != null)// .summary== "marketface")
                            {
                                // 市场表情，打印出来以便使用喵
                                var mi = JsonConvert.DeserializeObject<ImageRecvMarketFace>(data);
                                Logger.Log($"[市场表情]{mi.summary},{mi.emoji_package_id},{mi.emoji_id},{mi.key}");
                                msgs.Add(mi);
                            }
                            else
                            {
                                // 普通发图，也可能是[动画表情]
                                var ni = JsonConvert.DeserializeObject<ImageRecvNormal>(data);
                                msgs.Add(ni);
                            }

                            //Logger.Log(data); 
                            break;
                        case "face": msgs.Add(JsonConvert.DeserializeObject<Face>(data)); break;
                        case "at": msgs.Add(JsonConvert.DeserializeObject<At>(data)); break;
                        default: break;
                    }

                    //Log.Warn($"{"[0012] ParserPhase : Message Typo Error in "}{{{item["type"]}}}");
                }

            }
            catch
            {
                //Log.Warn("[0013] ParserPhase : Message Error in {" + messagestr + "}");
            }
        }

    }
}