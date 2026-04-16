using ImageMagick;
using Kugua.Core.Images;
using Kugua.Integrations.NTBot;
using Kugua.Mods.Base;
using System.Text.RegularExpressions;

namespace Kugua.Mods.ModImages
{
    // 基础图片编辑操作
    public partial class ModImage : Mod
    {
        

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
                    var res = ImageHandler.ImgShake(img, degree);
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
                    newImgs.Add(ImageHandler.ImgCut(img, (up ? valh : 0), (down ? valh : 0), (left ? valw : 0), (right ? valw : 0)));
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
            if (string.IsNullOrWhiteSpace(param[1])) { virtical = true; horizional = true; }
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
                    var res = ImageHandler.ImgRoll(img, degree, 1);
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
                    foreach (var image in oriImg)
                    {
                        image.Rotate(ro);
                    }
                    context.SendBackImage(oriImg);
                    findImg = true;

                }
            }
            if (!findImg) WaitNext(context, new ModCommand(new Regex(@"旋转(.*)", RegexOptions.Singleline), setImgRotate));
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
                    context.SendBackImage(ImageHandler.ImgMirror(oriImg, degree));
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
                foreach (var img in oriImgs)
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
                WaitNext(context, new ModCommand(new Regex(@"垂直翻转(.*)", RegexOptions.Singleline), setTouchBall));

            }
            return null;
        }



        /// <summary>
        /// 图像摸摸
        /// 摸摸[图片]
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string setTouchBall(MessageContext context, string[] param)
        {
            if (context.IsImage)
            {
                var oriImgs = Network.DownloadImages(context);
                var newImgs = new List<MagickImageCollection>();
                foreach (var img in oriImgs)
                {
                    newImgs.Add(ImageHandler.ToElasticBounceGif(img));
                }
                context.SendBackImages(newImgs);
            }
            else
            {
                WaitNext(context, new ModCommand(new Regex(@"摸摸", RegexOptions.Singleline), setImgVerticalFlip));

            }
            return null;
        }



    }
}
