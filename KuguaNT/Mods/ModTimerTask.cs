
using Kugua.Integrations.NTBot;
using System.Runtime.InteropServices.Marshalling;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using ZhipuApi;




namespace Kugua
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
        QQWry qqwry;
        //IpLocation ipLocation;



        public override bool Init(string[] args)
        {
            ModCommands.Add(new ModCommand(new Regex(@"^撤回(.*)"), handleRecall));
            ModCommands.Add(new ModCommand(new Regex(@"^帮我撤回(.*)"), handleRecall2));
            ModCommands.Add(new ModCommand(new Regex(@"^(拍拍|贴贴)"), sendPoke));
            //ModCommands.Add(new ModCommand(new Regex(@"^刷新列表"), refreshList));


            ModCommands.Add(new ModCommand(new Regex(@"^来点(\S+)"), getSome));
            ModCommands.Add(new ModCommand(new Regex(@"^动(\S+)"), getMoveEmoji));
            ModCommands.Add(new ModCommand(new Regex(@"^生图(.*)", RegexOptions.Singleline), genImg));

            ModCommands.Add(new ModCommand(new Regex(@"^查IP(.+)"), checkIP));

            // img
            ModCommands.Add(new ModCommand(new Regex(@"做旧(\S*)",RegexOptions.Singleline), getOldJpg));
            ModCommands.Add(new ModCommand(new Regex(@"(.+)倍速", RegexOptions.Singleline), setGifSpeed));
            ModCommands.Add(new ModCommand(new Regex(@"镜像(.*)", RegexOptions.Singleline), setImgMirror));
            ModCommands.Add(new ModCommand(new Regex(@"旋转(.*)", RegexOptions.Singleline), setImgRotate));
            ModCommands.Add(new ModCommand(new Regex(@"抠图(.*)", RegexOptions.Singleline), setRemoveBackground));


            // speak
            ModCommands.Add(new ModCommand(new Regex(@"^点歌(.+)"), getMusic));
            ModCommands.Add(new ModCommand(new Regex(@"^说[∶|:|：](.+)", RegexOptions.Singleline), say));
            ModCommands.Add(new ModCommand(new Regex(@"^你什么情况？"), checkState));




            // clocks
            ModCommands.Add(new ModCommand(new Regex(@"^(\d{1,2})[:：点]((\d{1,2})分?)?[叫喊]我(.*)"), setClock));
            ModCommands.Add(new ModCommand(new Regex(@"^闹钟(列表|信息|状态)\b+"), checkClock));
            ModCommands.Add(new ModCommand(new Regex(@"^(删除闹钟|别[叫喊][了我]?)"), removeClock));


            

            


            TaskTimer = new(1000 * 10); // 10s
            TaskTimer.AutoReset = true;
            TaskTimer.Start();
            TaskTimer.Elapsed += TaskTimer_Elapsed;
            try
            {
                qqwry = new QQWry(Config.Instance.ResourceFullPath("qqwry.dat"));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            //ipLocation = new IpLocation();


            return true;
        }

        /// <summary>
        /// emoji合成（直接发一到两个emoji给bot即可触发）/查看gif版的emoji（很好的gif，爱来自TG）
        /// 😀😀/😀/动😀
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string getMoveEmoji(MessageContext context, string[] param)
        {
            var elist = StaticUtil.ExtractEmojis(param[1]);
            if (elist.Count > 0)
            {
                List<Message> msgs = new List<Message>();
                foreach (var emoji in elist) 
                {
                    var fff = Directory.GetFiles(Config.Instance.ResourceFullPath($"emojitg/"), $"*{emoji.Replace("u", "")}*.gif");
                    if (fff.Length > 0)
                    {
                        msgs.Add(new Image($"file://{fff[MyRandom.Next(fff.Length)]}"));
                    }
                
                }
                if (msgs.Count > 0)
                {
                    _ = context.SendBack(msgs.ToArray());
                    return null;
                }
            }
            return "";
        }


        /// <summary>
        /// AI生成图片
        /// 生图 一个小女孩在下雨天奔跑
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string genImg(MessageContext context, string[] param)
        {
            var desc = param[1];
            if (context.isImage)
            {
                // 以图生图

                desc += GPT.Instance.ZPGetImgDesc(context.PNG1Base64, "请用详细文字描述这张图的内容，以便我根据你的描述用AI生成新的图片。注意强调艺术风格、肢体动作、表情、物品的位置等。");
                context.SendBackPlain(desc);
                if (desc.Contains("ERROR")) return null;
            }
            else if(string.IsNullOrWhiteSpace(desc))
            {
                WaitNext(context, new ModCommand(new Regex(@"生图(.*)", RegexOptions.Singleline), genImg));
                return null;
            }


            if(!string.IsNullOrWhiteSpace(desc))
            {

                GPT.Instance.ZPImage(context, desc);
                return null;
            }

            return "";
        }



        /// <summary>
        /// 图片顺时针旋转n度
        /// 旋转90[图片]
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string setImgRotate(MessageContext context, string[] param)
        {
            double ro = 0;
            if (!double.TryParse(param[1], out ro))
            { ro = 0; }
            bool findImg = false;
            foreach (var item in context.recvMessages)
            {
                //Logger.Log(item.type);
                if (item is Image itemImg)
                {
                    var oriImg = Network.DownloadImage(itemImg.url);
                    var newImgbase64 = ImageUtil.ImgRotate(oriImg, ro);
                    context.SendBack([
                        //new At(context.userId, null),
                        new Image($"base64://{newImgbase64}"),
                    ]);
                    findImg = true;
                        
                }
            }
            if (!findImg) WaitNext(context, new ModCommand(new Regex(@"旋转(.*)", RegexOptions.Singleline), setImgRotate));
            return null;
        }

        /// <summary>
        /// 图片去除背景
        /// 抠图[图片]
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string setRemoveBackground(MessageContext context, string[] param)
        {
            double ro = 0;
            if (!double.TryParse(param[1], out ro))
            { ro = 0; }
            bool findImg = false;
            foreach (var item in context.recvMessages)
            {
                //Logger.Log(item.type);
                if (item is Image itemImg)
                {
                    var oriImg = Network.DownloadImage(itemImg.url);
                    var newImgbase64 = ImageUtil.RemoveBackground(oriImg);
                    if (string.IsNullOrWhiteSpace(newImgbase64))
                    {
                        // fail
                        return "";
                    }
                    context.SendBack([
                        //new At(context.userId, null),
                        new Image($"base64://{newImgbase64}"),
                    ]);
                    findImg = true;

                }
            }
            if (!findImg) WaitNext(context, new ModCommand(new Regex(@"抠图(.*)", RegexOptions.Singleline), setRemoveBackground));
            return null;
        }


        /// <summary>
        /// 图像镜像化（1、2、3、4保留不同的部分）
        /// 镜像1[图片]
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string setImgMirror(MessageContext context, string[] param)
        {
            double degree = 1;
            if (!double.TryParse(param[1], out degree))
            {
                degree = 1;
            }
            
            bool findImg = false;
            foreach (var item in context.recvMessages)
            {
                //Logger.Log(item.type);
                if (item is Image itemImg)
                {
                    var oriImg = Network.DownloadImage(itemImg.url);
                    var newImgbase64 = ImageUtil.ImgMirror(oriImg, degree);
                    context.SendBack([
                        //new At(context.userId, null),
                        new Image($"base64://{newImgbase64}"),
                    ]);
                    findImg = true;
                }
            }
            if (!findImg) WaitNext(context, new ModCommand(new Regex(@"镜像(.*)", RegexOptions.Singleline), setImgMirror));
            return null;
        }

        /// <summary>
        /// 输入ip地址查属地
        /// 查IP 192.168.1.1
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string checkIP(MessageContext context, string[] param)
        {
            var ipstr = param[1].Trim();
            try
            {
                string ipcheck = $"https://ip.dnomd343.top/info/{ipstr}";
                var dd = Network.Get(ipcheck);
                if (dd != null)
                {
                    var jo = JsonObject.Parse(dd.ToString());
                    string res = $"{jo["detail"]} ({jo["loc"]})";
                    return res;
                }

                else
                {
                    // failed. use local qqwry.day
                    var info = qqwry.find_info(ipstr);
                    return $"{info.Item1}\n{info.Item2}\n{info.Item3}";
                }
            }
            catch(Exception ex)
            {
                Logger.Log(ex);
            }
            //var res = ipLocation.Find(ipstr);
            //if (res != null && res.Length > 0)
            //{

            //    string str = string.Join(",", res);
            //    return str;
            //    //return null;
            //}
            return "";
        }


        /// <summary>
        /// gif图修改播放速率。负数为倒放
        /// 3倍速[图片]/-0.5倍速[图片]
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string setGifSpeed(MessageContext context, string[] param)
        {
            double speed = 0;
            //Logger.Log("? == " + string.Join(",", param));
            if (double.TryParse(param[1], out speed))
            {
                bool findImg = false;
                foreach (var item in context.recvMessages)
                {
                    //Logger.Log(item.type);
                    if (item is Image itemImg)
                    {
                        var oriImg = Network.DownloadImage(itemImg.url);
                        var newImgbase64 = ImageUtil.GifSpeed(oriImg, speed);
                        context.SendBack([
                            //new At(context.userId, null),
                            new Image($"base64://{newImgbase64}"),
                        ]);
                        findImg = true;
                    }
                }
                if (!findImg) WaitNext(context, new ModCommand(new Regex(@"(.+)倍速", RegexOptions.Singleline), setGifSpeed));
                return null;
            }
           

            return "";
        }


        /// <summary>
        /// 图像包浆做旧
        /// 做旧[图片]
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string getOldJpg(MessageContext context, string[] param)
        {
            bool findImg = false; 
            double quality = 0.75;
            if (param.Length >= 2)
            {
                double.TryParse(param[1], out quality);
                if (quality < 0.1) quality = 0.75;
                if (quality > 0.95) quality = 0.95;
            }


            foreach (var item in context.recvMessages)
            {
                //Logger.Log(item.type);
                if (item is Image itemImg)
                {
                    var oriImg = Network.DownloadImage(itemImg.url);
                    //Logger.Log("? == " + oriImg.Count);
                    var newImgbase64 = ImageUtil.ImageGreen(oriImg, (int)(50*(1-quality)), quality);
                    //Logger.Log("?2,img=" + newImgbase64.Substring(0,100));
                    context.SendBack([
                        //new At(context.userId, null),
                        new Image($"base64://{newImgbase64}"),
                        ]);
                    findImg = true;
                }
            }

            if (!findImg) WaitNext(context, new ModCommand(new Regex(@"做旧(\S*)", RegexOptions.Singleline), getOldJpg));
            return null;
            
            
        }
        MusicDownloader musicDownloader = new MusicDownloader();

        /// <summary>
        /// 点歌（爱来自QQ音乐），搜到多首歌曲会唱第一首
        /// 点歌 初音未来的消失
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string getMusic(MessageContext context, string[] param)
        {
            try
            {
                string mname = param[1].Trim();
                string localPath = "";// Config.Instance.ResourceFullPath($"music/{mname}.mp3");
                string infoDesc = "";
                string url = "";
                int index = 1;
               
                var mlist = musicDownloader.GetMusicList(mname);
                if (mlist != null)
                {
                    foreach (var vm in mlist)
                    {
                        if (vm.CanDownload)
                        {
                            if (string.IsNullOrWhiteSpace(localPath))
                            {
                                localPath = Config.Instance.ResourceFullPath($"music/{vm.Name}_{vm.Singer}.mp3");
                                if (System.IO.File.Exists(localPath)) System.IO.File.Delete(localPath);
                                url = musicDownloader.GetMusicDownloadURL(vm.DownloadInfo, enmMusicSource.QQ);
                                Network.Download(url, localPath);
                                    
                                    
                                infoDesc += $"[{index++}]-{vm.Name} {vm.Singer} ({vm.Class})\n";
                            }
                            else
                            {
                                infoDesc += $" {index++} -{vm.Name} {vm.Singer} ({vm.Class})\n";
                            }
                                
                        }

                    }
                }
                
                if (!string.IsNullOrWhiteSpace(infoDesc))
                {
                    context.SendBackPlain(infoDesc, true);
                }
                if (!string.IsNullOrWhiteSpace(localPath) && System.IO.File.Exists(localPath))
                {
                    context.SendBack(new Message[] {
                                new Record($"file://{localPath}")
                            });
                    //var amrb64 = StaticUtil.Mp32AmrBase64(localPath);
                    //if (!string.IsNullOrWhiteSpace(amrb64))
                    //{
                    //    context.SendBack(new Message[] {
                    //            new Voice(null, null, null, amrb64)
                    //        });
                    //}
                }

                Task.Delay(2000).ContinueWith(t =>
                {
                    try
                    {
                        File.Delete(localPath);
                    }
                    catch { }
                } );

                return null;

            }
            catch(Exception ex)
            {
                Logger.Log(ex);

            }
            return $"不给点";


            




            //var music = Directory.GetFiles(@"D:\Projects\musicapi\music", $"{mname}.mp3");
            //if (music.Length > 0)
            //{
            //    var amrb64 = StaticUtil.Mp32AmrBase64(music.First());
            //    if (!string.IsNullOrWhiteSpace(amrb64))
            //    {
            //        context.SendBack(new Message[] {
            //            new Voice(null, null, null, amrb64)
            //        });
            //    }
                
            //    return null;
            //}
            //return $"曲库没有{mname}";
            //if(!string.IsNullOrWhiteSpace(mname))
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
        /// 在线棒读
        /// 说：你好我好，大家好
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string say(MessageContext context, string[] param)
        {
            string speakSentence = param[1];
            if (string.IsNullOrWhiteSpace(speakSentence)) return "";

            GPT.Instance.AITalk(context, $"{speakSentence}");


            return null;
        }

        class NewsInfo
        {
            public string title;
            public string desc;
            public string url;
            public static Dictionary<string,string> platform = new Dictionary<string, string>(){
            { "百度","baidu" },
            { "少数派","shaoshupai" },
            { "微博","weibo" },
            { "知乎","zhihu" },
            { "36氪","36kr" },
            { "52破解","52pojie" },
            { "b站","bilibili" },
            { "豆瓣","douban" },
            { "虎扑","hupu" },
            { "贴吧","tieba" },
            { "掘金","juejin" },
            { "抖音","douyin" },
            { "v2ex","v2ex" },
            { "头条","jinritoutiao" },
           };
        }


        /// <summary>
        /// 让bot来点什么
        /// 来点狐狸/小猫/非主流/猫姬/车万/emoji/头条/抖音/贴吧/b站/知乎/……)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string getSome(MessageContext context, string[] param)
        {
            try
            {
                var something = param[1].Trim();
                string[] files = null;
                if (something == "小猫")
                {
                    files = Directory.GetFiles($"{Config.Instance.ResourceRootPath}/gifscat", "*.gif");

                }
                else if (something == "狐狸")
                {
                    files = Directory.GetFiles($"{Config.Instance.ResourceRootPath}/gifsfox", "*.gif");
                }
                else if (something == "非主流")
                {
                    files = Directory.GetFiles($"{Config.Instance.ResourceRootPath}/imgfzl", "*.*");
                }
                else if (something == "杀马特")
                {
                    files = Directory.GetFiles($"{Config.Instance.ResourceRootPath}/imgsmt", "*.*");
                }
                else if (something == "猫姬")
                {
                    files = Directory.GetFiles($"{Config.Instance.ResourceRootPath}/imgmj", "*.*");
                }
                else if (something == "车万" || something=="东方")
                {
                    files = Directory.GetFiles($"{Config.Instance.ResourceRootPath}/imgth", "*.*");
                }
                else if (something == "相声")
                {
                    files = Directory.GetFiles($"{Config.Instance.ResourceRootPath}/xiangsheng", "*.*");
                    string fname = files[MyRandom.Next(files.Length)];
                    var fdesc = fname.Split('-')[1].Trim();
                    context.SendBackPlain($"▶ {fdesc}",true);
                    context.SendBack([
                        new Record($"file://{fname}"),
                        ]);
                }
                else if (something == "emoji")
                {
                    files = Directory.GetFiles(Config.Instance.ResourceFullPath("/emojimix/"), "*.png");
                    string fname = files[MyRandom.Next(files.Length)];
                    var f = Path.GetFileNameWithoutExtension(fname).Replace("-u200d","").Replace("-ufe0f","").Split("_");
                    if (f.Length == 2)
                    {
                        context.SendBack([
                        new Text($"{StaticUtil.UnicodePointsToEmoji(f[0])} + {StaticUtil.UnicodePointsToEmoji(f[1])} = "),
                        new Image($"file://{fname}"),
                        ]);
                    }
                    return null;
                }
                else if (NewsInfo.platform.ContainsKey(something))
                {
                    // 来点新闻系列
                    int newsLen = 25;
                    List<NewsInfo> news = new List<NewsInfo>();
                    string plat = NewsInfo.platform[something];
                   var jstr = Network.Get($"https://orz.ai/dailynews/?platform={plat}");
                    if (!string.IsNullOrWhiteSpace(jstr))
                    {
                        string res = "";
                        var jo = JsonObject.Parse(jstr);
                        if (jo["data"] != null)
                        {
                            foreach (var item in jo["data"]?.AsArray())
                            {
                                var n = new NewsInfo
                                {
                                    title = item["title"]?.ToString(),
                                    url = item["url"]?.ToString(),
                                    desc = item["desc"]?.ToString(),
                                };

                                news.Add(n);
                            }
                        }
                        if (news.Count > 0)
                        {
                            var nlist = news.Select(e=>e.title).ToArray();
                            StaticUtil.FisherYates(nlist);
                            for(int i=0; i < Math.Min(newsLen, news.Count); i++)
                            {
                                if (!string.IsNullOrWhiteSpace(nlist[i])) res += $"- {nlist[i]}\r\n";
                            }
                        }
                        //LocalStorage.writeLines(Config.Instance.ResourceFullPath("news.txt"), news.Select(v => $"{DateTime.Now.ToString("yyyyMMdd")}\t{v.title}\t{v.desc}\t{v.url}"));
                        context.SendBackPlain(res);
                        return null;
                    }
                }


                if (files != null)
                {
                    string fname = files[MyRandom.Next(files.Length)];
                    var msg = new Message[] {
                        new Image($"file://{fname}"),
                    };
                    context.SendBack(msg);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        
            return "";
        }

        ///// <summary>
        ///// bot自刷新好友和群列表（暂不可用）
        ///// </summary>
        ///// <param name="context"></param>
        ///// <param name="param"></param>
        ///// <returns></returns>
        //private string refreshList(MessageContext context, string[] param)
        //{
        //    //if (context.isGroup && Config.Instance.UserHasAdminAuthority(context.userId))
        //    //{
        //    //    Logger.Log($"更新好友列表和群列表...");
        //    //    RefreshFriendList();
        //    //    Logger.Log($"更新完毕，找到{Config.Instance.qqfriends.Count}个好友，{Config.Instance.qqgroups.Count}个群...");

        //    //    return $"更新完毕，找到{Config.Instance.qqfriends.Count}个好友，{Config.Instance.qqgroups.Count}个群...";
        //    //}
        //    return "";
        //}

        /// <summary>
        /// 让bot拍拍你
        /// 拍拍/贴贴
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string sendPoke(MessageContext context, string[] param)
        {
            context.client?.SendPoke(context.groupId, context.userId);
            //context.SendBack([new Poke { type="1", id="-1"}]);
            return null;
        }


        /// <summary>
        /// 让bot撤回最后n条消息
        /// 撤回/撤回N
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string handleRecall(MessageContext context, string[] param)
        {
            int num = 1;
            if (param.Length >= 2)
            {
                int.TryParse(param[1], out num);
            }
            if (num <= 0) num = 1;
            if (num >= 10) num = 10;
            var historys = HistoryManager.Instance.findMessage(Config.Instance.BotQQ, context.groupId);
            for (int i = 0; i < Math.Min(historys.Length, num); i++)
            {
                int hindex = historys.Length - i - 1;
                if (hindex < 0) break;
                string msgid = historys[hindex].messageId;
                if (string.IsNullOrEmpty(msgid))
                {
                    num++;
                }
                else
                {
                    Logger.Log($"?{msgid}");
                    context.client?.Send(new delete_msg(msgid));
                    historys[hindex].messageId = string.Empty;
                }
                

                //if (clientMirai != null)
                //{
                //    new GroupMessage(context.groupId, [
                //        new Quote(historys[i].messageId,context.groupId,context.userId,context.groupId,
                //            [new Plain(historys[i].message)])]
                //        ).Send(clientMirai);
                //    new Recall(historys[i].messageId).Send(clientMirai);
                //}
            }
            return null;

        }


        /// <summary>
        /// 帮你撤回消息。但bot必须有管理员权限。
        /// 帮我撤回/帮我撤回N
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string handleRecall2(MessageContext context, string[] param)
        {
            int num = 1;
            if (param.Length >= 2)
            {
                int.TryParse(param[1], out num);
            }
            if (num <= 0) num = 1;
            if (num >= 10) num = 10;
            var historys = HistoryManager.Instance.findMessage(context.userId, context.groupId);
            for (int i = 0; i < Math.Min(historys.Length, num); i++)
            {
                int hindex = historys.Length - i - 1;
                if (hindex < 0) break;
                string msgid = historys[hindex].messageId;
                if (string.IsNullOrEmpty(msgid))
                {
                    num++;
                }
                else
                {
                    Logger.Log($"?{msgid}");
                    context.client?.Send(new delete_msg(msgid));
                    historys[hindex].messageId = string.Empty;
                }
            }
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
            if(clientQQ != null)
            {
                foreach (var g in Config.Instance.groups)
                {
                    if ((long.Parse(g.Key) > 100000) && g.Value.Is("测试") && MyRandom.NextDouble() > (1 - 1 / 36))
                    {
                        //Logger.Log($"{g.Key} MMMM!");
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
        /// <summary>
        /// 刷新好友列表并更新配置文件
        /// </summary>
        public void RefreshFriendList()
        {
            try
            {
                //if (clientMirai != null)
                //{
                //    var fp = new FriendList().Send(clientMirai);
                //    Config.Instance.qqfriends.Clear();
                //    if (fp == null)
                //    {
                //        Logger.Log($"不会吧不会吧不会没有好友吧");

                //    }
                //    else
                //    {
                //        foreach (var f in fp)
                //        {
                //            var friend = Config.Instance.UserInfo(f.id);
                //            friend.Name = f.nickname;
                //            //friend.Mark = f.remark;
                //            friend.Tags.Add("好友");
                //            //friend.Type = PlayerType.Normal;
                //            Config.Instance.qqfriends.Add(f.id, f);
                //        }
                //    }




                //    var gp = new GroupList().Send(clientMirai);
                //    Config.Instance.qqgroups.Clear();
                //    Config.Instance.qqgroupMembers.Clear();
                //    if (gp == null)
                //    {
                //        Logger.Log($"不会吧不会吧不会没有群吧");

                //    }
                //    else
                //    {
                //        foreach (var g in gp)
                //        {
                //            var group = Config.Instance.GroupInfo(g.id);
                //            group.Name = g.name;
                //            var groupMembers = g.GetMemberList(clientMirai);
                //            if (groupMembers == null)
                //            {
                //                Logger.Log($"不会吧不会吧不会{g.id}是鬼群吧");
                //                continue;
                //            }
                //            Config.Instance.qqgroups.Add(g.id, g);
                //            Config.Instance.qqgroupMembers.Add(g.id, groupMembers);
                //            foreach (var gf in groupMembers)
                //            {
                //                var member = Config.Instance.UserInfo(gf.id);
                //                member.Mark = gf.memberName;    //群昵称？
                //            }
                //        }
                //    }

                //}
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
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












            if (context.isAskme)
            {
                // emoji deal
                //var group = Config.Instance.GroupInfo(context.groupId);
                //var user = Config.Instance.UserInfo(context.userId);

                var message = context.recvMessages.ToTextString();
                var elist = StaticUtil.ExtractEmojis(message);
                return DealEmojiMix(context,elist);
            }

                return false;

        }


        bool DealEmojiMix(MessageContext context, List<string> emojiList)
        {
            if (emojiList.Count >= 2)
            {
                string emojiA = emojiList[0];
                string emojiB = emojiList[1];
                var fres = new List<string>();
                fres.AddRange(Directory.GetFiles(Config.Instance.ResourceFullPath($"emojimix/"), $"{emojiA}*{emojiB}*.png"));
                fres.AddRange(Directory.GetFiles(Config.Instance.ResourceFullPath($"emojimix/"), $"{emojiB}*{emojiA}*.png"));
                if (fres.Count > 0)
                {
                    if (fres.Count > 1) Logger.Log($"{fres.Count} => {emojiA}*{emojiB}*.png");
                    _ = context.SendBack([
                        new Text($"{StaticUtil.UnicodePointsToEmoji(emojiA)}+{StaticUtil.UnicodePointsToEmoji(emojiB)}="),
                        new Image($"file://{fres.First()}"),
                    ]);
                    return true;
                }
            }
            else if (emojiList.Count == 1)
            {
                string emojiA = emojiList[0];
                var fff = Directory.GetFiles(Config.Instance.ResourceFullPath($"emojimix/"), $"*{emojiA}*.png");

                if (fff.Length > 0)
                {
                    var getf = fff[MyRandom.Next(fff.Length)];
                    if (File.Exists(getf))
                    {
                        var emojiB = Path.GetFileNameWithoutExtension(getf).Replace(emojiA, "").Replace("_", "").Replace("-ufe0f", "").Replace("-u200d", "");
                        _ = context.SendBack([
                            new Text($"{StaticUtil.UnicodePointsToEmoji(emojiA)}+{StaticUtil.UnicodePointsToEmoji(emojiB)}="),
                            new Image($"file://{getf}"),
                        ]);
                        return true;
                    }
                }
            }
            return false;
        }



        /// <summary>
        /// 查看并截断掉Message里的提示词
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        bool isAskMe(Message[] e)
        {
            foreach(var item in e)
            {
                if(item is At itemat)
                {
                    if(itemat.qq == Config.Instance.App.Avatar.myQQ)  return true;
                    
                }
                if(item is Text plain)
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
