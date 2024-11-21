using System.Text;

namespace Kugua
{

    /// <summary>
    /// 单条历史记录
    /// </summary>
    public class MessageHistory
    {
        public string messageId;      // 只存入内存，用于at和回撤等操作。
        public string userid;
        public string message;
        public DateTime date;

        public MessageHistory()
        {
            userid = "";
            message = "";
        }

        public MessageHistory(string _messageId, string _uid, string _message)
        {
            date = DateTime.Now;
            messageId = _messageId;
            userid = _uid;
            message = _message;
        }

        public override string ToString()
        {
            return $"{date:yyyy-MM-dd_HH:mm:ss}\t{userid}\t{message}";
        }
    }



    /// <summary>
    /// 某个人或组的全部历史记录
    /// </summary>
    public class MessageHistoryGroup
    {
        public string filePath = "";
        public string uid;
        public bool isGroup;
        public Queue<MessageHistory> history = new Queue<MessageHistory>();

        public MessageHistoryGroup(string _rootpath, string _gid, bool _isGroup)
        {
            isGroup = _isGroup;
            uid = _gid;
            filePath = $"{_rootpath}/{(isGroup ? HistoryManager.pathGroup : HistoryManager.pathPrivate)}/{_gid}.txt";
        }

        public void addMessage(string messageId, string user, string message)
        {
            MessageHistory h = new MessageHistory(messageId, user, message);
            history.Enqueue(h);
        }


        static readonly int maxWriteTime = 1000;
        public static DateTime maxWriteDate;
        public void write()
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                int nowtime = 0;
                while (history.Count > 0)
                {
                    // 只把日期老于maxWriteDate的历史记录归档
                    var checkpoint = history.Peek();
                    if (checkpoint.date > maxWriteDate) break;

                    sb.AppendLine(history.Dequeue().ToString());

                    // 每次不写入超过maxWriteTime条
                    if (nowtime++ >= maxWriteTime) break;
                }
                if (sb.Length > 0)
                {
                    File.AppendAllText(filePath, sb.ToString(), Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }

}
