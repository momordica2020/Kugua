using Kugua.Integrations.NTBot;
using System.Numerics;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Kugua
{
    /// <summary>
    /// 图片处理
    /// </summary>
    public class ModPicture : Mod
    {
        public override bool Init(string[] args)
        {
            ModCommands.Add(new ModCommand(new Regex(@"^来点(\S+)"), getSome));
            ModCommands.Add(new ModCommand(new Regex(@"^动(\S+)"), getMoveEmoji));
            ModCommands.Add(new ModCommand(new Regex(@"^生图(.*)", RegexOptions.Singleline), genImg));
            ModCommands.Add(new ModCommand(new Regex(@"^(\S+)语生图(.*)", RegexOptions.Singleline), genImg2));
            ModCommands.Add(new ModCommand(new Regex(@"^扭曲(.+)", RegexOptions.Singleline), genCaptcha));

            ModCommands.Add(new ModCommand(new Regex(@"做旧(\S*)", RegexOptions.Singleline), getOldJpg));
            ModCommands.Add(new ModCommand(new Regex(@"(.+)倍速", RegexOptions.Singleline), setGifSpeed));
            ModCommands.Add(new ModCommand(new Regex(@"镜像(.*)", RegexOptions.Singleline), setImgMirror));
            ModCommands.Add(new ModCommand(new Regex(@"旋转(.*)", RegexOptions.Singleline), setImgRotate));
            ModCommands.Add(new ModCommand(new Regex(@"抠图(.*)", RegexOptions.Singleline), setRemoveBackground));
            

            return true;
        }

 

        public override async Task<bool> HandleMessagesDIY(MessageContext context)
        {
            if (context.IsAskme)
            {
                // emoji deal
                //var group = Config.Instance.GroupInfo(context.groupId);
                //var user = Config.Instance.UserInfo(context.userId);

                var message = context.recvMessages.ToTextString();
                if (StaticUtil.isOnlyEmoji(message))
                {
                    // 只有纯emoji才触发拼合功能，防止误触
                    var elist = StaticUtil.ExtractEmojis(message);
                    return DealEmojiMix(context, elist);
                }
                
            }

            return false;
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
                        var newImgbase64 = ImageUtil.ImgSetGifSpeed(oriImg, speed);
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
                    var newImgbase64 = ImageUtil.ImgGreen(oriImg, (int)(50 * (1 - quality)), quality);
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


        /// <summary>
        /// emoji合成（直接发一到两个emoji给bot即可触发）/查看gif版的emoji（爱来自TG）
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
            if (context.IsImage)
            {
                // 以图生图

                desc += GPT.Instance.ZPGetImgDesc(context.PNG1Base64, "请用详细文字描述这张图的内容，以便我根据你的描述用AI生成新的图片。注意强调艺术风格、肢体动作、表情、物品的位置等。");
                context.SendBackPlain(desc);
                if (desc.Contains("ERROR")) return null;
            }
            else if (string.IsNullOrWhiteSpace(desc))
            {
                WaitNext(context, new ModCommand(new Regex(@"生图(.*)", RegexOptions.Singleline), genImg));
                return null;
            }
            
            if (!string.IsNullOrWhiteSpace(desc))
            {
                if(ModBank.Instance.GetPay(context.userId, GPT.ImgMoneyCost))
                {
                    if(!GPT.Instance.ZPImage(context, desc))
                    {
                        // fail
                        ModBank.Instance.TransMoney(Config.Instance.BotQQ, context.userId, GPT.ImgMoneyCost, out _);
                    }
                    return null;
                }
                else
                {
                    return $"{ModBank.unitName}不够，每次需{GPT.ImgMoneyCost.ToHans()}";
                } 
            }

            return "";
        }


        /// <summary>
        /// AI生成图片
        /// 外语生图 一个小女孩在下雨天奔跑
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string genImg2(MessageContext context, string[] param)
        {
            var lang = param[1];
            if (lang == "外") lang = "德";
            var desc = param[2];
            if (context.IsImage)
            {
                // 以图生图

                desc += GPT.Instance.ZPGetImgDesc(context.PNG1Base64, "请用详细文字描述这张图的内容，以便我根据你的描述用AI生成新的图片。注意强调艺术风格、肢体动作、表情、物品的位置等。");
                desc = ModTranslate.getTrans(desc, lang);
                context.SendBackPlain(desc);

                if (desc.Contains("ERROR")) return null;
            }
            else if (string.IsNullOrWhiteSpace(desc))
            {
                WaitNext(context, new ModCommand(new Regex(lang + @"语生图(.*)", RegexOptions.Singleline), genImg2));
                return null;
            }


            if (!string.IsNullOrWhiteSpace(desc))
            {
                if (!string.IsNullOrWhiteSpace(desc))
                {
                    if (ModBank.Instance.GetPay(context.userId, GPT.ImgMoneyCost))
                    {
                        desc = ModTranslate.getTrans(desc, lang);
                        if (!GPT.Instance.ZPImage(context, desc))
                        {
                            // fail
                            ModBank.Instance.TransMoney(Config.Instance.BotQQ, context.userId, GPT.ImgMoneyCost, out _);
                        }
                        return null;
                    }
                    else
                    {
                        return $"{ModBank.unitName}不够，每次需{GPT.ImgMoneyCost.ToHans()}";
                    }
                }

            }

            return "";
        }

        /// <summary>
        /// 生成风格化文本图片
        /// 扭曲 哈哈哈
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string genCaptcha(MessageContext context, string[] param)
        {
            string text = param[1].Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                var imgbase64 = ImageUtil.ImgGenerateCaptcha(text);
                context.SendBack([new Image($"base64://{imgbase64}")]);
            }

            return null;

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
                    var newImgbase64 = ImageUtil.ImgRemoveBackgrounds(oriImg);
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

        class NewsInfo
        {
            public string title;
            public string desc;
            public string url;
            public static Dictionary<string, string> platform = new Dictionary<string, string>(){
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
                else if (something == "车万" || something == "东方")
                {
                    files = Directory.GetFiles($"{Config.Instance.ResourceRootPath}/imgth", "*.*");
                }
                else if (something == "相声")
                {
                    files = Directory.GetFiles($"{Config.Instance.ResourceRootPath}/xiangsheng", "*.*");
                    string fname = files[MyRandom.Next(files.Length)];
                    var fdesc = fname.Split('-')[1].Trim();
                    context.SendBackPlain($"▶ {fdesc}", true);
                    context.SendBack([
                        new Record($"file://{fname}"),
                        ]);
                }
                else if (something == "emoji")
                {
                    files = Directory.GetFiles(Config.Instance.ResourceFullPath("/emojimix/"), "*.png");
                    string fname = files[MyRandom.Next(files.Length)];
                    var f = Path.GetFileNameWithoutExtension(fname).Replace("-u200d", "").Replace("-ufe0f", "").Split("_");
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
                            var nlist = news.Select(e => e.title).ToArray();
                            StaticUtil.FisherYates(nlist);
                            for (int i = 0; i < Math.Min(newsLen, news.Count); i++)
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



    }
}
