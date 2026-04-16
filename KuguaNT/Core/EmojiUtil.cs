using Kugua.Core;
using Microsoft.AspNetCore.Components.Forms;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Kugua.Core
{
    internal static class EmojiUtil
    {

        /// <summary>
        /// 将代码emoji变成真的emoji
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ConvertEmoji(string input)
        {
            // 匹配 [emoji=xxxx] 的格式
            const string pattern = @"\[emoji=([A-Fa-f0-9]+)\]";
            var match = Regex.Match(input, pattern);

            if (match.Success)
            {
                string hex = match.Groups[1].Value;

                // 将十六进制字符串转换为字节数组
                byte[] bytes = new byte[hex.Length / 2];
                for (int i = 0; i < hex.Length; i += 2)
                {
                    bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
                }

                // 将字节数组解码为字符串 (UTF-8)
                return Encoding.UTF8.GetString(bytes);
            }

            return input; // 如果不匹配格式，返回原字符串
        }

        public static string ConvertToUnicodeEscapeWithSurrogates(string input)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (char.IsHighSurrogate(c) && i + 1 < input.Length && char.IsLowSurrogate(input[i + 1]))
                {
                    int codePoint = char.ConvertToUtf32(c, input[i + 1]);
                    sb.AppendFormat("\\U{0:x8}", codePoint);
                    i++; // 跳过低位代理
                }
                else if (c >= 0x2000 && c <= 0xFFFF)
                //else if ((c >= 0x3400 && c <= 0x4DBF) || (c >= 0x4E00 && c <= 0x9FFF))
                {
                    sb.AppendFormat("\\u{0:x4}", (int)c);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString().Replace(",\"", ", \"").Replace(":\"",": \"");
        }

        public static string emojiChange(string emoji)
        {
            string res = emoji;

            var changeList = new Dictionary<string, string>
            {
                { "🐅" , "🐯" },
                { "" , "🐯" },
            };

            if (changeList.TryGetValue(emoji, out res))
            {
                return res;
            }

            return res;
        }



        /// <summary>
        /// 将内容中的emoji解析成emoji编号序列，形如 "u1f004"
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static List<string> ExtractEmojis(string input)
        {
            List<string> emojiList = new List<string>();

            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsHighSurrogate(input[i]) && i + 1 < input.Length && char.IsLowSurrogate(input[i + 1]))
                {
                    // 处理 emoji 的代理对
                    int codePoint = char.ConvertToUtf32(input, i);
                    if (IsEmojiCodePoint(codePoint))
                    {
                        // 处理为代理对格式 "uXXXX-uXXXX"
                        int highSurrogate = input[i];
                        int lowSurrogate = input[i + 1];
                        int allS = char.ConvertToUtf32(input, i);
                        emojiList.Add($"u{allS:x4}");
                    }
                    i++; // 跳过低代理字符
                }
                else if (!char.IsSurrogate(input[i]))
                {
                    // 处理非代理对的单个字符
                    int codePoint = char.ConvertToUtf32(input, i);
                    if (IsEmojiCodePoint(codePoint))
                    {
                        emojiList.Add($"u{codePoint:x4}");
                    }
                }
            }

            return emojiList;
        }


        /// <summary>
        /// 判断一个字符是否是 emoji
        /// </summary>
        /// <param name="codePoint"></param>
        /// <returns></returns>
        public static bool IsEmojiCodePoint(int codePoint)
        {
            // 一些常见 emoji 的 Unicode 范围
            return codePoint >= 0x1F600 && codePoint <= 0x1F64F  // 表情符号
                || codePoint >= 0x1F300 && codePoint <= 0x1F5FF  // 符号和图像
                || codePoint >= 0x1F680 && codePoint <= 0x1F6FF  // 交通符号
                || codePoint >= 0x1F700 && codePoint <= 0x1F77F  // 占卜符号
                || codePoint >= 0x1F780 && codePoint <= 0x1F7FF  // 地理符号
                || codePoint >= 0x1F800 && codePoint <= 0x1F8FF  // 中古字母
                || codePoint >= 0x1F900 && codePoint <= 0x1F9FF  // 表情符号补充
                || codePoint >= 0x1FA00 && codePoint <= 0x1FA6F  // 新增的表情符号
                || codePoint >= 0x1F000 && codePoint <= 0x1F02F  // 象棋和棋盘符号
                || codePoint >= 0x1F030 && codePoint <= 0x1F09F  // 麻将符号
                || codePoint >= 0x1F0A0 && codePoint <= 0x1F0FF  // 扑克牌符号
                || codePoint >= 0x1F100 && codePoint <= 0x1F1FF  // 字母和数字符号
                || codePoint >= 0x1F200 && codePoint <= 0x1F251  // 封闭式字符
                || codePoint >= 0x2600 && codePoint <= 0x26FF    // Miscellaneous Symbols
                || codePoint >= 0x2700 && codePoint <= 0x27BF    // Dingbats
                || codePoint >= 0x2B50 && codePoint <= 0x2B59   // 箭头符号
                ;
        }

        /// <summary>
        /// 判断输入的数据里是不是只有emoji，如果解析中途未匹配就返回false
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool isOnlyEmoji(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsHighSurrogate(input[i]) && i + 1 < input.Length && char.IsLowSurrogate(input[i + 1]))
                {
                    // 处理 emoji 的代理对
                    int codePoint = char.ConvertToUtf32(input, i);
                    if (!IsEmojiCodePoint(codePoint))
                    {
                        return false;
                    }
                    i++; // 跳过低代理字符
                }
                else if (!char.IsSurrogate(input[i]))
                {
                    // 处理非代理对的单个字符
                    int codePoint = char.ConvertToUtf32(input, i);
                    if (!IsEmojiCodePoint(codePoint))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 移除字符串中的emoji
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string RemoveEmojis(string input)
        {
            // 正则表达式匹配 Emoji 字符
            string emojiPattern = "[♂♀😀😁😂🤣😃😄😅😆😉😊😋😎😍😘🥰😗😙😚☺️🙂🤗🤩🤔🤨😐😑😶🙄😏😣😥😮🤐😯😪😫😴😌😛😜😝🤤😒😓😔😕🙃🤑😲☹️🙁😖😞😟😤😢😭😦😧😨😩🤯😬😰😱🥵🥶😳🤪😵😡😠🤬😷🤒🤕🤢🤮🤧😇🤠🤡🥳🥴🥺🤥🤫🤭🧐🤓😈👿👹👺💀👻👽🤖💩😺😸😹😻😼😽🙀😿😾👶👧🧒👦👩🧑👨👵🧓👴👲👳👳🧕🧔👱👱👨‍🦰👩‍🦰👨‍🦱👩‍🦱👨‍🦲👩‍🦲👨‍🦳👩‍🦳🦸🦸🦹🦹👮👮👷👷💂💂🕵️🕵️👩‍⚕️👨‍⚕️👩‍🌾👨‍🌾👩‍🍳👨‍🍳👩‍🎓👨‍🎓👩‍🎤👨‍🎤👩‍🏫👨‍🏫👩‍🏭👨‍🏭👩‍💻👨‍💻👩‍💼👨‍💼👩‍🔧👨‍🔧👩‍🔬👨‍🔬👩‍🎨👨‍🎨👩‍🚒👨‍🚒👩‍✈️👨‍✈️👩‍🚀👨‍🚀👩‍⚖️👨‍⚖️👰🤵👸🤴🤶🎅🧙🧙🧝🧝🧛🧛🧟🧟🧞🧞🧜🧜🧚🧚👼🤰🤱🙇🙇💁💁🙅🙅🙆🙆🙋🙋🤦🤦🤷🤷🙎🙎🙍🙍💇💇💆💆🧖🧖💅🤳💃🕺👯👯🕴🚶🚶🏃🏃👫👭👬💑👩‍❤️‍👩👨‍❤️‍👨💏👩‍❤️‍💋‍👩👨‍❤️‍💋‍👨👪👨‍👩‍👧👨‍👩‍👧‍👦👨‍👩‍👦‍👦👨‍👩‍👧‍👧👩‍👩‍👦👩‍👩‍👧👩‍👩‍👧‍👦👩‍👩‍👦‍👦👩‍👩‍👧‍👧👨‍👨‍👦👨‍👨‍👧👨‍👨‍👧‍👦👨‍👨‍👦‍👦👨‍👨‍👧‍👧👩‍👦👩‍👧👩‍👧‍👦👩‍👦‍👦👩‍👧‍👧👨‍👦👨‍👧👨‍👧‍👦👨‍👦‍👦👨‍👧‍👧🤲👐🙌👏🤝👍👎👊✊🤛🤜🤞✌️🤟🤘👌👈👉👆👇☝️✋🤚🖐🖖👋🤙💪🦵🦶🖕✍️🙏💍💄💋👄👅👂👃👣👁👀🧠🦴🦷🗣👤👥🧥👚👕👖👔👗👙👘👠👡👢👞👟🥾🥿🧦🧤🧣🎩🧢👒🎓⛑👑👝👛👜💼🎒👓🕶🥽🥼🌂🧵🧶👶🏻👦🏻👧🏻👨🏻👩🏻👱🏻👱🏻👴🏻👵🏻👲🏻👳🏻👳🏻👮🏻👮🏻👷🏻👷🏻💂🏻💂🏻🕵🏻🕵🏻👩🏻‍⚕️👨🏻‍⚕️👩🏻‍🌾👨🏻‍🌾👩🏻‍🍳👨🏻‍🍳👩🏻‍🎓👨🏻‍🎓👩🏻‍🎤👨🏻‍🎤👩🏻‍🏫👨🏻‍🏫👩🏻‍🏭👨🏻‍🏭👩🏻‍💻👨🏻‍💻👩🏻‍💼👨🏻‍💼👩🏻‍🔧👨🏻‍🔧👩🏻‍🔬👨🏻‍🔬👩🏻‍🎨👨🏻‍🎨👩🏻‍🚒👨🏻‍🚒👩🏻‍✈️👨🏻‍✈️👩🏻‍🚀👨🏻‍🚀👩🏻‍⚖️👨🏻‍⚖️🤶🏻🎅🏻👸🏻🤴🏻👰🏻🤵🏻👼🏻🤰🏻🙇🏻🙇🏻💁🏻💁🏻🙅🏻🙅🏻🙆🏻🙆🏻🙋🏻🙋🏻🤦🏻🤦🏻🤷🏻🤷🏻🙎🏻🙎🏻🙍🏻🙍🏻💇🏻💇🏻💆🏻💆🏻🕴🏻💃🏻🕺🏻🚶🏻🚶🏻🏃🏻🏃🏻🤲🏻👐🏻🙌🏻👏🏻🙏🏻👍🏻👎🏻👊🏻✊🏻🤛🏻🤜🏻🤞🏻✌🏻🤟🏻🤘🏻👌🏻👈🏻👉🏻👆🏻👇🏻☝🏻✋🏻🤚🏻🖐🏻🖖🏻👋🏻🤙🏻💪🏻🖕🏻✍🏻🤳🏻💅🏻👂🏻👃🏻👶🏼👦🏼👧🏼👨🏼👩🏼👱🏼👱🏼👴🏼👵🏼👲🏼👳🏼👳🏼👮🏼👮🏼👷🏼👷🏼💂🏼💂🏼🕵🏼🕵🏼👩🏼‍⚕️👨🏼‍⚕️👩🏼‍🌾👨🏼‍🌾👩🏼‍🍳👨🏼‍🍳👩🏼‍🎓👨🏼‍🎓👩🏼‍🎤👨🏼‍🎤👩🏼‍🏫👨🏼‍🏫👩🏼‍🏭👨🏼‍🏭👩🏼‍💻👨🏼‍💻👩🏼‍💼👨🏼‍💼👩🏼‍🔧👨🏼‍🔧👩🏼‍🔬👨🏼‍🔬👩🏼‍🎨👨🏼‍🎨👩🏼‍🚒👨🏼‍🚒👩🏼‍✈️👨🏼‍✈️👩🏼‍🚀👨🏼‍🚀👩🏼‍⚖️👨🏼‍⚖️🤶🏼🎅🏼👸🏼🤴🏼👰🏼🤵🏼👼🏼🤰🏼🙇🏼🙇🏼💁🏼💁🏼🙅🏼🙅🏼🙆🏼🙆🏼🙋🏼🙋🏼🤦🏼🤦🏼🤷🏼🤷🏼🙎🏼🙎🏼🙍🏼🙍🏼💇🏼💇🏼💆🏼💆🏼🕴🏼💃🏼🕺🏼🚶🏼🚶🏼🏃🏼🏃🏼🤲🏼👐🏼🙌🏼👏🏼🙏🏼👍🏼👎🏼👊🏼✊🏼🤛🏼🤜🏼🤞🏼✌🏼🤟🏼🤘🏼👌🏼👈🏼👉🏼👆🏼👇🏼☝🏼✋🏼🤚🏼🖐🏼🖖🏼👋🏼🤙🏼💪🏼🖕🏼✍🏼🤳🏼💅🏼👂🏼👃🏼👶🏽👦🏽👧🏽👨🏽👩🏽👱🏽👱🏽👴🏽👵🏽👲🏽👳🏽👳🏽👮🏽👮🏽👷🏽👷🏽💂🏽💂🏽🕵🏽🕵🏽👩🏽‍⚕️👨🏽‍⚕️👩🏽‍🌾👨🏽‍🌾👩🏽‍🍳👨🏽‍🍳👩🏽‍🎓👨🏽‍🎓👩🏽‍🎤👨🏽‍🎤👩🏽‍🏫👨🏽‍🏫👩🏽‍🏭👨🏽‍🏭👩🏽‍💻👨🏽‍💻👩🏽‍💼👨🏽‍💼👩🏽‍🔧👨🏽‍🔧👩🏽‍🔬👨🏽‍🔬👩🏽‍🎨👨🏽‍🎨👩🏽‍🚒👨🏽‍🚒👩🏽‍✈️👨🏽‍✈️👩🏽‍🚀👨🏽‍🚀👩🏽‍⚖️👨🏽‍⚖️🤶🏽🎅🏽👸🏽🤴🏽👰🏽🤵🏽👼🏽🤰🏽🙇🏽🙇🏽💁🏽💁🏽🙅🏽🙅🏽🙆🏽🙆🏽🙋🏽🙋🏽🤦🏽🤦🏽🤷🏽🤷🏽🙎🏽🙎🏽🙍🏽🙍🏽💇🏽💇🏽💆🏽💆🏽🕴🏼💃🏽🕺🏽🚶🏽🚶🏽🏃🏽🏃🏽🤲🏽👐🏽🙌🏽👏🏽🙏🏽👍🏽👎🏽👊🏽✊🏽🤛🏽🤜🏽🤞🏽✌🏽🤟🏽🤘🏽👌🏽👈🏽👉🏽👆🏽👇🏽☝🏽✋🏽🤚🏽🖐🏽🖖🏽👋🏽🤙🏽💪🏽🖕🏽✍🏽🤳🏽💅🏽👂🏽👃🏽👶🏾👦🏾👧🏾👨🏾👩🏾👱🏾👱🏾👴🏾👵🏾👲🏾👳🏾👳🏾👮🏾👮🏾👷🏾👷🏾💂🏾💂🏾🕵🏾🕵🏾👩🏾‍⚕️👨🏾‍⚕️👩🏾‍🌾👨🏾‍🌾👩🏾‍🍳👨🏾‍🍳👩🏾‍🎓👨🏾‍🎓👩🏾‍🎤👨🏾‍🎤👩🏾‍🏫👨🏾‍🏫👩🏾‍🏭👨🏾‍🏭👩🏾‍💻👨🏾‍💻👩🏾‍💼👨🏾‍💼👩🏾‍🔧👨🏾‍🔧👩🏾‍🔬👨🏾‍🔬👩🏾‍🎨👨🏾‍🎨👩🏾‍🚒👨🏾‍🚒👩🏾‍✈️👨🏾‍✈️👩🏾‍🚀👨🏾‍🚀👩🏾‍⚖️👨🏾‍⚖️🤶🏾🎅🏾👸🏾🤴🏾👰🏾🤵🏾👼🏾🤰🏾🙇🏾🙇🏾💁🏾💁🏾🙅🏾🙅🏾🙆🏾🙆🏾🙋🏾🙋🏾🤦🏾🤦🏾🤷🏾🤷🏾🙎🏾🙎🏾🙍🏾🙍🏾💇🏾💇🏾💆🏾💆🏾🕴🏾💃🏾🕺🏾🚶🏾🚶🏾🏃🏾🏃🏾🤲🏾👐🏾🙌🏾👏🏾🙏🏾👍🏾👎🏾👊🏾✊🏾🤛🏾🤜🏾🤞🏾✌🏾🤟🏾🤘🏾👌🏾👈🏾👉🏾👆🏾👇🏾☝🏾✋🏾🤚🏾🖐🏾🖖🏾👋🏾🤙🏾💪🏾🖕🏾✍🏾🤳🏾💅🏾👂🏾👃🏾👶🏿👦🏿👧🏿👨🏿👩🏿👱🏿👱🏿👴🏿👵🏿👲🏿👳🏿👳🏿👮🏿👮🏿👷🏿👷🏿💂🏿💂🏿🕵🏿🕵🏿👩🏿‍⚕️👨🏿‍⚕️👩🏿‍🌾👨🏿‍🌾👩🏿‍🍳👨🏿‍🍳👩🏿‍🎓👨🏿‍🎓👩🏿‍🎤👨🏿‍🎤👩🏿‍🏫👨🏿‍🏫👩🏿‍🏭👨🏿‍🏭👩🏿‍💻👨🏿‍💻👩🏿‍💼👨🏿‍💼👩🏿‍🔧👨🏿‍🔧👩🏿‍🔬👨🏿‍🔬👩🏿‍🎨👨🏿‍🎨👩🏿‍🚒👨🏿‍🚒👩🏿‍✈️👨🏿‍✈️👩🏿‍🚀👨🏿‍🚀👩🏿‍⚖️👨🏿‍⚖️🤶🏿🎅🏿👸🏿🤴🏿👰🏿🤵🏿👼🏿🤰🏿🙇🏿🙇🏿💁🏿💁🏿🙅🏿🙅🏿🙆🏿🙆🏿🙋🏿🙋🏿🤦🏿🤦🏿🤷🏿🤷🏿🙎🏿🙎🏿🙍🏿🙍🏿💇🏿💇🏿💆🏿💆🏿🕴🏿💃🏿🕺🏿🚶🏿🚶🏿🏃🏿🏃🏿🤲🏿👐🏿🙌🏿👏🏿🙏🏿👍🏿👎🏿👊🏿✊🏿🤛🏿🤜🏿🤞🏿✌🏿🤟🏿🤘🏿👌🏿👈🏿👉🏿👆🏿👇🏿☝🏿✋🏿🤚🏿🖐🏿🖖🏿👋🏿🤙🏿💪🏿🖕🏿✍🏿🤳🏿💅🏿👂🏿👃🏿🐶🐱🐭🐹🐰🦊🦝🐻🐼🦘🦡🐨🐯🦁🐮🐷🐽🐸🐵🙈🙉🙊🐒🐔🐧🐦🐤🐣🐥🦆🦢🦅🦉🦚🦜🦇🐺🐗🐴🦄🐝🐛🦋🐌🐚🐞🐜🦗🕷🕸🦂🦟🦠🐢🐍🦎🦖🦕🐙🦑🦐🦀🐡🐠🐟🐬🐳🐋🦈🐊🐅🐆🦓🦍🐘🦏🦛🐪🐫🦙🦒🐃🐂🐄🐎🐖🐏🐑🐐🦌🐕🐩🐈🐓🦃🕊🐇🐁🐀🐿🦔🐾🐉🐲🌵🎄🌲🌳🌴🌱🌿☘️🍀🎍🎋🍃🍂🍁🍄🌾💐🌷🌹🥀🌺🌸🌼🌻🌞🌝🌛🌜🌚🌕🌖🌗🌘🌑🌒🌓🌔🌙🌎🌍🌏💫⭐️🌟✨⚡️☄️💥🔥🌪🌈☀️🌤⛅️🌥☁️🌦🌧⛈🌩🌨❄️☃️⛄️🌬💨💧💦☔️☂️🌊🌫🍏🍎🍐🍊🍋🍌🍉🍇🍓🍈🍒🍑🍍🥭🥥🥝🍅🍆🥑🥦🥒🥬🌶🌽🥕🥔🍠🥐🍞🥖🥨🥯🧀🥚🍳🥞🥓🥩🍗🍖🌭🍔🍟🍕🥪🥙🌮🌯🥗🥘🥫🍝🍜🍲🍛🍣🍱🥟🍤🍙🍚🍘🍥🥮🥠🍢🍡🍧🍨🍦🥧🍰🎂🍮🍭🍬🍫🍿🧂🍩🍪🌰🥜🍯🥛🍼☕️🍵🥤🍶🍺🍻🥂🍷🥃🍸🍹🍾🥄🍴🍽🥣🥡🥢⚽️🏀🏈⚾️🥎🏐🏉🎾🥏🎱🏓🏸🥅🏒🏑🥍🏏⛳️🏹🎣🥊🥋🎽⛸🥌🛷🛹🎿⛷🏂🏋️🏋🏻🏋🏼🏋🏽🏋🏾🏋🏿🏋️🏋🏻🏋🏼🏋🏽🏋🏾🏋🏿🤼🤼🤸🤸🏻🤸🏼🤸🏽🤸🏾🤸🏿🤸🤸🏻🤸🏼🤸🏽🤸🏾🤸🏿⛹️⛹🏻⛹🏼⛹🏽⛹🏾⛹🏿⛹️⛹🏻⛹🏼⛹🏽⛹🏾⛹🏿🤺🤾🤾🏻🤾🏼🤾🏾🤾🏾🤾🏿🤾🤾🏻🤾🏼🤾🏽🤾🏾🤾🏿🏌️🏌🏻🏌🏼🏌🏽🏌🏾🏌🏿🏌️🏌🏻🏌🏼🏌🏽🏌🏾🏌🏿🏇🏇🏻🏇🏼🏇🏽🏇🏾🏇🏿🧘🧘🏻🧘🏼🧘🏽🧘🏾🧘🏿🧘🧘🏻🧘🏼🧘🏽🧘🏾🧘🏿🏄🏄🏻🏄🏼🏄🏽🏄🏾🏄🏿🏄🏄🏻🏄🏼🏄🏽🏄🏾🏄🏿🏊🏊🏻🏊🏼🏊🏽🏊🏾🏊🏿🏊🏊🏻🏊🏼🏊🏽🏊🏾🏊🏿🤽🤽🏻🤽🏼🤽🏽🤽🏾🤽🏿🤽🤽🏻🤽🏼🤽🏽🤽🏾🤽🏿🚣🚣🏻🚣🏼🚣🏽🚣🏾🚣🏿🚣🚣🏻🚣🏼🚣🏽🚣🏾🚣🏿🧗🧗🏻🧗🏼🧗🏽🧗🏾🧗🏿🧗🧗🏻🧗🏼🧗🏽🧗🏾🧗🏿🚵🚵🏻🚵🏼🚵🏽🚵🏾🚵🏿🚵🚵🏻🚵🏼🚵🏽🚵🏾🚵🏿🚴🚴🏻🚴🏼🚴🏽🚴🏾🚴🏿🚴🚴🏻🚴🏼🚴🏽🚴🏾🚴🏿🏆🥇🥈🥉🏅🎖🏵🎗🎫🎟🎪🤹🤹🏻🤹🏼🤹🏽🤹🏾🤹🏿🤹🤹🏻🤹🏼🤹🏽🤹🏾🤹🏿🎭🎨🎬🎤🎧🎼🎹🥁🎷🎺🎸🎻🎲🧩♟🎯🎳🎮🎰🚗🚕🚙🚌🚎🏎🚓🚑🚒🚐🚚🚛🚜🛴🚲🛵🏍🚨🚔🚍🚘🚖🚡🚠🚟🚃🚋🚞🚝🚄🚅🚈🚂🚆🚇🚊🚉✈️🛫🛬🛩💺🛰🚀🛸🚁🛶⛵️🚤🛥🛳⛴🚢⚓️⛽️🚧🚦🚥🚏🗺🗿🗽🗼🏰🏯🏟🎡🎢🎠⛲️⛱🏖🏝🏜🌋⛰🏔🗻🏕⛺️🏠🏡🏘🏚🏗🏭🏢🏬🏣🏤🏥🏦🏨🏪🏫🏩💒🏛⛪️🕌🕍🕋⛩🛤🛣🗾🎑🏞🌅🌄🌠🎇🎆🌇🌆🏙🌃🌌🌉🌁🆓📗📕⌚️📱📲💻⌨️🖥🖨🖱🖲🕹🗜💽💾💿📀📼📷📸📹🎥📽🎞📞☎️📟📠📺📻🎙🎚🎛⏱⏲⏰🕰⌛️⏳📡🔋🔌💡🔦🕯🗑🛢💸💵💴💶💷💰💳🧾💎⚖️🔧🔨⚒🛠⛏🔩⚙️⛓🔫💣🔪🗡⚔️🛡🚬⚰️⚱️🏺🧭🧱🔮🧿🧸📿💈⚗️🔭🧰🧲🧪🧫🧬🧯🔬🕳💊💉🌡🚽🚰🚿🛁🛀🛀🏻🛀🏼🛀🏽🛀🏾🛀🏿🧴🧵🧶🧷🧹🧺🧻🧼🧽🛎🔑🗝🚪🛋🛏🛌🖼🛍🧳🛒🎁🎈🎏🎀🎊🎉🧨🎎🏮🎐🧧✉️📩📨📧💌📥📤📦🏷📪📫📬📭📮📯📜📃📄📑📊📈📉🗒🗓📆📅📇🗃🗳🗄📋📁📂🗂🗞📰📓📔📒📕📗📘📙📚📖🔖🔗📎🖇📐📏📌📍✂️🖊🖋✒️🖌🖍📝✏️🔍🔎🔏🔐🔒🔓❤️🧡💛💚💙💜🖤💔❣️💕💞💓💗💖💘💝💟☮️✝️☪️🕉☸️✡️🔯🕎☯️☦️🛐⛎♈️♉️♊️♋️♌️♍️♎️♏️♐️♑️♒️♓️🆔⚛️🉑☢️☣️📴📳🈶🈚️🈸🈺🈷️✴️🆚💮🉐㊙️㊗️🈴🈵🈹🈲🅰️🅱️🆎🆑🅾️🆘❌⭕️🛑⛔️📛🚫💯💢♨️🚷🚯🚳🚱🔞📵🚭❗️❕❓❔‼️⁉️🔅🔆〽️⚠️🚸🔱⚜️🔰♻️✅🈯️💹❇️✳️❎🌐💠Ⓜ️🌀💤🏧🚾♿️🅿️🈳🈂️🛂🛃🛄🛅🚹🚺🚼🚻🚮🎦📶🈁🔣ℹ️🔤🔡🔠🆖🆗🆙🆒🆕🆓🔟🔢#️⃣*️⃣⏏️▶️⏸⏯⏹⏺⏭⏮⏩⏪⏫⏬◀️🔼🔽➡️⬅️⬆️⬇️↗️↘️↙️↖️↕️↔️↪️↩️⤴️⤵️🔀🔁🔂🔄🔃🎵🎶➕➖➗✖️♾💲💱™️©️®️〰️➰➿🔚🔙🔛🔝🔜✔️☑️🔘⚪️⚫️🔴🔵🔺🔻🔸🔹🔶🔷🔳🔲▪️▫️◾️◽️◼️◻️⬛️⬜️🔈🔇🔉🔊🔔🔕📣📢👁‍🗨💬💭🗯♠️♣️♥️♦️🃏🎴🀄️🕐🕑🕒🕓🕔🕕🕖🕗🕘🕙🕚🕛🕜🕝🕞🕟🕠🕡🕢🕣🕤🕥🕦🕧🏳️🏴🏁🚩🏳️‍🌈🏴‍☠️🇦🇫🇦🇽🇦🇱🇩🇿🇦🇸🇦🇩🇦🇴🇦🇮🇦🇶🇦🇬🇦🇷🇦🇲🇦🇼🇦🇺🇦🇹🇦🇿🇧🇸🇧🇭🇧🇩🇧🇧🇧🇾🇧🇪🇧🇿🇧🇯🇧🇲🇧🇹🇧🇴🇧🇦🇧🇼🇧🇷🇮🇴🇻🇬🇧🇳🇧🇬🇧🇫🇧🇮🇰🇭🇨🇲🇨🇦🇮🇨🇨🇻🇧🇶🇰🇾🇨🇫🇹🇩🇨🇱🇨🇳🇨🇽🇨🇨🇨🇴🇰🇲🇨🇬🇨🇩🇨🇰🇨🇷🇨🇮🇭🇷🇨🇺🇨🇼🇨🇾🇨🇿🇩🇰🇩🇯🇩🇲🇩🇴🇪🇨🇪🇬🇸🇻🇬🇶🇪🇷🇪🇪🇪🇹🇪🇺🇫🇰🇫🇴🇫🇯🇫🇮🇫🇷🇬🇫🇵🇫🇹🇫🇬🇦🇬🇲🇬🇪🇩🇪🇬🇭🇬🇮🇬🇷🇬🇱🇬🇩🇬🇵🇬🇺🇬🇹🇬🇬🇬🇳🇬🇼🇬🇾🇭🇹🇭🇳🇭🇰🇭🇺🇮🇸🇮🇳🇮🇩🇮🇷🇮🇶🇮🇪🇮🇲🇮🇱🇮🇹🇯🇲🇯🇵🎌🇯🇪🇯🇴🇰🇿🇰🇪🇰🇮🇽🇰🇰🇼🇰🇬🇱🇦🇱🇻🇱🇧🇱🇸🇱🇷🇱🇾🇱🇮🇱🇹🇱🇺🇲🇴🇲🇰🇲🇬🇲🇼🇲🇾🇲🇻🇲🇱🇲🇹🇲🇭🇲🇶🇲🇷🇲🇺🇾🇹🇲🇽🇫🇲🇲🇩🇲🇨🇲🇳🇲🇪🇲🇸🇲🇦🇲🇿🇲🇲🇳🇦🇳🇷🇳🇵🇳🇱🇳🇨🇳🇿🇳🇮🇳🇪🇳🇬🇳🇺🇳🇫🇰🇵🇲🇵🇳🇴🇴🇲🇵🇰🇵🇼🇵🇸🇵🇦🇵🇬🇵🇾🇵🇪🇵🇭🇵🇳🇵🇱🇵🇹🇵🇷🇶🇦🇷🇪🇷🇴🇷🇺🇷🇼🇼🇸🇸🇲🇸🇦🇸🇳🇷🇸🇸🇨🇸🇱🇸🇬🇸🇽🇸🇰🇸🇮🇬🇸🇸🇧🇸🇴🇿🇦🇰🇷🇸🇸🇪🇸🇱🇰🇧🇱🇸🇭🇰🇳🇱🇨🇵🇲🇻🇨🇸🇩🇸🇷🇸🇿🇸🇪🇨🇭🇸🇾🇹🇼🇹🇯🇹🇿🇹🇭🇹🇱🇹🇬🇹🇰🇹🇴🇹🇹🇹🇳🇹🇷🇹🇲🇹🇨🇹🇻🇻🇮🇺🇬🇺🇦🇦🇪🇬🇧🏴󠁧󠁢󠁥󠁮󠁧󠁿🏴󠁧󠁢󠁳󠁣󠁴󠁿🏴󠁧󠁢󠁷󠁬󠁳󠁿🇺🇳🇺🇸🇺🇾🇺🇿🇻🇺🇻🇦🇻🇪🇻🇳🇼🇫🇪🇭🇾🇪🇿🇲🇿🇼😃💁🐻🌻🍔🍹🎷⚽️🚘🌇💡🎉💖🔣🎌🏳️‍🌈🥰🥵🥶🥳🥴🥺👨‍🦰👩‍🦰👨‍🦱👩‍🦱👨‍🦲👩‍🦲👨‍🦳👩‍🦳🦸🦸🦸🦹🦹🦹🦵🦶🦴🦷🥽🥼🥾🥿🦝🦙🦛🦘🦡🦢🦚🦜🦞🦟🦠🥭🥬🥯🧂🥮🧁🧭🧱🛹🧳🧨🧧🥎🥏🥍🧿🧩🧸♟🧮🧾🧰🧲🧪🧫🧬🧯🧴🧵🧶🧷🧹🧺🧻🧼🧽♾🏴‍☠️]";

            // 替换 Emoji 字符为空字符串
            return Regex.Replace(input, emojiPattern, string.Empty);
        }

        public static string UnicodePointsToEmoji(string unicodePoints)
        {
            // 将输入的 code points 用 '-' 分割
            unicodePoints = unicodePoints.Replace("-ufe0f", "").Replace("-u200d", "");
            string[] parts = unicodePoints.Split('-');
            StringBuilder emojiBuilder = new StringBuilder();

            foreach (string part in parts)
            {
                var p = part.Trim();
                if (p.StartsWith('u')) p = p.Substring(1);
                if (p.StartsWith('U')) p = p.Substring(1);
                if (p.StartsWith('+')) p = p.Substring(1);

                try
                {// 将每个部分从 16 进制转为整型
                    //Logger.Log(p);
                    int codePoint = Convert.ToInt32(p, 16);
                    emojiBuilder.Append(char.ConvertFromUtf32(codePoint));
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }

            }

            return emojiBuilder.ToString();
        }
    }
}