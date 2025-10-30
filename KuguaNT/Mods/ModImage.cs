using ImageMagick;
using Kugua.Core;
using Kugua.Integrations.AI;
using Kugua.Integrations.NTBot;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Serialization;
using NvAPIWrapper.Native.GPU;
using OpenAI;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Numerics;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using ZhipuApi.Modules;

namespace Kugua.Mods
{
    /// <summary>
    /// 图片处理
    /// </summary>
    public class ModImage : Mod
    {
        public override bool Init(string[] args)
        {
            ModCommands.Add(new ModCommand(new Regex(@"^来点(\S+)"), getSome));
            ModCommands.Add(new ModCommand(new Regex(@"^动(\S+)"), getMoveEmoji));

            ModCommands.Add(new ModCommand(new Regex(@"^生图(.*)", RegexOptions.Singleline), genImg));
            ModCommands.Add(new ModCommand(new Regex(@"^(\S+)语生图(.*)", RegexOptions.Singleline), genImg2));
            ModCommands.Add(new ModCommand(new Regex(@"^扭曲(.+)", RegexOptions.Singleline), genCaptcha));
            ModCommands.Add(new ModCommand(new Regex(@"^噪声([0-9]*)", RegexOptions.Singleline), genRandomPixel));
            ModCommands.Add(new ModCommand(new Regex(@"^取色(.*)", RegexOptions.Singleline), getColorCode));
            ModCommands.Add(new ModCommand(new Regex(@"^CIS\s*(\S+)\s(\S+)\s(\S+)", RegexOptions.Singleline), convertCIS));
            ModCommands.Add(new ModCommand(new Regex(@"^#[0-9A-Fa-f]{6}$", RegexOptions.Singleline), showColor,_needAsk: false));
            
            
            //ModCommands.Add(new ModCommand(new Regex(@"^#[0-9A-Fa-f]{6}$", RegexOptions.Singleline), showColor));
            ModCommands.Add(new ModCommand(new Regex(@"^拆$", RegexOptions.Singleline), unzipGif));
            ModCommands.Add(new ModCommand(new Regex(@"^拆(.+)x(.+)$", RegexOptions.Singleline), unzipImage));
            ModCommands.Add(new ModCommand(new Regex(@"^删(.+)$", RegexOptions.Singleline), removeGifFrame));
            ModCommands.Add(new ModCommand(new Regex(@"^拆序列帧(.*)$", RegexOptions.Singleline), unzipGifFrameImg));
            ModCommands.Add(new ModCommand(new Regex(@"^合$", RegexOptions.Singleline), zipGif));
            ModCommands.Add(new ModCommand(new Regex(@"^竖拼$", RegexOptions.Singleline), combineV));
            ModCommands.Add(new ModCommand(new Regex(@"^横拼$", RegexOptions.Singleline), combineH));
            ModCommands.Add(new ModCommand(new Regex(@"^乱序$", RegexOptions.Singleline), randGif));

            ModCommands.Add(new ModCommand(new Regex(@"^做旧(\S*)", RegexOptions.Singleline), getOldJpg));
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
            

            return true;
        }



        /// <summary>
        /// 从gif的特定帧数起，删掉n帧
        /// 删 -1 [图片]/ 删 1,3 [图片]
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
                var frameParams = param[1].Split([',',' ','，'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
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


            if (!findImg) WaitNext(context, new ModCommand(new Regex(@"^删([\-0-9]+)(\.\.)?([\-0-9]*)", RegexOptions.Singleline), removeGifFrame));
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
                var randImgs = Util.FisherYates2(images);
                images.Clear();
                foreach (var img in randImgs) images.Add(img);
                images.OptimizeTransparency();
                context.SendBackImage(images);
            }


            if (!findImg) WaitNext(context, new ModCommand(new Regex(@"^乱序$", RegexOptions.Singleline), randGif));
            return null;
        }



        /// <summary>
        /// 拆序列帧，即将帧图像合并成条图
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
                int col = 1;
                int.TryParse(param[1], out col);
                if (col < 1) col = 1;

                findImg = true;
                var imgs = Network.DownloadImage(context.Images.First().url);
                if (param[1].Trim() == "竖")
                {
                    col = 1;
                }
                else if(col == 1) col = imgs.Count;
                var res = ImageUtil.GetGifFrames(imgs, col);
                context.SendBackImage(res, $"一共{imgs.Count}帧");
            }

            if (!findImg) WaitNext(context, new ModCommand(new Regex(@"^拆序列帧$", RegexOptions.Singleline), unzipGifFrameImg));
            return null;
        }


        /// <summary>
        /// 竖着拼在一起
        /// 竖拼 [图片1][图片2]
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string combineV(MessageContext context, string[] param)
        {
            if (!context.IsImage)
            {
                WaitNext(context, new ModCommand(new Regex(@"^竖拼$", RegexOptions.Singleline), combineV));
            }
            List<MagickImageCollection> list = Network.DownloadImages(context);
            MagickImageCollection res = list.First();
            list.RemoveAt(0);
            foreach (var img in list)
            {
                res = ImageUtil.combineImage(res, img, false);
            }
            context.SendBackImage(res);
            


            return null;
        }


        /// <summary>
        /// 横着拼在一起
        /// 横拼 [图片1][图片2]
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string combineH(MessageContext context, string[] param)
        {
            if (!context.IsImage)
            {
                WaitNext(context, new ModCommand(new Regex(@"^横拼$", RegexOptions.Singleline), combineH));
            }
            List<MagickImageCollection> list = Network.DownloadImages(context);
            MagickImageCollection res = list.First();
            list.RemoveAt(0);
            foreach (var img in list)
            {
                res = ImageUtil.combineImage(res, img, true);
            }
            context.SendBackImage(res);



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



        /// <summary>
        /// 拆解图片为col x row的碎片
        /// 拆3x4 [图片]
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string unzipImage(MessageContext context, string[] param)
        {
            bool findImg = false;
            if (context.IsImage)
            {
                findImg = true;

                uint row, col;
                uint.TryParse(param[1], out col);
                uint.TryParse(param[2], out row);
                if (row <= 0) row = 1;
                if (col <= 0) col = 1;
                var imgs = Network.DownloadImage(context.Images.First().url);
                imgs.Coalesce();
                List<Message> msgs = new List<Message>();

                foreach (var img in imgs)
                {
                    List<MagickImage> subImages = new List<MagickImage>();

                    uint width = (uint)(img.Width / col);
                    uint height = (uint)(img.Height / row);

                    for (int y = 0; y < row; y++)
                    {
                        for (int x = 0; x < col; x++)
                        {
                            MagickGeometry cropGeometry = new MagickGeometry((int)(x * width), (int)(y * height), width, height);
                            MagickImage subImage = (MagickImage)img.Clone();
                            subImage.Crop(cropGeometry);
                            subImage.Format = MagickFormat.Png;
                            msgs.Add(new ImageSend((MagickImage)subImage));
                            
                            //subImages.Add(subImage);
                        }
                    }


                    // only first img
                    break;

                    
                }

                context.SendForward(msgs.ToArray());
            }


            if (!findImg) WaitNext(context, new ModCommand(new Regex(@"^拆(.+)x(.+)$", RegexOptions.Singleline), unzipImage));
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

        /// <summary>
        /// CIS转换为RGB。传入参数是xyY
        /// CIS 0.595 0.328 0.092
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string convertCIS(MessageContext context, string[] param)
        {
            double x, y, Y;
            x = Double.Parse(param[1]);
            y = Double.Parse(param[2]);
            Y = Double.Parse(param[3]);
            (int R, int G, int B) = ColorConvertUtil.xyYtoRGB(x, y, Y);
            context.SendBackImage(ImageUtil.GetColorSamples([$"#{R:X2}{G:X2}{B:X2}"], 200), $"{R},{G},{B}(#{R:X2}{G:X2}{B:X2})");

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
                if (Util.isOnlyEmoji(message))
                {
                    // 只有纯emoji才触发拼合功能，防止误触
                    var elist = Util.ExtractEmojis(message);
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
        /// 图片抖动！数字是震级，一般取0~1的小数
        /// 抖[图片]/抖0,5[图片]/内抖0.3[图片]
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string getShake(MessageContext context, string[] param)
        {
            double degree = .5;
            if (!double.TryParse(param[1], out degree))
            {
                degree = .5;
            }
            if (context.IsImage)
            {
                var oriImgs = Network.DownloadImages(context);
                var resImgs = new List<MagickImageCollection>();
                foreach (var img in oriImgs)
                {
                    var res = ImageUtil.ImgShake(img, degree);
                    resImgs.Add(res);
                }
                context.SendBackImages(resImgs);
            }
            else
            {
                WaitNext(context, new ModCommand(new Regex(@"^(内?)抖(\S*)", RegexOptions.Singleline), getShake));

            }
            return null;
        }


        /// <summary>
        /// 图片切
        /// 右切50/左上切10%/上下左右切5.5%
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string setCut(MessageContext context, string[] param)
        {
            bool up = false, down = false, left = false, right = false;
            bool percent = false;
            double inputval = 0;
            if (param[1].Contains("上")) up = true;
            if (param[1].Contains("下")) down = true;
            if (param[1].Contains("左")) left = true;
            if (param[1].Contains("右")) right = true;
            if (param[2].EndsWith("%"))
            {
                percent = true;
                double.TryParse(param[2].TrimEnd('%'), out inputval);
            }
            else
            {
                double.TryParse(param[2], out inputval);
            }
                




            string res = "";
            if (inputval < 0) inputval = 0;
            if (context.IsImage)
            {
                var oriImgs = Network.DownloadImages(context);
                var newImgs = new List<MagickImageCollection>();
                foreach (var img in oriImgs)
                {
                   

                    uint originalWidth = img.First().Width;
                    uint originalHeight = img.First().Height;
                    int valh = 0;
                    int valw = 0;
                    if (percent)
                    {
                        valh = (int)(originalHeight * inputval);
                        valw = (int)(originalWidth * inputval);
                    }
                    else if (double.Abs(inputval) < 1)
                    {
                        // 按比例
                        valh = (int)(inputval * originalHeight);
                        valw = (int)(inputval * originalWidth);
                    }
                    else
                    {
                        valh = (int)inputval;
                        valw = (int)inputval;
                    }
                    newImgs.Add(ImageUtil.ImgCut(img, (up ? valh : 0), (down ? valh : 0), (left ? valw : 0), (right ? valw : 0)));
                    int startX = left ? (int)valw : 0;
                    int startY = up ? (int)valh : 0;
                    uint newWidth = originalWidth - (uint)startX - (right ? (uint)valw : 0);
                    uint newHeight = originalHeight - (uint)startY - (down ? (uint)valh : 0);
                    res += $"{newWidth}x{newHeight}\r\n";
                }
                context.SendBackImages(newImgs, res);
            }
            else
            {
                WaitNext(context, new ModCommand(new Regex(@"^([上下左右]+)切(.+)", RegexOptions.Singleline), setCut));

            }
            return null;
        }


        /// <summary>
        /// 图片缩放
        /// 横缩放0.1/横竖缩放2.2/缩放0.5
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string setResize(MessageContext context, string[] param)
        {
            bool virtical = false, horizional = false;
            double inputval = 0;
            if (string.IsNullOrWhiteSpace(param[1])) { virtical= true;horizional = true; }
            if (param[1].Contains("横")) horizional = true;
            if (param[1].Contains("竖")) virtical = true;
            double.TryParse(param[2], out inputval);
            if (inputval <= 0) inputval = 0.1;
            string res = "";
            if (context.IsImage)
            {
                var oriImgs = Network.DownloadImages(context);
                foreach (var img in oriImgs)
                {
                    uint newW = (uint)(img.First().Width * (horizional ? inputval : 1));
                    uint newH = (uint)(img.First().Height * (virtical ? inputval : 1));
                    res += $"{newW}x{newH}\r\n";
                    foreach (var frame in img)
                    {
                        frame.Resize(newW, newH);
                    }
                }
                context.SendBackImages(oriImgs, res);
            }
            else
            {
                WaitNext(context, new ModCommand(new Regex(@"^([横竖]*)缩放(.+)", RegexOptions.Singleline), setResize));

            }
            return null;
        }

        /// <summary>
        /// 图片滚动，参数是方位的角度
        /// 滚动90[图片]
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string getRoll(MessageContext context, string[] param)
        {
            double degree = 0;
            if (!double.TryParse(param[1], out degree))
            {
                degree = 0;
            }
            degree = degree * Math.PI / 180;
            if (context.IsImage)
            {
                var oriImgs = Network.DownloadImages(context);
                var resImgs = new List<MagickImageCollection>();
                foreach (var img in oriImgs)
                {
                    var res = ImageUtil.ImgRoll(img, degree, 1);
                    resImgs.Add(res);
                }
                context.SendBackImages(resImgs);
            }
            else
            {
                WaitNext(context, new ModCommand(new Regex(@"^滚动(\S*)", RegexOptions.Singleline), getRoll));

            }
            return null;
        }

        /// <summary>
        /// 图片反色
        /// 反色[图片]
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>

        private string changeColor(MessageContext context, string[] param)
        {
            if (!context.IsImage) WaitNext(context, new ModCommand(new Regex(@"反色(\S*)", RegexOptions.Singleline), changeColor));
            else
            {
                foreach (var itemImg in context.Images)
                {
                    var oriImg = Network.DownloadImage(itemImg.url);
                    oriImg.Coalesce();
                    foreach (var image in oriImg)
                    {
                        image.Negate();
                    }

                    oriImg.OptimizeTransparency();

                    //Logger.Log("? == " + oriImg.Count);
                    context.SendBackImage(oriImg);
                }
            }

            return null;


        }

        /// <summary>
        /// 图片幻影坦克
        /// 幻影[白底图片][黑底图片]
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>

        private string setHYTK(MessageContext context, string[] param)
        {
            
            if (!context.IsImage || context.Images.Count < 2) WaitNext(context, new ModCommand(new Regex(@"^(彩色)?幻影(坦克)?$", RegexOptions.Singleline), setHYTK));
            else
            {
                MagickImage img1 = (MagickImage)Network.DownloadImage(context.Images[0].url).First();
                MagickImage img2 = (MagickImage)Network.DownloadImage(context.Images[1].url).First();
                if (!string.IsNullOrWhiteSpace(param[1]))
                {
                    var res = ImageUtil.ImageBlendColorful(img1, img2);
                    context.SendBackImage(res);
                }
                else
                {
                    var res = ImageUtil.ImageBlend(img1, img2);
                    context.SendBackImage(res);
                }
                
                
            }

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
            var elist = Util.ExtractEmojis(param[1]);
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
            //if (context.IsImage)
            //{
            //    // 以图生图

            //    desc += LLM.Instance.HSGetImgDesc(context.PNG1Base64, "请用200字以内的文字描述这张图的内容，务必注意细节。注意强调艺术风格、肢体动作、表情、物品的位置等。", "png");
            //    context.SendBackText(desc);
            //    if (desc.Contains("ERROR")) return null;
            //}
            //else 
            if (string.IsNullOrWhiteSpace(desc))
            {
                WaitNext(context, new ModCommand(new Regex(@"生图(.*)", RegexOptions.Singleline), genImg));
                return null;
            }

            return GenerateImageAndSendback(context, desc, null);

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
            //if (context.IsImage)
            //{
            //    // 以图生图
            //    desc = ModTranslate.getTrans(desc, lang);
            //    context.SendBackText(desc);

            //    if (desc.Contains("ERROR")) return null;
            //}
            //else 
            if (string.IsNullOrWhiteSpace(desc))
            {
                WaitNext(context, new ModCommand(new Regex(lang + @"语生图(.*)", RegexOptions.Singleline), genImg2));
                return null;
            }

            desc = ModTranslate.getTrans(desc, lang);
            // TODO image to image
            return GenerateImageAndSendback(context, desc, null);

        }

        string GenerateImageAndSendback(MessageContext context, string prompt, string[] oriImages = null)
        {
            if (string.IsNullOrWhiteSpace(prompt)) return "";
            BigInteger imgCost = BigInteger.Max(1000, context.User.Money / 33);
            if (ModBank.Instance.GetPay(context.userId, imgCost))
            {
                var imgBase64 = LLM.Instance.HSGetImg(prompt);
                if (imgBase64 == null)
                {
                    // 生图出错，返还钱币
                    ModBank.Instance.TransMoney(Config.Instance.BotQQ, context.userId, imgCost, out _);
                    return "画不出来";
                }
                else
                {
                    context.SendBack([new ImageSend($"base64://{imgBase64}")]);
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
                context.SendBackImage(ImageUtil.ImgGenerateCaptcha(text));
            }

            return null;

        }



        /// <summary>
        /// 生成12pixel像素字文本图片
        /// 像素字 哈哈哈
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string getPixelWords(MessageContext context, string[] param)
        {
            if (string.IsNullOrWhiteSpace(param[1])) return null;
            string[] text = param[1].Trim().Split('\n');
            MagickImageCollection imgs = new MagickImageCollection();
            foreach(var sentence in text)
            {
                var img2 = ImageUtil.ImgGeneratePixel2(sentence, "凤凰点阵体 12px", 12);
                imgs.Add(img2);
            }
            MagickImage img = (MagickImage)imgs.AppendVertically();
            
            //InstalledFontCollection MyFont = new InstalledFontCollection();
            //FontFamily[] MyFontFamilies = MyFont.Families;
            context.SendBackImage(img);
            

            return null;

        }
        

        /// <summary>
        /// 生成噪声图片
        /// 噪声100
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string genRandomPixel(MessageContext context, string[] param)
        {
            string text = param[1].Trim();
            int size = 0;
            int.TryParse(text, out size);
            if (!string.IsNullOrWhiteSpace(text))
            {
                context.SendBackImage(ImageUtil.ImgGenerateRandomPixel(size));
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
        /// 图片网格化扩张一倍
        /// 网格化[图片]
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string setPixelChange1(MessageContext context, string[] param)
        {
            bool findImg = false;
            foreach (var item in context.Images)
            {
                var oriImg = Network.DownloadImage(item.url);
                context.SendBackImage(ImageUtil.setPixelChange1(oriImg));
                findImg = true;
            }
            if (!context.IsImage) WaitNext(context, new ModCommand(new Regex(@"网格化(.*)", RegexOptions.Singleline), setPixelChange1));
            return null;
        }


        /// <summary>
        /// 图片网格化扣洞
        /// 网格化2[图片]
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string setPixelChange2(MessageContext context, string[] param)
        {
            bool findImg = false;
            foreach (var item in context.Images)
            {
                var oriImg = Network.DownloadImage(item.url);
                context.SendBackImage(ImageUtil.setPixelChange2(oriImg));
                findImg = true;
            }
            if (!context.IsImage) WaitNext(context, new ModCommand(new Regex(@"网格化2(.*)", RegexOptions.Singleline), setPixelChange2));
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
            //double ro = 0;
            //if (!double.TryParse(param[1], out ro))
            //{ ro = 0; }
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

        /// <summary>
        /// 图像水平翻转
        /// 水平翻转[图片]
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string setImgHorzontalFlip(MessageContext context, string[] param)
        {
            if (context.IsImage)
            {
                var oriImgs = Network.DownloadImages(context);
                foreach(var img in oriImgs)
                {
                    foreach (var image in img)
                    {
                        image.Flop();
                    }
                }
                context.SendBackImages(oriImgs);
            }
            else
            {
                WaitNext(context, new ModCommand(new Regex(@"水平翻转(.*)", RegexOptions.Singleline), setImgHorzontalFlip));

            }
            return null;
        }


        /// <summary>
        /// 图像垂直翻转
        /// 垂直翻转[图片]
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string setImgVerticalFlip(MessageContext context, string[] param)
        {
            if (context.IsImage)
            {
                var oriImgs = Network.DownloadImages(context);
                foreach (var img in oriImgs)
                {
                    foreach (var image in img)
                    {
                        image.Flip();
                    }
                }
                context.SendBackImages(oriImgs);
            }
            else
            {
                WaitNext(context, new ModCommand(new Regex(@"垂直翻转(.*)", RegexOptions.Singleline), setImgVerticalFlip));

            }
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
                        new Text($"{Util.UnicodePointsToEmoji(emojiA)}+{Util.UnicodePointsToEmoji(emojiB)}="),
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
                            new Text($"{Util.UnicodePointsToEmoji(emojiA)}+{Util.UnicodePointsToEmoji(emojiB)}="),
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
                        new Text($"{Util.UnicodePointsToEmoji(f[0])} + {Util.UnicodePointsToEmoji(f[1])} = "),
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
                            Util.FisherYates(nlist);
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
