using System;

using System.IO;


namespace MMDK.Util
{
    enum LogType
    {
        System,
        Virtual,
        Mirai,
        Debug
    }

    class Logger
    {
        private static readonly Lazy<Logger> instance = new Lazy<Logger>(() => new Logger());
        private readonly StreamWriter writer;
        private static readonly object lockObject = new object(); // 用于线程安全


        public static readonly string logFilePath = "application.log";


        //public Logger(string logFilePath)
        //{
        //    writer = new StreamWriter(logFilePath, true); // 以追加模式打开
        //}

        // 私有构造函数
        private Logger()
        {
            writer = new StreamWriter(logFilePath, true) { AutoFlush = true }; // 以追加模式打开文件
        }

        // 公共静态属性获取实例
        public static Logger Instance => instance.Value;

        // 记录日志
        public void Log(string message, LogType logType = LogType.System)
        {
            lock (lockObject) // 确保线程安全
            {
                try
                {
                    writer.WriteLine($"[{DateTime.Now:G}][{GetLogTypeName(logType)}]{message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
                }
            }
        }
        public void Log(Exception ex, LogType logType = LogType.System)
        {
            Log($"{ex.Message}\r\n{ex.StackTrace}", logType);
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
                default: return "未知";
            }
        }




    }
}
