using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using OpenBLive.Runtime.Utilities;
using MeowMiraiLib.Msg.Type;
using MeowMiraiLib;


namespace LiveServer
{


    public class LocalBotOutMsg
    {
        public long userId;
        public string userName;
        public MeowMiraiLib.Msg.Type.Message[] messages;
    }
    public class LocalBotInMsg
    {
        public long userId;
        public string userName;
        public string type;
        public MeowMiraiLib.Msg.Type.Message[] messages;
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

            FormMonitor.SendMsgEvent("Client connected");

            byte[] buffer = new byte[1024];
            while (clientSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var result = await clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await clientSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    FormMonitor.SendMsgEvent("Client disconnected");
                }
                else
                {
                    string jsonReceived = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    OnMessageReceived?.Invoke(jsonReceived);
                }
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
        public static Message[] RectifyMessage(string messagestr)
        {
            try
            {
                List<Message> list = new List<Message>();
                foreach (JToken item in JArray.Parse(messagestr))
                {
                    switch (item["type"].ToString())
                    {
                        case "Source":
                            list.Add(item.ToObject<Source>());
                            continue;
                        case "Quote":
                            list.Add(new Quote(item["id"].ToObject<long>(), item["groupId"].ToObject<long>(), item["senderId"].ToObject<long>(), item["targetId"].ToObject<long>(), RectifyMessage(item["origin"].ToString())));
                            continue;
                        case "At":
                            list.Add(item.ToObject<At>());
                            continue;
                        case "AtAll":
                            list.Add(item.ToObject<AtAll>());
                            continue;
                        case "Face":
                            list.Add(item.ToObject<Face>());
                            continue;
                        case "Plain":
                            list.Add(item.ToObject<Plain>());
                            continue;
                        case "Image":
                            list.Add(item.ToObject<Image>());
                            continue;
                        case "FlashImage":
                            list.Add(item.ToObject<FlashImage>());
                            continue;
                        case "Voice":
                            list.Add(item.ToObject<Voice>());
                            continue;
                        case "Xml":
                            list.Add(item.ToObject<Xml>());
                            continue;
                        case "Json":
                            list.Add(item.ToObject<Json>());
                            continue;
                        case "App":
                            list.Add(item.ToObject<App>());
                            continue;
                        case "Poke":
                            list.Add(item.ToObject<Poke>());
                            continue;
                        case "Dice":
                            list.Add(item.ToObject<Dice>());
                            continue;
                        case "MarketFace":
                            list.Add(item.ToObject<MarketFace>());
                            continue;
                        case "MusicShare":
                            list.Add(item.ToObject<MusicShare>());
                            continue;
                        case "Forward":
                            list.Add(item.ToObject<ForwardMessage>());
                            continue;
                        case "MiraiCode":
                            list.Add(item.ToObject<MiraiCode>());
                            continue;
                    }

                    Global.Log.Warn($"{"[0012] ParserPhase : Message Typo Error in "}{{{item["type"]}}}");
                }

                return list.ToArray();
            }
            catch
            {
                Global.Log.Warn("[0013] ParserPhase : Message Error in {" + messagestr + "}");
                return null;
            }
        }

    }
}