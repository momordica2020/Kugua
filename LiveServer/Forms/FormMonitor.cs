using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
using System.Net.WebSockets;
using WebSocket4Net;
using Kugua.Integrations.NTBot;
using Message = Kugua.Integrations.NTBot.Message;
using System.Text;
using LiveServer.NTBot;
using LiveServer.Blive;


namespace LiveServer
{

    partial class FormMonitor : Form
    {


        DateTime beginTime;
        bool IsEnterAutoSend = true;
        bool IsVirtualGroup = false;

        public delegate void sendString(string msg);
        public static sendString SendMsgEvent;

        //public delegate void SendBotMsg(LocalBotOutMsg msg);
        //public static SendBotMsg SendBotMsgEvent;

        bool botRun = false;

        bool serverRun = false;


        public FormMonitor()
        {
            SendMsgEvent = HandleShowLoginfo;
            //SendBotMsgEvent = HandleBotMsg;
            WebSocketServer.OnMessageReceived += HandleBotMsg;
            InitializeComponent();

            this.DoubleBuffered = true; // 设置窗体的双缓冲

            //MMDKBot.Instance._sendLog = HandleShowLoginfo;
        }

        /// <summary>
        /// 设置缓冲阻止频闪。？
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // Turn on WS_EX_COMPOSITED 
                return cp;
            }
        }




        public void HandleShowLoginfo(string log)
        {
            int maxlen = 100000;
            try
            {
                Invoke((Action)(() =>
                {
                    tbLog.AppendText($"{log}\r\n");
                    if (tbLog.TextLength > maxlen)
                    {
                        tbLog.Text = tbLog.Text.Substring(tbLog.TextLength - maxlen);
                    }
                    tbLog.ScrollToCaret();
                }));
            }
            catch (Exception ex)
            {
                //Logger.Log(ex);
            }

        }




        private void 清空日志ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tbLog.Clear();
        }



        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            BServer.Stop();
            WebSocketServer.Stop();
        }

        private void Form1_Load(object sender, EventArgs e)
        {


        }



        /// <summary>
        /// 模拟bot的输入
        /// </summary>
        /// <param name="message"></param>
        public void virtualInput(string message)
        {
            string userId;
            string groupId;
            bool isAtMe = false;

            if (IsVirtualGroup)
            {
                LBotRequest msg = new LBotRequest
                {
                    userId = -1,
                    userName = "测试",
                    type = "danmu",
                    messages = { new MessageInfo( new Text(message))}
                };
                WebSocketServer.SendMessageAsync(msg);


                //userId = -1;
                //groupId = 1;
                textLocalTestGroup.AppendText($"[me]:{message}\r\n");

            }
            else
            {

                userId = "";
                groupId = "";
                textLocalTest.AppendText($"[me]:{message}\r\n");

                isAtMe = true;
            }



        }

        public void virtualPrintline(string text)
        {
            Invoke(new Action(() =>
            {
                if (IsVirtualGroup)
                {
                    textLocalTestGroup.AppendText($"[bot]:{text}\r\n");
                }
                else
                {
                    textLocalTest.AppendText($"[bot]:{text}\r\n");
                }
            }));

        }

        public void virtualOutput(List<Message> msgs)
        {
            
            foreach(var msg in msgs)
            {
                virtualPrintline(Util.Util.GetDesc(msg));
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string msg = textInputTest.Text.Trim();
            textInputTest.Clear();
            virtualInput(msg);

        }

        private void textInputTest_TextChanged(object sender, EventArgs e)
        {
            if (IsEnterAutoSend && textInputTest.Text.EndsWith("\n"))
            {
                string msg = textInputTest.Text.Trim();
                textInputTest.Clear();
                virtualInput(msg);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }








        private async void 存档当前配置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (serverRun)
            {
                serverRun = false;
                WebSocketServer.Stop();
                存档当前配置ToolStripMenuItem.Text = "开始WS监听";
            }
            else
            {
                serverRun = true;
                string prefix = "http://localhost:8849/";
                SendMsgEvent($"开始启动监听{prefix}");

                WebSocketServer.OnMessageReceived += (message) =>
                {
                    SendMsgEvent("Delegate received message: " + message);
                    
                };

                WebSocketServer.Start(prefix);

                //// 模拟延迟发送的消息
                //await Task.Delay(2000);
                //await WebSocketServer.SendMessageAsync(new { text = "Hello from Server after 2 seconds" });

                

                存档当前配置ToolStripMenuItem.Text = "停止WS监听";

            }


        }


        ///// <summary>
        ///// 更新显示界面信息
        ///// </summary>
        //private void UpdateMonitorInfo()
        //{
        //    if (State != BotRunningState.stop)
        //    {
        //        var cpu = Config.Instance.systemInfo.CpuLoad;
        //        var mem = 100.0 - ((double)Config.Instance.systemInfo.MemoryAvailable * 100 / Config.Instance.systemInfo.PhysicalMemory);
        //        try
        //        {
        //            Invoke((Action)(() =>
        //            {

        //                lbCPU.Text = $"CPU\n({cpu.ToString(".0")}%)";
        //                lbMem.Text = $"内存\n({mem.ToString(".0")}%)";
        //                lbBeginTime.Text = $"{beginTime.ToString("yyyy-MM-dd")}\r\n{beginTime.ToString("HH:mm:ss")}";
        //                lbTimeSpan.Text = $"{(DateTime.Now - beginTime).Days}天\r\n{(DateTime.Now - beginTime).Hours}小时{(DateTime.Now - beginTime).Minutes}分{(DateTime.Now - beginTime).Seconds}秒";
        //                lbQQ.Text = $"{Config.Instance.App.Avatar.myQQ}";
        //                lbPort.Text = $"{Config.Instance.App.IO.MiraiPort}";
        //                lbVersion.Text = $"{Config.Instance.App.Version}\n({StaticUtil.GetBuildDate().ToString("F")})";
        //                lbUpdateTime.Text = $"{StaticUtil.GetBuildDate().ToString("yyyy-MM-dd")}";


        //                lbFriendNum.Text = $"{Config.Instance.players.Count}";
        //                lbGroupNum.Text = $"{Config.Instance.playgroups.Count}";
        //                lbUseNum.Text = $"{Config.Instance.App.Log.playTimePrivate + Config.Instance.App.Log.playTimeGroup}";

        //                pbCPU.Value = (int)(cpu);
        //                pbMem.Value = (int)(mem);


        //            }));
        //        }
        //        catch (Exception ex)
        //        {

        //        }



        //        Thread.Sleep(500);     // 1s
        //    }
        //}

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            IsEnterAutoSend = checkBox1.Checked;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                // private
                IsVirtualGroup = false;
                button2.Text = "发送（私聊）";
            }
            else
            {
                IsVirtualGroup = true;
                button2.Text = "发送（群组）";
            }
        }

        private void 清空私聊窗口ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textLocalTest.Clear();
        }

        private void 清空群聊窗口ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textLocalTestGroup.Clear();
        }




        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //DealFilterFile("FilterNormal");
            //DealFilterFile("FilterStrict");
        }


        private void 测试gptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //workTest();
        }



        private void 启动b站监听ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!BServer.IsLive)
            {
                BServer.Run();
            }
            else
            {
                BServer.Stop();
            }
            
        }



        public void HandleBotMsg(string msgStr)
        {
            JObject jo = JObject.Parse(msgStr);
            LBotResponse msg = new LBotResponse();
            msg.userId = int.Parse(jo["userId"].ToString());
            msg.userName = jo["userName"].ToString();
            
            // jo.ToObject<LBotResponse>();
            List<Message> msgs = new List<Message>();
            WebSocketServer.Parse(jo["messages"].ToString(), msgs);

            

            //TODO: 处理bot返回的消息，语音说出或者控制live2d
            //SendMsgEvent($"[BOT]@{msg.userId},{msg.messages.MGetPlainString()}");
            virtualOutput(msgs);
        }


       

    }




}
