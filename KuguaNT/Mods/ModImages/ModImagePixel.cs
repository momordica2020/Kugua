using ImageMagick;
using Kugua.Core.Images;
using Kugua.Mods.Base;
using System.Text.RegularExpressions;

namespace Kugua.Mods.ModImages
{
    // 像素相关
    public partial class ModImage : Mod
    {
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
                context.SendBackImage(ImageHandler.setPixelChange1(oriImg));
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
                context.SendBackImage(ImageHandler.setPixelChange2(oriImg));
                findImg = true;
            }
            if (!context.IsImage) WaitNext(context, new ModCommand(new Regex(@"网格化2(.*)", RegexOptions.Singleline), setPixelChange2));
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
                context.SendBackImage(ImageHandler.ImgGenerateRandomPixel(size));
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
            foreach (var sentence in text)
            {
                var img2 = ImageHandler.ImgGeneratePixel2(sentence, "凤凰点阵体 12px", 12);
                imgs.Add(img2);
            }
            MagickImage img = (MagickImage)imgs.AppendVertically();

            //InstalledFontCollection MyFont = new InstalledFontCollection();
            //FontFamily[] MyFontFamilies = MyFont.Families;
            context.SendBackImage(img);


            return null;

        }



    }
}
