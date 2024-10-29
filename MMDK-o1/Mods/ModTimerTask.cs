using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChatGPT.Net.DTO.ChatGPT;
using ChatGPT.Net;
using MeowMiraiLib;
using MeowMiraiLib.GenericModel;
using MeowMiraiLib.Msg;
using MeowMiraiLib.Msg.Sender;
using MeowMiraiLib.Msg.Type;
using Microsoft.VisualBasic.ApplicationServices;
using MMDK.Util;

namespace MMDK.Mods
{
    internal class ModTimerTask : Mod, ModWithMirai
    {
        class MyTask
        {
            public long UserId;
            public long GroupId;
            public DateTime Time;
            public string Message;
            //public System.Timers.Timer TaskTimer;
        }

        System.Timers.Timer TaskTimer;

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


            TaskTimer = new(1000 * 10);
            TaskTimer.AutoReset = true;
            TaskTimer.Start();



            

        }
        public bool HandleText(long userId, long groupId, string message, List<string> results)
        {
            return false;   // 暂时屏蔽这个入口，改用直连mirai
            

        }


        int TaskRemove(long userId, long groupId)
        {
            int haveTask = 0;
            for (int i = tasks.Count - 1; i >= 0; i--)
            {
                var task = tasks[i];
                if (task.UserId == userId && task.GroupId == groupId)
                {
                    haveTask++;
                    tasks.RemoveAt(i); // 倒序删除元素不会影响遍历
                }
            }

            return haveTask;
        }


        void ModWithMirai.OnFriendMessageReceive(FriendMessageSender s, MeowMiraiLib.Msg.Type.Message[] e)
        {
            //throw new NotImplementedException();
        }

        void ModWithMirai.OnGroupMessageReceive(GroupMessageSender s, MeowMiraiLib.Msg.Type.Message[] e)
        {
            
            var message = e.MGetPlainString();
            long groupId = s.group.id;
            long userId = s.id;
            if (!isAskMe(e)) return ;
            try
            {
                //var group = Config.Instance.GetGroupInfo(groupId);
                //if (group.Is("AI模式"))
                //{
                //    string sentence = e.MGetPlainString();
                //    // 用rwkv回复
                //    AIReply(groupId, userId, sentence);
                //    e = new List<MeowMiraiLib.Msg.Type.Message>().ToArray();    // 在此截断
                //    return;
                //}





                var regex = new Regex(@"帮我撤回(\d{1,2})?条?");
                var match = regex.Match(message);
                if (match.Success)
                {
                    int quantity = 1;
                    if (match.Groups[1].Success)
                    {
                        quantity = int.Parse(match.Groups[1].Value);
                    }
                    var historys = HistoryManager.Instance.findMessage(userId, groupId);
                    for (int i = 0; i < Math.Min(historys.Length, quantity); i++)
                    {
                        Logger.Instance.Log($"?{historys[i].messageId}");

                        new GroupMessage(groupId, [
                            new Quote(historys[i].messageId,groupId,userId,groupId,
                                [new Plain(historys[i].message)]),
                                ]).Send(client);
                        new Recall(historys[i].messageId).Send(client);
                    }
                    return;
                    //new MeowMiraiLib.Msg.GroupMessage(groupId, [
                    //new At(userId, ""),
                    //    new Plain($"?")
                    //]).Send(client);
                    //return true;
                }

                regex = new Regex(@"你什么情况？");
                match = regex.Match(message);
                if (match.Success)
                {
                    var res = new BotProfile().Send(client);
                    string data = "";
                    data += $"我是{res.nickname}，{(res.sex == "FEMALE" ? "女" : "男")}，QQ等级{res.level}，年龄{res.age}，邮箱是{res.email}，个性签名是\"{res.sign}\"。你们别骂我了！\n";
                    foreach (var msg in e)
                    {
                        data += $"{msg.type}/";

                    }
                    new GroupMessage(groupId, [
                            //new At(userId, ""),
                            new Plain($"{data}")
                            ]).Send(client);
                    //var ress = new Anno_publish(groupId, "Bot 公告推送").Send(client);
                    //var res2 = new Anno_list(groupId).Send(client);
                    //foreach(var ano in res2)
                    //{
                    //    data += $"{ano.content}\n望周知！\n";
                    //}

                    //new GroupMessage(groupId, [
                    //    //new At(userId, ""),
                    //    new Plain($"{data}")
                    //    ]).Send(client);
                    return;
                }

                regex = new Regex(@"(\d{1,2})[:：点](\d{1,2})(分)?[叫喊]我(.*)");
                match = regex.Match(message);
                if (match.Success)
                {
                    string hourString = match.Groups[1].Value;
                    string minuteString = match.Groups[2].Value;
                    string alertMsg = match.Groups.Count > 3 ? match.Groups[3].Value.Trim() : "";
                    int hour = int.Parse(hourString);
                    int minute = int.Parse(minuteString);
                    DateTime alertTime = DateTime.Today.AddHours(hour).AddMinutes(minute);
                    if (alertTime < DateTime.Now) alertTime.AddDays(1);

                    MyTask newTask = new MyTask
                    {
                        UserId = userId,
                        GroupId = groupId,
                        Time = alertTime,
                        Message = alertMsg,
                    };

                    new MeowMiraiLib.Msg.GroupMessage(groupId, [
                        new At(userId, ""),
                            new Plain($"帮你设了{alertTime.ToString("dd号HH点mm分")}{alertMsg}的闹钟")
                        ]).Send(client);


                    TaskTimer.Elapsed += (s, e) =>
                    {
                        if (DateTime.Now >= alertTime)
                        {
                            new MeowMiraiLib.Msg.GroupMessage(groupId, [
                                new At(userId, ""),
                                    new Plain($"{DateTime.Now.ToString("HH:mm")}到了{(string.IsNullOrEmpty(alertMsg)?"":$"，{alertMsg}，请")}")
                                ]).Send(client);
                            //new MeowMiraiLib.Msg.FriendMessage(userId, new Message[] { new Plain($"{DateTime.Now.Hour}点 啦!") }).Send(c);

                        }
                    };

                    tasks.Add(newTask);
                    return;
                }

                regex = new Regex(@"别[叫喊][了我]?");
                match = regex.Match(message);
                if (match.Success)
                {
                    int haveTask = TaskRemove(userId, groupId);
                    if (haveTask > 0)
                    {
                        new MeowMiraiLib.Msg.GroupMessage(groupId, [
                            new At(userId, ""),
                                new Plain($"彳亍!{haveTask}个闹钟以取消")
                            ]).Send(client);
                        return;
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
            }

            return ;
        }




        bool isAskMe(MeowMiraiLib.Msg.Type.Message[] e)
        {
            foreach(var item in e)
            {
                if (item is AtAll) return true;
                if(item is At itemat)
                {
                    if(itemat.target == Config.Instance.App.Avatar.myQQ)  return true;
                    
                }
                if(item is Plain plain)
                {
                    if (plain.text.TrimStart().StartsWith(Config.Instance.App.Avatar.askName))
                    {
                        plain.text = plain.text.TrimStart().Substring(Config.Instance.App.Avatar.askName.Length);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
