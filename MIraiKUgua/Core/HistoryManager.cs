using MMDK.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MMDK.Core
{
    public class HistoryManager
    {
        public bool run = false;
        public string path;

        public Thread DataSavingThread;
        public object savemsgMutex = new object();

        Dictionary<string, MessageHistoryGroup> history = new Dictionary<string, MessageHistoryGroup>();

        string pathGroup = "group/";
        string pathPrivate = "private/";

        public void init(string _path)
        {
            path = _path;

            try
            {
                if (!path.EndsWith("/")) path = path + "/";
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                if (!Directory.Exists(path + pathGroup)) Directory.CreateDirectory(path + pathGroup);
                if (!Directory.Exists(path + pathPrivate)) Directory.CreateDirectory(path + pathPrivate);
            }
            catch
            {

            }


            run = true;

            DataSavingThread = new Thread(workDealHistory);
            DataSavingThread.Start();
        }

        public void workDealHistory()
        {
            while (run)
            {
                try
                {
                    var items = history.Values.AsParallel().ToArray();
                    //var items = history.Values.ToArray();
                    for (int i = 0; i < items.Length; i++)
                    {
                        try
                        {
                            items[i].write();
                        }
                        catch
                        {

                        }
                    }
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    FileHelper.Log(ex);
                }
            }
        }



        /// <summary>
        /// 记录群/私人聊天信息到文件中
        /// </summary>
        /// <param name="group"></param>
        /// <param name="user"></param>
        /// <param name="msg"></param>
        public void saveMsg(long group, long user, string msg)
        {
            try
            {
                bool isGroup = (group <= 0 ? false : true);
                long uid = (isGroup ? group : user);
                string key = (isGroup ? $"G{group}" : $"P{user}");

                if (!history.ContainsKey(key)) history[key] = new MessageHistoryGroup(path, uid, isGroup);
                history[key].addMessage(user, msg);


            }
            catch (Exception ex)
            {
                FileHelper.Log(ex);
            }
        }
    }
}
