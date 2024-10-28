using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MeowMiraiLib;
using MeowMiraiLib.Msg.Type;
using MMDK.Util;

namespace MMDK.Mods
{
    internal class ModTimerTask : Mod, ModWithMirai
    {
        class MyTask
        {
            public long UserId;
            public long GroupId;
            public System.Timers.Timer TaskTimer;


        }


        MeowMiraiLib.Client client;


        List<MyTask> tasks = new List<MyTask>();

        public void Exit()
        {
        }
        public bool Init(string[] args)
        {
            return true;
        }
        public void InitMiraiClient(Client _client)
        {
            client = _client;
        }
        public bool HandleText(long userId, long groupId, string message, List<string> results)
        {
            bool isGroup = groupId > 0;
            
            if (string.IsNullOrWhiteSpace(message)) return false;
            try
            {
                if (isGroup)
                {
                    var regex = new Regex(@"^帮我撤回(\d{1,2})条?");
                    var match = regex.Match(message);
                    if (match.Success)
                    {
                        new MeowMiraiLib.Msg.GroupMessage(groupId, [
                            new At(userId, ""), 
                            new Plain($"?")
                            ]).Send(client);
                        return true;
                    }


                    regex = new Regex(@"^(\d{1,2})[:：点](\d{1,2})(分)?叫我");
                    match = regex.Match(message);
                    if (match.Success)
                    {
                        string hourString = match.Groups[1].Value;
                        string minuteString = match.Groups[2].Value;
                        int hour = int.Parse(hourString);
                        int minute = int.Parse(minuteString);
                        DateTime alertTime = DateTime.Today.AddHours(hour).AddMinutes(minute);
                        if (alertTime < DateTime.Now) alertTime.AddDays(1);

                        MyTask newTask = new MyTask
                        {
                            UserId = userId,
                            GroupId = groupId,
                            TaskTimer = new(1000 * 10),
                        };

                        new MeowMiraiLib.Msg.GroupMessage(groupId, [
                            new At(userId, ""), 
                            new Plain($"帮你设了{alertTime.ToString("dd号HH点mm分")}的闹钟")
                            ]).Send(client);


                        newTask.TaskTimer.AutoReset = false; // 设置为非自动重置
                        newTask.TaskTimer.Start(); //启动计时器
                        newTask.TaskTimer.Elapsed += (s, e) =>
                        {
                            if (DateTime.Now >= alertTime) //检测当前的分钟是否为0 (整点)
                            {
                                new MeowMiraiLib.Msg.GroupMessage(groupId, [
                                    new At(userId, ""), 
                                    new Plain($"{DateTime.Now.ToString("HH:mm")}啦!")
                                    ]).Send(client);
                                //new MeowMiraiLib.Msg.FriendMessage(userId, new Message[] { new Plain($"{DateTime.Now.Hour}点 啦!") }).Send(c);

                            }
                        };

                        tasks.Add(newTask);
                        return true;
                    }

                    regex = new Regex(@"^别叫[了我]?");
                    match = regex.Match(message);
                    if (match.Success)
                    {
                        int haveTask = 0;
                        for (int i = tasks.Count - 1; i >= 0; i--)
                        {
                            var task = tasks[i];
                            if (task.UserId == userId && task.GroupId == groupId)
                            {
                                task.TaskTimer.Stop();
                                haveTask ++;
                                tasks.RemoveAt(i); // 倒序删除元素不会影响遍历
                            }
                        }
                        if (haveTask > 0)
                        {
                            new MeowMiraiLib.Msg.GroupMessage(groupId, [
                                new At(userId, ""),
                                new Plain($"彳亍!{haveTask}个闹钟以取消")
                                ]).Send(client);
                            return true;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.Instance.Log(ex);
            }

            return false;

        }




    }
}
