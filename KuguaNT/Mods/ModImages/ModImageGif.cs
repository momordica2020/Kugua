using ImageMagick;
using Kugua.Core.Algorithms;
using Kugua.Core.Images;
using Kugua.Integrations.NTBot;
using Kugua.Mods.Base;
using System.Text.RegularExpressions;

namespace Kugua.Mods.ModImages
{
    // gif

    public partial class ModImage : Mod
    {



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
                var frameParams = param[1].Split([',', ' ', '，'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (frameParams.Length >= 1) int.TryParse(frameParams[0], out beginFrame);
                if (frameParams.Length >= 2) int.TryParse(frameParams[1], out sumFrame);
                if (beginFrame < 0) beginFrame = thisimgs.Count - (Math.Abs(beginFrame) % thisimgs.Count);
                if (sumFrame == 0) sumFrame = 1;
                else if (sumFrame < 0)
                {
                    // 反向删帧
                    sumFrame = Math.Abs(sumFrame) % thisimgs.Count;
                    beginFrame = beginFrame - sumFrame;
                }
                List<IMagickImage<ushort>> removes = new List<IMagickImage<ushort>>();
                for (int i = 0; i < thisimgs.Count; i++)
                {
                    if (i + 1 >= beginFrame)
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
                var randImgs = Shuffle.FisherYates2(images);
                images.Clear();
                foreach (var img in randImgs) images.Add(img);
                images.OptimizeTransparency();
                context.SendBackImage(images);
            }


            if (!findImg) WaitNext(context, new ModCommand(new Regex(@"^乱序$", RegexOptions.Singleline), randGif));
            return null;
        }
        /// <summary>
        /// webp=>gif，单张图的话会变成png
        /// gif [图片]
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string webpToGif(MessageContext context, string[] param)
        {
            if (context.IsImage)
            {
                List<MagickImageCollection> images = new List<MagickImageCollection>();
                foreach (var img in context.Images)
                {
                    var thisimgs = Network.DownloadImage(img.url);
                    if (thisimgs.Count <= 1)
                    {
                        thisimgs.First().Format = MagickFormat.Png;
                    }
                    else
                    {
                        thisimgs.Coalesce();
                        foreach (var thisimg in thisimgs)
                        {

                            thisimg.Format = MagickFormat.Gif;
                            thisimg.GifDisposeMethod = GifDisposeMethod.Background;
                        }
                        thisimgs.OptimizeTransparency();
                    }

                    images.Add(thisimgs);

                }
                context.SendBackImages(images);
                //var randImgs = Util.FisherYates2(images);
                //images.Clear();
                //foreach (var img in randImgs) images.Add(img);
                //images.OptimizeTransparency();

            }
            else
            {
                WaitNext(context, new ModCommand(new Regex(@"^gif$", RegexOptions.Singleline), webpToGif));
            }
            return null;
        }



        /// <summary>
        /// 拆序列帧，即将帧图像合并成条图
        /// 拆帧 [图片]
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
                else if (col == 1) col = imgs.Count;
                var res = ImageHandler.GetGifFrames(imgs, col);
                context.SendBackImage(res, $"一共{imgs.Count}帧");
            }

            if (!findImg) WaitNext(context, new ModCommand(new Regex(@"^拆帧$", RegexOptions.Singleline), unzipGifFrameImg));
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
                res = ImageHandler.combineImage(res, img, false);
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
                res = ImageHandler.combineImage(res, img, true);
            }
            context.SendBackImage(res);



            return null;
        }

        /// <summary>
        /// 合并图片序列到一个gif里，或者合并序列帧
        /// 合 [图片1][图片2]/ 合 3x4 [图片]
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
                if (context.Images.Count >= 2)
                {
                    // multi images => gif
                    foreach (var img in context.Images)
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

                }
                else if (context.Images.Count == 1)
                {
                    // sprite frame => gif
                    int row = 1;
                    int col = 1;
                    if (param[1].Length > 1 && param[1].Contains('x'))
                    {
                        row = int.Parse(param[1].Split('x')[0]);
                        col = int.Parse(param[1].Split('x')[1]);
                        if (row <= 0) row = 1;
                        if (col <= 0) col = 1;
                    }
                    var thisimg = Network.DownloadImage(context.Images.First().url).First();
                    uint frameW = (uint)(thisimg.Width / col);
                    uint frameH = (uint)(thisimg.Height / row);
                    for (int r = 0; r < row; r++)
                    {
                        for (int c = 0; c < col; c++)
                        {
                            MagickGeometry cropGeometry = new MagickGeometry((int)(c * frameW), (int)(r * frameH), frameW, frameH);
                            MagickImage frameImg = (MagickImage)thisimg.Clone();
                            frameImg.Crop(cropGeometry);
                            frameImg.ResetPage();
                            frameImg.Format = MagickFormat.Gif;
                            frameImg.AnimationDelay = 5;
                            frameImg.GifDisposeMethod = GifDisposeMethod.Background;
                            images.Add(frameImg);
                        }
                    }
                }

                images.OptimizeTransparency();
                context.SendBackImage(images, $"一共{images.Count}帧");
            }


            if (!findImg) WaitNext(context, new ModCommand(new Regex(@"^合(\S*)$", RegexOptions.Singleline), zipGif));
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
                msgs.Add(new Text($"一共{imgs.Count()}张"));

                foreach (var img in imgs)
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
                msgs.Add(new Text($"一共{imgs.Count()}张"));
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
                        context.SendBackImage(ImageHandler.ImgSetGifSpeed(oriImg, speed));
                        findImg = true;
                    }
                }
                if (!findImg) WaitNext(context, new ModCommand(new Regex(@"(.+)倍速", RegexOptions.Singleline), setGifSpeed));
                return null;
            }


            return "";
        }



    }
}
