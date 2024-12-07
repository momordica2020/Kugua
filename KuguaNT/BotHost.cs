using System.Net.WebSockets;
using System.Text;
using System;
using Kugua.Integrations.NTBot;



namespace Kugua
{
    public class BotHost
    {
        private static readonly Lazy<BotHost> instance = new Lazy<BotHost>(() => new BotHost());

        // 私有构造函数
        private BotHost()
        {

        }
        public static BotHost Instance => instance.Value;

        public static NTBot ClientX;
        public static LocalClient ClientLocal = new LocalClient("local");

        public List<Mod> Mods = new List<Mod>();


        public SendLogDelegate _sendLog;
        //public MMDKBot(MMDK.Util.SendLogDelegate sendLogEvent=null)
        //{
        //    _sendLog = sendLogEvent;
        //}



        void sendLog(LogInfo logInfo)
        {
            if (_sendLog != null) { _sendLog(logInfo); }

            // print log?
            Console.WriteLine(logInfo.ToDescription());
        }
        /// <summary>
        /// 检查并初始化配置
        /// </summary>
        /// <returns></returns>
        bool checkAndSetConfigValid()
        {
            try
            {

                if (string.IsNullOrWhiteSpace(Config.Instance.App.Version)) Config.Instance.App.Version = "v0.0.1";
                if (Config.Instance.App.Avatar == null)
                {
                    return false;
                }
                // qq info
                if (string.IsNullOrWhiteSpace(Config.Instance.App.Avatar.myQQ.ToString())) Config.Instance.App.Avatar.myQQ = "";


                Config.Instance.StartTime = DateTime.Now;


            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }

            return true;
        }
        public void Start()
        {
            try
            {


                Logger.Instance.OnBroadcastLogEvent += sendLog;
                Logger.Log("开始初始化配置文件。");

                Config.Instance.Load();
                bool isValid = checkAndSetConfigValid();
                if (!isValid)
                {
                    Logger.Log("配置文件读取失败，中止运行");
                    return;
                }


                Logger.Log($"启用过滤器...");
                Filter.Instance.Init();



                Logger.Log($"开始启动bot...");
                Mods = new List<Mod>
                {
                    new ModAdmin(),
                    ModBank.Instance,
                    ModRaceHorse.Instance,
                    ModSlotMachine.Instance,
                    ModRoulette.Instance,
                    ModDiceGame.Instance,
                    new ModDice(),
                    new ModProof(),
                    new ModTextFunction(),
                    new ModZhanbu(),
                    new ModTranslate(),
                    new ModTimerTask(),
                    new ModNLP(),
                    new ModRandomChat(),    // 这个会用闲聊收尾

                };



                //bot = new MainProcess();
                //bot.Init(config);



                if (true)
                {
                    // 打开历史记录，不会是真的吧
                    string HistoryPath = Config.Instance.ResourceFullPath("HistoryPath");
                    if (!Directory.Exists(HistoryPath)) Directory.CreateDirectory(HistoryPath);
                    Logger.Log($"历史记录保存在 {HistoryPath} 里");
                    HistoryManager.Instance.Init(HistoryPath);
                }
                else
                {
                    Logger.Log($"历史记录不会有记录");
                }

                foreach (var mod in Mods)
                {
                    try
                    {
                        mod.clientQQ = ClientX;
                        mod.clientLocal = ClientLocal;
                        if (mod.Init(null))
                        {
                            Logger.Log($"模块{mod.GetType().Name}已初始化");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                    }
                }

                //State = BotRunningState.ok;
                


                //mirai = new MiraiLink();
                if (!string.IsNullOrWhiteSpace(Config.Instance.App.Net.QQWS))
                {

                    //string verifyKey = "123456";
                    string connectUri = $"{Config.Instance.App.Net.QQWS}";
                    Logger.Log($"正在连接QQ...{connectUri}");
                    ClientX = new(connectUri);
                    //ClientX._OnServeiceConnected += ServiceConnected;
                    //ClientX._OnServeiceError += OnServeiceError;
                    //ClientX._OnServiceDropped += OnServiceDropped;


                    ClientX.ConnectAsync();
                    
                    

                    ClientX.OnPrivateMessageReceive += OnPrivateMessageReceive;
                    ClientX.OnGroupMessageReceive += OnGroupMessageReceive;
                    //ClientX.OnTempMessageReceive += OnTempMessageReceive;
                    //ClientX.OnEventBotInvitedJoinGroupRequestEvent += OnEventBotInvitedJoinGroupRequestEvent;
                    //ClientX.OnEventNewFriendRequestEvent += OnEventNewFriendRequestEvent;
                    //ClientX.OnEventFriendNickChangedEvent += OnEventFriendNickChangedEvent;


                   
                }
                else
                {
                    Logger.Log($"QQ未启动");
                }
                Logger.Log($"启用GPT相关接口...");
                GPT.Instance.Init();
                if (!string.IsNullOrWhiteSpace(Config.Instance.App.Net.TTSUri))
                {
                    Logger.Log($"TTS连接至：{Config.Instance.App.Net.TTSUri}");
                    
                }
                else
                {
                    Logger.Log($"TTS未启动");
                }
                if (!string.IsNullOrWhiteSpace(Config.Instance.App.Net.OllamaUri))
                {
                    Logger.Log($"Ollama连接至：{Config.Instance.App.Net.OllamaUri}");
                    
                }
                else
                {
                    Logger.Log($"Ollama未启动");
                }


                LinkLocal();




                Logger.Log($"======= bot启动完成 =======");
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public void LinkLocal()
        {
            if (string.IsNullOrWhiteSpace(Config.Instance.App.Net.LocalWS))
            {
                Logger.Log($"本地WS未启动");
            }
            else
            {
                Logger.Log($"本地WS连接至：{Config.Instance.App.Net.LocalWS}");
                ClientLocal.Link(Config.Instance.App.Net.LocalWS);
            }
            
        }

        /// <summary>
        /// 模块介绍信息
        /// </summary>
        /// <returns></returns>
        public string ModsDesc()
        {
            StringBuilder res = new StringBuilder();
            res.AppendLine($"模块指令介绍 : ({Mods.Count}个模块)");
            foreach (var mod in Mods)
            {
                res.AppendLine($"{mod.GetType().Name}模块：");
                res.AppendLine($"{mod.GetCommandDescriptions()}");
            }
            return res.ToString();
        }
        public string SelfCheckInfo()
        {
            StringBuilder res = new StringBuilder();
            string filePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            FileInfo fileInfo = new FileInfo(filePath);

            res.AppendLine($"现在是{DateTime.Now.ToString()}");
            res.AppendLine($"运行路径：{filePath}");
            res.AppendLine($"代码最后修改日期：{fileInfo.LastWriteTime.ToString()}");
            
            var codes = StaticUtil.GetCodeLineNum();
            res.AppendLine($"源码有{codes.Count}个文件({codes.Sum(s=>s.Item2)}行)");
            //foreach (var code in codes)
            //{
            //    if(code.Item2>100)  res.AppendLine($"({Path.GetFileName(code.Item1)}) - {code.Item2} lines");
            //}


            return res.ToString();
        }



        public void Stop()
        {
            try
            {
                foreach (var mod in Mods)
                {
                    try
                    {
                        mod.Exit();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                    }

                }
                HistoryManager.Instance.Dispose();
                Config.Instance.Save();

                Logger.Instance.Close();
                //Environment.Exit(0);

                //State = BotRunningState.exit;
            }
            catch
            {

            }
        }
        
        public void HandleLocalAPI(string message)
        {
            Logger.Log($"*WEB*>>{message}");


            List<Message> msgs = new List<Message>()
            {
                new At(Config.Instance.BotQQ),
                new Text(message),
            }; 
            
            group_message_event gms = new()
            {
                user_id = "",
                message = msgs,
            };

            OnGroupMessageReceive(gms);

            System.Diagnostics.Debug.WriteLine(message);
            //Console.WriteLine(message);
        }


        /// <summary>
        /// 刷新好友列表并更新配置文件
        /// </summary>
        



        async void OnPrivateMessageReceive(private_message_event e)
        {
            Logger.Log($"好友信息 [qq:{e.message_id},昵称:{e.sender.nickname}] \n内容:{e.raw_message}", LogType.Debug);
            var uinfo = Config.Instance.UserInfo(e.user_id);
            uinfo.Name = e.sender.nickname;
            var context = new MessageContext
            {
                userId = e.user_id,
                groupId = "",
                client = ClientX,
                recvMessages = e.message,
                isAskme = true
            };
            HistoryManager.Instance.saveMsg(e.message_id, context.groupId, context.userId, e.raw_message);
            HandlePrivateMessageReceiveMultiIO(context);
        }


        async void HandlePrivateMessageReceiveMultiIO(MessageContext context)
        {
            if (!Config.Instance.AllowPlayer(context.userId)) return; // 黑名单
            if (context.recvMessages == null) return;
            var uinfo = Config.Instance.UserInfo(context.userId);

            //var sourceItem = context.recvMessages.First() as Source;
            //HistoryManager.Instance.saveMsg("", "", context.userId, context.recvMessages.MGetPlainString());
            
            bool talked = false;
            foreach(var mod in Mods)
            {
                var succeed = await mod.HandleMessages(context);
                if (succeed)
                {
                    talked = true;
                    break;
                }
            }

            // 计数统计
            if (talked)
            {
                //Config.Instance.UserInfo(context.userId).UseTimes += 1;
            }
        }
        //async void OnTempMessageReceive(TempMessageSender s, MeowMiraiLib.Msg.Type.Message[] e)
        //{
        //    if (e == null) return;
        //    var sourceItem = e.First() as Source;
        //    Logger.Log($"[{sourceItem.id}]群({s.group.id})信息 [qq:{s.id},昵称:{s.memberName}] \n内容:{e.MGetPlainString()}", LogType.Debug);
        //    if (!Config.Instance.AllowPlayer(s.id) || !Config.Instance.AllowGroup(s.group.id)) return; // 黑名单

        //    var uinfo = Config.Instance.UserInfo(s.id);
        //    uinfo.Name = s.memberName;

        //    var context = new MessageContext
        //    {
        //        userId = s.id,
        //        groupId = s.group.id,
        //        client = ClientX,
        //        recvMessages = e,
        //        isTemp=true,
        //    };
        //    HistoryManager.Instance.saveMsg(sourceItem.id, context.groupId, context.userId, context.recvMessages.MGetPlainString());
        //    HandleGroupMessageReceiveMultiIO(context);
        //}
        async void OnGroupMessageReceive(group_message_event e)
        {
            if (e == null) return;
            Logger.Log($"[{e.message_id}]群({e.group_id})信息 [qq:{e.user_id},昵称:{e.sender.nickname}] \n内容:{e.raw_message}", LogType.Debug);
            if (!Config.Instance.AllowPlayer(e.user_id) || !Config.Instance.AllowGroup(e.group_id)) return; // 黑名单

            var uinfo = Config.Instance.UserInfo(e.user_id);
            uinfo.Name = e.sender.nickname;

            var context = new MessageContext
            {
                userId = e.user_id,
                groupId = e.group_id,
                client = ClientX,
                recvMessages = e.message,
            };
            HistoryManager.Instance.saveMsg(e.message_id, context.groupId, context.userId, e.raw_message);
            HandleGroupMessageReceiveMultiIO(context);
        }

        public async void HandleGroupMessageReceiveMultiIO(MessageContext context)
        {
            // ask me?
            context.isAskme = false;
            if (context.recvMessages != null)
            {
                foreach (var item in context.recvMessages)
                {
                    if (item is At itemat)
                    {
                        if (itemat.qq == Config.Instance.BotQQ)
                        {
                            context.isAskme = true;
                            break;
                        }
                    }
                    if (item is Text plain)
                    {
                        if (plain.text.TrimStart().StartsWith(Config.Instance.BotName))
                        {
                            plain.text = plain.text.TrimStart().Substring(Config.Instance.BotName.Length);
                            context.isAskme = true;
                            break;
                        }
                    }
                }
            }

            //var uinfo = Config.Instance.UserInfo(s.id);
            //Logger.Log($"<IN>{(context.isAskme?"!":" ")}{context.recvMessages.MGetPlainString()}");
            var sendNum = 0;
            foreach (var mod in Mods)
            {
                var succeed = await mod.HandleMessages(context);
                if (succeed)
                {
                    sendNum++;
                    break;
                }
            }

            if (sendNum > 0)
            {
                //p.UseTimes += 1;
                //Config.Instance.UserInfo(context.userId).UseTimes += 1;
            }
        }

        void ServiceConnected(string e)
        {
            Logger.Log($"***连接成功：{e}", LogType.Mirai);





            //Logger.Log($"更新好友列表和群列表...");
            //RefreshFriendList();
            //Logger.Log($"更新完毕，找到{Config.Instance.friends.Count}个好友，{Config.Instance.groups.Count}个群...");



        }

        void OnServeiceError(Exception e)
        {
            Logger.Log($"***连接出错：{e.Message}\r\n{e.StackTrace}", LogType.Mirai);
        }

        void OnServiceDropped(string e)
        {
            Logger.Log($"***连接中断：{e}", LogType.Mirai);
        }

        //void OnClientOnlineEvent(OtherClientOnlineEvent e)
        //{
        //    Logger.Log($"***其他平台登录（标识：{e.id}，平台：{e.platform}", LogType.Mirai);
        //}
        //void OnEventBotInvitedJoinGroupRequestEvent(BotInvitedJoinGroupRequestEvent e)
        //{
        //    Logger.Log($"受邀进群（用户：{e.fromId}，群：{e.groupName}({e.groupId})消息：{e.message}", LogType.Mirai);
        //    var g = Config.Instance.GroupInfo(e.groupId);
        //    var u = Config.Instance.UserInfo(e.fromId);
        //    if (g.Is("黑名单") || u.Is("黑名单"))
        //    {
        //        e.Deny(ClientX, "非好友不接受邀请谢谢");
        //        return;
        //    }
        //    if (Config.Instance.qqfriends.ContainsKey(e.fromId) || u.Is("管理员") || u.Is("好友") || e.fromId == Config.Instance.App.Avatar.adminQQ)
        //    {
        //        e.Grant(ClientX);
        //        return;
        //    }
        //}

        //void OnEventNewFriendRequestEvent(NewFriendRequestEvent e)
        //{
        //    Logger.Log($"好友申请：{e.nick}({e.fromId})(来自{e.groupId})消息：{e.message}", LogType.Mirai);
        //    if (!string.IsNullOrWhiteSpace(e.message) && e.message.StartsWith(Config.Instance.App.Avatar.askName))
        //    {
        //        e.Grant(ClientX, "来了来了");
        //        var user = Config.Instance.UserInfo(e.fromId);
        //        user.Name = e.nick;
        //        //user.Mark = e.nick;
        //        user.Tags.Add("好友");
        //        user.Type = PlayerType.Normal;
        //    }
        //    else
        //    {
        //        e.Deny(ClientX, "密码错误");
        //    }
        //}

        //void OnEventFriendNickChangedEvent(FriendNickChangedEvent e)
        //{
        //    Logger.Log($"好友改昵称（{e.friend.id}，{e.from}->{e.to}", LogType.Mirai);
        //    var user = Config.Instance.UserInfo(e.friend.id);
        //    user.Name = e.to;

        //}


    }

    
   







}
