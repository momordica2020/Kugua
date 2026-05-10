using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Text;


namespace Kugua
{
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
}
