using Kugua.Core;
using System.Diagnostics;
using System.Management;

namespace Kugua.Integrations.NTBot
{
    public class NapcatManager
    {
        public static void RestartNapcat()
        {
            string batPath = Config.Instance.App.Net.NapcatBat;

            Logger.Log("正在尝试重启 NapCat 脚本...");

            // 1. 关闭当前运行的 bat
            KillSpecificBat(batPath);

            // 2. 稍微等待一下，确保资源释放
            System.Threading.Thread.Sleep(1000);

            // 3. 重新启动 bat
            RestartBat(batPath);

            Logger.Log("重启指令已发送。");
            Console.ReadLine();
        }

    /// <summary>
    /// 寻找并精准关闭指定路径的 Bat 进程
    /// </summary>
    static void KillSpecificBat(string batFilePath)
        {
            try
            {
                // 使用 WMI 查询所有名为 cmd.exe 的进程及其命令行参数
                string query = "SELECT ProcessId, CommandLine FROM Win32_Process WHERE Name = 'cmd.exe'";
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
                using (ManagementObjectCollection objects = searcher.Get())
                {
                    foreach (ManagementObject obj in objects)
                    {
                        string commandLine = obj["CommandLine"]?.ToString() ?? "";
                        int processId = Convert.ToInt32(obj["ProcessId"]);

                        // 检查这个 cmd.exe 的启动参数里是否包含我们要找的 bat 路径
                        if (commandLine.Contains(batFilePath, StringComparison.OrdinalIgnoreCase))
                        {
                            Logger.Log($"找到正在运行的目标进程 [PID: {processId}]，准备关闭...");
                            Process proc = Process.GetProcessById(processId);

                            // 强制结束 cmd 进程及其树下的子进程（比如 NapCatWinBootMain.exe）
                            proc.Kill(entireProcessTree: true);
                            Logger.Log("原进程及子程序已成功关闭。");
                            return;
                        }
                    }
                }
                Logger.Log("未发现正在运行的目标批处理进程。");
            }
            catch (Exception ex)
            {
                Logger.Log($"关闭进程时发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 启动指定路径的 Bat
        /// </summary>
        static void RestartBat(string batFilePath)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = batFilePath,
                    // 重要：设置工作目录为 bat 所在的文件夹，否则 bat 内部的相对路径（如 .\NapCatWinBootMain.exe）会失效！
                    WorkingDirectory = Path.GetDirectoryName(batFilePath),
                    UseShellExecute = true, // 必须为 true 才能启动 .bat
                    CreateNoWindow = false  // 如果你想看到黑窗口，设为 false；想后台隐藏运行，设为 true
                };

                Process.Start(startInfo);
                Logger.Log("新批处理窗口已成功启动！");
            }
            catch (Exception ex)
            {
                Logger.Log($"启动批处理时发生错误: {ex.Message}");
            }
        }
    }
}
