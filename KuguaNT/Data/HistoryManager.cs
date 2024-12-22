
using System.Security.Cryptography;
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
        /// <summary>
        /// 一次最多写入数量
        /// </summary>
        public static int maxWriteNum = 1000;

        /// <summary>
        /// 存档间隔
        /// </summary>
        public static TimeSpan SaveSpan = new TimeSpan(0, 5, 0);    // 5min


        /// <summary>
        /// 保留在内存中的撤回时间，早于该日期则清除内存
        /// </summary>
        public static DateTime minCallbackDate;

        /// <summary>
        /// 触发清理容量（触发内存清理的上限）
        /// </summary>
        public static int maxCapacity = 200;

        /// <summary>
        /// 内存保留容量（触发内存清理的下限）
        /// </summary>
        public static int trimCapacity = 10;


        private static readonly Lazy<HistoryManager> instance = new Lazy<HistoryManager>(() => new HistoryManager());

        public string path;
        public static string pathGroup = "group";
        public static string pathPrivate = "private";

       
        public System.Timers.Timer SaverTask;

        Dictionary<string, HistoryStorage> Storages = new Dictionary<string, HistoryStorage>();



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
            SaverTask = new System.Timers.Timer(1000 * 30);
            SaverTask.Start();
            SaverTask.Elapsed += workSaverTask;
        }

        public void Dispose()
        {
            if (SaverTask != null)
            {
                SaverTask.Stop();     // 停止定时器
                SaverTask.Dispose();


                // 立即把没归档的都归档
                
                workSaverTask(null, null);
            }

        }

        private void workSaverTask(object sender, ElapsedEventArgs e)
        {

            try
            {
                SaveAllToLocal();
            }
            catch (Exception ex)
            {
                Logger.Log(ex, LogType.Debug);
            }

        }

        public void SaveAllToLocal(bool force = false)
        {
            lock (Storages)
            {
                foreach (var storage in Storages)
                {
                    storage.Value.Save(force);
                }
            }
        }


        /// <summary>
        /// 记录群/私人聊天信息到文件中
        /// </summary>
        /// <param name="sourceId">MessageId</param>
        /// <param name="group"></param>
        /// <param name="user"></param>
        /// <param name="msg"></param>
        public void Add(string sourceId, string group, string user, string msg)
        {
            try
            {
                bool isGroup = !string.IsNullOrWhiteSpace(group);
                string uid = isGroup ? group : user;
                string key = isGroup ? $"G{group}" : $"P{user}";
                //Logger.Log($"=SAVE={key},{sourceId},{user},{msg}");
                if (!Storages.ContainsKey(key))
                {
                    lock (Storages)
                    {
                        Storages.Add(key, new HistoryStorage(path, uid, isGroup));
                    }
                }
                Storages[key].Add(new HistoryItem(sourceId, user, msg));
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
        public HistoryItem[] SearchByUser(string userId, string groupId, string keyWord = "", int maxCount=10)
        {
            List<HistoryItem> results = new List<HistoryItem>();

            try
            {
                //Logger.Log($"=FIND={userId},{groupId},{(history.ContainsKey($"G{groupId}"))}");
                string key = string.IsNullOrWhiteSpace(groupId) ? $"P{userId}" : $"G{groupId}";
                if (Storages.TryGetValue(key, out HistoryStorage g))
                {

                    //var lines = g.history.ToArray();
                    //return lines;
                    int i = 1;
                    foreach (var line in g.History)
                    {
                        //Logger.Log($"=FIND={line.messageId},{line.userid},{line.message}");

                        if (line.UserId == userId)
                        {
                            if (string.IsNullOrWhiteSpace(keyWord) || line.Content.Contains(keyWord))
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

        /// <summary>
        /// 找出特定群记录文件路径，如果不输入群号，返回全部记录文件路径
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static string[] GetGroupHistoryFiles(string groupId = "")
        {
            var files = new List<string>();
            string historyPath = Path.GetFullPath($"{Config.Instance.ResourceFullPath("HistoryPath")}/group");
            string historyPath2 = Path.GetFullPath($"{Config.Instance.ResourceFullPath("HistoryPath")}/group2");

            if (Directory.Exists(historyPath))
            {
                files.AddRange(Directory.GetFiles(historyPath, $"{(string.IsNullOrWhiteSpace(groupId) ? "*" : groupId)}.txt"));
            }
            if (Directory.Exists(historyPath2))
            {
                files.AddRange(Directory.GetFiles(historyPath2, $"{(string.IsNullOrWhiteSpace(groupId) ? "*" : groupId)}.txt"));
            }

            return files.ToArray();
        }

        /// <summary>
        /// 读取本地保存过的群聊历史记录
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static List<HistoryItem> GetGroupHistoryFromFile(string groupId)
        {
            var historys = new List<HistoryItem>();
            
            foreach (var file in GetGroupHistoryFiles(groupId))
            {
                try
                {
                    foreach (var line in LocalStorage.ReadLines(file))
                    {
                        var items = line.Split('\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        if (items.Length >= 3 && Regex.IsMatch(items[0], @"^\d{4}-\d{2}-\d{2}_\d{2}:\d{2}:\d{2}$"))
                        {
                            historys.Add(new HistoryItem
                            {
                                RecvDate = DateTime.Parse(items[0].Replace("_", " ")),
                                UserId = items[1],
                                Content = string.Join("\t", items.Skip(2))
                            });
                        }
                        else if (historys.Count > 0)
                        {
                            historys.Last().Content += "\r\n" + line;
                        }
                    }
                }catch (Exception e)
                {
                    Logger.Log(e);
                }

            }
            

            return historys;
        }



        /// <summary>
        /// 打开历史记录，不会是真的吧？
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public HistoryItem[] Search(string groupId, string input)
        {
            string[] chineseDays = { "星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六" };
            var keywords = input.Split(new[] { '\r', '\n', ' ', '，', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                           .Select(k => k.Trim())
                           .ToList(); 
            var keywordPatterns = keywords.Select(k => $@"{Regex.Escape(k)}").ToList();
            
            var historys = GetGroupHistoryFromFile(groupId);
            if (historys.Count > 0)
            {
                //var lines = g.history.ToArray();
                //return lines;
                var matchres = historys.Select(line =>
                {
                    int matchCount = keywordPatterns.Count(pattern =>
                    Regex.IsMatch($"{line.RecvDate.ToString("yyyy-MM-dd")} {chineseDays[((int)line.RecvDate.DayOfWeek)]} {line.UserId} {Config.Instance.UserInfo(line.UserId).Name} {line.Content}", pattern, RegexOptions.IgnoreCase));
                    return new { Content = line, MatchCount = matchCount };
                });
                return matchres
                    .Where(m => m.MatchCount > 0 && m.Content.UserId!=Config.Instance.BotQQ && !m.Content.Content.Contains("群内搜索"))
                    .OrderByDescending(r => r.MatchCount)
                    .ThenByDescending(r => r.Content.RecvDate)
                    .Select(r => r.Content)
                    .ToArray();
            }
           

            // empty
            return new List<HistoryItem>().ToArray();
        }
    }
}
