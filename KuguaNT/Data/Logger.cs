using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;


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
    enum LogLevel
    {
        Nope,
        System,
        Debug,
    }

    public class LogInfo
    {

        public string Message { get; set; }

        public LogType Type { get; set; }

        public DateTime HappendTime { get; set; }

        public string ToDescription()
        {
            //StackTrace stackTrace = new StackTrace();

            ////// 遍历堆栈帧
            //for (int i = 0; i < stackTrace.FrameCount; i++)
            //{
            //    StackFrame frame = stackTrace.GetFrame(i);
            //    Console.WriteLine($"Frame {i}: {frame.GetMethod().Name}");
            //}

            //// 获取调用当前函数的上一个函数
            //if (stackTrace.FrameCount > 1)
            //{
            //    StackFrame callerFrame = stackTrace.GetFrame(1); // 1 表示上一帧
            //    string callerMethodName = callerFrame.GetMethod().Name;
            //    Console.WriteLine($"Caller method name: {callerMethodName}");
            //}

            string msg = $"[{HappendTime}][{Logger.GetLogTypeName(Type)}][{new StackTrace().GetFrame(4).GetMethod().Name}]{Message}";
            return msg;
        }

    }
    public class Logger
    {

        private static readonly Lazy<Logger> instance = new Lazy<Logger>(() => new Logger());
        private readonly StreamWriter writer;
        private static readonly object lockObject = new object(); // 用于线程安全

        // 当前日志等级
        static LogLevel logLevel = LogLevel.System;

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

        public static void Log(string message, LogType logType = LogType.System)
        {
            Instance.tLog(message, logType);
        }
        public static void Log(Exception ex, LogType logType = LogType.System)
        {
            Instance.tLog(ex, logType);
        }
        // 记录日志
        void tLog(string message, LogType logType = LogType.System)
        {
            lock (lockObject) // 确保线程安全
            {
                try
                {
                    if (logLevel == LogLevel.Nope)
                    {
                        return;
                    }
                    LogInfo info = new LogInfo
                    {
                        Message = message,
                        Type = logType,
                        HappendTime = DateTime.Now,
                    };
                    writer.WriteLine(info.ToDescription());



                    if (logType==LogType.Debug && logLevel == LogLevel.System)
                    {
                        // system模式下不在界面显示debug日志
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
