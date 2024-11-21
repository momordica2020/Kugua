using Microsoft.AspNetCore.SignalR.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocket4Net;

namespace Kugua.Integrations.NTBot
{
    public class NTBot
    {
        //
        // 摘要:
        //     客户端Websocket
        public WebSocket ws { get; }

        public Dictionary<string, SenderAPI> sessions=new Dictionary<string, SenderAPI>();
 
        //
        // 摘要:
        //     重连标识
        public int reconnect { get; private set; }
        public Action<PrivateMessageEvent> OnPrivateMessageReceive { get; internal set; }
        public Action<GroupMessageEvent> OnGroupMessageReceive { get; internal set; }

        public Queue<JObject> SSMRequestList { get; set; }

        public string url;






        public NTBot(string url, int reconnect = -1)
        {
            NTBot client = this;
            this.url = url;
            this.reconnect = reconnect;
            ws = new WebSocket(url);
            ws.Opened += delegate (object? s, EventArgs e)
            {
                Logger.Log("[MeowMiraiLib-SocketWatchdog] - Socket Opened -");
                //client._OnServeiceConnected?.Invoke(e.ToString());
            };
            ws.Error += delegate (object? s, SuperSocket.ClientEngine.ErrorEventArgs e)
            {
                //client._OnServeiceError?.Invoke(e.Exception);
                Logger.Log("[MeowMiraiLib-SocketWatchdog] - Socket Error , Running to Close or Reconnect -");
                Logger.Log($"{e.Exception}");
                client.ws.Close();
            };
            ws.Closed += delegate (object? s, EventArgs e)
            {
                //client._OnServiceDropped?.Invoke(e.ToString());
                Logger.Log("[MeowMiraiLib-SocketWatchdog] - Socket Closed -");
                while (reconnect == -1 || reconnect-- > 0)
                {
                    if (client.ws.State == WebSocketState.Open)
                    {
                        Logger.Log("[MeowMiraiLib-SocketWatchdog] - Reconnect Complete-");
                        break;
                    }

                    WebSocketState state = client.ws.State;
                    if (state != 0 && state != WebSocketState.Closing)
                    {
                        Logger.Log("[MeowMiraiLib-SocketWatchdog] - Tryin Reconnecting");
                        client.ws.Open();
                    }

                    Logger.Log("[MeowMiraiLib-SocketWatchdog] - Trying To Reconnect (in 5 second)-");
                    Task.Delay(5000).GetAwaiter().GetResult();
                }
            };
            ws.MessageReceived += Ws_MessageReceived;
        }




        //public static List<Message> ParseMessageArray(JArray messageArray)
        //{
        //    List<Message> messages = new List<Message>();

        //    foreach (var messageSegment in messageArray)
        //    {
        //        var type = messageSegment["type"].ToString(); // 获取类型字段

        //        Message? message = type switch
        //        {
        //            "text" => messageSegment.ToObject<Plain>(),
        //            "image" => messageSegment.ToObject<Image>(),
        //            "face" => messageSegment.ToObject<Face>(),
        //            "record" => messageSegment.ToObject<Record>(),
        //            "video" => messageSegment.ToObject<Video>(),
        //            "share" => messageSegment.ToObject<Share>(),
        //            "at" => messageSegment.ToObject<At>(),
        //            "anonymous" => messageSegment.ToObject<AnonymousMesssage>(),
        //            "contact" => messageSegment.ToObject<Contact>(),
        //            "location" => messageSegment.ToObject<Location>(),
        //            "music" => messageSegment.ToObject<Music>(),
        //            "reply" => messageSegment.ToObject<Reply>(),
        //            "forward" => messageSegment.ToObject<ForwardMessage>(),
        //            "poke" => messageSegment.ToObject<Poke>(),
        //            _ => messageSegment.ToObject<MessageData>(),// throw new Exception($"Unknown message type: {type}")
        //        };

        //        messages.Add(message);
        //    }

        //    return messages;
        //}









        public async void Send(SenderData sender)
        {
            SenderAPI api = new SenderAPI
            {
                action = sender.GetType().Name,
                echo = MyRandom.NextString(10),
                Params = sender,
            };
            sessions[api.echo] = api;

            string jsonStr = JsonConvert.SerializeObject(api);
            //Logger.Log(jsonStr,LogType.Mirai);
            try
            {
                var v = Task.Run(delegate
                {
                    if (ws == null)
                    {
                        Logger.Log(" - Socket Closed -");
                        ws.Close();
                        Logger.Log(" - Trying Reconnect Socket -");
                        ws.Open();
                    }

                    ws?.Send(jsonStr);
                    return Task.CompletedTask;
                });
    

            }
            catch(Exception ex)
            {
                Logger.Log(ex);
            }
            finally
            {
            }
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

        private async void Ws_MessageReceived(object? s, MessageReceivedEventArgs e)
        {
            string json = e.Message;

            JObject jo = JObject.Parse(json);
            //Logger.Log(jo.ToString());
            if (jo["status"] != null)
            {
                // reply!
                var reply = JsonConvert.DeserializeObject<SenderReplyAPI>(json);
                if (reply != null)
                {
                    if (sessions.ContainsKey(reply.echo))
                    {
                        var sd = sessions[reply.echo];
                        switch (sd.action)
                        {
                            case "send_private_msg":
                                var oo = JsonConvert.DeserializeObject<send_msg_reply>(jo["data"].ToString());
                                break;
                            default:
                                break;
                        }
                        sessions.Remove(reply.echo);

                    }
                }
                return;
            }
            else
            {
                var baseEvent = JsonConvert.DeserializeObject<BaseEvent>(json);

                // 根据 post_type 选择解析对应的事件类型

                switch (baseEvent.post_type)
                {
                    case "message":
                        // 根据 message_type 判断是私聊消息还是群消息
                        if (jo["message_type"].ToString() == "private")
                        {
                            var eo = JsonConvert.DeserializeObject<PrivateMessageEvent>(json);
                            eo.message = new List<Message>();
                            if (jo?["message"] != null)
                            {
                                foreach (var mj in jo["message"].ToArray())
                                {
                                    if (mj["type"] != null)
                                    {
                                        string typename = mj["type"].ToString();
                                        switch (typename)
                                        {
                                            case "text": eo.message.Add(JsonConvert.DeserializeObject<Text>(mj["data"].ToString())); break;
                                            case "image": eo.message.Add(JsonConvert.DeserializeObject<Image>(mj["data"].ToString())); break;
                                            case "face": eo.message.Add(JsonConvert.DeserializeObject<Face>(mj["data"].ToString())); break;
                                            case "at": eo.message.Add(JsonConvert.DeserializeObject<At>(mj["data"].ToString())); break;
                                            case "video": eo.message.Add(JsonConvert.DeserializeObject<Video>(mj["data"].ToString())); break;
                                            case "rps": eo.message.Add(JsonConvert.DeserializeObject<Rps>(mj["data"].ToString())); break;
                                            case "dice": eo.message.Add(JsonConvert.DeserializeObject<Dice>(mj["data"].ToString())); break;
                                            case "shake": eo.message.Add(JsonConvert.DeserializeObject<Shake>(mj["data"].ToString())); break;
                                            case "poke": eo.message.Add(JsonConvert.DeserializeObject<Poke>(mj["data"].ToString())); break;
                                            case "anonymous": eo.message.Add(JsonConvert.DeserializeObject<AnonymousMesssage>(mj["data"].ToString())); break;
                                            case "share": eo.message.Add(JsonConvert.DeserializeObject<Share>(mj["data"].ToString())); break;
                                            case "contact": eo.message.Add(JsonConvert.DeserializeObject<Contact>(mj["data"].ToString())); break;
                                            case "location": eo.message.Add(JsonConvert.DeserializeObject<Location>(mj["data"].ToString())); break;
                                            case "music": eo.message.Add(JsonConvert.DeserializeObject<Music>(mj["data"].ToString())); break;
                                            case "reply": eo.message.Add(JsonConvert.DeserializeObject<Reply>(mj["data"].ToString())); break;
                                            case "record": eo.message.Add(JsonConvert.DeserializeObject<Record>(mj["data"].ToString())); break;
                                            case "xml": eo.message.Add(JsonConvert.DeserializeObject<XmlData>(mj["data"].ToString())); break;
                                            case "json": eo.message.Add(JsonConvert.DeserializeObject<JsonData>(mj["data"].ToString())); break;
                                            default: break;
                                        }

                                    }
                                }
                            }
                            OnPrivateMessageReceive?.Invoke(eo);
                        }
                        else if (jo["message_type"].ToString() == "group")
                        {
                            var eo = JsonConvert.DeserializeObject<GroupMessageEvent>(json);
                            eo.message = new List<Message>();
                            if (jo?["message"] != null)
                            {
                                foreach (var mj in jo["message"].ToArray())
                                {
                                    if (mj["type"] != null)
                                    {
                                        string typename = mj["type"].ToString();
                                        switch (typename)
                                        {
                                            case "text": eo.message.Add(JsonConvert.DeserializeObject<Text>(mj["data"].ToString())); break;
                                            case "image": eo.message.Add(JsonConvert.DeserializeObject<Image>(mj["data"].ToString())); break;
                                            case "face": eo.message.Add(JsonConvert.DeserializeObject<Face>(mj["data"].ToString())); break;
                                            case "at": eo.message.Add(JsonConvert.DeserializeObject<At>(mj["data"].ToString())); break;
                                            case "video": eo.message.Add(JsonConvert.DeserializeObject<Video>(mj["data"].ToString())); break;
                                            case "rps": eo.message.Add(JsonConvert.DeserializeObject<Rps>(mj["data"].ToString())); break;
                                            case "dice": eo.message.Add(JsonConvert.DeserializeObject<Dice>(mj["data"].ToString())); break;
                                            case "shake": eo.message.Add(JsonConvert.DeserializeObject<Shake>(mj["data"].ToString())); break;
                                            case "poke": eo.message.Add(JsonConvert.DeserializeObject<Poke>(mj["data"].ToString())); break;
                                            case "anonymous": eo.message.Add(JsonConvert.DeserializeObject<AnonymousMesssage>(mj["data"].ToString())); break;
                                            case "share": eo.message.Add(JsonConvert.DeserializeObject<Share>(mj["data"].ToString())); break;
                                            case "contact": eo.message.Add(JsonConvert.DeserializeObject<Contact>(mj["data"].ToString())); break;
                                            case "location": eo.message.Add(JsonConvert.DeserializeObject<Location>(mj["data"].ToString())); break;
                                            case "music": eo.message.Add(JsonConvert.DeserializeObject<Music>(mj["data"].ToString())); break;
                                            case "reply": eo.message.Add(JsonConvert.DeserializeObject<Reply>(mj["data"].ToString())); break;
                                            case "record": eo.message.Add(JsonConvert.DeserializeObject<Record>(mj["data"].ToString())); break;
                                            case "xml": eo.message.Add(JsonConvert.DeserializeObject<XmlData>(mj["data"].ToString())); break;
                                            case "json": eo.message.Add(JsonConvert.DeserializeObject<JsonData>(mj["data"].ToString())); break;
                                            default: break;
                                        }

                                    }
                                }
                            }
                            OnGroupMessageReceive?.Invoke(eo);
                        }
                        break;

                    case "notice":
                        // 处理通知事件
                        if (jo["notice_type"].ToString() == "group_upload")
                        {
                            var eo = JsonConvert.DeserializeObject<GroupFileUploadEvent>(json);
                        }
                        else if (jo["notice_type"].ToString() == "group_admin")
                        {
                            var eo = JsonConvert.DeserializeObject<GroupAdminChangeEvent>(json);
                        }
                        break;

                    case "meta_event":
                        // 处理元事件
                        if (jo["meta_event_type"].ToString() == "lifecycle")
                        {
                            var eo = JsonConvert.DeserializeObject<LifecycleEvent>(json);
                        }
                        else if (jo["meta_event_type"].ToString() == "heartbeat")
                        {
                            var eo = JsonConvert.DeserializeObject<HeartbeatEvent>(json);
                        }
                        break;

                    case "request":
                        // 处理请求事件
                        if (jo["request_type"].ToString() == "friend")
                        {
                            var eo = JsonConvert.DeserializeObject<FriendRequestEvent>(json);
                        }
                        else if (jo["request_type"].ToString() == "group")
                        {
                            var eo = JsonConvert.DeserializeObject<GroupRequestEvent>(json);
                        }
                        break;

                    default:
                        Logger.Log("Unsupported post_type");
                        break;
                }


            }

        }



    }

}
