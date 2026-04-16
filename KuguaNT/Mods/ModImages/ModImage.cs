using ImageMagick;
using Kugua.Core;
using Kugua.Core.Algorithms;
using Kugua.Core.Images;
using Kugua.Integrations.AI;
using Kugua.Integrations.NTBot;
using Kugua.Mods.Base;
using Kugua.Mods.ModTextFunctions;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Serialization;
using NvAPIWrapper.Native.Display;
using NvAPIWrapper.Native.GPU;
using OpenAI;
using OpenAI.Responses;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Numerics;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using ZhipuApi.Modules;
using static System.Net.Mime.MediaTypeNames;
using Text = Kugua.Integrations.NTBot.Text;

namespace Kugua.Mods.ModImages
{
    /// <summary>
    /// 图片处理
    /// </summary>
    public partial class ModImage : Mod
    {
        public override bool Init(string[] args)
        {
            ModCommands.Add(new ModCommand(new Regex(@"^来点(\S+)"), getSome));
            ModCommands.Add(new ModCommand(new Regex(@"^动(\S+)"), getMoveEmoji));


            ModCommands.Add(new ModCommand(new Regex(@"^群(\S*)的头像$"), getGroupAvatar));
            ModCommands.Add(new ModCommand(new Regex(@"^(\S+)的头像$"), getUserAvatar));


            ModCommands.Add(new ModCommand(new Regex(@"^(\S*?)生图(.*)", RegexOptions.Singleline), genImg));
            //ModCommands.Add(new ModCommand(new Regex(@"^(\S+)语生图(.*)", RegexOptions.Singleline), genImg2));
            ModCommands.Add(new ModCommand(new Regex(@"^扭曲(.+)", RegexOptions.Singleline), genCaptcha));
            ModCommands.Add(new ModCommand(new Regex(@"^噪声([0-9]*)", RegexOptions.Singleline), genRandomPixel));
            ModCommands.Add(new ModCommand(new Regex(@"^取色(.*)", RegexOptions.Singleline), getColorCode));
            ModCommands.Add(new ModCommand(new Regex(@"^CIS\s*(\S+)\s(\S+)\s(\S+)", RegexOptions.Singleline), convertCIS));
            ModCommands.Add(new ModCommand(new Regex(@"^#[0-9A-Fa-f]{6}$", RegexOptions.Singleline), showColor,_needAsk: false));
            
            
            //ModCommands.Add(new ModCommand(new Regex(@"^#[0-9A-Fa-f]{6}$", RegexOptions.Singleline), showColor));
            ModCommands.Add(new ModCommand(new Regex(@"^拆$", RegexOptions.Singleline), unzipGif));
            ModCommands.Add(new ModCommand(new Regex(@"^拆(.+)x(.+)$", RegexOptions.Singleline), unzipImage));
            ModCommands.Add(new ModCommand(new Regex(@"^删(.+)$", RegexOptions.Singleline), removeGifFrame));
            ModCommands.Add(new ModCommand(new Regex(@"^拆帧(.*)$", RegexOptions.Singleline), unzipGifFrameImg));
            ModCommands.Add(new ModCommand(new Regex(@"^合(\S*)$", RegexOptions.Singleline), zipGif));
            ModCommands.Add(new ModCommand(new Regex(@"^竖拼$", RegexOptions.Singleline), combineV));
            ModCommands.Add(new ModCommand(new Regex(@"^横拼$", RegexOptions.Singleline), combineH));
            ModCommands.Add(new ModCommand(new Regex(@"^乱序$", RegexOptions.Singleline), randGif));
            ModCommands.Add(new ModCommand(new Regex(@"^gif$", RegexOptions.Singleline), webpToGif));

            ModCommands.Add(new ModCommand(new Regex(@"^识图$", RegexOptions.Singleline), GetOCR));
            ModCommands.Add(new ModCommand(new Regex(@"^(高清|放大)(\S*)$", RegexOptions.Singleline), Get2X));
            ModCommands.Add(new ModCommand(new Regex(@"^做旧(\S*)$", RegexOptions.Singleline), getOldJpg));
            ModCommands.Add(new ModCommand(new Regex(@"^(遗像|遗照)$", RegexOptions.Singleline), getYixiang));
            ModCommands.Add(new ModCommand(new Regex(@"^像素字(.*)", RegexOptions.Singleline), getPixelWords));
            ModCommands.Add(new ModCommand(new Regex(@"^(内?)抖(\S*)", RegexOptions.Singleline), getShake));
            ModCommands.Add(new ModCommand(new Regex(@"^滚动(\S*)", RegexOptions.Singleline), getRoll));
            ModCommands.Add(new ModCommand(new Regex(@"^反色$", RegexOptions.Singleline), changeColor));
            ModCommands.Add(new ModCommand(new Regex(@"^(.+)倍速$", RegexOptions.Singleline), setGifSpeed));
            ModCommands.Add(new ModCommand(new Regex(@"^镜像(.*)", RegexOptions.Singleline), setImgMirror));
            ModCommands.Add(new ModCommand(new Regex(@"^水平翻转(.*)", RegexOptions.Singleline), setImgHorzontalFlip));
            ModCommands.Add(new ModCommand(new Regex(@"^垂直翻转(.*)", RegexOptions.Singleline), setImgVerticalFlip));
            ModCommands.Add(new ModCommand(new Regex(@"^旋转(.*)", RegexOptions.Singleline), setImgRotate));
            ModCommands.Add(new ModCommand(new Regex(@"^抠图", RegexOptions.Singleline), setRemoveBackground));
            ModCommands.Add(new ModCommand(new Regex(@"^([上下左右]+)切(.+)$", RegexOptions.Singleline), setCut));
            ModCommands.Add(new ModCommand(new Regex(@"^([横竖]*)缩放(.+)$", RegexOptions.Singleline), setResize));
            ModCommands.Add(new ModCommand(new Regex(@"^(彩色)?幻影(坦克)?$", RegexOptions.Singleline), setHYTK));

            ModCommands.Add(new ModCommand(new Regex(@"^网格化2$", RegexOptions.Singleline), setPixelChange2));
            ModCommands.Add(new ModCommand(new Regex(@"^网格化$", RegexOptions.Singleline), setPixelChange1));

            ModCommands.Add(new ModCommand(new Regex(@"^摸摸$", RegexOptions.Singleline), setTouchBall));

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
                if (EmojiUtil.isOnlyEmoji(message))
                {
                    // 只有纯emoji才触发拼合功能，防止误触
                    var elist = EmojiUtil.ExtractEmojis(message);
                    return DealEmojiMix(context, elist);
                }
                
            }
            
            if(context.IsGroup && context.Group.Is("存图") && context.IsAdminUser)
            {
                // save image
                var imgs = context.Images;
                if (imgs.Count <= 0) return false;
                string dirName = $"{DateTime.Now.ToString("yyyyMMddHHmmss")}";
                string fullPath = Config.Instance.FullPath($"save/{dirName}");
                Directory.CreateDirectory(fullPath);
                int imgIndex = 1;
                foreach (var img in imgs)
                {
                    var mimg = Network.DownloadImage(img.url);
                    string imgName = $"{dirName}_{imgIndex++:D3}{Path.GetExtension(img.file)}";
                    mimg.Write($"{fullPath}/{imgName}");
                }
                context.SendBackText($"已存{imgs.Count}张图片，路径是{fullPath}");
                return true;
            }



            return false;
        }








        /// <summary>
        /// 图片文字识别（OCR）
        /// 识图[图片]
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string GetOCR(MessageContext context, string[] param)
        {
            StringBuilder res = new StringBuilder();
            foreach (var item in context.Images)
            {
                var oriImg = Network.DownloadImage(item.url);
                foreach (var frame in oriImg)
                {
                    res.AppendLine(context.client.GetOCR($"base64://{frame.ToBase64()}"));
                }
                
            }
            if (res.Length>0) context.SendBackText(res.ToString());

            if (!context.IsImage) WaitNext(context, new ModCommand(new Regex(@"^识图$", RegexOptions.Singleline), GetOCR));
            return null;


        }




        private string getUserQQNumberByText(MessageContext context, string text)
        {
            
            if (string.IsNullOrWhiteSpace(text)) return "";
            
            var qqMatch = Regex.Match(text, @"[1-9][0-9]{4,}");
            if (qqMatch.Success)
            {
                return qqMatch.Value;
            }
            else if(text=="我")
            {
                return context.userId;
            }
            else if (text == "你")
            {
                return Config.Instance.BotQQ;
            }
            else if(context.Ats.Count > 0)
            {
                var at = context.Ats.First();
                return at.qq;
            }
            else if(context.IsGroup)
            {
                var members = context.client.GetGroupMemberList(context.groupId);
                try
                {
                    var q = members.Where(m=>m.nickname==text || m.card==text).FirstOrDefault().user_id;
                    return q;
                }
                catch (Exception ex) { Logger.Log(ex); }
                
            }
            return "";
        }

        /// <summary>
        /// 获取用户头像图片
        /// 我的头像/ 287859992的头像
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string getUserAvatar(MessageContext context, string[] param)
        {
            var qq = getUserQQNumberByText(context, param[1]);
            if (string.IsNullOrWhiteSpace(qq)) return "";
            _ = context.SendBack([
                new Text($"qq={qq}"),
                new ImageSend($"https://q1.qlogo.cn/g?b=qq&nk={qq}&s=0")]);
            return null;
        }

        /// <summary>
        /// 获取群头像图片
        /// 群的头像 / 群1005625206的头像
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string getGroupAvatar(MessageContext context, string[] param)
        {
            string groupid = "";
            Match gm = Regex.Match(param[1], @"[1-9][0-9]{4,}");
            if(gm.Success)
            {
                groupid = gm.Value;
            }
            if (string.IsNullOrWhiteSpace(groupid)) groupid = context.groupId;

            List<Message> msgs = new List<Message>();
            msgs.Add(new Text($"group={groupid}"));
            int maxnum = 100;
            int skilleft = 1;
            for(int i=1;i<=maxnum;i++)
            {
                string uri = $"https://p.qlogo.cn/gh/0/{groupid}_{i}/0";
                var img = Network.DownloadImage(uri);
                if (img.First().Width < 41 && img.First().Height < 41)
                {
                    if(skilleft--<0) break;
                }
                else
                {
                    msgs.Add(new ImageSend(uri));
                }
            }
            msgs.Insert(1, new Text($"共找到{msgs.Count-1}个历史头像"));
            context.SendForward(msgs.ToArray());
            
            return null;
        }


        /// <summary>
        /// AI生成图片，可以以图生图，加pro前缀可以改为nano pro，否则用2.5。提示词会先转化为英语后生成
        /// 生图 [图片]将这张图改为真人风格 / pro生图 [图片]将这张图改为真人风格
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string genImg(MessageContext context, string[] param)
        {
            var type = param[1];
            var desc = param[2];
            //if (context.IsImage)
            //{
            //    // 以图生图
            //    //

            //        //var res = LLM.Instance.GenerateImage(desc, context.PNGBase64s);
            //        //context.SendBackText(desc);
            //        //if (res.Contains("ERROR")) return null;
            //}
            
            if (string.IsNullOrWhiteSpace(desc))
            {
                WaitNext(context, new ModCommand(new Regex(@"(\S*)生图(.*)", RegexOptions.Singleline), genImg));
                return null;
            }
            if (type.EndsWith("语"))
            {
                if (type == "外语") type = "英语";
                var desc2 = ModTranslate.getTrans(desc, type);
                if (string.IsNullOrWhiteSpace(desc2)) desc2 = desc;
                desc = desc2;
                type = "";
            }
            return GenerateImageAndSendback(context, desc, context.PNGBase64s, type);

        }


        ///// <summary>
        ///// AI生成图片
        ///// 外语生图 一个小女孩在下雨天奔跑
        ///// </summary>
        ///// <param name="context"></param>
        ///// <param name="param"></param>
        ///// <returns></returns>
        //private string genImg2(MessageContext context, string[] param)
        //{
        //    var lang = param[1];
        //    if (lang == "外") lang = "德";
        //    var desc = param[2];
        //    //if (context.IsImage)
        //    //{
        //    //    // 以图生图
        //    //    desc = ModTranslate.getTrans(desc, lang);
        //    //    context.SendBackText(desc);

        //    //    if (desc.Contains("ERROR")) return null;
        //    //}
        //    //else 
        //    if (string.IsNullOrWhiteSpace(desc))
        //    {
        //        WaitNext(context, new ModCommand(new Regex(lang + @"语生图(.*)", RegexOptions.Singleline), genImg2));
        //        return null;
        //    }

        //    desc = ModTranslate.getTrans(desc, lang);
        //    // TODO image to image
        //    return GenerateImageAndSendback(context, desc, null);

        //}

        string GenerateImageAndSendback(MessageContext context, string prompt, List<string> oriImages = null, string type="")
        {
            if (string.IsNullOrWhiteSpace(prompt)) return "(缺少描述)";
            BigInteger imgCost = BigInteger.Max(1000, context.User.Money / 33);
            if (ModBank.Instance.GetPay(context.userId, imgCost))
            {
                var imgBase64 = LLM.Instance.GenerateImage(prompt,oriImages, type);
                if (imgBase64 == null || imgBase64.Count <= 0)
                {
                    // 生图出错，返还钱币
                    ModBank.Instance.TransMoney(Config.Instance.BotQQ, context.userId, imgCost, out _);
                    return "画不出来";
                }
                else
                {
                    context.SendBack(imgBase64.Select(img=> new ImageSend($"base64://{img}")).ToArray());
                    return null;

                }
                
            }
            else
            {
                return $"{ModBank.unitName}不够，以你的身价得花{imgCost.ToHans()}";
            }
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
                context.SendBackImage(ImageHandler.ImgGenerateCaptcha(text));
            }

            return null;

        }



        





        class NewsInfo
        {
            public string title;
            public string desc;
            public string url;
            public static Dictionary<string, string> platform = new Dictionary<string, string>(){
            { "百度","Jb0vmloB1G" },
            { "少数派","Y2KeDGQdNP" },
            { "微博","KqndgxeLl9" },
            { "知乎","mproPpoq6O" },
                {"微信","WnBe01o371" },
                {"澎湃","wWmoO5Rd4E" },
            { "36氪","Q1Vd5Ko85R" },
            { "52破解","NKGoRAzel6" },
            { "b站","74KvxwokxM" },
            { "抖音","DpQvNABoNE" },
            { "头条","x9ozB4KoXb" },
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
                    files = Directory.GetFiles($"{Config.Instance.RootPath}/gifscat", "*.gif");

                }
                else if (something == "狐狸")
                {
                    files = Directory.GetFiles($"{Config.Instance.RootPath}/gifsfox", "*.gif");
                }
                else if (something == "非主流")
                {
                    files = Directory.GetFiles($"{Config.Instance.RootPath}/imgfzl", "*.*");
                }
                else if (something == "杀马特")
                {
                    files = Directory.GetFiles($"{Config.Instance.RootPath}/imgsmt", "*.*");
                }
                else if (something == "猫姬")
                {
                    files = Directory.GetFiles($"{Config.Instance.RootPath}/imgmj", "*.*");
                }
                else if (something == "车万" || something == "东方")
                {
                    files = Directory.GetFiles($"{Config.Instance.RootPath}/imgth", "*.*");
                }
                else if (something == "相声")
                {
                    files = Directory.GetFiles($"{Config.Instance.RootPath}/xiangsheng", "*.*");
                    string fname = files[MyRandom.Next(files.Length)];
                    var fdesc = fname.Split('-')[1].Trim();
                    context.SendBackText($"▶ {fdesc}", true);
                    context.SendBack([
                        new Record($"file://{fname}"),
                        ]);
                }
                else if (something == "emoji")
                {
                    files = Directory.GetFiles(Config.Instance.FullPath("/emojimix/"), "*.png");
                    string fname = files[MyRandom.Next(files.Length)];
                    var f = Path.GetFileNameWithoutExtension(fname).Replace("-u200d", "").Replace("-ufe0f", "").Split("_");
                    if (f.Length == 2)
                    {
                        context.SendBack([
                        new Text($"{EmojiUtil.UnicodePointsToEmoji(f[0])} + {EmojiUtil.UnicodePointsToEmoji(f[1])} = "),
                        new ImageSend($"file://{fname}"),
                        ]);
                    }
                    return null;
                }
                else if (NewsInfo.platform.ContainsKey(something))
                {
                    // 来点新闻系列
                    int newsLen = 15;
                    List<NewsInfo> news = new List<NewsInfo>();
                    string platcode = NewsInfo.platform[something];
                    var webstr = Network.Get($"https://tophub.today/n/{platcode}");
                    if (!string.IsNullOrWhiteSpace(webstr))
                    {
                        string res = "";
                        // Regular expression pattern to match the news titles
                        Regex regex = new Regex(@"<a href=""[^""]+"" target=""_blank"" rel=""nofollow"" itemid=""\d+"">([^<]+)<\/a>");

                        // Find matches
                        MatchCollection matches = regex.Matches(webstr);

                        // Store the matched titles in a list
                        List<string> newsTitles = new List<string>();

                        foreach (Match match in matches)
                        {
                            // The captured group contains the news title
                            newsTitles.Add(match.Groups[1].Value);
                        }

                        // Output the titles
                        foreach (string title in newsTitles)
                        {
                            var n = new NewsInfo
                            {
                                title = title,
                            };

                            news.Add(n);
                            //Console.WriteLine(title);
                        }
                        //var jo = JsonObject.Parse(webstr);
                        //if (jo["data"] != null)
                        //{
                        //    foreach (var item in jo["data"]?.AsArray())
                        //    {
                        //        var n = new NewsInfo
                        //        {
                        //            title = item["title"]?.ToString(),
                        //            url = item["url"]?.ToString(),
                        //            desc = item["desc"]?.ToString(),
                        //        };

                        //        news.Add(n);
                        //    }
                        //}
                        if (news.Count > 0)
                        {
                            var nlist = news.Select(e => e.title).ToArray();
                            Shuffle.FisherYates(nlist);
                            for (int i = 0; i < Math.Min(newsLen, news.Count); i++)
                            {
                                if (!string.IsNullOrWhiteSpace(nlist[i])) res += $"·{nlist[i]}\r\n";
                            }
                        }
                        //LocalStorage.writeLines(Config.Instance.ResourceFullPath("news.txt"), news.Select(v => $"{DateTime.Now.ToString("yyyyMMdd")}\t{v.title}\t{v.desc}\t{v.url}"));
                        context.SendBackText(res);
                        return null;
                    }
                }


                if (files != null)
                {
                    string fname = files[MyRandom.Next(files.Length)];
                    context.SendBack([new ImageSend($"file://{fname}")]);
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
