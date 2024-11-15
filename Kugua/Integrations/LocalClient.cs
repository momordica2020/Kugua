using MeowMiraiLib.Msg.Type;
using MeowMiraiLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Kugua
{
    public class LocalClient : Client
    {
        public string type;
        public string linkUri;
        
        public static WebSocket4Net.WebSocket localSocket = null;
        public LocalClient(string _type):base("wss://localhost", -1)
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
                    Logger.Instance.Log("[LocalSocket] - Socket Opened -");
                };

                localSocket.Error += delegate (object? sender, SuperSocket.ClientEngine.ErrorEventArgs e)
                {
                    //Logger.Instance.Log("[LocalSocket] - Socket Error , Running to Close or Reconnect -");
                    //Logger.Instance.Log($"{e.Exception}");
                };
                localSocket.MessageReceived += WebSocket4Net_MessageReceived;
                localSocket.Open();
                //Logger.Instance.Log(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":ws开始连接");


                localSocket.Closed += delegate (object? s, EventArgs e)
                {
                    Logger.Instance.Log("[LocalSocket] - Socket Closed -");


                    //Task.Delay(10 * 1000).Wait();

                    //Link(linkUri);
                    //TryReconnect(5);
                };
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
            }
          
        }



        private static readonly long BiliBiliGroupId = 2;
        private void WebSocket4Net_MessageReceived(object? sender, WebSocket4Net.MessageReceivedEventArgs e)
        {
            try
            {
                //Logger.Instance.Log($"[WS]{e.Message}", LogType.Net);
                LocalBotInMsg msg = JsonConvert.DeserializeObject<LocalBotInMsg>(e.Message);
                JObject jo = JObject.Parse(e.Message);
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
                List<Message> msgWithAt = new List<Message>();
                msgWithAt.Add(new At(Config.Instance.BotQQ, Config.Instance.BotName));
                msgWithAt.AddRange(messages);
                msg.messages = msgWithAt.ToArray();

                Logger.Instance.Log($"[WS][{msg.type}] [id:{msg.userId},昵称:{msg.userName}] \n内容:{msg.messages.MGetPlainString()}", LogType.Net);
                if (!Config.Instance.AllowPlayer(msg.userId)) return; // 黑名单

                var uinfo = Config.Instance.UserInfo(msg.userId);
                uinfo.Name = msg.userName;

                var context = new MessageContext
                {
                    userId = msg.userId,
                    groupId = BiliBiliGroupId,
                    client = this,
                    recvMessages = msg.messages,
                };
                HistoryManager.Instance.saveMsg(msg.userId, context.groupId, context.userId, context.recvMessages.MGetPlainString());

                BotHost.Instance.HandleGroupMessageReceiveMultiIO(context);
                //JObject jo = JObject.Parse(e.Message);
                
            }
            catch(Exception ex)
            {
                Logger.Instance.Log(ex);
            }
        }
        private static Message[] RectifyMessage(string messagestr)
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

        public void HandleMessage(long userId, string userName, Message[] messages)
        {
            if (messages == null) return;

            var lomsg = new LocalBotOutMsg
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
        public class LocalBotOutMsg {
            public long userId { get; set; }
            public string userName { get; set; }
            public Message[] messages { get; set; }
        }
        public class LocalBotInMsg
        {
            public long userId { get; set; }
            public string userName { get; set; }
            public string type { get; set; }
            public Message[] messages { get; set; }
        }
    }

    
   







}
