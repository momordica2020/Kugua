
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Kugua.Integrations.NTBot;


namespace Kugua
{
    public class LocalClient : NTBot
    {
        public string type;
        public string linkUri;
        
        public static WebSocket4Net.WebSocket localSocket = null;
        public LocalClient(string _type):base("wss://localhost")
        {
            
        }
        public void Link(string wsurl)
        {
            try
            {
                linkUri = wsurl;
                localSocket = new WebSocket4Net.WebSocket(wsurl);
                localSocket.Opened += delegate (object? s, EventArgs e)
                {
                    Logger.Log("[LocalSocket] - Socket Opened -");
                };

                localSocket.Error += delegate (object? sender, SuperSocket.ClientEngine.ErrorEventArgs e)
                {
                    //Logger.Log("[LocalSocket] - Socket Error , Running to Close or Reconnect -");
                    //Logger.Log($"{e.Exception}");
                };
                localSocket.MessageReceived += WebSocket4Net_MessageReceived;
                localSocket.Open();
                //Logger.Log(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":ws开始连接");


                localSocket.Closed += delegate (object? s, EventArgs e)
                {
                    Logger.Log("[LocalSocket] - Socket Closed -");


                    //Task.Delay(10 * 1000).Wait();

                    //Link(linkUri);
                    //TryReconnect(5);
                };
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
          
        }



        private static readonly string BiliBiliGroupId = "2";
        private void WebSocket4Net_MessageReceived(object? sender, WebSocket4Net.MessageReceivedEventArgs e)
        {
            try
            {
                //Logger.Log($"[WS]{e.Message}", LogType.Net);
                //LBotRequest msg = JsonConvert.DeserializeObject<LBotRequest>(e.Message);
                LBotRequest msg = new LBotRequest();
                JObject jo = JObject.Parse(e.Message);
                msg.userId = jo["userId"].ToString();
                msg.userName = jo["userName"].ToString();
                msg.type = jo["type"].ToString();
                // json:{userId=123123, userName=haha, type=gift, message=latiao*1}
                //
                //LocalBotInMsg msg = jo.ToObject<LocalBotInMsg>();
                //if (msg.userId < 0)
                //{
                //    return;
                //}
                //long userId = jo["userId"].ToObject<long>();
                //string userName = jo["userName"].ToString().Trim();
                //string type = jo["type"].ToString().Trim();

                var messages = RectifyMessage(jo["messages"].ToString());
                List<MessageInfo> msgWithAt = new List<MessageInfo>();
                msgWithAt.Add(new MessageInfo(new At(Config.Instance.BotQQ)));
                foreach(var m in messages)
                {
                    msgWithAt.Add(new MessageInfo(m));
                }
                msg.messages = msgWithAt;

                Logger.Log($"[WS][{msg.type}] [id:{msg.userId},昵称:{msg.userName}] \n内容:{msg.messages.ToTextString()}", LogType.Net);
                if (!Config.Instance.AllowPlayer(msg.userId)) return; // 黑名单

                var uinfo = Config.Instance.UserInfo(msg.userId);
                uinfo.Name = msg.userName;
                var msgs = new List<Message>();
                foreach(var mi in msg.messages)
                {
                    msgs.Add(mi.data);
                }
                var context = new MessageContext
                {
                    userId = msg.userId,
                    groupId = BiliBiliGroupId,
                    client = this,
                    recvMessages = msgs,
                };
                HistoryManager.Instance.Add(msg.userId, context.groupId, context.userId, msg.messages.ToTextString());

                BotHost.Instance.HandleGroupMessageReceiveMultiIO(context);
                //JObject jo = JObject.Parse(e.Message);
                
            }
            catch(Exception ex)
            {
                Logger.Log(ex);
            }
        }
        private static List<Message> RectifyMessage(string messagestr)
        {
            try
            {
                List<Message> msgs = new List<Message>();
                
                foreach (JToken item in JArray.Parse(messagestr))
                {
                    string typename = item["type"].ToString();
                    string data = item["data"].ToString();
                    switch (typename)
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
                        default: Logger.Log($"[0012] ParserPhase : Message Typo Error in {typename}"); break;
                    }

                    //Logger.Log($"{"[0012] ParserPhase : Message Typo Error in "}{{{item["type"]}}}");
                }

                return msgs;
            }
            catch
            {
                Logger.Log("[0013] ParserPhase : Message Error in {" + messagestr + "}");
                return null;
            }
        }

        public void HandleMessage(string userId, string userName, List<MessageInfo> messages)
        {
            if (messages == null) return;

            var lomsg = new LBotResponse
            {
                userId = userId,
                userName = userName,
                messages = messages,
            };

            JObject jo  = JObject.FromObject(lomsg);
            localSocket.Send(jo.ToString());
        }
  
        //public delegate void HandleMessage(Message[] messages);
        //public HandleMessage handleMessage;
        public class LBotResponse {
            public string userId { get; set; }
            public string userName { get; set; }
            public List<MessageInfo> messages { get; set; }
        }
        public class LBotRequest
        {
            public string userId { get; set; }
            public string userName { get; set; }
            public string type { get; set; }
            public List<MessageInfo> messages { get; set; }
        }
    }

    
   







}
