using ImageMagick;
using Kugua.Integrations.AI;
using Kugua.Integrations.NTBot;
using Newtonsoft.Json.Serialization;
using NvAPIWrapper.Native.GPU;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using ZhipuApi.Modules;

namespace Kugua.Mods
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
            ModCommands.Add(new ModCommand(new Regex(@"^取色(.*)", RegexOptions.Singleline), getColorCode));
            ModCommands.Add(new ModCommand(new Regex(@"^#[0-9A-Fa-f]{6}$", RegexOptions.Singleline), showColor,false));
            ModCommands.Add(new ModCommand(new Regex(@"^拆$", RegexOptions.Singleline), unzipGif));
            ModCommands.Add(new ModCommand(new Regex(@"^删帧(.+)$", RegexOptions.Singleline), removeGifFrame));
            ModCommands.Add(new ModCommand(new Regex(@"^拆序列帧$", RegexOptions.Singleline), unzipGifFrameImg));
            ModCommands.Add(new ModCommand(new Regex(@"^合$", RegexOptions.Singleline), zipGif));
            ModCommands.Add(new ModCommand(new Regex(@"^乱序$", RegexOptions.Singleline), randGif));

            ModCommands.Add(new ModCommand(new Regex(@"做旧(\S*)", RegexOptions.Singleline), getOldJpg));
            ModCommands.Add(new ModCommand(new Regex(@"(.+)倍速", RegexOptions.Singleline), setGifSpeed));
            ModCommands.Add(new ModCommand(new Regex(@"镜像(.*)", RegexOptions.Singleline), setImgMirror));
            ModCommands.Add(new ModCommand(new Regex(@"旋转(.*)", RegexOptions.Singleline), setImgRotate));
            ModCommands.Add(new ModCommand(new Regex(@"抠图(.*)", RegexOptions.Singleline), setRemoveBackground));
            

            return true;
        }



        /// <summary>
        /// 从gif的特定帧数起，删掉n帧
        /// 删帧 -1 [图片]/ 删帧 1,3 [图片]
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string removeGifFrame(MessageContext context, string[] param)
        {
            bool findImg = false;

            if (context.IsImage)
            {
                findImg = true;
                var thisimgs = Network.DownloadImage(context.Images.FirstOrDefault().url);
                thisimgs.Coalesce();

                int beginFrame = 0;
                int sumFrame = 0;
                if (string.IsNullOrWhiteSpace(param[1])) return "";
                var frameParams = param[1].Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if(frameParams.Length >=1) int.TryParse(frameParams[0], out beginFrame);
                if (frameParams.Length >= 2) int.TryParse(frameParams[1], out sumFrame);
                if (beginFrame < 0) beginFrame = thisimgs.Count - (Math.Abs(beginFrame) % thisimgs.Count) ;
                if (sumFrame == 0) sumFrame = 1;
                else if(sumFrame < 0)
                {
                    // 反向删帧
                    sumFrame = Math.Abs(sumFrame) % thisimgs.Count;
                    beginFrame = beginFrame - sumFrame;
                }
                List<IMagickImage<ushort>> removes = new List<IMagickImage<ushort>>();
                for(int i = 0; i < thisimgs.Count; i++)
                {
                    if(i + 1 >= beginFrame)
                    {
                        removes.Add(thisimgs[i]);
                        if (removes.Count > sumFrame) break;
                    }
                }
                foreach (var removeFrame in removes) thisimgs.Remove(removeFrame);
                //thisimgs.OptimizeTransparency();
                thisimgs.Optimize();
                context.SendBackImage(thisimgs, $"剩余{thisimgs.Count}帧");
                return null;

            }


            if (!findImg) WaitNext(context, new ModCommand(new Regex(@"^删帧([\-0-9]+)(\.\.)?([\-0-9]*)", RegexOptions.Singleline), removeGifFrame));
            return null;
        }



        /// <summary>
        /// 乱序gif
        /// 乱序 [图片]
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string randGif(MessageContext context, string[] param)
        {
            bool findImg = false;
            if (context.IsImage)
            {
                findImg = true;
                MagickImageCollection images = new MagickImageCollection();
                foreach (var img in context.Images)
                {
                    var thisimgs = Network.DownloadImage(img.url);
                    thisimgs.Coalesce();
                    foreach (var thisimg in thisimgs)
                    {
                        thisimg.Format = MagickFormat.Gif;
                        images.Add(thisimg);
                    }
                }
                var randImgs = StaticUtil.FisherYates2(images);
                images.Clear();
                foreach (var img in randImgs) images.Add(img);
                images.OptimizeTransparency();
                context.SendBackImage(images);
            }


            if (!findImg) WaitNext(context, new ModCommand(new Regex(@"^乱序$", RegexOptions.Singleline), randGif));
            return null;
        }



        /// <summary>
        /// 拆序列帧
        /// 拆序列帧 [图片]
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string unzipGifFrameImg(MessageContext context, string[] param)
        {
            bool findImg = false;
            if (context.IsImage)
            {
                findImg = true;
                var imgs = Network.DownloadImage(context.Images.First().url);
                var res = ImageUtil.GetGifFrames(imgs);
                context.SendBackImage(res, $"一共{imgs.Count}帧");
            }

            if (!findImg) WaitNext(context, new ModCommand(new Regex(@"^拆序列帧$", RegexOptions.Singleline), unzipGifFrameImg));
            return null;
        }



        /// <summary>
        /// 合并图片序列到一个gif里
        /// 合 [图片1][图片2]
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string zipGif(MessageContext context, string[] param)
        {
            bool findImg = false;
            if (context.IsImage)
            {
                findImg = true;
                MagickImageCollection images = new MagickImageCollection();
                uint minH = 400;
                uint minW = 400;
                foreach(var img in context.Images)
                {
                    var thisimgs = Network.DownloadImage(img.url);
                    thisimgs.Coalesce();
                    foreach (var thisimg in thisimgs)
                    {
                        thisimg.Format = MagickFormat.Gif;
                        thisimg.AnimationDelay = 5;
                        thisimg.GifDisposeMethod = GifDisposeMethod.Background;
                        if (thisimg.Height < thisimg.Width)
                        {
                            uint targetWidth = (uint)((double)thisimg.Width / thisimg.Height * minH);
                            thisimg.Resize(targetWidth, minH);
                        }
                        else
                        {
                            uint targetHeight = (uint)((double)thisimg.Height / thisimg.Width * minW);
                            thisimg.Resize(minW, targetHeight);
                        }
                            
                        images.Add(thisimg);
                    }
                }

                images.OptimizeTransparency();
                context.SendBackImage(images,$"一共{images.Count}帧");
            }


            if (!findImg) WaitNext(context, new ModCommand(new Regex(@"^合$", RegexOptions.Singleline), zipGif));
            return null;
        }


        /// <summary>
        /// 拆解gif图片成为一组图片
        /// 拆 [图片]
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string unzipGif(MessageContext context, string[] param)
        {
            bool findImg = false;
            if (context.IsImage)
            {
                findImg = true;
                var imgs = Network.DownloadImage(context.Images.First().url);
                imgs.Coalesce();
                List<Message> msgs = new List<Message>();
                
                foreach(var img in imgs)
                {
                    img.Format = MagickFormat.Png;
                    msgs.Add(new ImageSend((MagickImage)img));
                }

                context.SendForward(msgs.ToArray());
            }


            if (!findImg) WaitNext(context, new ModCommand(new Regex(@"^拆$", RegexOptions.Singleline), unzipGif));
            return null;
        }

        private string showColor(MessageContext context, string[] param)
        {
            string colorCode = param[0];
            context.SendBack([new ImageSend(ImageUtil.GetColorSample(colorCode))]);
            return null;
        }


        /// <summary>
        /// 提取图片色卡
        /// 取色[图片]
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string getColorCode(MessageContext context, string[] param)
        {
            bool findImg = false;
            if (context.IsImage)
            {
                findImg = true;
                var colors = ImageUtil.ImageColorExtract(Network.DownloadImage(context.Images.First().url), 3);
                List<string> colorCodes = new List<string>();
                foreach(var color in colors)
                {
                    colorCodes.Add(color.ToHexString());
                }
                context.SendBack([
                    new ImageSend(ImageUtil.GetColorSamples(colorCodes)),
                    new Text(string.Join("   ", colorCodes)),
                ]);
            }


            if (!findImg) WaitNext(context, new ModCommand(new Regex(@"取色(\S*)", RegexOptions.Singleline), getColorCode));
            return null;
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
                    if (item is ImageBasic itemImg)
                    {
                        var oriImg = Network.DownloadImage(itemImg.url);
                        context.SendBackImage(ImageUtil.ImgSetGifSpeed(oriImg, speed));
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
                if (item is ImageBasic itemImg)
                {
                    var oriImg = Network.DownloadImage(itemImg.url);
                    //Logger.Log("? == " + oriImg.Count);
                    context.SendBackImage(ImageUtil.ImgGreen(oriImg, (int)(50 * (1 - quality)), quality));
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
                    var fff = Directory.GetFiles(Config.Instance.FullPath($"emojitg/"), $"*{emoji.Replace("u", "")}*.gif");
                    if (fff.Length > 0)
                    {
                        msgs.Add(new ImageSend($"file://{fff[MyRandom.Next(fff.Length)]}"));
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

                desc += LLM.Instance.HSGetImgDesc(context.PNG1Base64, "请用200字以内的文字描述这张图的内容，务必注意细节。注意强调艺术风格、肢体动作、表情、物品的位置等。", "png");
                context.SendBackText(desc);
                if (desc.Contains("ERROR")) return null;
            }
            else if (string.IsNullOrWhiteSpace(desc))
            {
                WaitNext(context, new ModCommand(new Regex(@"生图(.*)", RegexOptions.Singleline), genImg));
                return null;
            }
            
            if (!string.IsNullOrWhiteSpace(desc))
            {
                if(ModBank.Instance.GetPay(context.userId, LLM.ImgMoneyCost))
                {
                    if(!LLM.Instance.ZPImage(context, desc))
                    {
                        // fail
                        ModBank.Instance.TransMoney(Config.Instance.BotQQ, context.userId, LLM.ImgMoneyCost, out _);
                    }
                    return null;
                }
                else
                {
                    return $"{ModBank.unitName}不够，每次需{LLM.ImgMoneyCost.ToHans()}";
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

                desc += LLM.Instance.ZPGetImgDesc(context.PNG1Base64, "请用详细文字描述这张图的内容，以便我根据你的描述用AI生成新的图片。注意强调艺术风格、肢体动作、表情、物品的位置等。");
                desc = ModTranslate.getTrans(desc, lang);
                context.SendBackText(desc);

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
                    if (ModBank.Instance.GetPay(context.userId, LLM.ImgMoneyCost))
                    {
                        desc = ModTranslate.getTrans(desc, lang);
                        if (!LLM.Instance.ZPImage(context, desc))
                        {
                            // fail
                            ModBank.Instance.TransMoney(Config.Instance.BotQQ, context.userId, LLM.ImgMoneyCost, out _);
                        }
                        return null;
                    }
                    else
                    {
                        return $"{ModBank.unitName}不够，每次需{LLM.ImgMoneyCost.ToHans()}";
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
                context.SendBackImage(ImageUtil.ImgGenerateCaptcha(text));
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
                if (item is ImageBasic itemImg)
                {
                    var oriImg = Network.DownloadImage(itemImg.url);
                    context.SendBackImage(ImageUtil.ImgRotate(oriImg, ro));
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
                if (item is ImageBasic itemImg)
                {
                    var oriImg = Network.DownloadImage(itemImg.url);
                    context.SendBackImage(ImageUtil.ImgRemoveBackgrounds(oriImg));

                    //if (string.IsNullOrWhiteSpace(newImgbase64))
                    //{
                    //    // fail
                    //    return "";
                    //}
                    //context.SendBack([
                    //    //new At(context.userId, null),
                    //    new ImageSend($"base64://{newImgbase64}"),
                    //]);
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
                if (item is ImageBasic itemImg)
                {
                    var oriImg = Network.DownloadImage(itemImg.url);
                    context.SendBackImage(ImageUtil.ImgMirror(oriImg, degree));
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
                fres.AddRange(Directory.GetFiles(Config.Instance.FullPath($"emojimix/"), $"{emojiA}*{emojiB}*.png"));
                fres.AddRange(Directory.GetFiles(Config.Instance.FullPath($"emojimix/"), $"{emojiB}*{emojiA}*.png"));
                if (fres.Count > 0)
                {
                    if (fres.Count > 1) Logger.Log($"{fres.Count} => {emojiA}*{emojiB}*.png");
                    _ = context.SendBack([
                        new Text($"{StaticUtil.UnicodePointsToEmoji(emojiA)}+{StaticUtil.UnicodePointsToEmoji(emojiB)}="),
                        new ImageSend($"file://{fres.First()}"),
                    ]);
                    return true;
                }
            }
            else if (emojiList.Count == 1)
            {
                string emojiA = emojiList[0];
                var fff = Directory.GetFiles(Config.Instance.FullPath($"emojimix/"), $"*{emojiA}*.png");

                if (fff.Length > 0)
                {
                    var getf = fff[MyRandom.Next(fff.Length)];
                    if (File.Exists(getf))
                    {
                        var emojiB = Path.GetFileNameWithoutExtension(getf).Replace(emojiA, "").Replace("_", "").Replace("-ufe0f", "").Replace("-u200d", "");
                        _ = context.SendBack([
                            new Text($"{StaticUtil.UnicodePointsToEmoji(emojiA)}+{StaticUtil.UnicodePointsToEmoji(emojiB)}="),
                            new ImageSend($"file://{getf}"),
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
                        new Text($"{StaticUtil.UnicodePointsToEmoji(f[0])} + {StaticUtil.UnicodePointsToEmoji(f[1])} = "),
                        new ImageSend($"file://{fname}"),
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
