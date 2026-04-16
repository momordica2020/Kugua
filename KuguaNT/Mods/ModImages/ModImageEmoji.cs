using ImageMagick;
using Kugua.Core;
using Kugua.Core.Algorithms;
using Kugua.Core.Images;
using Kugua.Integrations.NTBot;
using Kugua.Mods.Base;
using System.Text.RegularExpressions;

namespace Kugua.Mods.ModImages
{
    // emojis
    public partial class ModImage : Mod
    {


        /// <summary>
        /// emoji合成（直接发一到两个emoji给bot即可触发）/查看gif版的emoji（爱来自TG）
        /// 😀😀/😀/动😀
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string getMoveEmoji(MessageContext context, string[] param)
        {
            var elist = EmojiUtil.ExtractEmojis(param[1]);
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
                        new Text($"{EmojiUtil.UnicodePointsToEmoji(emojiA)}+{EmojiUtil.UnicodePointsToEmoji(emojiB)}="),
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
                            new Text($"{EmojiUtil.UnicodePointsToEmoji(emojiA)}+{EmojiUtil.UnicodePointsToEmoji(emojiB)}="),
                            new ImageSend($"file://{getf}"),
                        ]);
                        return true;
                    }
                }
            }
            return false;
        }


    }
}
