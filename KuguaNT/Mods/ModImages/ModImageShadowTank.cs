using ImageMagick;
using Kugua.Core.Images;
using Kugua.Mods.Base;
using System.Text.RegularExpressions;

namespace Kugua.Mods.ModImages
{
    public partial class ModImage : Mod
    {
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
                    var res = ImageHandler.ImageBlendColorful(img1, img2);
                    context.SendBackImage(res);
                }
                else
                {
                    var res = ImageHandler.ImageBlend(img1, img2);
                    context.SendBackImage(res);
                }


            }

            return null;


        }
    }
}
