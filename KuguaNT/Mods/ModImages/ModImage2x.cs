using ImageMagick;
using Kugua.Core.Images;
using Kugua.Mods.Base;
using System.Text;
using System.Text.RegularExpressions;

namespace Kugua.Mods.ModImages
{
    // 图像放大
    public partial class ModImage : Mod
    {

        /// <summary>
        /// 图片高清放大
        /// 放大[图片]/高清[图片]
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string Get2X(MessageContext context, string[] param)
        {
            List<MagickImageCollection> res = new List<MagickImageCollection>();
            foreach (var item in context.Images)
            {
                var oriImg = Network.DownloadImage(item.url);
                for(int i=0;i<oriImg.Count;i++ )
                {
                    var newframe = Esrgan.SingleImageEsrgan((MagickImage)oriImg[i]);
                    oriImg[i].Dispose();
                    oriImg[i] = newframe;
                }
                res.Add(oriImg);
            }
            if(res.Count > 0) context.SendBackImages(res);

            if (!context.IsImage) WaitNext(context, new ModCommand(new Regex(@"^(高清|放大)(\S*)$", RegexOptions.Singleline), Get2X));
            return null;


        }

    }
}
