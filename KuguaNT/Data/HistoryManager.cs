
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace Kugua
{



    /// <summary>
    /// 管理历史记录数据
    /// </summary>
    public class HistoryManager
    {
        private static readonly Lazy<HistoryManager> instance = new Lazy<HistoryManager>(() => new HistoryManager());

        public string path;

        public System.Timers.Timer writeHistoryTask;
        //public object savemsgMutex = new object();

        Dictionary<string, MessageHistoryGroup> history = new Dictionary<string, MessageHistoryGroup>();

        public static string pathGroup = "group";
        public static string pathPrivate = "private";

        private HistoryManager()
        {

        }

        public static HistoryManager Instance => instance.Value;

        public void Init(string _path)
        {
            path = _path;

            try
            {
                string path = Config.Instance.ResourceFullPath("HistoryPath");
                if (!Directory.Exists(path))
                {
                    Logger.Log($"新建历史记录文件夹，路径是{path}", LogType.Debug);
                    Directory.CreateDirectory(path);
                }
                if (!Directory.Exists($"{path}/{pathGroup}")) Directory.CreateDirectory($"{path}/{pathGroup}");
                if (!Directory.Exists($"{path}/{pathPrivate}")) Directory.CreateDirectory($"{path}/{pathPrivate}");
            }
            catch (Exception e)
            {

                Logger.Log(e, LogType.System);
            }


            // 每30秒一归档
            writeHistoryTask = new System.Timers.Timer(1000 * 30);
            writeHistoryTask.Start();
            writeHistoryTask.Elapsed += workDealHistory;
        }

        public void Dispose()
        {
            if (writeHistoryTask != null)
            {
                writeHistoryTask.Stop();     // 停止定时器
                writeHistoryTask.Dispose();


                // 立即把没归档的都归档
                MessageHistoryGroup.maxWriteDate = DateTime.Now.AddHours(1);
                workDealHistory(null, null);
            }

        }

        private void workDealHistory(object sender, ElapsedEventArgs e)
        {

            try
            {
                // 帮助配置文件一起定期存掉
                Config.Instance.Save();




                //Logger.Log($"把日期前的聊天记录归档中：{MessageHistoryGroup.maxWriteDate.ToString("F")}", LogType.Debug);
                var items = history.Values.AsParallel().ToArray();
                //var items = history.Values.ToArray();
                for (int i = 0; i < items.Length; i++)
                {
                    items[i].write();
                }

                MessageHistoryGroup.maxWriteDate = DateTime.Now.AddMinutes(-5);
            }
            catch (Exception ex)
            {
                Logger.Log(ex, LogType.Debug);
            }

        }



        /// <summary>
        /// 记录群/私人聊天信息到文件中
        /// </summary>
        /// <param name="sourceId">MessageId</param>
        /// <param name="group"></param>
        /// <param name="user"></param>
        /// <param name="msg"></param>
        public void saveMsg(string sourceId, string group, string user, string msg)
        {
            try
            {
                bool isGroup = !string.IsNullOrWhiteSpace(group);
                string uid = isGroup ? group : user;
                string key = isGroup ? $"G{group}" : $"P{user}";
                //Logger.Log($"=SAVE={key},{sourceId},{user},{msg}");
                if (!history.ContainsKey(key)) history[key] = new MessageHistoryGroup(path, uid, isGroup);
                history[key].addMessage(sourceId, user, msg);


            }
            catch (Exception ex)
            {
                Logger.Log(ex, LogType.Debug);
            }
        }


        /// <summary>
        /// 搜索历史记录
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="groupId"></param>
        /// <param name="keyWord"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        public MessageHistory[] findMessage(string userId, string groupId, string keyWord = "", int maxCount=10)
        {
            List<MessageHistory> results = new List<MessageHistory>();

            try
            {
                //Logger.Log($"=FIND={userId},{groupId},{(history.ContainsKey($"G{groupId}"))}");
                if (history.TryGetValue(string.IsNullOrWhiteSpace(groupId)?$"P{userId}":$"G{groupId}", out MessageHistoryGroup g))
                {

                    //var lines = g.history.ToArray();
                    //return lines;
                    int i = 1;
                    foreach (var line in g.history)
                    {
                        //Logger.Log($"=FIND={line.messageId},{line.userid},{line.message}");

                        if (line.userid == userId)
                        {
                            if (string.IsNullOrWhiteSpace(keyWord) || line.message.Contains(keyWord))
                            {
                                results.Add(line);
                                if (i++ > maxCount) break;
                            }
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                Logger.Log(ex, LogType.Debug);
            }

            return results.ToArray();
        }

        public static List<MessageHistory> GetGroupMessageFromFile(string groupId)
        {
            var historys = new List<MessageHistory>();
            var files = new List<string>();
            string historyPath = Path.GetFullPath($"{Config.Instance.ResourceFullPath("HistoryPath")}/group");
            string historyPath2 = Path.GetFullPath($"{Config.Instance.ResourceFullPath("HistoryPath")}/group2");

            if (Directory.Exists(historyPath))
            {
                files.AddRange(Directory.GetFiles(historyPath, $"{groupId}.txt"));
            }
            if (Directory.Exists(historyPath2))
            {
                files.AddRange(Directory.GetFiles(historyPath2, $"{groupId}.txt"));
            }
            foreach (var file in files)
            {
                foreach(var line in LocalStorage.ReadLines(file))
                {
                    var items = line.Trim().Split('\t');
                    if(items.Length >=3)historys.Add(new MessageHistory { date = DateTime.Parse(items[0].Replace("_"," ")), userid = items[1], message = items[2] });
                }
            }
            

            return historys;
        }

        public MessageHistory[] Search(string groupId, string input)
        {
            string[] chineseDays = { "星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六" };
            var keywords = input.Split(new[] { '\r', '\n', ' ', '，', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                           .Select(k => k.Trim())
                           .ToList(); 
            var keywordPatterns = keywords.Select(k => $@"{Regex.Escape(k)}").ToList();
            
            var historys = GetGroupMessageFromFile(groupId);
            if (historys.Count > 0)
            {
                //var lines = g.history.ToArray();
                //return lines;
                var matchres = historys.Select(line =>
                {
                    int matchCount = keywordPatterns.Count(pattern =>
                    Regex.IsMatch($"{line.date.ToString("yyyy-MM-dd")} {chineseDays[((int)line.date.DayOfWeek)]} {line.userid} {Config.Instance.UserInfo(line.userid).Name} {line.message}", pattern, RegexOptions.IgnoreCase));
                    return new { Content = line, MatchCount = matchCount };
                });
                return matchres
                    .Where(m => m.MatchCount > 0)
                    .OrderByDescending(r => r.MatchCount)
                    .ThenByDescending(r => r.Content.date)
                    .Select(r => r.Content)
                    .ToArray();
            }
           

            // empty
            return new List<MessageHistory>().ToArray();
        }
    }
}
