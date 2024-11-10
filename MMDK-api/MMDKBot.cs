using MeowMiraiLib.Msg.Type;
using MeowMiraiLib.Msg;
using MeowMiraiLib;
using MeowMiraiLib.Event;
using MMDK.Mods;
using MMDK.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MeowMiraiLib.Msg.Sender;
using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using MMDK.Mods;
using System.Reflection;

namespace MMDK
{
    public class MMDKBot
    {
        private static readonly Lazy<MMDKBot> instance = new Lazy<MMDKBot>(() => new MMDKBot());

        // 私有构造函数
        private MMDKBot()
        {

        }
        public static MMDKBot Instance => instance.Value;

        public static MeowMiraiLib.Client ClientX;
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
                if (string.IsNullOrWhiteSpace(Config.Instance.App.Avatar.myQQ.ToString())) Config.Instance.App.Avatar.myQQ = 00000;


                Config.Instance.App.Log.StartTime = DateTime.Now;

                Config.Instance.App.Log.beginTimes += 1;

            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
                return false;
            }

            return true;
        }
        public void Start()
        {
            try
            {
                ClientLocal.handleMessage = new LocalClient.HandleMessage(messages =>
                {
                    Logger.Instance.Log("*LOCAL OUT*<<" + messages.MGetPlainString());
                });


                Logger.Instance.OnBroadcastLogEvent += sendLog;
                Logger.Instance.Log("开始初始化配置文件。");

                Config.Instance.Load();
                bool isValid = checkAndSetConfigValid();
                if (!isValid)
                {
                    Logger.Instance.Log("配置文件读取失败，中止运行");
                    return;
                }


                Logger.Instance.Log($"启用过滤器...");
                IOFilter.Instance.Init();



                Logger.Instance.Log($"开始启动bot...");
                Mods = new List<Mod>
                {
                    new ModAdmin(),
                    ModBank.Instance,
                    ModRaceHorse.Instance,
                    ModSlotMachine.Instance,
                    new ModDice(),
                    new ModProof(),
                    new ModTextFunction(),
                    new ModZhanbu(),
                    //new ModTranslate(),
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
                    Logger.Instance.Log($"历史记录保存在 {HistoryPath} 里");
                    HistoryManager.Instance.Init(HistoryPath);
                }
                else
                {
                    Logger.Instance.Log($"历史记录不会有记录");
                }

                foreach (var mod in Mods)
                {
                    try
                    {
                        mod.clientMirai = ClientX;
                        mod.clientLocal = ClientLocal;
                        if (mod.Init(null))
                        {
                            Logger.Instance.Log($"模块{mod.GetType().Name}已初始化");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Log(ex);
                    }
                }

                //State = BotRunningState.ok;
                Logger.Instance.Log($"bot启动完成");


                //mirai = new MiraiLink();
                if (Config.Instance.App.IO.MiraiRun)
                {

                    //string verifyKey = "123456";
                    string connectUri = $"{Config.Instance.App.IO.MiraiWS}:{Config.Instance.App.IO.MiraiPort}/all?qq={Config.Instance.App.Avatar.myQQ}";
                    Logger.Instance.Log($"正在连接Mirai...{connectUri}");
                    ClientX = new(connectUri);
                    ClientX._OnServeiceConnected += ServiceConnected;
                    ClientX._OnServeiceError += OnServeiceError;
                    ClientX._OnServiceDropped += OnServiceDropped;


                    ClientX.Connect();

                    //foreach (var mod in Mods)
                    //{
                    //    if (mod is ModWithMirai modWithMirai)
                    //    {
                    //        modWithMirai.InitMiraiClient(ClientX);
                    //    }
                    //}

                    Logger.Instance.Log($"启用GPT接口...");
                    GPT.Instance.Init(ClientX);

                    ClientX.OnFriendMessageReceive += OnFriendMessageReceive;
                    ClientX.OnGroupMessageReceive += OnGroupMessageReceive;
                    ClientX.OnEventBotInvitedJoinGroupRequestEvent += OnEventBotInvitedJoinGroupRequestEvent;
                    ClientX.OnEventNewFriendRequestEvent += OnEventNewFriendRequestEvent;
                    ClientX.OnEventFriendNickChangedEvent += OnEventFriendNickChangedEvent;


                }
                else
                {
                    Logger.Instance.Log($"不启动Mirai，启动本地应答");

                }




            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
            }
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
                        Logger.Instance.Log(ex);
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
            Logger.Instance.Log($"*WEB*>>{message}");

            GroupMessageSender gms = new()
            {
                id = -1,
                group = new GroupMessageSender.Group
                {
                    id = 2,
                }
            };

            List<Message> msgs = new List<Message>()
            {
                new At(Config.Instance.BotQQ, Config.Instance.BotName),
                new Plain(message)
            };
            OnGroupMessageReceive(gms, msgs.ToArray());

            System.Diagnostics.Debug.WriteLine(message);
            //Console.WriteLine(message);
        }

        /* APIs
         * OnEventGroupNameChangeEvent	GroupNameChangeEvent	群名改变信息
OnEventGroupEntranceAnnouncementChangeEvent	GroupEntranceAnnouncementChangeEvent	某个群的入群公告改变
OnEventGroupMuteAllEvent	GroupMuteAllEvent	群全员禁言
OnEventGroupAllowAnonymousChatEvent	GroupAllowAnonymousChatEvent	某个群更改了群匿名聊天状态
OnEventGroupAllowConfessTalkEvent	GroupAllowConfessTalkEvent	某个群更改了坦白说的状态
OnEventGroupAllowMemberInviteEvent	GroupAllowMemberInviteEvent	某个群员邀请好友加群


        OnEventGroupRecallEvent	GroupRecallEvent	某群员撤回信息
OnEventMemberJoinEvent	MemberJoinEvent	某群有新人入群了
OnEventMemberLeaveEventKick	MemberLeaveEventKick	某群把某人踢出了(不是Bot)
OnEventMemberLeaveEventQuit	MemberLeaveEventQuit	某群有成员主动退群了
OnEventCardChangeEvent	MemberCardChangeEvent	某群有人的群名片改动了
OnEventSpecialTitleChangeEvent	MemberSpecialTitleChangeEvent	某群群主改动了某人头衔
OnEventPermissionChangeEvent	MemberPermissionChangeEvent	某群有某个成员权限被改变了(不是Bot)
OnEventMemberMuteEvent	MemberMuteEvent	某群的某个群成员被禁言
OnEventMemberUnmuteEvent	MemberUnmuteEvent	某群的某个群成员被取消禁言
OnEventMemberHonorChangeEvent	MemberHonorChangeEvent	某群的某个成员的群称号改变
OnEventMemberJoinRequestEvent	MemberJoinRequestEvent	接收到用户入群申请

        OnEventNewFriendRequestEvent	NewFriendRequestEvent	接收到新好友请求
OnEventFriendInputStatusChangedEvent	FriendInputStatusChangedEvent	好友的输入状态改变
OnEventFriendNickChangedEvent	FriendNickChangedEvent	好友的昵称改变
OnEventFriendRecallEvent	FriendRecallEvent	好友撤回信息


        OnEventBotGroupPermissionChangeEvent	BotGroupPermissionChangeEvent	Bot在群里的权限被改变了
OnEventBotMuteEvent	BotMuteEvent	Bot被禁言
OnEventBotUnmuteEvent	BotUnmuteEvent	Bot被解除禁言
OnEventBotJoinGroupEvent	BotJoinGroupEvent	Bot加入新群
OnEventBotLeaveEventActive	BotLeaveEventActive	Bot主动退群
OnEventBotLeaveEventKick	BotLeaveEventKick	Bot被群踢出
OnEventNudgeEvent	NudgeEvent	Bot被戳一戳
OnEventBotInvitedJoinGroupRequestEvent	BotInvitedJoinGroupRequestEvent	Bot被邀请入群申请

        OnEventBotOnlineEvent	BotOnlineEvent	Mirai后台证实QQ已上线
OnEventBotOfflineEventActive	BotOfflineEventActive	Mirai后台证实QQ主动离线
OnEventBotOfflineEventForce	BotOfflineEventForce	Mirai后台证实QQ被挤下线
OnEventBotOfflineEventDropped	BotOfflineEventDropped	Mirai后台证实QQ由于网络问题掉线
OnEventBotReloginEvent	BotReloginEvent	Mirai后台证实QQ重新连接完毕

        OnFriendMessageReceive	FriendMessageSender, Message[]	接收到好友私聊信息
OnGroupMessageReceive	GroupMessageSender, Message[]	接收到群消息
OnTempMessageReceive	TempMessageSender, Message[]	接收到临时信息
OnStrangerMessageReceive	StrangerMessageSender, Message[]	接收到陌生人消息
OnOtherMessageReceive	OtherClientMessageSender, Message[]	接收到其他类型消息
OnFriendSyncMessageReceive	FriendSyncMessageSender, Message[]	接收到好友同步消息
OnGroupSyncMessageReceive	GroupSyncMessageSender, Message[]	接收到群同步消息
OnTempSyncMessageReceive	TempSyncMessageSender, Message[]	接收到临时同步消息
OnStrangerSyncMessageReceive	StrangerSyncMessageSender, Message[]	接收到陌生人同步消息

        MGetPlainString 获取消息中的所有字符集合
c.OnFriendMessageReceive += (s, e) =>
{
    if(s.id != qqid) //过滤自己发出的信息
    {
        var str = e.MGetPlainString();
        Console.WriteLine(str);
    }
};
2. MGetPlainString 获取消息中的所有字符集合并且使用(splitor参数)分割
c.OnFriendMessageReceive += (s, e) =>
{
    if(s.id != qqid) //过滤自己发出的信息
    {
        var str = e.MGetPlainStringSplit(); //默认使用空格分隔
        //var str = e.MGetPlainStringSplit(","); //使用逗号分割
        Console.WriteLine(str);
    }
};
3. MGetEachImageUrl 获取消息中的所有图片集合的Url
c.OnFriendMessageReceive += (s, e) =>
{
    if(s.id != qqid) //过滤自己发出的信息
    {
        var sx = e.MGetEachImageUrl();
        Console.WriteLine(sx[1].url);
    }
};
4. SendToFriend 信息类前置发送好友信息
new Message[] { new Plain("...") }.SendToFriend(qqnumber,c);
5. SendToGroup 信息类前置发送群信息
new Message[] { new Plain("...") }.SendToGroup(qqgroupnumber,c);
6. SendToTemp 信息类前置发送临时信息
new Message[] { new Plain("...") }.SendToTemp(qqnumber,qqgroupnumber,c);
7. SendMessage 对于 GenericModel 的群发信息逻辑
注:您也可以使用foreach对每个群/好友/群员发送

var msg = new Message[] { new Plain("...") };//要发送的信息

var fl = new FriendList().Send(c);//获取好友列表

fl[0].SendMessage(msg,c);//朝好友列表的1号好友发送信息(原生写法)
(fl[0], msg).SendMessage(c); //朝好友列表的1号好友发送信息(简单写法)

foreach(var i in fl) //朝好友列表的所有好友发送信息(原生写法)
{
    i.SendMessage(msg,c);
}

var gl = new GroupList().Send(c);//获取群列表
var gml = gl[0].GetMemberList(c);//获取群1的群员列表

gml[0].SendMessage(msg,c);//朝群1的1号群员发送msg信息(原生写法)
(gml[0], msg).SendMessage(c);//朝群1的1号群员发送msg信息(简单写法)

foreach(var i in gml) //朝群1的所有群员发送信息(原生写法)
{
    i.SendMessage(msg,c);
}

foreach(var i in gl) //朝所有群发送群信息(原生写法)
{
    i.SendMessage(msg,c);
}


        Instance	MessageId	SendMsgBack	Message[],
Opt ConClient?	往原处发送信息
Instance	async Task	SendMsgBackAsync	Message[],
Opt ConClient?	往原处发送信息
Instance	MessageId	SendMessageToFriend	Message[],
Opt ConClient?	强行往发送者的私聊发送信息
Instance	async Task	SendMessageToFriendAsync	Message[],
Opt ConClient?	强行往发送者的私聊发送信息
Instance	MessageId	SendMessageToGroup	Message[],
Opt ConClient?	强行往发送者的群发送信息(如果有)
Instance	async Task	SendMessageToGroupAsync	Message[],
Opt ConClient?	强行往发送者的群发送信息(如果有)



        _OnServeiceConnected	string	接收到WS连接成功信息
_OnServeiceError	Exception	接收到WS错误信息
_OnServiceDropped	string	接收到WS断连信息
_OnClientOnlineEvent	OtherClientOnlineEvent	接收到其他客户端上线通知
_OnOtherClientOfflineEvent	OtherClientOfflineEvent	接收到其他客户端下线通知
_OnCommandExecutedEvent	CommandExecutedEvent	接收到后端传送命令执行
_OnUnknownEvent	string	接收到后端传送未知指令
         * 
        var bp = new BotProfile().Send(c); //获取Bot资料
        var fp = new FriendProfile(qqnumber).Send(c);//获取好友资料
        var mp = new MemberProfile(qqgroup, qqnumber).Send(c);//获取群员资料
        var up = new UserProfile(qqnumber).Send(c);//获取用户资料
                                                   //获取群公告&&推送群公告
        var k = new Anno_list(qqgroup).Send(c);
        k[1].Delete(c);//删除群公告1 (快速写法)
        var k1 = new Anno_publish(qqgroup, "Bot 公告推送").Send(c);
        var k2 = new Anno_publish(qqgroup, "Bot 带图公告推送实验", imageUrl: "https://www.baidu.com/img/PCtm_d9c8750bed0b3c7d089fa7d55720d6cf.png").Send(c);
        */



        /// <summary>
        /// 刷新好友列表并更新配置文件
        /// </summary>
        public void RefreshFriendList()
        {
            try
            {
                if (ClientX != null)
                {
                    var fp = new FriendList().Send(ClientX);
                    Config.Instance.friends.Clear();
                    if (fp == null)
                    {
                        Logger.Instance.Log($"不会吧不会吧不会没有好友吧");

                    }
                    else
                    {
                        foreach (var f in fp)
                        {
                            var friend = Config.Instance.UserInfo(f.id);
                            friend.Name = f.nickname;
                            //friend.Mark = f.remark;
                            friend.Tags.Add("好友");
                            //friend.Type = PlayerType.Normal;
                            Config.Instance.friends.Add(f.id, f);
                        }
                    }




                    var gp = new GroupList().Send(ClientX);
                    Config.Instance.groups.Clear();
                    Config.Instance.groupMembers.Clear();
                    if (gp == null)
                    {
                        Logger.Instance.Log($"不会吧不会吧不会没有群吧");

                    }
                    else
                    {
                        foreach (var g in gp)
                        {
                            var group = Config.Instance.GroupInfo(g.id);
                            group.Name = g.name;
                            var groupMembers = g.GetMemberList(ClientX);
                            if (groupMembers == null)
                            {
                                Logger.Instance.Log($"不会吧不会吧不会{g.id}是鬼群吧");
                                continue;
                            }
                            Config.Instance.groups.Add(g.id, g);
                            Config.Instance.groupMembers.Add(g.id, groupMembers);
                            foreach (var gf in groupMembers)
                            {
                                var member = Config.Instance.UserInfo(gf.id);
                                member.Mark = gf.memberName;    //群昵称？
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
            }

        }



        async void OnFriendMessageReceive(FriendMessageSender s, Message[] e)
        {
            Logger.Instance.Log($"好友信息 [qq:{s.id},昵称:{s.nickname},备注:{s.remark}] \n内容:{e.MGetPlainString()}", LogType.Mirai);
            var uinfo = Config.Instance.UserInfo(s.id);
            uinfo.Name = s.nickname;
            uinfo.Mark = s.remark;
            var context = new MessageContext
            {
                userId = s.id,
                groupId = 0,
                client = ClientX,
                recvMessages = e,
                isAskme = true
            };
            HandleFriendMessageReceiveMultiIO(context);
        }


        async void HandleFriendMessageReceiveMultiIO(MessageContext context)
        {
            if (!Config.Instance.AllowPlayer(context.userId)) return; // 黑名单
            if (context.recvMessages == null) return;
            var uinfo = Config.Instance.UserInfo(context.userId);

            var sourceItem = context.recvMessages.First() as Source;
            HistoryManager.Instance.saveMsg(sourceItem.id, 0, context.userId, context.recvMessages.MGetPlainString());
            
            bool talked = false;
            foreach(var mod in Mods)
            {
                var succeed = await mod.HandleFriendMessage(context);
                if (succeed)
                {
                    talked = true;
                    break;
                }
            }

            // 计数统计
            if (talked)
            {
                Config.Instance.UserInfo(context.userId).UseTimes += 1;
                Config.Instance.App.Log.playTimePrivate += 1;
            }
        }

        async void OnGroupMessageReceive(GroupMessageSender s, MeowMiraiLib.Msg.Type.Message[] e)
        {
            if (e == null) return;
            var sourceItem = e.First() as Source;
            Logger.Instance.Log($"[{sourceItem.id}]群({s.group.id})信息 [qq:{s.id},昵称:{s.memberName}] \n内容:{e.MGetPlainString()}", LogType.Mirai);
            if(!Config.Instance.AllowPlayer(s.id) || !Config.Instance.AllowGroup(s.group.id)) return; // 黑名单

            var uinfo = Config.Instance.UserInfo(s.id);
            uinfo.Name = s.memberName;

            var context = new MessageContext
            {
                userId = s.id,
                groupId = s.group.id,
                client = ClientX,
                recvMessages = e,
            };
            HistoryManager.Instance.saveMsg(sourceItem.id, context.groupId, context.userId, context.recvMessages.MGetPlainString());
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
                    if (item is AtAll)
                    {
                        context.isAskme = true;
                        break;
                    }
                    if (item is At itemat)
                    {
                        if (itemat.target == Config.Instance.BotQQ)
                        {
                            context.isAskme = true;
                            break;
                        }
                    }
                    if (item is Plain plain)
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

            var sendNum = 0;
            foreach (var mod in Mods)
            {
                var succeed = await mod.HandleGroupMessage(context);
                if (succeed)
                {
                    sendNum++;
                    break;
                }
            }

            if (sendNum > 0)
            {
                //p.UseTimes += 1;
                Config.Instance.UserInfo(context.userId).UseTimes += 1;
                Config.Instance.App.Log.playTimeGroup += 1;
            }
        }

        void ServiceConnected(string e)
        {
            Logger.Instance.Log($"***连接成功：{e}", LogType.Mirai);





            //Logger.Instance.Log($"更新好友列表和群列表...");
            //RefreshFriendList();
            //Logger.Instance.Log($"更新完毕，找到{Config.Instance.friends.Count}个好友，{Config.Instance.groups.Count}个群...");



        }

        void OnServeiceError(Exception e)
        {
            Logger.Instance.Log($"***连接出错：{e.Message}\r\n{e.StackTrace}", LogType.Mirai);
        }

        void OnServiceDropped(string e)
        {
            Logger.Instance.Log($"***连接中断：{e}", LogType.Mirai);
        }

        void OnClientOnlineEvent(OtherClientOnlineEvent e)
        {
            Logger.Instance.Log($"***其他平台登录（标识：{e.id}，平台：{e.platform}", LogType.Mirai);
        }
        void OnEventBotInvitedJoinGroupRequestEvent(BotInvitedJoinGroupRequestEvent e)
        {
            Logger.Instance.Log($"受邀进群（用户：{e.fromId}，群：{e.groupName}({e.groupId})消息：{e.message}", LogType.Mirai);
            var g = Config.Instance.GroupInfo(e.groupId);
            var u = Config.Instance.UserInfo(e.fromId);
            if (g.Is("黑名单") || u.Is("黑名单"))
            {
                e.Deny(ClientX, "非好友不接受邀请谢谢");
                return;
            }
            if (Config.Instance.friends.ContainsKey(e.fromId) || u.Is("管理员") || u.Is("好友") || e.fromId == Config.Instance.App.Avatar.adminQQ)
            {
                e.Grant(ClientX);
                return;
            }
        }

        void OnEventNewFriendRequestEvent(NewFriendRequestEvent e)
        {
            Logger.Instance.Log($"好友申请：{e.nick}({e.fromId})(来自{e.groupId})消息：{e.message}", LogType.Mirai);
            if (!string.IsNullOrWhiteSpace(e.message) && e.message.StartsWith(Config.Instance.App.Avatar.askName))
            {
                e.Grant(ClientX, "来了来了");
                var user = Config.Instance.UserInfo(e.fromId);
                user.Name = e.nick;
                //user.Mark = e.nick;
                user.Tags.Add("好友");
                user.Type = PlayerType.Normal;
            }
            else
            {
                e.Deny(ClientX, "密码错误");
            }
        }

        void OnEventFriendNickChangedEvent(FriendNickChangedEvent e)
        {
            Logger.Instance.Log($"好友改昵称（{e.friend.id}，{e.from}->{e.to}", LogType.Mirai);
            var user = Config.Instance.UserInfo(e.friend.id);
            user.Name = e.to;

        }


    }




    public class LocalClient : Client
    {
        public string type;

        public LocalClient(string _type):base("ws://localhost", -1)
        {

            type = _type;
        }

        public delegate void HandleMessage(Message[] messages);
        public HandleMessage handleMessage;

    }
   







}
