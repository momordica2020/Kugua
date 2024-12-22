using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Kugua
{

    /// <summary>
    /// 单条历史记录
    /// </summary>
    public class HistoryItem
    {
        /// <summary>
        /// 只存入内存，用于引用和回撤等操作。
        /// </summary>
        public string MessageId; 

        /// <summary>
        /// 发送者qq号
        /// </summary>
        public string UserId;

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Content;

        /// <summary>
        /// 发送日期
        /// </summary>
        public DateTime RecvDate;

        /// <summary>
        /// 是否被保存过
        /// </summary>
        public bool IsSaved = false;

        public HistoryItem()
        {
            UserId = "";
            Content = "";
        }

        public HistoryItem(string _messageId, string _uid, string _message)
        {
            RecvDate = DateTime.Now;
            MessageId = _messageId;
            UserId = _uid;
            Content = _message;
        }

        public override string ToString()
        {
            return $"{RecvDate:yyyy-MM-dd_HH:mm:ss}\t{UserId}\t{Content}";
        }
    }



    /// <summary>
    /// 某个人或组的全部历史记录
    /// </summary>
    public class HistoryStorage
    {
        /// <summary>
        /// 上次存档日期
        /// </summary>
        DateTime lastSaveDate;

        string FilePath = "";

        /// <summary>
        /// 标记，群号或私聊对象qq号
        /// </summary>
        public string Uid;

        /// <summary>
        /// 是否群组？
        /// </summary>
        public bool IsGroup;

        /// <summary>
        /// 历史消息数据
        /// </summary>
        public List<HistoryItem> History = new List<HistoryItem>();
        //public Queue<MessageHistory> historyForCallback = new Queue<MessageHistory>();

        public HistoryStorage(string _rootpath, string _gid, bool _isGroup)
        {
            IsGroup = _isGroup;
            Uid = _gid;
            FilePath = $"{_rootpath}/{(IsGroup ? HistoryManager.pathGroup : HistoryManager.pathPrivate)}/{_gid}.txt";
        }

        public void Add(HistoryItem h)
        {
            //HistoryItem h = new HistoryItem(messageId, user, message);
            lock (History)
            {
                History.Add(h);
            }
        }


        public void Save(bool force=false)
        {
            try
            {
                if (!History.Any()) return;

                lock (History)
                {
                    var res =
                        History
                        .Where(h => !h.IsSaved && (force || h.RecvDate < lastSaveDate + HistoryManager.SaveSpan))
                        .Take(HistoryManager.maxWriteNum);
                    File.AppendAllLines(FilePath, res.Select(h => h.ToString()), Encoding.UTF8);
                    foreach (HistoryItem h in res) h.IsSaved = true;
                    //Logger.Log($"SAVE=>{Uid} - {res.Count()} - {DateTime.Now}");

                    if (History.Count > HistoryManager.maxCapacity)
                    {
                        // 移除最早的对象
                        // History.Sort((x, y) => x.date.CompareTo(y.date));
                        int itemsToRemove = History.Count - HistoryManager.trimCapacity;
                        History.RemoveRange(0, itemsToRemove);
                        //Logger.Log($"Trimmed {itemsToRemove} items from memory.");
                    }
                }
                lastSaveDate = DateTime.Now;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }

}
