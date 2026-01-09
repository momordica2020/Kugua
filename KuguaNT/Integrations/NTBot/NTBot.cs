using Kugua.Core;
using Microsoft.AspNetCore.SignalR.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Nodes;
using WebSocket4Net;
using ZhipuApi;
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
        JsonSerializerSettings parseSettings = new JsonSerializerSettings
        {
            MaxDepth = 128 // 设置更大的深度限制
        };

        //
        // 摘要:
        //     重连标识
        public bool reconnecting { get; private set; }
        public Action<private_message_event> OnPrivateMessageReceive { get; internal set; }
        public Action<group_message_event> OnGroupMessageReceive { get; internal set; }

        public Action<ForwardContent> OnGroupForwardMessageReceive { get; internal set; }

        public object SessionLock = new object();
        public Queue<JObject> SSMRequestList { get; set; }

        public string url;


        private CancellationTokenSource _reconnectCts = new CancellationTokenSource();



        public NTBot(string url)
        {
            NTBot client = this;
            this.url = url;
            this.reconnecting = false;
            ws = new WebSocket(url);
            ws.Opened += delegate (object? s, EventArgs e)
            {
                Logger.Log("[NT Socket] - Socket 已连接 -");
                //client._OnServeiceConnected?.Invoke(e.ToString());
            };
            ws.Error += delegate (object? s, SuperSocket.ClientEngine.ErrorEventArgs e)
            {
                //client._OnServeiceError?.Invoke(e.Exception);  
                Logger.Log($"{e.Exception}", LogType.Debug);
                //Logger.Log("[NT Socket] - Socket报错，自动重连之 -");
                if (!reconnecting)
                {
                    client.ws.Close();
                }
                
            };
            ws.Closed += async delegate (object? s, EventArgs e)
            {
                reconnecting = true;
                //Logger.Log("[NT Socket] - 连接已断开...");
                await InfiniteReconnectAsync(_reconnectCts.Token);


                ////client._OnServiceDropped?.Invoke(e.ToString());
                //Logger.Log("[NT Socket] - Socket Closed -");
                //while (reconnect == -1 || reconnect-- > 0)
                //{
                //    if (client.ws.State == WebSocketState.Open)
                //    {
                //        Logger.Log("[NT Socket] - Reconnect Complete-");
                //        //reconnect = 0;
                //        break;
                //    }

                //    WebSocketState state = client.ws.State;
                //    if (state != 0 && state != WebSocketState.Closing)
                //    {
                //        Logger.Log("[NT Socket] - Tryin Reconnecting");
                //        client.ws.Open();
                //    }

                //    Logger.Log("[NT Socket] - Trying To Reconnect (in 5 second)-");
                //    Task.Delay(5000).GetAwaiter().GetResult();
                //}
            };
            ws.MessageReceived += (s, e) =>
            {
                Task.Run(() =>
                {
                    OnMessageReceived(s,e);
                });
            };
        }


        // 无限重连核心逻辑
        private async Task InfiniteReconnectAsync(CancellationToken ct)
        {
            const int baseDelay = 5000;    // 重试间隔

            while (!ct.IsCancellationRequested) 
            {
                try
                {
                    // 检查是否已经手动重连成功
                    if (ws.State == WebSocketState.Open) break;

                    // 仅在关闭/异常状态下尝试重连
                    if (ws.State == WebSocketState.Closed)
                    {
                        Logger.Log($"[NT Socket] - 尝试重新连接 (间隔: {baseDelay}ms)", LogType.Debug);
                        await ws.OpenAsync(); 

                        //Logger.Log("[NT Socket] - 重连成功");
                        reconnecting = false;
                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    Logger.Log("[NT Socket] - 重连已取消", LogType.Debug);
                    reconnecting = false;
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Log($"[NT Socket] - 重连失败: {ex.Message}", LogType.Debug);
                }

                // 指数退避策略：失败后等待时间逐渐增加，但不超过最大值
                await Task.Delay(baseDelay, ct);
            }
        }

        // 程序退出时调用此方法停止重连
        public void StopReconnect()
        {
            _reconnectCts.Cancel();
            Logger.Log("[NT Socket] - 已停止自动重连");
        }


        /// <summary>
        /// 发送同意好友申请
        /// </summary>
        public async void SendAddFriendAccept(string flag)
        {
            string uri = Config.Instance.App.Net.QQHTTP + "/set_friend_add_request";
            //Logger.Log(uri);
            string res = await Network.PostJsonAsync(uri, JsonConvert.SerializeObject(new set_friend_add_request { flag = flag, approve=true}));
            Logger.Log(res);
        }

        /// <summary>
        /// 获取转发内容详情
        /// </summary>
        public void GetForwardMessage(ForwardNodeExist forward)
        {
            
            try
            {
                string uri = Config.Instance.App.Net.QQHTTP + "/get_forward_msg";
                Logger.Log($"get_forward_msg ID = {forward.id}");
                Logger.Log($"{JsonConvert.SerializeObject(new get_forward_msg(forward.id))}");
                string json = Network.PostJsonAsync(uri, JsonConvert.SerializeObject(new get_forward_msg(forward.id))).Result;
                //Logger.Log("GET!");
                Logger.Log(json);
                JObject jo = JObject.Parse(json);
                forward.content = new List<forward_message_node>();

                //var msgall = JsonConvert.DeserializeObject(json)["data"]["messages"];
                if (jo["status"].ToString() != "ok")
                {
                    // error
                    Logger.Log($"return not ok:{jo["status"]}");
                    return;
                }

                //List<forward_message_node> getnodes = new List<forward_message_node>();
                foreach (var msgnode in jo["data"]["messages"].ToArray())
                {
                    var node = JsonConvert.DeserializeObject<forward_message_node>(msgnode.ToString());
                    node.message = new List<Message>();
                    parseMessages(node.sender, node.message, msgnode["message"].ToArray());
                    forward.content.Add(node);
                    //OnPrivateMessageReceive?.Invoke(eo);
                }




                //SendForwardToGroupSimply("953445012", id);
                //SendForwardMessage("953445012", jo["data"].ToString());
                return;
            }
            catch(Exception ex)
            {
                Logger.Log(ex);
            }
            return;
        }


        /// <summary>
        /// 转发单条消息，必须要有单条消息的message id才行
        /// </summary>
        /// <param name="group"></param>
        /// <param name="mid"></param>
        public bool SendForwardToGroupSimply(string group, string mid)
        {
            string uri = Config.Instance.App.Net.QQHTTP + "/forward_group_single_msg";
            var sender = new ForwardSenderSingle { group_id = group, message_id = mid };
            string json = Network.PostJsonAsync(uri, JsonConvert.SerializeObject(sender), false).Result;

            //Logger.Log(json);
            var jo = JObject.Parse(json);
            if (jo["status"].ToString() != "ok")
            {
                //failed
                Logger.Log("发送失败捏");
                Logger.Log(json);
                return false;
            }
            return true;
        }


        /// <summary>
        /// 向群发送构造好的转发内容
        /// </summary>
        /// <param name="group"></param>
        /// <param name="nodes"></param>
        /// <param name="prompt"></param>
        public async void SendForwardMessageToGroup(string group,  List<Message> nodes, string prompt="")
        {
            try
            {
                string uri = Config.Instance.App.Net.QQHTTP + "/send_group_forward_msg";

                var senderMessages = new List<MessageInfo>();
                foreach(var node in nodes)
                {
                    var n = new MessageInfo(node);
                    senderMessages.Add(n);
                }
                if (senderMessages.Count <= 0)
                {
                    // pass
                    return;
                }
                var sender = new send_forward_msg { 
                    group_id = group, 
                    prompt = prompt, 
                    messages = senderMessages,
                    //summary = "test1",
                    //source= "https://multimedia.nt.qq.com.cn/download?appid=1407&fileid=EhRz0xy_F9a5b5satPHlrCzvbKfL_xjINyD_CiiLpoywoeWLAzIEcHJvZFCAvaMBWhDimL5BPcELXG9QGWSHmUMy&rkey=CAESKBkcro_MGujoMv-TbDIUJ9Yn8vCSgH8FD1D9McsN6pkMLVQkHgi5-s0",
                };
                //Logger.Log($"转发消息到{group}，长度{senderMessages.Count}");
                //Logger.Log($"{JsonConvert.SerializeObject(sender)}");

                string json = await Network.PostJsonAsync(uri, JsonConvert.SerializeObject(sender),false);
                //Logger.Log(json);
                var reply = JsonConvert.DeserializeObject<ForwardContent>(json);
            }
            catch(Exception ex)
            {
                Logger.Log(ex);
            }

        }



        /// <summary>
        /// 获取特定表情有几个点赞
        /// </summary>
        /// <param name="msg_id"></param>
        /// <param name="emoji_id"></param>
        /// <param name="emoji_type"></param>
        /// <returns></returns>
        public int getEmojiLikeNumber(string msg_id, string emoji_id, string emoji_type)
        {
            try
            {
                string uri = Config.Instance.App.Net.QQHTTP + "/fetch_emoji_like";
                string json = Network.PostJsonAsync(uri, JsonConvert.SerializeObject(new fetch_emoji_like
                {
                    message_id = msg_id,
                    emojiId = emoji_id,
                    emojiType = emoji_type
                })).Result;
                //Logger.Log(json);
                var r = JObject.Parse(json);

                return r["data"]["emojiLikesList"].Count();
            }
            catch (Exception ex)
            {
                //Logger.Log(ex);
                return -1;
            }
            return 0;
            // {"status":"ok","retcode":0,"data":{"result":0,"errMsg":"","emojiLikesList":[{"tinyId":"287859992","nickName":"","headUrl":""}],"cookie":"","isLastPage":true,"isFirstPage":true},"message":"","wording":"","echo":null}
            //JObject jo = JObject.Parse(json);
            //var reply = JsonConvert.DeserializeObject<ForwardContent>(json);

        }

        /// <summary>
        /// 发送对特定消息的表情回应。
        /// </summary>
        /// <param name="msg_id"></param>
        /// <param name="emoji_id"></param>
        /// <param name="set"></param>
        public void SendEmojiLike(string msg_id, int emoji_id, bool set=true)
        {
            try
            {
                string uri = Config.Instance.App.Net.QQHTTP + "/set_msg_emoji_like";
                string json = Network.PostJsonAsync(
                    uri, JsonConvert.SerializeObject(
                    new send_emoji_like
                    {
                        message_id = msg_id,
                        emoji_id = emoji_id,
                        set = set
                    })).Result;
                //Logger.Log(json);
                //JObject jo = JObject.Parse(json);
            }
            catch(Exception ex)
            {
                Logger.Log(ex);
            }

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


        /// <summary>
        /// 更新群列表信息，主要是群号和群名
        /// </summary>
        public void UpdateGroupInfo()
        {
            try
            {
                string uri = Config.Instance.App.Net.QQHTTP + "/get_group_list";
                string json = Network.PostJsonAsync(uri, JsonConvert.SerializeObject(new send_get_group_list()),false).Result;
                //Logger.Log(json);
                JObject jo = JObject.Parse(json);
                if (jo["status"].ToString() == "ok")
                {
                    foreach(var item in jo["data"].ToArray())
                    {
                        string gid = item["group_id"].ToString();
                        string gname = item["group_name"].ToString();
                        Config.Instance.GroupInfo(gid).Name = gname;
                        Logger.Log($"更新群状态[{gid}]{gname}({item["member_count"]}/{item["max_member_count"]})");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

        }



        /// <summary>
        /// 读取群成员列表
        /// </summary>
        /// <param name="groupid"></param>
        /// <returns></returns>
        public List<get_group_member_info_reply> GetGroupMemberList(string groupid)
        {
            List<get_group_member_info_reply> res = new List<get_group_member_info_reply>();
            try
            {

                string uri = Config.Instance.App.Net.QQHTTP + "/get_group_member_list";
                string json = Network.PostJsonAsync(uri, JsonConvert.SerializeObject(new get_group_member_list { group_id=groupid, no_cache=false }), false).Result;
                //Logger.Log(json);
                JObject jo = JObject.Parse(json);
                if (jo["status"].ToString() == "ok")
                {
                    
                    foreach (var item in jo["data"].ToArray())
                    {
                        var memberinfo = JsonConvert.DeserializeObject<get_group_member_info_reply>(item.ToString());
                        res.Add(memberinfo);
                        Logger.Log($"{memberinfo.user_id}[{memberinfo.nickname}]");
                        //string gid = item["group_id"].ToString();
                        //string gname = item["group_name"].ToString();
                        //Config.Instance.GroupInfo(gid).Name = gname;
                        //Logger.Log($"更新群成员列表[{gid}]{gname}({item["member_count"]}/{item["max_member_count"]})");
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return res;
        }



        /// <summary>
        /// OCR
        /// </summary>
        /// <param name="groupid"></param>
        /// <returns></returns>
        public string GetOCR(string imgpath)
        {
            StringBuilder sb = new StringBuilder();
            try
            {

                string uri = Config.Instance.App.Net.QQHTTP + "/ocr_image";
                string json = Network.PostJsonAsync(uri, JsonConvert.SerializeObject(new get_ocr { image = imgpath }), false).Result;
                //Logger.Log(json);
                JObject jo = JObject.Parse(json);
                if (jo["status"].ToString() == "ok")
                {

                    foreach (var item in jo["data"].ToArray())
                    {
                        sb.AppendLine(item["text"].ToString());
                        Logger.Log($"{item["text"]}");
                        //string gid = item["group_id"].ToString();
                        //string gname = item["group_name"].ToString();
                        //Config.Instance.GroupInfo(gid).Name = gname;
                        //Logger.Log($"更新群成员列表[{gid}]{gname}({item["member_count"]}/{item["max_member_count"]})");
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return sb.ToString();
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
                   //Logger.Log(jsonStr, LogType.Mirai);
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

        //private void SaveMarketImage

        private void parseMessages(message_sender sender, List<Message> msgs, JToken[] jo)
        {
            try
            {
                if (msgs == null) return;
                foreach (var mj in jo)
                {
                    if (mj["type"] != null)
                    {
                        string typename = mj["type"].ToString();
                        string data = mj["data"].ToString();
                        //Logger.Log($"-{typename}!");
                        //
                        switch (typename)
                        {
                            case "text": msgs.Add(JsonConvert.DeserializeObject<Text>(data)); break;
                            case "image":
                                //var imageBasic = JsonConvert.DeserializeObject<ImageRecvBasic>(data);
                                if (mj["data"]["emoji_id"]!=null)// .summary== "marketface")
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
                            case "video":
                                Logger.Log($"{data}");
                                msgs.Add(JsonConvert.DeserializeObject<Video>(data)); 
                                break;

                            case "rps": msgs.Add(JsonConvert.DeserializeObject<Rps>(data)); break;
                            case "dice": msgs.Add(JsonConvert.DeserializeObject<Dice>(data)); break;
                            //case "shake": msgs.Add(JsonConvert.DeserializeObject<Shake>(data)); break;
                            case "poke": msgs.Add(JsonConvert.DeserializeObject<Poke>(data)); break;
                            case "anonymous": msgs.Add(JsonConvert.DeserializeObject<AnonymousMesssage>(data)); break;
                            case "share": msgs.Add(JsonConvert.DeserializeObject<Share>(data)); break;
                            case "contact": msgs.Add(JsonConvert.DeserializeObject<Contact>(data)); break;
                            case "location": msgs.Add(JsonConvert.DeserializeObject<Location>(data)); break;
                            case "music": msgs.Add(JsonConvert.DeserializeObject<Music>(data)); break;
                            case "reply": msgs.Add(JsonConvert.DeserializeObject<Reply>(data)); break;
                            case "record": msgs.Add(JsonConvert.DeserializeObject<Record>(data));

                                Logger.Log(data);
                                
                                
                                break;
                            case "xml": msgs.Add(JsonConvert.DeserializeObject<XmlData>(data)); break;
                            case "json": msgs.Add(JsonConvert.DeserializeObject<JsonData>(data)); break;
                            case "forward":
                                
                                var d = JsonConvert.DeserializeObject<ForwardNodeExist>(data);
                                d.content = new List<forward_message_node>();
                                Logger.Log($"看到了{sender.user_id}的转发喵。id={d.id}");
                                if (sender.user_id == Config.Instance.BotQQ)
                                {
                                    //Logger.Log($"是我自己喵。");
                                }


                                if(sender.user_id == "287859992")  Logger.Log($"\r\n{mj["data"]}");
                                JObject jjo = JObject.Parse(data);
                                foreach (var nodej in mj["data"]["content"].ToArray())
                                {
                                    var node = JsonConvert.DeserializeObject<forward_message_node>(nodej.ToString());
                                    node.message = new List<Message>();
                                    parseMessages(node.sender, node.message, nodej["message"].ToArray());
                                    d.content.Add(node);
                                }

                                msgs.Add(d);
                                //var res = GetForwardMessage(d.id);
                                //if (res != null)
                                //{
                                //    OnGroupForwardMessageReceive?.Invoke(eo);
                                //}
                                
                                
                                break;
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
                    //Logger.Log(json);

                    //var reply = JsonConvert.DeserializeObject<SenderReplyAPI>(json);
                    //Logger.Log(jo.ToString());
                    if (jo.ContainsKey("echo"))
                    {
                        var echo = jo["echo"].ToString();
                        if (sessions.ContainsKey(echo))
                        {
                            var sd = sessions[echo];
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
                                                session_messageid[echo] = oo.message_id.ToString();
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
                                            parseMessages(oo.sender, oo.message, jo["data"]["message"].ToArray());
                                        }
                                            lock (SessionLock)
                                            {
                                                session_messageid[echo] = oo.message.ToTextString();
                                            }
                                            
                                        //Logger.Log("READ DATA  " + oo.message.ToTextString());
                                        break;
                                    }
                                default:
                                    break;
                            }
                            sessions.Remove(echo);

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
                                    parseMessages(eo.sender, eo.message, jo["message"].ToArray());
                                }
                                OnPrivateMessageReceive?.Invoke(eo);
                            }
                            else if (jo["message_type"].ToString() == "group")
                            {
                                var eo = JsonConvert.DeserializeObject<group_message_event>(json);
                                eo.message = new List<Message>();
                                if (jo?["message"] != null)
                                {
                                    parseMessages(eo.sender, eo.message, jo["message"].ToArray());
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
                                        Logger.Log($"[群减员][{eo.group_id}]{eo.user_id}{(eo.sub_type == "leave" ? "主动退群" : "被踢出群")}{(!string.IsNullOrWhiteSpace(eo.operator_id) ? $"  操作者{eo.operator_id}" : "")}");
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
                                                if (eo.target_id == Config.Instance.BotQQ)
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
                                            else if (eo.sub_type == "lucky_king")
                                            {
                                                Logger.Log($"[群红包运气王][{eo.group_id}]{eo.user_id}所发的红包被领光，{eo.target_id}成为运气王");
                                            }
                                        }
                                        break;
                                    }
                                case "group_msg_emoji_like":
                                    //Logger.Log(json);
                                    var data = JsonConvert.DeserializeObject<notify_group_msg_emoji_like>(json);
                                    var likes = jo["likes"].ToArray();
                                    if (likes.Length > 1)
                                    {
                                        Logger.Log("?响应数量超过1，是不是bug");
                                        Logger.Log(json);
                                    }
                                    else
                                    {
                                        // [{"emoji_id":"10068","count":1}]
                                        var emoji_id = likes.First()["emoji_id"].ToString();
                                        //Logger.Log(emoji_id);
                                        var emoji = EmojiReact.Instance.GetById(emoji_id);//  EmojiReact.Instance.emojiTypeInfos.TryGetValue(emoji_id, out var emoji);
                                        if (emoji != null)
                                        {
                                            Logger.Log($"[消息响应][群{data.group_id}]{data.user_id}给{data.message_id}点了个{emoji.name}");
                                            var seo = new group_message_event
                                            {
                                                message_id = data.message_id,
                                                group_id = data.group_id,
                                                user_id = data.user_id,
                                                self_id = data.self_id,
                                                sender = new message_sender
                                                {
                                                    user_id = data.user_id,
                                                    nickname = data.user_id,
                                                }
                                            };
                                            seo.message = new List<Message>() { new ReactLike(emoji) };
                                            OnGroupMessageReceive?.Invoke(seo);
                                            //getEmojiLikeNumber(data.message_id, emoji);
                                        }
                                        
                                    }
                                    
                                    break;
                                default:                                    
                                    break;
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
                                await Task.Delay(1000);
                               // SendAddFriendAccept(eo.flag);
                                Send(new set_friend_add_request { approve = true, flag = eo.flag, remark="" });
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
                                    Send(new set_group_add_request { type = "invite", approve = true, flag = eo.flag, reason="" });
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
