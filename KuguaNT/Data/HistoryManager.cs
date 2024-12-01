
using System.Text;
using System.Timers;

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

                MessageHistoryGroup.maxWriteDate = DateTime.Now.AddHours(-1);
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
        /// <returns></returns>
        public MessageHistory[] findMessage(string userId, string groupId, string keyWord = "")
        {
            List<MessageHistory> results = new List<MessageHistory>();

            try
            {
                //Logger.Log($"=FIND={userId},{groupId},{(history.ContainsKey($"G{groupId}"))}");
                if (history.TryGetValue(string.IsNullOrWhiteSpace(groupId)?$"P{userId}":$"G{groupId}", out MessageHistoryGroup g))
                {
                    
                    //var lines = g.history.ToArray();
                    //return lines;

                    foreach (var line in g.history)
                    {
                        //Logger.Log($"=FIND={line.messageId},{line.userid},{line.message}");

                        if (line.userid == userId)
                        {
                            if (string.IsNullOrWhiteSpace(keyWord) || line.message.Contains(keyWord)) results.Add(line);
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
    }
}
