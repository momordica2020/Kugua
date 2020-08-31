using MMDK.Core;
using MMDK.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMDKMonitor
{
    enum runState
    {
        stop,
        miraiInit,
        mmdkInit,
        ok,
        exit,
    }
    public partial class Form1 : Form
    {
        string libPath = "./lib";
        string configPath = "config.txt";

        string httpConfigPath = "plugins/MiraiAPIHTTP/setting.yml";
        string miraiPath = "miraiOK_windows_386.exe";
        string miraiKey = "WocaoEsuAAAAAAAAA!";

        DateTime beginTime;

        Process MiraiProcess;
        MainProcess bot;
        MiraiInfo miraiInfo;
        Config config;

        runState _state;
        runState State
        {
            set
            {
                try {
                    _state = value;
                    string text = "";
                    switch (value)
                    {
                        case runState.stop: text = "已停止"; break;
                        case runState.miraiInit: text = "正在启动Mirai"; break;
                        case runState.mmdkInit: text = "正在启动Bot"; break;
                        case runState.ok: text = "正在运行"; break;
                        default: break;
                    }
                    Invoke(new EventHandler(delegate{
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
        public Form1()
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
                if (config == null)
                {
                    return false;
                }

                if (string.IsNullOrWhiteSpace(config["version"])) config["version"] = "v0.0.1";
                if (string.IsNullOrWhiteSpace(config["debug"])) config["debug"] = "0";

                // qq info
                if (string.IsNullOrWhiteSpace(config["qq"])) config["qq"] = "00000";
                if (string.IsNullOrWhiteSpace(config["passwd"])) config["passwd"] = "nopasswd";

                // mirai config
                if (string.IsNullOrWhiteSpace(config["host"])) config["host"] = "127.0.0.1";
                if (string.IsNullOrWhiteSpace(config["port"])) config["port"] = "9999";
                //if (string.IsNullOrWhiteSpace(config["key"])) config["key"] = "null";
                miraiInfo = new MiraiInfo();
                miraiInfo.authKey = miraiKey;
                miraiInfo.port = int.Parse(config["port"]);
                miraiInfo.host = config["host"];
                refreshMiraiHttpPluginYml();

                beginTime = DateTime.Now;
                config["starttime"] = beginTime.ToString("G");
                if (string.IsNullOrWhiteSpace(config["historySave"])) config["historySave"] = "0";
                if (string.IsNullOrWhiteSpace(config["historyPath"])) config["historyPath"] = "./history/";
                if (string.IsNullOrWhiteSpace(config["startnum"])) config["startnum"] = "0";
                config.setInt("startnum", config.getInt("startnum") + 1); 
                if (string.IsNullOrWhiteSpace(config["playtimeprivate"])) config["playtimeprivate"] = "0";
                if (string.IsNullOrWhiteSpace(config["playtimegroup"])) config["playtimegroup"] = "0";
                if (string.IsNullOrWhiteSpace(config["errtime"])) config["errtime"] = "0";
                if (string.IsNullOrWhiteSpace(config["askname"])) config["askname"] = ".";
                if (string.IsNullOrWhiteSpace(config["money"])) config["money"] = "比特币";
                if (string.IsNullOrWhiteSpace(config["ignoreall"])) config["ignoreall"] = "0";
                if (string.IsNullOrWhiteSpace(config["testonly"])) config["testonly"] = "0";
                if (string.IsNullOrWhiteSpace(config["master"])) config["master"] = "0";
                if (string.IsNullOrWhiteSpace(config["groupmsgbuff"])) config["groupmsgbuff"] = "0";
            }
            catch (Exception ex)
            {
                FileHelper.Log(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 更新mirai的插件配置文件
        /// </summary>
        void refreshMiraiHttpPluginYml()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                using (FileStream fs = new FileStream($"{libPath}/{httpConfigPath}", FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        while (!sr.EndOfStream)
                        {
                            var line = sr.ReadLine();
                            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                            {
                                // skip
                                sb.AppendLine(line);
                            }
                            else if (line.StartsWith("host:"))
                            {
                                sb.AppendLine($"host: '{miraiInfo.host}'");
                            }
                            else if (line.StartsWith("port:"))
                            {
                                sb.AppendLine($"port: {miraiInfo.port}");
                            }
                            else if (line.StartsWith("authKey:"))
                            {
                                sb.AppendLine($"authKey: {miraiInfo.authKey}");
                            }
                            else
                            {
                                sb.AppendLine(line);
                            }
                        }
                    }
                }
                File.WriteAllText($"{libPath}/{httpConfigPath}", sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                FileHelper.Log(ex);
            }
            
        }


        public void logMMDK(string str)
        {
            try
            {
                Invoke(new EventHandler(delegate
                {
                    tbMmdk.AppendText(str + "\r\n");
                    tbMmdk.ScrollToCaret();
                }));
                FileHelper.Log($"[MMDK]{str}");
            }
            catch (Exception ex)
            {
                FileHelper.Log(ex);
            }

        }

        public void logMirai(string str)
        {
            try
            {
                Invoke(new EventHandler(delegate
                {
                    tbMirai.AppendText(str + "\r\n");
                    tbMirai.ScrollToCaret();
                }));
                FileHelper.Log($"[Mirai]{str}");
            }
            catch (Exception ex)
            {
                FileHelper.Log(ex);
            }
        }

        public void workRunMirai()
        {
            try
            {
                //logMirai($"正在启动Mirai...");
                State = runState.miraiInit;
                MiraiProcess = new Process();

                MiraiProcess.StartInfo.WorkingDirectory = libPath;
                //MiraiProcess.StartInfo.FileName = miraiPath;
                MiraiProcess.StartInfo.FileName = $"{libPath}/{miraiPath}";
                MiraiProcess.StartInfo.UseShellExecute = false;
                MiraiProcess.StartInfo.CreateNoWindow = true;
                MiraiProcess.StartInfo.RedirectStandardOutput = true;
                MiraiProcess.StartInfo.RedirectStandardInput = true;
                MiraiProcess.StartInfo.StandardOutputEncoding = Encoding.UTF8;

                MiraiProcess.Start();
                MiraiProcess.StandardInput.AutoFlush = true;
                //MiraiProcess.StandardInput.WriteLine($"");
                //MiraiProcess.StandardInput.WriteLine("exit");
                //MiraiProcess.StandardInput.Close();


                var reader = MiraiProcess.StandardOutput;

                
                while (!reader.EndOfStream && State != runState.exit)
                {
                    string line = reader.ReadLine();
                    logMirai(line);

                    if (line.Contains("启动完成"))
                    {
                        // ok
                        // State = runState.mmdkInit;
                        string cmd = $"login {config["qq"]} {config["passwd"]}";
                        logMirai($">> {cmd}");
                        MiraiProcess.StandardInput.WriteLine(cmd);
                    }
                    else if (line.Contains("Login successful") && State == runState.miraiInit) 
                    {
                        // ok 2
                        State = runState.mmdkInit;
                    }
                }
                //MiraiProcess.StandardInput.WriteLine("exit");
                //MiraiProcess.StandardInput.Close();

                logMirai("退出Mirai");
                //output = MiraiProcess.StandardOutput.ReadToEnd();
                MiraiProcess.WaitForExit();

            }
            catch (Exception ex)
            {
                logMirai(ex.Message + "\r\n" + ex.StackTrace);
            }

        }

        public void workRunMMDK()
        {
            try
            {
                logMMDK($"正在等待Mirai启动...");

                while (State != runState.mmdkInit)
                {
                    Thread.Sleep(500);
                    if(State == runState.exit)
                    {
                        // exit
                        logMMDK("退出bot");
                        return;
                    }
                }

                logMMDK($"Mirai启动完成，开始启动bot...");

                bot = new MainProcess();
                bot.processOutput += new processOutputHandler(logMMDK);
                bot.Init(config, miraiInfo);

                State = runState.ok;
                logMMDK($"bot启动完成，开始接受数据。");

            }
            catch (Exception ex)
            {
                logMMDK(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        public void workMonitor()
        {
            SystemInfo systemInfo = new SystemInfo();
            while(State != runState.exit)
            {
                var cpu = systemInfo.CpuLoad;
                var mem = 100.0 - ((double)systemInfo.MemoryAvailable * 100/ systemInfo.PhysicalMemory);

                try
                {
                    Invoke(new EventHandler(delegate {
                        lbCPU.Text = $"CPU\n({cpu.ToString(".0")}%)";
                        lbMem.Text = $"内存\n({mem.ToString(".0")}%)";
                        lbBeginTime.Text = $"{beginTime.ToString("yyyy-MM-dd")}\r\n{beginTime.ToString("HH:mm:ss")}";
                        lbTimeSpan.Text = $"{(DateTime.Now - beginTime).Days}天\r\n{(DateTime.Now - beginTime).Hours}小时{(DateTime.Now - beginTime).Minutes}分{(DateTime.Now - beginTime).Seconds}秒";
                        lbQQ.Text = $"{config["qq"]}";
                        lbPort.Text = $"{config["port"]}";
                        lbVersion.Text = $"{config["version"]}";
                        lbFriendNum.Text = $"{config["friendnum"]}";
                        lbGroupNum.Text = $"{config["groupnum"]}";
                        lbUseNum.Text = $"{config["playtimegroup"]}";
                        if (bot != null)
                        {
                            lbFriendNum.Text = $"{bot.friends.Count}";
                            lbGroupNum.Text = $"{bot.groups.Count}";
                        }
                        
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
            button1.Enabled = false;

            new Thread(workRunMirai).Start();
            new Thread(workRunMMDK).Start();
            button1.Text = "已启动";
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

        private void 清空日志ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            tbMirai.Clear();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                config.save();
                if (MiraiProcess != null)
                {
                    bot.Exit();
                    SystemInfo.EndProcess(MiraiProcess.ProcessName);
                    SystemInfo.EndProcess("java");
                    MiraiProcess.Dispose();
                    //MiraiProcess.Kill();
                    //MiraiProcess.StandardInput.WriteLine("exit");
                }
                

                State = runState.exit;
                
                //Environment.Exit(0);
            }
            catch
            {

            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            logMMDK("开始初始化配置文件。");

            config = new Config(configPath);
            bool isValid = checkAndSetConfigValid();
            if (!isValid)
            {
                logMMDK("配置文件读取失败，中止运行");
                return;
            }


            logMMDK("配置文件读取完毕。");

            new Thread(workMonitor).Start();
        }
    }
}
