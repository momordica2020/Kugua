
using System.Text.RegularExpressions;
using MeowMiraiLib;
using MeowMiraiLib.Msg;
using MeowMiraiLib.Msg.Sender;
using MeowMiraiLib.Msg.Type;
using MMDK.Util;
using static MeowMiraiLib.Msg.Sender.GroupMessageSender;


namespace MMDK.Mods
{
    internal class ModTimerTask : Mod
    {
        class MyTask
        {
            public DateTime Time;
            public string Message;
            public MessageContext context;
            //public System.Timers.Timer TaskTimer;
        }

        System.Timers.Timer TaskTimer;



        List<MyTask> tasks = new List<MyTask>();





        public override bool Init(string[] args)
        {
            ModCommands[new Regex(@"^帮我撤回(\d{1,2})?条?")] = handleRecall;
            ModCommands[new Regex(@"^刷新列表")] = refreshList;
            
            ModCommands[new Regex(@"^来点狐狸")] = getFox;
            ModCommands[new Regex(@"^说(.+)")] = say;
            ModCommands[new Regex(@"^你什么情况？")] = checkState;
            
            ModCommands[new Regex(@"^(\d{1,2})[:：点]((\d{1,2})分?)?[叫喊]我(.*)")] = setClock;

            ModCommands[new Regex(@"^闹钟(列表|信息|状态)\b+")] = checkClock;

            ModCommands[new Regex(@"^(删除闹钟|别[叫喊][了我]?)")] = removeClock;


            TaskTimer = new(1000 * 10);
            TaskTimer.AutoReset = true;
            TaskTimer.Start();
            TaskTimer.Elapsed += TaskTimer_Elapsed;
            
            
            return true;
        }



        private string removeClock(MessageContext context, string[] param)
        {
            int haveTask = TaskRemove(context.userId, context.groupId);
            if (haveTask > 0)
            {
                return $"彳亍!{haveTask}个闹钟以取消";

            }

            return "";
        }

        private string checkClock(MessageContext context, string[] param)
        {
            string res = "";
            int no = 1;
            for (int i = tasks.Count - 1; i >= 0; i--)
            {
                try
                {
                    var task = tasks[i];
                    if (task.context.userId == context.userId && task.context.groupId == context.groupId)
                    {
                        res += $"- {task.Time.ToString("MM月dd日HH时mm分")} {task.Message}\n";
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.Log(ex);
                }
            }
            string result = "";
            if (context.isGroup)
            {
                if (no > 1)
                {
                    result = $"你在本群订了{no - 1}个闹钟：\n{res}";
                }
                else
                {
                    result = $"你没设过闹钟";
                }
            }
            else
            {
                if (no > 1)
                {
                    result = $"你订了{no - 1}个闹钟：\n{res}";
                }
                else
                {
                    result = $"你没设过闹钟";
                }
            }
            return result;
        }

        private string setClock(MessageContext context, string[] param)
        {
            string hourString = param[1];
            string minuteString = param[3];
            string alertMsg = param[4];
            if (string.IsNullOrWhiteSpace(alertMsg)) alertMsg = "";
            int hour; int minute;
            if (!int.TryParse(hourString, out hour)) return "";
            if (!int.TryParse(minuteString, out minute)) minute = 0;
            DateTime alertTime = DateTime.Today.AddHours(hour).AddMinutes(minute);
            if (alertTime < DateTime.Now) alertTime = alertTime.AddDays(1);

            MyTask newTask = new MyTask
            {
                context=context,
                Time = alertTime,
                Message = alertMsg,
            };
            tasks.Add(newTask);

            return $"帮你设了{alertTime.ToString("d号H点m分")}{alertMsg}的闹钟";
        }

        private string checkState(MessageContext context, string[] param)
        {
            if (context.client is LocalClient) return "";
            var res = new BotProfile().Send(context.client);
            string data = "";
            data += $"我是{res.nickname}，{(res.sex == "FEMALE" ? "女" : "男")}，QQ等级{res.level}，年龄{res.age}，邮箱是{res.email}，个性签名是\"{res.sign}\"。你们别骂我了！\n";

            GPT.Instance.AITalk(context.groupId, context.userId, $"你是谁啊？");


            //foreach (var msg in e)
            //{
            //    data += $"{msg.type}/";

            //}



            //new GroupMessage(groupId, [
            //        //new At(userId, ""),
            //        new Plain($"{data}"),
            //        new Image(null, "https://s3.bmp.ovh/imgs/2024/10/31/ce9c165d2d4c274a.gif"),

            //        ]).Send(client);


            //var ress = new Anno_publish(groupId, "Bot 公告推送").Send(client);
            //var res2 = new Anno_list(groupId).Send(client);
            //foreach (var ano in res2)
            //{
            //    data += $"{ano.content}\n望周知！\n";
            //}

            //new GroupMessage(groupId, [
            //    //new At(userId, ""),
            //    new Plain($"{data}")
            //    ]).Send(client);
            return null;
        }

        private string say(MessageContext context, string[] param)
        {
            string speakSentence = param[1];
            if (string.IsNullOrWhiteSpace(speakSentence)) return "";

            GPT.Instance.AITalk(context.groupId, context.userId, $"{speakSentence}");


            return null;
        }

        private string getFox(MessageContext context, string[] param)
        {
            var files = Directory.GetFiles($"{Config.Instance.ResourceRootPath}/gifs", "*.gif");
            if (files != null)
            {
                string fname = files[MyRandom.Next(files.Length)];
                var msg = new Message[] {
                     new Image(null,null,fname),
                };
                context.SendBack(msg);

                return null;
            }

            return "";
        }

        private string refreshList(MessageContext context, string[] param)
        {
            if (context.isGroup && Config.Instance.UserHasAdminAuthority(context.userId))
            {
                Logger.Instance.Log($"更新好友列表和群列表...");
                RefreshFriendList();
                Logger.Instance.Log($"更新完毕，找到{Config.Instance.friends.Count}个好友，{Config.Instance.groups.Count}个群...");

                return $"更新完毕，找到{Config.Instance.friends.Count}个好友，{Config.Instance.groups.Count}个群...";
            }
            return "";
        }

        private string handleRecall(MessageContext context, string[] param)
        {
            if (context.isGroup && Config.Instance.UserHasAdminAuthority(context.userId))
            {
                int quantity = 1;
                if (!string.IsNullOrWhiteSpace(param[1]))
                {
                    quantity = int.Parse(param[1]);
                }
                var historys = HistoryManager.Instance.findMessage(context.userId, context.groupId);
                for (int i = 0; i < Math.Min(historys.Length, quantity); i++)
                {
                    Logger.Instance.Log($"?{historys[i].messageId}");

                    if (clientMirai != null)
                    {
                        new GroupMessage(context.groupId, [
                            new Quote(historys[i].messageId,context.groupId,context.userId,context.groupId,
                                [new Plain(historys[i].message)])]
                            ).Send(clientMirai);
                        new Recall(historys[i].messageId).Send(clientMirai);
                    }
                }
                return null;
                //new MeowMiraiLib.Msg.GroupMessage(groupId, [
                //new At(userId, ""),
                //    new Plain($"?")
                //]).Send(client);
                //return true;
            }
            return "";
        }

        private void TaskTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            for(int i = tasks.Count - 1; i >= 0; i--)
            {
                try
                {
                    var t = tasks[i];
                    if (DateTime.Now >= t.Time)
                    {
                        t.context.SendBackPlain($"{DateTime.Now.ToString("HH:mm")}到了{(string.IsNullOrEmpty(t.Message) ? "" : $"，{t.Message}，请")}", true);

                        tasks.Remove(t);
                    }
                }
                catch { }
            }

        }
        /// <summary>
        /// 刷新好友列表并更新配置文件
        /// </summary>
        public void RefreshFriendList()
        {
            try
            {
                if (clientMirai != null)
                {
                    var fp = new FriendList().Send(clientMirai);
                    Config.Instance.friends.Clear();
                    if (fp == null)
                    {
                        Logger.Instance.Log($"不会吧不会吧不会没有好友吧");

                    }
                    else
                    {
                        foreach (var f in fp)
                        {
                            var friend = Config.Instance.UserInfo(f.id);
                            friend.Name = f.nickname;
                            //friend.Mark = f.remark;
                            friend.Tags.Add("好友");
                            //friend.Type = PlayerType.Normal;
                            Config.Instance.friends.Add(f.id, f);
                        }
                    }




                    var gp = new GroupList().Send(clientMirai);
                    Config.Instance.groups.Clear();
                    Config.Instance.groupMembers.Clear();
                    if (gp == null)
                    {
                        Logger.Instance.Log($"不会吧不会吧不会没有群吧");

                    }
                    else
                    {
                        foreach (var g in gp)
                        {
                            var group = Config.Instance.GroupInfo(g.id);
                            group.Name = g.name;
                            var groupMembers = g.GetMemberList(clientMirai);
                            if (groupMembers == null)
                            {
                                Logger.Instance.Log($"不会吧不会吧不会{g.id}是鬼群吧");
                                continue;
                            }
                            Config.Instance.groups.Add(g.id, g);
                            Config.Instance.groupMembers.Add(g.id, groupMembers);
                            foreach (var gf in groupMembers)
                            {
                                var member = Config.Instance.UserInfo(gf.id);
                                member.Mark = gf.memberName;    //群昵称？
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
            }

        }



        int TaskRemove(long userId, long groupId)
        {
            int haveTask = 0;
            for (int i = tasks.Count - 1; i >= 0; i--)
            {
                try
                {
                    var task = tasks[i];
                    if (task.context.userId == userId && task.context.groupId == groupId)
                    {
                        haveTask++;
                        tasks.RemoveAt(i); // 倒序删除元素不会影响遍历
                    }
                }
                catch { }
            }

            return haveTask;
        }


        public override async Task<bool> HandleMessagesDIY(MessageContext context)
        {
            if (context.recvMessages == null || context.recvMessages.Length < 2) return false;

            var source = context.recvMessages[0];
            if (context.isPrivate)
            {
                foreach (var msg in context.recvMessages)
                {
                    if (msg is Plain plain)
                    {
                        var str = plain.text;
                        if (str == "测试")
                        {
                            //new FriendMessage(s.id, [
                            //new Voice(null,voice.url)
                            //]).Send(client);
                        }
                        //string msg2 = StaticUtil.RemoveEmojis(plain.text);
                        //if (string.IsNullOrWhiteSpace(msg2)) return false;
                        //new FriendMessage(s.id, [
                        //    new Plain(msg2)
                        //    ]).Send(client);
                        return false;
                    }
                    if (msg is Voice voice)
                    {

                        //new FriendMessage(s.id, [
                        //    new Voice(null,voice.url)
                        //    ]).Send();
                        return true;
                    }
                }
                return false;
            }
            else if (context.isGroup)
            {
                var message = context.recvMessages.MGetPlainString();
                var group = Config.Instance.GroupInfo(context.groupId);
                var user = Config.Instance.UserInfo(context.userId);

                if (context.isAskme)
                {
                    if (Config.Instance.GroupInfo(context.groupId).Is("正常模式"))
                    {
                        List<string> imgPaths = new List<string>();
                        string cmd = context.recvMessages.MGetPlainString();
                        foreach (var item in context.recvMessages)
                        {
                            if (item is Image image)
                            {
                                //Logger.Instance.Log("img!");
                                //string userImgDict = $"{Config.Instance.ResourceFullPath("HistoryImagePath")}{Path.DirectorySeparatorChar}{userId}";
                                //if (!Directory.Exists(userImgDict)) Directory.CreateDirectory(userImgDict);
                                //string imgPath = $"{userImgDict}{Path.DirectorySeparatorChar}{image.imageId}";
                                //WebLinker.DownloadImageAsync(image.url, imgPath);
                                var base64data = await WebLinker.ConvertImageUrlToBase64(image.url);
                                imgPaths.Add(base64data);
                            }
                        }
                        //Logger.Instance.Log($"{imgPaths.Count}");
                        if (imgPaths.Count > 0)
                        {
                            GPT.Instance.AIReplyWithImage(context.groupId, context.userId, cmd, imgPaths.ToArray());
                            return true;
                        }

                    }


                    // 
                    foreach (var msg in context.recvMessages)
                    {

                        if (msg is Plain plain)
                        {
                            if (plain.text == "发语音")
                            {
                                //string inputwav = @"D:\Projects\win-ChatTTS-ui-v1.0\static\wavs\1106-173028_9.4s-seedseed_1694_restored_emb-covert.pt-temp0.11-top_p0.05-top_k15-len28-86146-0-0.wav";
                                //string inputpcm = @"D:\Projects\win-ChatTTS-ui-v1.0\static\wavs\test.pcm";
                                //string outputSilk = @"D:\Projects\win-ChatTTS-ui-v1.0\static\wavs\test.silk";
                                //string mp3file = @"D:\Projects\win-ChatTTS-ui-v1.0\static\wavs\test.mp3";
                                //string amrfile = @"D:\Projects\win-ChatTTS-ui-v1.0\static\wavs\test.amr";
                                ////SilkSharp.Encoder encoder = new();
                                ////encoder.EncodeAsync(inputpcm, outputSilk);
                                //new GroupMessage(s.group.id, [
                                //    new Voice(null,null,amrfile)
                                //    ]).Send(client);

                                //return true;
                            }
                        }



                    }
                }


                //string logstr = "";
                //foreach (var msg in context.recvMessages)
                //{
                //    logstr += $",{msg.GetType()}";
                //}
                //Logger.Instance.Log(logstr);

                foreach (var msg in context.recvMessages)
                {
                    //if (msg is Voice voice)
                    //{
                    //    new GroupMessage(s.group.id, [
                    //            new Voice(voice.voiceId)
                    //            ]).Send(client);

                    //    return true;
                    //}

                    if (msg is Image itemImg)
                    {
                        string userImgDict = $"{Config.Instance.ResourceFullPath("HistoryImagePath")}/{context.userId}";
                        if (!Directory.Exists(userImgDict)) Directory.CreateDirectory(userImgDict);
                        WebLinker.DownloadImageAsync(itemImg.url, $"{userImgDict}/{itemImg.imageId}");
                    }
                    //ForwardMessage fm = new ForwardMessage([new ForwardMessage.Node(Config.Instance.App.Avatar.myQQ, DateTime.Now.Ticks, Config.Instance.App.Avatar.myName, e.Skip(1).ToArray(), source.id)]);

                    //if (userId == Config.Instance.App.Avatar.adminQQ)
                    //{
                    //    //if (item is ForwardMessage gmsg)
                    //    {
                    //        // new MeowMiraiLib.Msg.GroupMessage(groupId, [
                    //        //  new At(userId, ""),
                    //        //new ForwardMessage()
                    //        //new Voice(null,null,@"D:\Projects\SummerTTS_VS-main\x64\Debug\out.wav")
                    //        //new ForwardMessage.Node()
                    //        //]).Send(client);
                    //        //return true;
                    //    }
                    //}
                }

                return false;
            }

            return false;

        }



        /// <summary>
        /// 查看并截断掉Message里的提示词
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
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
