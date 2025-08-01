﻿using NvAPIWrapper.GPU;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;

namespace Kugua
{
    ///  
    /// 系统信息类 - 获取CPU、内存、磁盘、进程信息 
    ///  
    public class SystemInfo
    {
        private int m_ProcessorCount = 0;   //CPU个数 
        private PerformanceCounter pcCpuLoad;   //CPU计数器 
        private long m_PhysicalMemory = 0;   //物理内存 

        private const int GW_HWNDFIRST = 0;
        private const int GW_HWNDNEXT = 2;
        private const int GWL_STYLE = (-16);
        private const int WS_VISIBLE = 268435456;
        private const int WS_BORDER = 8388608;

        #region AIP声明 
        [DllImport("IpHlpApi.dll")]
        extern static public uint GetIfTable(byte[] pIfTable, ref uint pdwSize, bool bOrder);

        [DllImport("User32")]
        private extern static int GetWindow(int hWnd, int wCmd);

        [DllImport("User32")]
        private extern static int GetWindowLongA(int hWnd, int wIndx);

        [DllImport("user32.dll")]
        private static extern bool GetWindowText(int hWnd, StringBuilder title, int maxBufSize);

        [DllImport("user32", CharSet = CharSet.Auto)]
        private extern static int GetWindowTextLength(IntPtr hWnd);
        #endregion

        #region 构造函数 
        ///  
        /// 构造函数，初始化计数器等 
        ///  
        public SystemInfo()
        {
            try
            {
                //初始化CPU计数器 
                pcCpuLoad = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                pcCpuLoad.MachineName = ".";
                pcCpuLoad.NextValue();

                //CPU个数 
                m_ProcessorCount = Environment.ProcessorCount;

                //获得物理内存 
                ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if (mo["TotalPhysicalMemory"] != null)
                    {
                        m_PhysicalMemory = long.Parse(mo["TotalPhysicalMemory"].ToString());
                    }
                }
            }
           catch(Exception ex)
            {
                Logger.Log(ex);
            }
        }
        #endregion

        #region CPU个数 
        ///  
        /// 获取CPU个数 
        ///  
        public int ProcessorCount
        {
            get
            {
                return m_ProcessorCount;
            }
        }
        #endregion

        #region CPU占用率 
        ///  
        /// 获取CPU占用率 
        ///  
        public float CpuLoad
        {
            get
            {
                return pcCpuLoad.NextValue();
            }
        }
        #endregion

        #region 可用内存 
        ///  
        /// 获取可用内存 
        ///  
        public long MemoryAvailable
        {
            get
            {
                long availablebytes = 0;
                //ManagementObjectSearcher mos = new ManagementObjectSearcher("SELECT * FROM Win32_PerfRawData_PerfOS_Memory"); 
                //foreach (ManagementObject mo in mos.Get()) 
                //{ 
                //    availablebytes = long.Parse(mo["Availablebytes"].ToString()); 
                //} 
                ManagementClass mos = new ManagementClass("Win32_OperatingSystem");
                foreach (ManagementObject mo in mos.GetInstances())
                {
                    if (mo["FreePhysicalMemory"] != null)
                    {
                        availablebytes = 1024 * long.Parse(mo["FreePhysicalMemory"].ToString());
                    }
                }
                return availablebytes;
            }
        }
        #endregion

        #region 物理内存 
        ///  
        /// 获取物理内存 
        ///  
        public long PhysicalMemory
        {
            get
            {
                return m_PhysicalMemory;
            }
        }
        #endregion

        //#region 获得分区信息 
        /////  
        ///// 获取分区信息 
        /////  
        //public List GetLogicalDrives()
        //{
        //    List drives = new List();
        //    ManagementClass diskClass = new ManagementClass("Win32_LogicalDisk");
        //    ManagementObjectCollection disks = diskClass.GetInstances();
        //    foreach (ManagementObject disk in disks)
        //    {
        //        // DriveType.Fixed 为固定磁盘(硬盘) 
        //        if (int.Parse(disk["DriveType"].ToString()) == (int)DriveType.Fixed)
        //        {
        //            drives.Add(new DiskInfo(disk["Name"].ToString(), long.Parse(disk["Size"].ToString()), long.Parse(disk["FreeSpace"].ToString())));
        //        }
        //    }
        //    return drives;
        //}
        /////  
        ///// 获取特定分区信息 
        /////  
        ///// 盘符 
        //public List GetLogicalDrives(char DriverID)
        //{
        //    List drives = new List();
        //    WqlObjectQuery wmiquery = new WqlObjectQuery("SELECT * FROM Win32_LogicalDisk WHERE DeviceID = ’" + DriverID + ":’");
        //    ManagementObjectSearcher wmifind = new ManagementObjectSearcher(wmiquery);
        //    foreach (ManagementObject disk in wmifind.Get())
        //    {
        //        if (int.Parse(disk["DriveType"].ToString()) == (int)DriveType.Fixed)
        //        {
        //            drives.Add(new DiskInfo(disk["Name"].ToString(), long.Parse(disk["Size"].ToString()), long.Parse(disk["FreeSpace"].ToString())));
        //        }
        //    }
        //    return drives;
        //}
        //#endregion

        //#region 获得进程列表 
        /////  
        ///// 获得进程列表 
        /////  
        //public List GetProcessInfo()
        //{
        //    List pInfo = new List();
        //    Process[] processes = Process.GetProcesses();
        //    foreach (Process instance in processes)
        //    {
        //        try
        //        {
        //            pInfo.Add(new ProcessInfo(instance.Id,
        //                instance.ProcessName,
        //                instance.TotalProcessorTime.TotalMilliseconds,
        //                instance.WorkingSet64,
        //                instance.MainModule.FileName));
        //        }
        //        catch { }
        //    }
        //    return pInfo;
        //}
        /////  
        ///// 获得特定进程信息 
        /////  
        ///// 进程名称 
        //public List GetProcessInfo(string ProcessName)
        //{
        //    List pInfo = new List();
        //    Process[] processes = Process.GetProcessesByName(ProcessName);
        //    foreach (Process instance in processes)
        //    {
        //        try
        //        {
        //            pInfo.Add(new ProcessInfo(instance.Id,
        //                instance.ProcessName,
        //                instance.TotalProcessorTime.TotalMilliseconds,
        //                instance.WorkingSet64,
        //                instance.MainModule.FileName));
        //        }
        //        catch { }
        //    }
        //    return pInfo;
        //}
        //#endregion

        #region 结束指定进程 
        ///  
        /// 结束指定进程 
        ///  
        /// 进程的 Process ID 
        public static void EndProcess(int pid)
        {
            try
            {
                Process process = Process.GetProcessById(pid);
                process.Kill();
            }
            catch { }
        }


        public static void EndProcess(string processName)
        {
            string res = RunCmd($"taskkill /im {processName}.exe /f ");
            Console.WriteLine(res);
        }


        #endregion


        public static string RunCmd(string command)
        {
            //實例一個Process類，啟動一個獨立進程   
            System.Diagnostics.Process p = new System.Diagnostics.Process();

            //Process類有一個StartInfo屬性，這個是ProcessStartInfo類，包括了一些屬性和方法，下面我們用到了他的幾個屬性：   

            p.StartInfo.FileName = "cmd.exe";           //設定程序名   
            p.StartInfo.Arguments = "/c " + command;    //設定程式執行參數   
            p.StartInfo.UseShellExecute = false;        //關閉Shell的使用   
            p.StartInfo.RedirectStandardInput = true;   //重定向標準輸入   
            p.StartInfo.RedirectStandardOutput = true;  //重定向標準輸出   
            p.StartInfo.RedirectStandardError = true;   //重定向錯誤輸出   
            p.StartInfo.CreateNoWindow = true;          //設置不顯示窗口   

            p.Start();   //啟動   

            //p.StandardInput.WriteLine(command);       //也可以用這種方式輸入要執行的命令   
            //p.StandardInput.WriteLine("exit");        //不過要記得加上Exit要不然下一行程式執行的時候會當機   

            return p.StandardOutput.ReadToEnd();        //從輸出流取得命令執行結果   

        }


        //#region 查找所有应用程序标题 
        /////  
        ///// 查找所有应用程序标题 
        /////  
        ///// 应用程序标题范型 
        //public static List FindAllApps(int Handle)
        //{
        //    List Apps = new List();

        //    int hwCurr;
        //    hwCurr = GetWindow(Handle, GW_HWNDFIRST);

        //    while (hwCurr > 0)
        //    {
        //        int IsTask = (WS_VISIBLE | WS_BORDER);
        //        int lngStyle = GetWindowLongA(hwCurr, GWL_STYLE);
        //        bool TaskWindow = ((lngStyle & IsTask) == IsTask);
        //        if (TaskWindow)
        //        {
        //            int length = GetWindowTextLength(new IntPtr(hwCurr));
        //            StringBuilder sb = new StringBuilder(2 * length + 1);
        //            GetWindowText(hwCurr, sb, sb.Capacity);
        //            string strTitle = sb.ToString();
        //            if (!string.IsNullOrEmpty(strTitle))
        //            {
        //                Apps.Add(strTitle);
        //            }
        //        }
        //        hwCurr = GetWindow(hwCurr, GW_HWNDNEXT);
        //    }

        //    return Apps;
        //}
        //#endregion












        /// <summary>
        /// 获取GPU当前状态描述
        /// </summary>
        /// <returns></returns>
        public static string GetNvidiaGpuAndMemoryUsage()
        {
            // 获取物理 GPU 的第一个实例
            var gpu = PhysicalGPU.GetPhysicalGPUs()[0];

            // 获取 GPU 使用率
            var utilization = gpu.UsageInformation;
            int gpuUsagePercent = utilization.GPU.Percentage; // 使用 GPU.Usage 获取百分比

            // 获取显存使用率
            var memoryInfo = gpu.MemoryInformation;
            float totalMemory = memoryInfo.AvailableDedicatedVideoMemoryInkB; // 总显存（MB）
            float unusedMemory = memoryInfo.CurrentAvailableDedicatedVideoMemoryInkB; // 已用显存（MB）
            float memoryUsagePercent = (1 - (unusedMemory / totalMemory)) * 100;

            // 格式化成字符串
            return $"GPU({gpuUsagePercent.ToString(".0")}%) 显存({memoryUsagePercent.ToString(".0")}%)";
        }
    }
}
