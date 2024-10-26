using MeowMiraiLib;
using MeowMiraiLib.Event;
using MeowMiraiLib.Msg;
using MeowMiraiLib.Msg.Sender;
using MeowMiraiLib.Msg.Type;
using MMDK.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace MMDK
{

    partial class FormMonitor : Form
    {

        public delegate void sendString(string msg);

        public static MeowMiraiLib.Client ClientX = new("ws://localhost:8080/all?verifyKey=1234560&qq=2963959417");


        public Bank money = new Bank();
        public HistoryManager history = new HistoryManager();
        public ModeHelper mode = new ModeHelper();
        public DiceHelper dice = new DiceHelper();
        public Bank bank = new Bank();
        public DivinationHelper divi = new DivinationHelper();


        DateTime beginTime;


        runState _state;
        runState State
        {
            set
            {
                try
                {
                    _state = value;
                    string text = "";
                    switch (value)
                    {
                        case runState.stop: text = "已停止"; break;
                        case runState.mmdkInit: text = "正在启动Bot"; break;
                        case runState.ok: text = "正在运行"; break;
                        default: break;
                    }
                    Invoke(new EventHandler(delegate
                    {
                        lbState.Text = text;
                    }));
                }
                catch
                {

                }

            }
            get
            {
                return _state;
            }
        }
        public FormMonitor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 检查并初始化配置
        /// </summary>
        /// <returns></returns>
        bool checkAndSetConfigValid()
        {
            try
            {

                if (string.IsNullOrWhiteSpace(Config.Instance.appConfig.Version)) Config.Instance.appConfig.Version = "v0.0.1";

                // qq info
                if (string.IsNullOrWhiteSpace(Config.Instance.appConfig.Avatar.myQQ.ToString())) Config.Instance.appConfig.Avatar.myQQ = 00000;


                beginTime = DateTime.Now;
                Config.Instance.appConfig.Log.StartTime = beginTime;

                Config.Instance.appConfig.Log.beginTimes += 1;

            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
                return false;
            }

            return true;
        }



        public void logWindow(string str, LogType logType = LogType.System)
        {
            int maxlen = 100000;
            try
            {
                Invoke(new EventHandler(delegate
                {
                    tbMmdk.AppendText($"[{DateTime.Now:G}][{Logger.GetLogTypeName(logType)}]{str}\r\n");
                    if (tbMmdk.TextLength > maxlen)
                    {
                        tbMmdk.Text = tbMmdk.Text.Substring(tbMmdk.TextLength - maxlen);
                    }
                    tbMmdk.ScrollToCaret();
                }));

                Logger.Instance.Log(str, logType);
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
            }

        }

 

        public void OnFriendMessageReceive(FriendMessageSender s, MeowMiraiLib.Msg.Type.Message[] e)
        {
            if (s.id == Config.Instance.appConfig.Avatar.myQQ) return;
            logWindow($"好友信息 [qq:{s.id},昵称:{s.nickname},备注:{s.remark}] \n内容:{e.MGetPlainString()}");
            history.saveMsg(0, s.id, e.MGetPlainString());
            string cmd = e.MGetPlainString().Trim();
            bool talked = false;

            if (cmd.Length > 0)
            {
                List<string> res = new List<string>();

                if (dice.HandleMessage(cmd, ref res) ||
                    divi.HandleMessage(cmd, ref res) ||
                    bank.HandleMessage(s.id, 0, cmd, ref res) ||
                    mode.HandleMessage(s.id, 0, cmd, ref res, true))
                {
                    foreach (var msg in res)
                    {
                        if (msg.Trim().Length <= 0) continue;
                        var output = new MeowMiraiLib.Msg.Type.Message[]
                        {

                            new Plain(msg)
                        };
                        new FriendMessage(s.id, output).Send(ClientX);
                        talked = true;
                    }
                }
            }
            // update player info
            Player p = Config.Instance.GetPlayerInfo(s.id);
            p.Name = s.nickname;
            p.Mark = s.remark;
            // 计数统计
            if (talked)
            {
                p.UseTimes += 1;
                Config.Instance.appConfig.Log.playTimePrivate += 1;
            }
        }



        public void OnGroupMessageReceive(GroupMessageSender s, MeowMiraiLib.Msg.Type.Message[] e)
        {
            if (s.id == Config.Instance.appConfig.Avatar.myQQ) return;
            logWindow($"群友信息 [qq:{s.id},昵称:{s.memberName}] \n内容:{e.MGetPlainString()}");
            history.saveMsg(s.group.id, s.id, e.MGetPlainString());

            // 检查群聊是否需要bot回复
            bool isAtMe = false;
            bool talked = false;
            string cmd = e.MGetPlainString().Trim();
            if (cmd.Length > 0)
            {

            }
            foreach (var v in e)
            {
                if (v.type == "At" && ((MeowMiraiLib.Msg.Type.At)v).target == Config.Instance.appConfig.Avatar.myQQ)
                {
                    isAtMe = true;
                    break;
                }
            }
            if (Config.Instance.isAskMe(cmd))
            {
                isAtMe = true;
                cmd = cmd.Substring(Config.Instance.appConfig.Avatar.askName.Length);
            }
            List<string> res = new List<string>();
            if ((isAtMe && (
                dice.HandleMessage(cmd, ref res) ||
                divi.HandleMessage(cmd, ref res) ||
                bank.HandleMessage(s.id, s.group.id, cmd, ref res)))||
                mode.HandleMessage(s.id, s.group.id, cmd, ref res, isAtMe))
            {

            }

            if (res != null)
            {
                var rres = res.ToArray();
                if (rres != null && rres.Length > 0)
                {
                    for (int i = 0; i < rres.Length; i++)
                    {
                        string text = rres[i];
                        if (text == null) continue;
                        text = text.Trim(); if (text.Length <= 0) continue;
                        if (i == 0)
                        {
                            // 第一条信息 at 一下，后续就不了
                            var output = new MeowMiraiLib.Msg.Type.Message[]
                            {
                                         new At(s.id, s.memberName),
                                         new Plain(" " + rres[i])
                            };
                            new GroupMessage(s.group.id, output).Send(ClientX);
                            talked = true;
                        }
                        else
                        {
                            var output = new MeowMiraiLib.Msg.Type.Message[]
                            {
                                          new Plain(rres[i])
                            };
                            new GroupMessage(s.group.id, output).Send(ClientX);
                            talked = true;
                        }
                    }
                }
            }

            // update player info
            Playgroup p = Config.Instance.GetGroupInfo(s.group.id);
            p.Name = s.group.name;
            if (talked)
            {
                p.UseTimes += 1;
                Config.Instance.GetPlayerInfo(s.id).UseTimes += 1;
                Config.Instance.appConfig.Log.playTimeGroup += 1;
            }
        }

        void ServiceConnected(string e)
        {
            logWindow($"连接成功：{e}");
        }

        void OnServeiceError(Exception e)
        {
            logWindow($"连接出错：{e.Message}\r\n{e.StackTrace}");
        }

        void OnServiceDropped(string e)
        {
            logWindow($"连接中断：{e}");
        }

        void OnClientOnlineEvent(OtherClientOnlineEvent e)
        {
            logWindow($"其他平台登录（标识：{e.id}，平台：{e.platform}");
        }
        void OnEventBotInvitedJoinGroupRequestEvent(BotInvitedJoinGroupRequestEvent e)
        {
            if (e.message.StartsWith(Config.Instance.appConfig.Avatar.askName) || e.fromId == Config.Instance.appConfig.Avatar.adminQQ )  //某个条件
            {
                e.Grant(ClientX);
            }
            else
            {
                e.Deny(ClientX);
            }
        }
        public void workRunMMDK()
        {
            try
            {
                //mirai = new MiraiLink();
                logWindow($"正在连接Mirai本地...");
                ClientX._OnServeiceConnected += ServiceConnected;
                ClientX._OnServeiceError += OnServeiceError;
                ClientX._OnServiceDropped += OnServiceDropped;
                
                ClientX.Connect();




                logWindow($"开始启动bot...");
                //bot = new MainProcess();
                //bot.Init(config);
                Config.Instance.Load();

                money = new Bank();
                money.Init();

                if (true)
                {
                    // 打开历史记录，不会是真的吧
                    string HistoryPath = Config.Instance.ResourceFullPath("HistoryPath");
                    if (!Directory.Exists(HistoryPath)) Directory.CreateDirectory(HistoryPath);
                    logWindow($"历史记录保存在 {HistoryPath} 里");
                    history.Init(HistoryPath);
                }
                else
                {
                    logWindow($"历史记录不会有记录");
                }


                bank.Init();
                divi.Init();
                mode.Init();


                State = runState.ok;
                logWindow($"bot启动完成，开始绑定接受数据回调");

                ClientX.OnFriendMessageReceive += OnFriendMessageReceive;
                ClientX.OnGroupMessageReceive += OnGroupMessageReceive;
                ClientX.OnEventBotInvitedJoinGroupRequestEvent += OnEventBotInvitedJoinGroupRequestEvent;

            }
            catch (Exception ex)
            {
                logWindow(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void ClientX__OnServeiceConnected(string e)
        {
            throw new NotImplementedException();
        }

        public void workMonitor()
        {
            SystemInfo systemInfo = new SystemInfo();
            while (State != runState.exit)
            {
                var cpu = systemInfo.CpuLoad;
                var mem = 100.0 - ((double)systemInfo.MemoryAvailable * 100 / systemInfo.PhysicalMemory);

                try
                {
                    Invoke(new EventHandler(delegate
                    {
                        lbCPU.Text = $"CPU\n({cpu.ToString(".0")}%)";
                        lbMem.Text = $"内存\n({mem.ToString(".0")}%)";
                        lbBeginTime.Text = $"{beginTime.ToString("yyyy-MM-dd")}\r\n{beginTime.ToString("HH:mm:ss")}";
                        lbTimeSpan.Text = $"{(DateTime.Now - beginTime).Days}天\r\n{(DateTime.Now - beginTime).Hours}小时{(DateTime.Now - beginTime).Minutes}分{(DateTime.Now - beginTime).Seconds}秒";
                        lbQQ.Text = $"{Config.Instance.appConfig.Avatar.myQQ}";
                        lbPort.Text = $"{Config.Instance.appConfig.IO.MiraiPort}";
                        lbVersion.Text = $"{Config.Instance.appConfig.Version}";
                        //lbFriendNum.Text = $"{config["friendnum"]}";
                        //lbGroupNum.Text = $"{config["groupnum"]}";
                        lbUseNum.Text = $"{Config.Instance.appConfig.Log.playTimePrivate + Config.Instance.appConfig.Log.playTimeGroup}";
                        //if (bot != null)
                        //{
                        //    lbFriendNum.Text = $"{bot.friends.Count}";
                        //    lbGroupNum.Text = $"{bot.groups.Count}";
                        //}

                        pbCPU.Value = (int)(cpu);
                        pbMem.Value = (int)(mem);
                    }));
                }
                catch
                {

                }



                Thread.Sleep(500);     // 1s
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //button1.Enabled = false;

            new Thread(workRunMMDK).Start();
            button1.Text = "点我重启";
        }

        private void tbMmdk_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void tbMirai_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void 清空日志ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tbMmdk.Clear();
        }



        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                history.run = false;
                Config.Instance.Save();
                State = runState.exit;
                Logger.Instance.Close();
                //Environment.Exit(0);
            }
            catch
            {

            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            logWindow("开始初始化配置文件。");

            Config.Instance.Load();
            bool isValid = checkAndSetConfigValid();
            if (!isValid)
            {
                logWindow("配置文件读取失败，中止运行");
                return;
            }


            logWindow("配置文件读取完毕。");

            new Thread(workMonitor).Start();
        }

        /// <summary>
        /// 模拟输入给bot
        /// </summary>
        /// <param name="str"></param>
        public void virtualInput(string str)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            virtualInput(textInputTest.Text.Trim());
            textInputTest.Clear();
        }

        private void textInputTest_TextChanged(object sender, EventArgs e)
        {
            if (textInputTest.Text.EndsWith("\n"))
            {
                virtualInput(textInputTest.Text.Trim());
                textInputTest.Clear();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Config.Instance.Save();
        }
    }
    enum runState
    {
        stop,
        mmdkInit,
        ok,
        exit,
    }
}
