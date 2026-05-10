using System.Diagnostics;


namespace Kugua
{
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
}
