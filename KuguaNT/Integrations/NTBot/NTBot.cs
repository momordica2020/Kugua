using Microsoft.AspNetCore.SignalR.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;
using WebSocket4Net;
using static System.Net.WebRequestMethods;

namespace Kugua.Integrations.NTBot
{
    public class NTBot
    {
        //
        // 摘要:
        //     客户端Websocket
        public WebSocket ws { get; }

        public Dictionary<string, SenderAPI> sessions=new Dictionary<string, SenderAPI>();
        public Dictionary<string, string> session_messageid = new Dictionary<string, string>();


        //
        // 摘要:
        //     重连标识
        public int reconnect { get; private set; }
        public Action<private_message_event> OnPrivateMessageReceive { get; internal set; }
        public Action<group_message_event> OnGroupMessageReceive { get; internal set; }
        public object SessionLock = new object();
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
                    OnMessageReceived(s,e);
                });
            };
        }




        /// <summary>
        /// 发送戳一戳
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        public async void SendPoke(string groupId, string userId)
        {
            string uri = Config.Instance.App.Net.QQHTTP + (string.IsNullOrWhiteSpace(groupId)?"/friend_poke":"/group_poke");
            //Logger.Log(uri);
            string res = await Network.PostJsonAsync(uri, JsonConvert.SerializeObject(new SendPoke { group_id=groupId, user_id=userId }));
            Logger.Log(res);
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
            sessions[api.echo] = api;

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
                   // Logger.Log(jsonStr, LogType.Mirai);
                    ws?.Send(jsonStr);


                    while(true)
                    {
                        await Task.Delay(100);
                        lock (SessionLock)
                        {
                            if (session_messageid.ContainsKey(api.echo))
                            {
                                messageId = session_messageid[api.echo];
                                session_messageid.Remove(api.echo);
                                break;
                            }
                        }

                    }
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
                            //case "shake": msgs.Add(JsonConvert.DeserializeObject<Shake>(mj["data"].ToString())); break;
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
            }catch(Exception ex)
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
                    // reply!
                    var reply = JsonConvert.DeserializeObject<SenderReplyAPI>(json);
                    //Logger.Log(jo.ToString());
                    if (reply != null && reply.echo!=null)
                    {
                        if (sessions.ContainsKey(reply.echo))
                        {
                            var sd = sessions[reply.echo];
                            switch (sd.action)
                            {
                                case "send_private_msg":
                                case "send_group_msg":
                                    {
                                        var oo = JsonConvert.DeserializeObject<send_msg_reply>(jo["data"].ToString());
                                        if (oo != null &&  oo.message_id != null)
                                        {
                                            lock (SessionLock)
                                            {
                                                session_messageid[reply.echo] = oo.message_id.ToString();
                                            }
                                        } 
                                        //Logger.Log("SEND SESSION  " + oo.message_id);
                                        break;
                                    }
                                case "get_msg":
                                    {
                                        var oo = JsonConvert.DeserializeObject<get_msg_reply>(jo["data"].ToString());
                                        oo.message = new List<Message>();
                                        if (jo?["data"]?["message"] != null)
                                        {
                                            parseMessages(oo.message, jo["data"]["message"].ToArray());
                                        }
                                            lock (SessionLock)
                                            {
                                                session_messageid[reply.echo] = oo.message.ToTextString();
                                            }
                                            
                                        //Logger.Log("READ DATA  " + oo.message.ToTextString());
                                        break;
                                    }
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
                    // event!
                    var baseEvent = JsonConvert.DeserializeObject<event_base>(json);

                    // 根据 post_type 选择解析对应的事件类型

                    switch (baseEvent.post_type)
                    {
                        case "message":
                            // 根据 message_type 判断是私聊消息还是群消息
                            if (jo["message_type"]?.ToString() == "private")
                            {
                                var eo = JsonConvert.DeserializeObject<private_message_event>(json);
                                eo.message = new List<Message>();
                                if (jo?["message"] != null)
                                {
                                    parseMessages(eo.message, jo["message"].ToArray());
                                }
                                OnPrivateMessageReceive?.Invoke(eo);
                            }
                            else if (jo["message_type"].ToString() == "group")
                            {
                                var eo = JsonConvert.DeserializeObject<group_message_event>(json);
                                eo.message = new List<Message>();
                                if (jo?["message"] != null)
                                {
                                    parseMessages(eo.message, jo["message"].ToArray());
                                }
                                OnGroupMessageReceive?.Invoke(eo);
                            }
                            break;

                        case "notice":
                            // 处理通知事件
                            string notice_type = jo["notice_type"]?.ToString();
                            switch (notice_type)
                            {
                                case "group_upload":
                                {
                                    var eo = JsonConvert.DeserializeObject<group_upload_event>(json);
                                    Logger.Log($"[群文件][{eo.group_id}]{eo.user_id}上传了文件{eo.file.name}(大小{eo.file.size}B)");
                                    break;
                                }   
                                case "group_admin":
                                {
                                    var eo = JsonConvert.DeserializeObject<group_admin_event>(json);
                                    Logger.Log($"[群管理][{eo.group_id}]{eo.user_id}{(eo.sub_type == "set" ? "被设为" : "被取消")}管理员");
                                    break;
                                }
                                case "group_decrease":
                                    {
                                        var eo = JsonConvert.DeserializeObject<group_decrease_event>(json);
                                        Logger.Log($"[群减员][{eo.group_id}]{eo.user_id}{(eo.sub_type == "leave" ? "主动退群" : "被踢出群")}{(!string.IsNullOrWhiteSpace(eo.operator_id)?$"  操作者{eo.operator_id}":"")}");
                                        break;
                                    }
                                case "group_increase":
                                    {
                                        var eo = JsonConvert.DeserializeObject<group_decrease_event>(json);
                                        Logger.Log($"[群加人][{eo.group_id}]{eo.user_id}{(eo.sub_type == "approve" ? "被邀入群" : "主动加群")}{(!string.IsNullOrWhiteSpace(eo.operator_id) ? $"  操作者{eo.operator_id}" : "")}");
                                        break;
                                    }
                                case "group_ban":
                                    {
                                        var eo = JsonConvert.DeserializeObject<group_ban_event>(json);
                                        Logger.Log($"[群禁言][{eo.group_id}]{eo.user_id}{(eo.sub_type == "ban" ? ($"被禁言{eo.duration}秒") : "被解除禁言")}{(!string.IsNullOrWhiteSpace(eo.operator_id) ? $"  操作者{eo.operator_id}" : "")}");
                                        break;
                                    }
                                case "friend_add":
                                    {
                                        var eo = JsonConvert.DeserializeObject<friend_add_event>(json);
                                        Logger.Log($"[加好友]{eo.user_id}加你为好友");
                                        break;
                                    }
                                case "group_recall":
                                    {
                                        var eo = JsonConvert.DeserializeObject<group_recall_event>(json);
                                        Logger.Log($"[群撤回][{eo.group_id}]{eo.user_id}发的消息，编号{eo.message_id}，被撤回{(!string.IsNullOrWhiteSpace(eo.operator_id) ? $"  操作者{eo.operator_id}" : "")}");
                                        break;
                                    }
                                case "friend_recall":
                                    {
                                        var eo = JsonConvert.DeserializeObject<friend_recall_event>(json);
                                        Logger.Log($"[私聊撤回]{eo.user_id}发的消息，编号{eo.message_id}，被撤回");
                                        break;
                                    }
                                case "notify":
                                    {
                                        if (jo["sub_type"]?.ToString() == "honor")
                                        {
                                            // honor_event
                                            var eo = JsonConvert.DeserializeObject<notify_honor_event>(json);
                                            Logger.Log($"[群荣耀][{eo.group_id}]{eo.user_id}获得群荣耀{eo.honor_type}");
                                        }
                                        else
                                        {
                                            var eo = JsonConvert.DeserializeObject<notify_event>(json);
                                            if (eo.sub_type == "poke")
                                            {
                                                Logger.Log($"[戳一戳][{eo.group_id}]{eo.user_id}戳了戳{eo.target_id}");
                                                if(eo.target_id == Config.Instance.BotQQ)
                                                {
                                                    if (string.IsNullOrWhiteSpace(eo.group_id))
                                                    {
                                                        // friend
                                                        var seo = new private_message_event
                                                        {
                                                            user_id = eo.user_id,
                                                            self_id = eo.self_id,
                                                            sender = new message_sender
                                                            {
                                                                user_id = eo.user_id,
                                                                nickname = eo.user_id,
                                                            }
                                                        };
                                                        seo.message = new List<Message>() { new Text(" ") };
                                                        OnPrivateMessageReceive?.Invoke(seo);
                                                    }
                                                    else
                                                    {
                                                        // group
                                                        var seo = new group_message_event
                                                        {
                                                            message_id = "",
                                                            group_id = eo.group_id,
                                                            user_id = eo.user_id,
                                                            self_id = eo.self_id,
                                                            sender = new message_sender
                                                            {
                                                                user_id = eo.user_id,
                                                                nickname = eo.user_id,
                                                            }
                                                        };
                                                        seo.message = new List<Message>() { new At(eo.target_id) };
                                                        OnGroupMessageReceive?.Invoke(seo);
                                                    }
                                                  
                                                }
                                            }
                                            else if(eo.sub_type == "lucky_king")
                                            {
                                                Logger.Log($"[群红包运气王][{eo.group_id}]{eo.user_id}所发的红包被领光，{eo.target_id}成为运气王");
                                            }
                                        }
                                        break;
                                    }
                                default:break;
                            }
                        

                            break;

                        case "meta_event":
                            // 处理元事件
                            if (jo["meta_event_type"]?.ToString() == "lifecycle")
                            {
                                var eo = JsonConvert.DeserializeObject<lifecycle_event>(json);
                                Logger.Log($"[生命周期]{eo.sub_type}");
                            }
                            else if (jo["meta_event_type"]?.ToString() == "heartbeat")
                            {
                                var eo = JsonConvert.DeserializeObject<heartbeat_event>(json);
                                Logger.Log($"[心跳]({eo.status})间隔={eo.interval}ms");
                            }
                            break;

                        case "request":
                            // 处理请求事件
                            if (jo["request_type"]?.ToString() == "friend")
                            {
                                var eo = JsonConvert.DeserializeObject<friend_request_event>(json);
                                Logger.Log($"[好友请求]{eo.user_id}附带消息：{eo.comment} flag={eo.flag}");
                                Send(new set_friend_add_request { approve = true, flag = eo.flag });
                            }
                            else if (jo["request_type"]?.ToString() == "group")
                            {
                                var eo = JsonConvert.DeserializeObject<group_request_event>(json);
                                if (eo.sub_type == "add")
                                {
                                    // join in my group
                                    Logger.Log($"[加群请求][{eo.group_id}]申请人{eo.user_id}，附带消息：{eo.comment} flag={eo.flag}");
                                    //Send(new set_group_add_request { type = "add", approve = true, flag = eo.flag });
                                }
                                else if(eo.sub_type == "invite")
                                {
                                    // invite me to the group
                                    Logger.Log($"[邀请入群][{eo.group_id}]邀请者{eo.user_id}，附带消息：{eo.comment} flag={eo.flag}");
                                    Send(new set_group_add_request { type = "invite", approve = true, flag = eo.flag });
                                }
                            
                            }
                            break;

                        default:
                            Logger.Log(json);
                            Logger.Log("-- Unsupported post_type --");
                            break;
                    }

                }
            }catch(Exception ex)
            {
                Logger.Log(ex.ToString());
            }

        }



    }

}
