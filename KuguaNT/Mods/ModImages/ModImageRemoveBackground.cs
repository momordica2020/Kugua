using Kugua.Core.Images;
using Kugua.Integrations.NTBot;
using Kugua.Mods.Base;
using System.Text.RegularExpressions;

namespace Kugua.Mods.ModImages
{
    public partial class ModImage : Mod
    {

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
                    context.SendBackImage(ImageHandler.ImgRemoveBackgrounds(oriImg));

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

    }
}
