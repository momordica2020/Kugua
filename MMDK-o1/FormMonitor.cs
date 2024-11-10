using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Text;
//using MMDK.Util;
using Newtonsoft.Json.Linq;
using MMDK.Util;
using static MMDK.Util.GPT;
//using static MMDK.Util.GPT;


namespace MMDK
{

    partial class FormMonitor : Form
    {
     


        //public SendMsgDelegate sendMsgDelegate;


        #region 窗体相关定义



        DateTime beginTime;
        bool IsEnterAutoSend = true;
        bool IsVirtualGroup = false;

        public delegate void sendString(string msg);

        public enum BotRunningState
        {
            stop,
            mmdkInit,
            ok,
            exit
        }
        public BotRunningState _state;
        BotRunningState State
        {
            get => _state;
            set
            {
                _state = value;

                var stateMessages = new Dictionary<BotRunningState, string>
                    {
                        { BotRunningState.stop, "已停止" },
                        { BotRunningState.mmdkInit, "正在启动Bot" },
                        { BotRunningState.ok, "正在运行" }
                    };

                string text = stateMessages.ContainsKey(value) ? stateMessages[value] : string.Empty;
                //更新显示窗口
                try
                {
                    Invoke((Action)(() =>
                    {
                        lbState.Text = text;
                    }));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating state label: {ex.Message}\r\n{ex.StackTrace}");
                }
            }
        }



        #endregion

        public FormMonitor()
        {
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




        public void HandleShowLoginfo(LogInfo log)
        {
            int maxlen = 100000;
            try
            {
                Invoke((Action)(() =>
                {
                    tbLog.AppendText($"{log.ToDescription()}\r\n");
                    if (tbLog.TextLength > maxlen)
                    {
                        tbLog.Text = tbLog.Text.Substring(tbLog.TextLength - maxlen);
                    }
                    tbLog.ScrollToCaret();
                }));
            }
            catch (Exception ex)
            {
                //Logger.Instance.Log(ex);
            }

        }










        private void tbMmdk_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void tbMirai_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void 清空日志ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tbLog.Clear();
        }



        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

           
            Task.Run(() =>
            {
                while (State != BotRunningState.exit)
                {
                   // UpdateMonitorInfo(); // 更新状态

                    // 控制更新频率，比如每秒更新一次
                    Thread.Sleep(200);
                }
            });
            //new Thread(workMonitor).Start();
        }

        //private void StartBot()
        //{
        //    //button1.Enabled = false;

        //    Task.Run(() =>
        //    {
        //        //workRunBot();
        //    });

        //    textInputTest.Focus();
        //}

        /// <summary>
        /// 模拟bot的输入
        /// </summary>
        /// <param name="message"></param>
        public void virtualInput(string message)
        {
            long userId;
            long groupId;
            bool isAtMe = false;

            if (IsVirtualGroup)
            {

                userId = -1;
                groupId = 1;
                textLocalTestGroup.AppendText($"[me]:{message}\r\n");

                //if (Config.Instance.isAskMe(message))
                //{
                //    isAtMe = true;
                //    message = message.Substring(Config.Instance.App.Avatar.askName.Length);
                //}
            }
            else
            {

                userId = -1;
                groupId = 0;
                textLocalTest.AppendText($"[me]:{message}\r\n");

                isAtMe = true;
            }



            //try
            //{

            //    List<string> res = new List<string>();
            //    foreach (var mod in Mods)
            //    {
            //        if (!isAtMe) break;
            //        var succeed = mod.HandleText(userId, groupId, message, res);
            //        if (succeed)
            //        {
            //            break;
            //        }
            //    }
            //    foreach (var result in res)
            //    {
            //        virtualOutput(result);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    textLocalTest.AppendText($"[error]:{ex.Message}\r\n{ex.StackTrace}\r\n");
            //}


        }



        public void virtualOutput(string result)
        {
            if (IsVirtualGroup)
            {
                textLocalTestGroup.AppendText($"[bot]:{result}\r\n");
            }
            else
            {
                textLocalTest.AppendText($"[bot]:{result}\r\n");
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

        private void 启动botToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //MMDKBot.Instance.Start();
            启动botToolStripMenuItem.Enabled = false;
            启动botToolStripMenuItem.Text = "（正在运行）";
        }

        private void 存档当前配置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Config.Instance.Save();
            Logger.Instance.Log($"已储存当前配置");
        }


        /// <summary>
        /// 更新显示界面信息
        /// </summary>
        private void UpdateMonitorInfo()
        {
            if (State != BotRunningState.exit)
            {
                var cpu = Config.Instance.systemInfo.CpuLoad;
                var mem = 100.0 - ((double)Config.Instance.systemInfo.MemoryAvailable * 100 / Config.Instance.systemInfo.PhysicalMemory);
                try
                {
                    Invoke((Action)(() =>
                    {

                        lbCPU.Text = $"CPU\n({cpu.ToString(".0")}%)";
                        lbMem.Text = $"内存\n({mem.ToString(".0")}%)";
                        lbBeginTime.Text = $"{beginTime.ToString("yyyy-MM-dd")}\r\n{beginTime.ToString("HH:mm:ss")}";
                        lbTimeSpan.Text = $"{(DateTime.Now - beginTime).Days}天\r\n{(DateTime.Now - beginTime).Hours}小时{(DateTime.Now - beginTime).Minutes}分{(DateTime.Now - beginTime).Seconds}秒";
                        lbQQ.Text = $"{Config.Instance.App.Avatar.myQQ}";
                        lbPort.Text = $"{Config.Instance.App.IO.MiraiPort}";
                        lbVersion.Text = $"{Config.Instance.App.Version}\n({StaticUtil.GetBuildDate().ToString("F")})";
                        lbUpdateTime.Text = $"{StaticUtil.GetBuildDate().ToString("yyyy-MM-dd")}";


                        lbFriendNum.Text = $"{Config.Instance.players.Count}";
                        lbGroupNum.Text = $"{Config.Instance.playgroups.Count}";
                        lbUseNum.Text = $"{Config.Instance.App.Log.playTimePrivate + Config.Instance.App.Log.playTimeGroup}";

                        pbCPU.Value = (int)(cpu);
                        pbMem.Value = (int)(mem);


                    }));
                }
                catch (Exception ex)
                {

                }



                Thread.Sleep(500);     // 1s
            }
        }

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

        
        private async void workTest()
        {



            //var opt = new ChatGptUnofficialOptions
            //{
            //    BaseUrl = "http://127.0.0.1:8000/switch-model",
            //    Model = "rwkv"
            //};
            //var opt2 = new ChatGptOptions();
            //opt2.BaseUrl = "http://localhost:11434/v1/chat/completions";
            //opt2.Model = "modelscope.cn/Qwen/Qwen2.5-3B-Instruct-GGUF:latest";
            ////opt2.Temperature = 0.9; // Default: 0.9;
            ////opt2.TopP = 1.0; // Default: 1.0;
            ////opt2.MaxTokens = 100; // Default: 64;
            ////opt2.Stop = ["User:"]; // Default: null;
            ////opt2.PresencePenalty = 0.5; // Default: 0.0;
            ////opt2.FrequencyPenalty = 0.5; // Default: 0.0;

            //var bot = new ChatGpt("", opt2);

            //// get response
            //string ask = $"你好?有没有旅行建议？他说：";
            //Logger.Instance.Log($"[AskAI]{ask}");
            //var response = await bot.Ask(ask, "旅行建议");
            //Logger.Instance.Log($"[AI]{response}");
            ////Console.WriteLine(response);


            // get response for a specific conversation
            //response = await bot.Ask("今天天气如何", "conversation name");
            //Logger.Instance.Log($"[AI]{response}");
            //Console.WriteLine(response);


        }

        private void 测试gptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            workTest();
        }
    }




}
