using ImageMagick;
using Kugua.Core.Images;
using Kugua.Integrations.NTBot;
using Kugua.Mods.Base;
using System.Text.RegularExpressions;

namespace Kugua.Mods.ModImages
{
    // 图片滤镜相关
    public partial class ModImage : Mod
    {



        private string showColor(MessageContext context, string[] param)
        {
            string colorCode = param[0];
            context.SendBack([new ImageSend(ImageHandler.GetColorSample(colorCode))]);
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
                var colors = ImageHandler.ImageColorExtract(Network.DownloadImage(context.Images.First().url), 3);
                List<string> colorCodes = new List<string>();
                foreach (var color in colors)
                {
                    colorCodes.Add(color.ToHexString());
                }
                context.SendBack([
                    new ImageSend(ImageHandler.GetColorSamples(colorCodes)),
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
            (int R, int G, int B) = ColorConvert.xyYtoRGB(x, y, Y);
            context.SendBackImage(ImageHandler.GetColorSamples([$"#{R:X2}{G:X2}{B:X2}"], 200), $"{R},{G},{B}(#{R:X2}{G:X2}{B:X2})");

            return null;
        }

        /// <summary>
        /// 黑白图像
        /// 遗像[图片]/遗照[图片]
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string getYixiang(MessageContext context, string[] param)
        {
            double quality = 0.75;
            if (param.Length >= 2)
            {
                double.TryParse(param[1], out quality);
                if (quality < 0.1) quality = 0.75;
                if (quality > 0.95) quality = 0.95;
            }

            List<MagickImageCollection> imgs = new List<MagickImageCollection>();
            foreach (var item in context.Images)
            {
                var oriImg = Network.DownloadImage(item.url);
                foreach (var frame in oriImg)
                {
                    frame.Grayscale(PixelIntensityMethod.Rec601Luminance);
                }
                imgs.Add(oriImg);
            }
            if (imgs.Count > 0) context.SendBackImages(imgs);

            if (!context.IsImage) WaitNext(context, new ModCommand(new Regex(@"^遗像$", RegexOptions.Singleline), getYixiang));
            return null;


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
            double quality = 0.75;
            if (param.Length >= 2)
            {
                double.TryParse(param[1], out quality);
                if (quality < 0.1) quality = 0.75;
                if (quality > 0.95) quality = 0.95;
            }


            foreach (var item in context.Images)
            {
                var oriImg = Network.DownloadImage(item.url);
                context.SendBackImage(ImageHandler.ImgGreen(oriImg, (int)(50 * (1 - quality)), quality));

            }

            if (!context.IsImage) WaitNext(context, new ModCommand(new Regex(@"^做旧(\S*)$", RegexOptions.Singleline), getOldJpg));
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

    }
}
