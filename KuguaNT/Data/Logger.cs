using Microsoft.AspNetCore.SignalR;
using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;


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
    public enum LogLevel
    {
        Nope,
        System,
        Debug,
    }
    public static class StackTraceCleaner
    {
        private static readonly HashSet<string> NoisePatterns = new(StringComparer.OrdinalIgnoreCase)
    {
        "MoveNext",                 // 状态机
        "InnerInvoke",              // Task
        "<.ctor>b__",               // 闭包/匿名方法
        "Start",                    // Task.Start
        "ExecuteWithThreadLocal",
        "RunFromThreadPoolDispatchLoop",
        "Dispatch",                 // 调度器
        "AwaitUnsafeOnCompleted",
        "AwaitOnCompleted",
        "SetResult", "SetException", // TaskCompletionSource
        "Continuation",             // 各种 continuation
        "RunContinuations",         // Task
        "ExecutionContext",         // ExecutionContext.Run
        "ThreadHelper",             // ThreadPool
        "ThreadPoolWorkQueue",      // ThreadPool
        "ValueTask", "IValueTaskSource",  // .NET 8+ 常见
        "AwaitTaskContinuation",
        "AsyncTaskMethodBuilder",
        "ThreadPoolTypedWorkItemQueue",
        "Callback",
        "$",
    };

        private static readonly HashSet<string> NoiseDeclaringTypes = new(StringComparer.Ordinal)
    {
        //"System.Threading.Tasks.AwaitTaskContinuation",
        //"System.Threading.Tasks.TaskContinuation",
        //"System.Threading.Tasks.Task`1",
        //"System.Runtime.CompilerServices.AsyncTaskMethodBuilder",
        //"System.Runtime.CompilerServices.AsyncValueTaskMethodBuilder",
        //"System.Threading.ExecutionContext",
        //"System.Threading.ThreadPool",
        //"System.Threading.Tasks.ValueTask",
        "ThreadPoolTypedWorkItemQueue",
        "System.Threading",
        "System.Runtime",
        "Timer",
        "Log",
    };

        //public static string GetCleanCallStack(this Exception ex, int maxFrames = 20)
        //{
        //    var frames = new StackTrace(ex, true).GetFrames()
        //        .Take(maxFrames)
        //        .Where(f => f != null)
        //        .Select(f => f.GetMethod())
        //        .Where(m => m != null)
        //        .Where(m =>
        //        {
        //            string name = m.Name;
        //            string typeName = m.DeclaringType?.FullName ?? "";

        //            // 跳过噪音方法名
        //            if (NoisePatterns.Any(p => name.Contains(p))) return false;

        //            // 跳过噪音类型
        //            if (NoiseDeclaringTypes.Any(p => name.StartsWith(p))) return false;

        //            // 跳过编译器生成的异步状态机类型（通常以 <>c__DisplayClass 或 StateMachine 结尾）
        //            if (typeName.Contains("<") && typeName.Contains(">")) return false;

        //            return true;
        //        })
        //        .Select(m => FormatMethod(m!))
        //        .ToList();

        //    return string.Join("→", frames);
        //}
        public static List<string> GetCleanCallStack(StackTrace stackTrace)
        {
            var frames = stackTrace.GetFrames()
                .Where(f => f != null)
                .Select(f => f.GetMethod())
                .Where(m => m != null)
                .Where(m =>
                {
                    string name = m.Name;
                    string typeName = m.DeclaringType?.FullName ?? "";
                    string typeName2 = m.DeclaringType?.Name ?? "";
                    // 跳过噪音方法名
                    if (NoisePatterns.Any(p => name.Contains(p))) return false;

                    // 跳过噪音类型
                    if (NoiseDeclaringTypes.Any(p => typeName.StartsWith(p))) return false;
                    if (NoiseDeclaringTypes.Any(p => typeName2.StartsWith(p))) return false;

                    // 跳过编译器生成的异步状态机类型（通常以 <>c__DisplayClass 或 StateMachine 结尾）
                    if (typeName.Contains("<") && typeName.Contains(">")) return false;

                    return true;
                })
                .Select(m => FormatMethod(m!))
                .Reverse()
                .ToList();

            return frames;// string.Join("→", frames);
        }

        private static string FormatMethod(MethodBase m)
        {
            var sb = new StringBuilder();

            if (m.DeclaringType != null)
            {
                string type = m.DeclaringType.Name;
                // 去掉状态机后缀
                int backtickIndex = type.IndexOf('`');
                if (backtickIndex > 0) type = type.Substring(0, backtickIndex);
                sb.Append(type).Append('.');
            }

            string name = m.Name;
            // 去掉编译器生成的匿名方法/闭包前缀
            if (name.StartsWith("<")) name = name.Split('>')[1].Split('`')[0];

            sb.Append(name);

            // 可选：加上参数类型签名（如果需要更精确区分重载）
            // var ps = m.GetParameters();
            // if (ps.Length > 0) sb.Append('(').Append(string.Join(",", ps.Select(p => ShortType(p.ParameterType)))).Append(')');

            return sb.ToString();
        }

        // private static string ShortType(Type t) => t.Name;  // 可进一步简化如 List`1 → List<T>
    }
    public class LogInfo
    {

        public string Message { get; set; }

        public LogType Type { get; set; }

        public DateTime HappendTime { get; set; }

        public string CallerName;

        public string CallerFile;

        public int CallerLine;

        private string GetCallerName
        {
            get
            {
                string name = "";

                StackTrace stackTrace = new StackTrace();

                ////// 遍历堆栈帧
                //for (int i = stackTrace.FrameCount-2; i>= 2; i--)
                //{
                //    StackFrame frame = stackTrace.GetFrame(i);
                //    var methodName = frame.GetMethod().Name;
                //    if (methodName.EndsWith("Log")) continue;
                //    name += $"{methodName}=>";
                //    //Console.WriteLine($"Frame {i}: {frame.GetMethod().Name}");
                //}
                //name = name.TrimEnd('=', '>');

                var funcList = StackTraceCleaner.GetCleanCallStack(stackTrace);
                try
                {
                    funcList.RemoveAt(funcList.Count - 1); // 移除当前函数    
                    funcList.RemoveAll(f => f.EndsWith("Log"));
                    name = funcList.Last();
                }
                catch { }
                if (!string.IsNullOrWhiteSpace(name))
                {
                    name = $"[{name}]";
                }

                //name = string.Join("=>", funcList);
                //// 获取调用当前函数的上一个函数
                //if (stackTrace.FrameCount > 1)
                //{
                //    StackFrame callerFrame = stackTrace.GetFrame(1); // 1 表示上一帧
                //    string callerMethodName = callerFrame.GetMethod().Name;
                //    Console.WriteLine($"Caller method name: {callerMethodName}");
                //}
                //name = new StackTrace().GetFrame(6).GetMethod().Name;
                return name;
            }
        }

        public string ToDescription()
        {
            string msg = $"[{HappendTime}][{Logger.GetLogTypeName(Type)}]{GetCallerName}{Message}";
            //string msg = $"[{HappendTime}][{Logger.GetLogTypeName(Type)}][{CallerName}]{Message}";
            return msg;
        }

    }
    public class Logger
    {

        private static readonly Lazy<Logger> instance = new Lazy<Logger>(() => new Logger());
        private readonly StreamWriter writer;
        private static readonly object lockObject = new object(); // 用于线程安全

        // 当前日志等级
        public static LogLevel logLevel = LogLevel.System;

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
                    if (logLevel == LogLevel.Nope)
                    {
                        return;
                    }
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
