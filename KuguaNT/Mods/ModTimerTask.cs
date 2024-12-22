
using Kugua.Integrations.NTBot;
using System.Runtime.InteropServices.Marshalling;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using ZhipuApi;




namespace Kugua
{
    /// <summary>
    /// 定时任务有关
    /// </summary>
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
            ModCommands.Add(new ModCommand(new Regex(@"^你什么情况？"), checkState));


            // clocks
            ModCommands.Add(new ModCommand(new Regex(@"^(\d{1,2})[:：点]((\d{1,2})分?)?[叫喊]我(.*)"), setClock));
            ModCommands.Add(new ModCommand(new Regex(@"^闹钟(列表|信息|状态)\b+"), checkClock));
            ModCommands.Add(new ModCommand(new Regex(@"^(删除闹钟|别[叫喊][了我]?)"), removeClock));

            


            TaskTimer = new(1000 * 10); // 10s
            TaskTimer.AutoReset = true;
            TaskTimer.Start();
            TaskTimer.Elapsed += TaskTimer_Elapsed;
           

            //ipLocation = new IpLocation();


            return true;
        }

       

       
      
        
        /// <summary>
        /// 关闭⏰
        /// 别叫我了/删除闹钟
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string removeClock(MessageContext context, string[] param)
        {
            int haveTask = TaskRemove(context.userId, context.groupId);
            if (haveTask > 0)
            {
                return $"彳亍!{haveTask}个闹钟以取消";

            }

            return "";
        }

        /// <summary>
        /// 看看有无⏰
        /// 闹钟列表
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
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
                    Logger.Log(ex);
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

        /// <summary>
        /// 设置⏰
        /// 17点30叫我吃饭
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
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
            //var res = new BotProfile().Send(context.client);
            string data = "";
            //data += $"我是{res.nickname}，{(res.sex == "FEMALE" ? "女" : "男")}，QQ等级{res.level}，年龄{res.age}，邮箱是{res.email}，个性签名是\"{res.sign}\"。你们别骂我了！\n";

            //GPT.Instance.AITalk(context, $"你是谁啊？");


            //foreach (var msg in e)
            //{
            //    data += $"{msg.type}/";

            //}



            //new GroupMessage(context.groupId, [
            //        //new At(userId, ""),
            //        //new Plain($"{data}"),
            //        //new Image(null, "https://s3.bmp.ovh/imgs/2024/10/31/ce9c165d2d4c274a.gif"),
            //        new Voice(null,null,"D:\\Projects\\momordica2020\\Kugua\\output\\Debug\\net8.0\\RunningData\\music\\殇_徐嘉良.mp3")
            //        ]).Send(context.client);


            //var ress = new Anno_publish(context.groupId, "Bot 公告推送").Send(context.client);
            //var res2 = new Anno_list(context.groupId).Send(context.client);
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



     




        /// <summary>
        /// 定时任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaskTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // 系统定时任务

            // 帮助配置文件一起定期存掉
            Config.Instance.Save();

            if (clientQQ != null)
            {
                foreach (var g in Config.Instance.groups)
                {
                    // 随机触发自言自语
                    if (g.Value.Is("自言自语") && MyRandom.NextDouble() > (1 - 1.0 / 360))
                    {
                        Logger.Log($"{g.Key} MMMM!");
                        var context = new MessageContext
                        {
                            groupId = g.Key,
                            client = clientQQ,
                        };
                        var r = ModRandomChat.getHistoryReact(context);
                        foreach (var item in r)
                        {
                            context.SendBackPlain(item);
                        }
                    }
                }
            }



            // 用户的定时任务
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
       
        
        
     


        int TaskRemove(string userId, string groupId)
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
            if (context.recvMessages == null || context.recvMessages.Count<=0) return false;


            if (context.isPrivate)
            {
                foreach (var msg in context.recvMessages)
                {
                    if (msg is Text plain)
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
                    if (msg is Record voice)
                    {

                        //new FriendMessage(s.id, [
                        //    new Voice(null,voice.url)
                        //    ]).Send();
                        return true;
                    }
                }
            }
            else if (context.isGroup)
            {
                //if (Config.Instance.GroupInfo(context.groupId).Is("正常模式"))
                //{
                //    List<string> imgPaths = new List<string>();
                //    string cmd = context.recvMessages.ToTextString();
                //    foreach (var item in context.recvMessages)
                //    {
                //        if (item is Image image)
                //        {
                //            //Logger.Log("img!");
                //            //string userImgDict = $"{Config.Instance.ResourceFullPath("HistoryImagePath")}{Path.DirectorySeparatorChar}{userId}";
                //            //if (!Directory.Exists(userImgDict)) Directory.CreateDirectory(userImgDict);
                //            //string imgPath = $"{userImgDict}{Path.DirectorySeparatorChar}{image.imageId}";
                //            //WebLinker.DownloadImageAsync(image.url, imgPath);
                //            var base64data = await Network.ConvertImageUrlToBase64(image.url);
                //            imgPaths.Add(base64data);
                //        }
                //    }
                //    //Logger.Log($"{imgPaths.Count}");
                //    if (imgPaths.Count > 0)
                //    {
                //        GPT.Instance.AIReplyWithImage(context, imgPaths.ToArray());
                //        return true;
                //    }

                //}


                //// 
                //foreach (var msg in context.recvMessages)
                //{

                //    if (msg is Text plain)
                //    {
                //        if (plain.text == "发语音")
                //        {
                //            //string inputwav = @"D:\Projects\win-ChatTTS-ui-v1.0\static\wavs\1106-173028_9.4s-seedseed_1694_restored_emb-covert.pt-temp0.11-top_p0.05-top_k15-len28-86146-0-0.wav";
                //            //string inputpcm = @"D:\Projects\win-ChatTTS-ui-v1.0\static\wavs\test.pcm";
                //            //string outputSilk = @"D:\Projects\win-ChatTTS-ui-v1.0\static\wavs\test.silk";
                //            //string mp3file = @"D:\Projects\win-ChatTTS-ui-v1.0\static\wavs\test.mp3";
                //            //string amrfile = @"D:\Projects\win-ChatTTS-ui-v1.0\static\wavs\test.amr";
                //            ////SilkSharp.Encoder encoder = new();
                //            ////encoder.EncodeAsync(inputpcm, outputSilk);
                //            //new GroupMessage(s.group.id, [
                //            //    new Voice(null,null,amrfile)
                //            //    ]).Send(client);

                //            //return true;
                //        }
                //    }



                //}
                


                //string logstr = "";
                //foreach (var msg in context.recvMessages)
                //{
                //    logstr += $",{msg.GetType()}";
                //}
                //Logger.Log(logstr);

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
                        //string userImgDict = $"{Config.Instance.ResourceFullPath("HistoryImagePath")}/{context.userId}";
                        //if (!Directory.Exists(userImgDict)) Directory.CreateDirectory(userImgDict);
                        //Network.DownloadAsync(itemImg.url, $"{userImgDict}/{}.jpg");
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

                //return false;
            }












            

                return false;

        }


        ///// <summary>
        ///// 查看并截断掉Message里的提示词
        ///// </summary>
        ///// <param name="e"></param>
        ///// <returns></returns>
        //bool isAskMe(Message[] e)
        //{
        //    foreach(var item in e)
        //    {
        //        if(item is At itemat)
        //        {
        //            if(itemat.qq == Config.Instance.App.Avatar.myQQ)  return true;
                    
        //        }
        //        if(item is Text plain)
        //        {
        //            if (plain.text.TrimStart().StartsWith(Config.Instance.App.Avatar.askName))
        //            {
        //                plain.text = plain.text.TrimStart().Substring(Config.Instance.App.Avatar.askName.Length);
        //                return true;
        //            }
        //        }
        //    }

        //    return false;
        //}
    }
}
