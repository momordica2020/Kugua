using Kugua.Integrations.NTBot;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using WebSocket4Net;

namespace Kugua.Integrations.VTubeStudio
{
    public class VTSClient
    {
        //
        // 摘要:
        //     客户端Websocket
        public WebSocket ws { get; }

        //
        // 摘要:
        //     重连标识
        public int reconnect { get; private set; }
        public Action<private_message_event> OnPrivateMessageReceive { get; internal set; }
        public Action<group_message_event> OnGroupMessageReceive { get; internal set; }
        public object SessionLock = new object();
        public Queue<JObject> SSMRequestList { get; set; }

        public string url;






        public VTSClient(string url, int reconnect = -1)
        {
            VTSClient client = this;
            this.url = url;
            this.reconnect = reconnect;
            ws = new WebSocket(url);
            ws.Opened += delegate (object? s, EventArgs e)
            {
                Logger.Log("[SocketWatchdog] - Socket Opened -");
                //client._OnServeiceConnected?.Invoke(e.ToString());
            };
            ws.Error += delegate (object? s, SuperSocket.ClientEngine.ErrorEventArgs e)
            {
                //client._OnServeiceError?.Invoke(e.Exception);
                Logger.Log("[SocketWatchdog] - Socket Error , Running to Close or Reconnect -");
                Logger.Log($"{e.Exception}");
                client.ws.Close();
            };
            ws.Closed += delegate (object? s, EventArgs e)
            {
                //client._OnServiceDropped?.Invoke(e.ToString());
                Logger.Log("[SocketWatchdog] - Socket Closed -");
                while (reconnect == -1 || reconnect-- > 0)
                {
                    if (client.ws.State == WebSocketState.Open)
                    {
                        Logger.Log("[SocketWatchdog] - Reconnect Complete-");
                        break;
                    }

                    WebSocketState state = client.ws.State;
                    if (state != 0 && state != WebSocketState.Closing)
                    {
                        Logger.Log("[SocketWatchdog] - Tryin Reconnecting");
                        client.ws.Open();
                    }

                    Logger.Log("[SocketWatchdog] - Trying To Reconnect (in 5 second)-");
                    Task.Delay(5000).GetAwaiter().GetResult();
                }
            };
            ws.MessageReceived += (s, e) =>
            {
                Task.Run(() =>
                {
                    OnMessageReceived(s, e);
                });
            };
        }







        public async Task<string> Send(SenderData sender)
        {
            string messageId = "";
            SenderAPI api = new SenderAPI
            {
                action = sender.GetType().Name,
                echo = MyRandom.NextString(10),
                Params = sender,
            };

            string jsonStr = JsonConvert.SerializeObject(api);
            //Logger.Log(jsonStr,LogType.Mirai);
            var timeout = TimeSpan.FromMilliseconds(5000);
            using var cts = new CancellationTokenSource(timeout);
            var v = Task.Run(async delegate
            {
                try
                {
                    if (ws == null)
                    {
                        Logger.Log(" - Socket Closed -");
                        ws.Close();
                        Logger.Log(" - Trying Reconnect Socket -");
                        ws.Open();
                    }

                    ws?.Send(jsonStr);
                    //return Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            }, cts.Token);
            var completedTask = await Task.WhenAny(v, Task.Delay(timeout));






            return messageId;
        }

        //
        // 摘要:
        //     链接
        public void Connect()
        {
            if (string.IsNullOrEmpty(url))
            {
                //Global.Log.Error("[0001] InitPhase : No Url Specific.");
                throw new Exception("[0001] InitPhase : No Url Specific.");
            }
            ws.Open();
        }

        //
        // 摘要:
        //     异步链接
        public Task<bool> ConnectAsync()
        {
            if (string.IsNullOrEmpty(url))
            {
                //Global.Log.Error("[0001] InitPhase : No Url Specific.");
                throw new Exception("[0001] InitPhase : No Url Specific.");
            }

            return ws.OpenAsync();
        }

        //private Message[] RectifyMessage(string messagestr)
        //{
        //    try
        //    {
        //        List<Message> list = new List<Message>();
        //        foreach (JToken item in JArray.Parse(messagestr))
        //        {
        //            switch (item["type"].ToString())
        //            {
        //                case "Source":
        //                    //list.Add(item.ToObject<Source>());
        //                    continue;

        //            }

        //            //Global.Log.Warn($"{"[0012] ParserPhase : Message Typo Error in "}{{{item["type"]}}}");
        //        }

        //        return list.ToArray();
        //    }
        //    catch
        //    {
        //        //Global.Log.Warn("[0013] ParserPhase : Message Error in {" + messagestr + "}");
        //        return null;
        //    }
        //}
        private void parseMessages(List<Message> msgs, JToken[] jo)
        {
            try
            {
                if (msgs == null) return;
                foreach (var mj in jo)
                {
                    if (mj["type"] != null)
                    {
                        string typename = mj["type"].ToString();
                        switch (typename)
                        {
                            case "text": msgs.Add(JsonConvert.DeserializeObject<Text>(mj["data"].ToString())); break;
                            case "image": msgs.Add(JsonConvert.DeserializeObject<Image>(mj["data"].ToString())); break;
                            case "face": msgs.Add(JsonConvert.DeserializeObject<Face>(mj["data"].ToString())); break;
                            case "at": msgs.Add(JsonConvert.DeserializeObject<At>(mj["data"].ToString())); break;
                            case "video": msgs.Add(JsonConvert.DeserializeObject<Video>(mj["data"].ToString())); break;
                            case "rps": msgs.Add(JsonConvert.DeserializeObject<Rps>(mj["data"].ToString())); break;
                            case "dice": msgs.Add(JsonConvert.DeserializeObject<Dice>(mj["data"].ToString())); break;
                            case "shake": msgs.Add(JsonConvert.DeserializeObject<Shake>(mj["data"].ToString())); break;
                            case "poke": msgs.Add(JsonConvert.DeserializeObject<Poke>(mj["data"].ToString())); break;
                            case "anonymous": msgs.Add(JsonConvert.DeserializeObject<AnonymousMesssage>(mj["data"].ToString())); break;
                            case "share": msgs.Add(JsonConvert.DeserializeObject<Share>(mj["data"].ToString())); break;
                            case "contact": msgs.Add(JsonConvert.DeserializeObject<Contact>(mj["data"].ToString())); break;
                            case "location": msgs.Add(JsonConvert.DeserializeObject<Location>(mj["data"].ToString())); break;
                            case "music": msgs.Add(JsonConvert.DeserializeObject<Music>(mj["data"].ToString())); break;
                            case "reply": msgs.Add(JsonConvert.DeserializeObject<Reply>(mj["data"].ToString())); break;
                            case "record": msgs.Add(JsonConvert.DeserializeObject<Record>(mj["data"].ToString())); break;
                            case "xml": msgs.Add(JsonConvert.DeserializeObject<XmlData>(mj["data"].ToString())); break;
                            case "json": msgs.Add(JsonConvert.DeserializeObject<JsonData>(mj["data"].ToString())); break;
                            default: break;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

        }


        private async void OnMessageReceived(object? s, MessageReceivedEventArgs e)
        {
            try
            {
                string json = e.Message;

                JObject jo = JObject.Parse(json);
                //Logger.Log(jo.ToString());
                if (jo["status"] != null)
                {
                    
                    return;
                }
                
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }

        }



    }
}
