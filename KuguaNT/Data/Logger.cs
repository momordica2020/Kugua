using Microsoft.AspNetCore.SignalR;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;


namespace Kugua
{




    public enum LogType
    {
        System,
        Virtual,
        Mirai,
        Net,
        Debug
    }
    public class Logger
    {

        private static readonly Lazy<Logger> instance = new Lazy<Logger>(() => new Logger());
        private readonly StreamWriter writer;
        private static readonly object lockObject = new object(); // 用于线程安全

        // 当前日志等级
        public static LogType LogPrintType = LogType.Net;

        public readonly string logFilePath;

        public delegate void BroadcastLog(LogInfo info);

        public event BroadcastLog OnBroadcastLogEvent;

        //public Logger(string logFilePath)
        //{
        //    writer = new StreamWriter(logFilePath, true); // 以追加模式打开
        //}

        // 私有构造函数
        private Logger()
        {
            string logDict = $"{Directory.GetCurrentDirectory()}/LogNT";
            if (!Directory.Exists(logDict)) Directory.CreateDirectory(logDict);
            logFilePath = $"{logDict}/{DateTime.Today.ToString("yyyyMMdd")}.log";
            writer = new StreamWriter(logFilePath, true) { AutoFlush = true }; // 以追加模式打开文件

        }


        // 公共静态属性获取实例
        public static Logger Instance => instance.Value;

        public static void Log(
            string message, 
            LogType logType = LogType.System, 
            [CallerMemberName] string callerMethod = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0)
        {
            Instance.tLog(message, logType,callerMethod, callerFile,callerLine);
        }
        public static void Log(Exception ex, LogType logType = LogType.System)
        {
            Instance.tLog(ex, logType);
        }
        // 记录日志
        void tLog(string message, LogType logType = LogType.System, string CallerName = "", string CallerFile = "", int CallerLine=0)
        {
            lock (lockObject) // 确保线程安全
            {
                try
                {
                    LogInfo info = new LogInfo
                    {
                        Message = message,
                        Type = logType,
                        HappendTime = DateTime.Now,
                        CallerName = CallerName,
                        CallerFile = CallerFile,
                        CallerLine = CallerLine
                    };
                    writer.WriteLine(info.ToDescription());



                    if (logType > LogPrintType)
                    {
                        // 不显示更细节的日志类型
                        return ;
                    }
                    OnBroadcastLogEvent?.Invoke(info);

                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
                }
            }
        }
        void tLog(Exception ex, LogType logType = LogType.System)
        {
            Log($"{ex.Message}\r\n{ex.StackTrace}", logType);
            Config.Instance.ErrorNum++;
        }

        // 关闭日志文件
        public void Close()
        {
            lock (lockObject)
            {
                writer?.Close();
            }
        }


        public static string GetLogTypeName(LogType logType)
        {
            switch (logType)
            {
                case LogType.System: return "系统";
                case LogType.Mirai: return "Mirai组件";
                case LogType.Virtual: return "本地";
                case LogType.Debug: return "调试";
                case LogType.Net: return "网络";
                default: return "未知";
            }
        }




    }
}
