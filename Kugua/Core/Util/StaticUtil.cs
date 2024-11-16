using MeowMiraiLib.Msg.Type;
using Microsoft.AspNetCore.Components.Forms;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kugua
{
    public delegate void SendMsgDelegate(string msg);
    public delegate void SendLogDelegate(LogInfo info);



    /// <summary>
    /// 全局功能
    /// </summary>
    public class StaticUtil
    {
        /// <summary>
        /// 标点符号
        /// </summary>
        private static readonly HashSet<char> symbols = new HashSet<char>
        {
            '，', '。', '、', '；', '：', '【', '】', '？', '“', '”', '‘', '’', '《', '》',
            '！', '￥', '…', '—', '{', '}', '[', ']', '(', ')', '+', '=', '-', '*', '/',
            '!', '@', '#', '$', '%', '^', '&', '_', '|', ',', '.', '?', ':', ';', '\\',
            '\'', '\"', '\t', '\r', '\n'
        };

        /// <summary>
        /// 检查字符是否为符号
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        public static bool IsSymbol(char ch)
        {
            return symbols.Contains(ch);
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

        /// <summary>
        /// 去除字符串中的中英文标点和特殊字符
        /// </summary>
        /// <param name="ori">原始字符串</param>
        /// <returns>去除符号后的字符串</returns>
        public static string RemoveSymbol(string ori)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var c in ori)
            {
                // 仅当字符不是符号时才添加到结果中
                if (!IsSymbol(c))
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }


        #region DateTime时间与unix时间戳互转
        /// <summary>
        /// 将 Unix 时间戳转换为 DateTime
        /// </summary>
        /// <param name="timestamp">Unix 时间戳</param>
        /// <param name="isMilliseconds">是否为毫秒级时间戳，默认是 false（秒级）</param>
        /// <returns>转换后的 DateTime 对象（本地时间）</returns>
        public static DateTime ConvertTimestampToDateTime(long timestamp, bool isMilliseconds = false)
        {
            DateTime dateTime;

            if (isMilliseconds)
            {
                dateTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
            }
            else
            {
                dateTime = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
            }

            // 转换为本地时间
            return dateTime.ToLocalTime();
        }

        /// <summary>
        /// 将 DateTime 转换为 Unix 时间戳
        /// </summary>
        /// <param name="dateTime">需要转换的 DateTime 对象</param>
        /// <param name="toMilliseconds">是否转换为毫秒级时间戳，默认是 false（秒级）</param>
        /// <returns>对应的 Unix 时间戳</returns>
        public static long ConvertDateTimeToTimestamp(DateTime dateTime, bool toMilliseconds = false)
        {
            DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTime);

            if (toMilliseconds)
            {
                return dateTimeOffset.ToUnixTimeMilliseconds();
            }
            else
            {
                return dateTimeOffset.ToUnixTimeSeconds();
            }
        }

        #endregion

        /// <summary>
        /// 计算基尼系数
        /// </summary>
        /// <param name="incomes"></param>
        /// <returns></returns>
        public static double CalculateGiniCoefficient(List<long> incomes)
        {
            // Sort incomes in ascending order
            var sortedIncomes = incomes.OrderBy(income => income).ToList();

            int count = sortedIncomes.Count;
            if (count == 0) return 0.0;

            double totalIncome = sortedIncomes.Sum();
            if (totalIncome == 0) return 0.0;

            // Calculate cumulative proportions
            double cumulativeIncome = 0;
            double cumulativeProportionSum = 0;

            for (int i = 0; i < count; i++)
            {
                cumulativeIncome += sortedIncomes[i];
                double currentCumulativeProportion = cumulativeIncome / totalIncome;
                cumulativeProportionSum += currentCumulativeProportion;
            }

            // Gini coefficient formula
            double giniCoefficient = 1 - 2 * cumulativeProportionSum / count;
            return Math.Round(giniCoefficient, 2);
        }

        /// <summary>
        /// 获取本程序集的编译日期
        /// </summary>
        /// <param name="assembly">目标程序集</param>
        /// <returns>编译日期</returns>
        public static DateTime GetBuildDate()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            // 获取程序集文件的路径
            var filePath = assembly.Location;
            var fileInfo = new System.IO.FileInfo(filePath);

            // 获取编译日期，文件的最后写入时间
            return fileInfo.LastWriteTime;
        }



        #region 洗牌算法们

        public static void FisherYates(char[] input)
        {
            // Fisher-Yates 洗牌算法，完全打乱
            for (int i = input.Length - 1; i > 0; i--)
            {
                int j = MyRandom.Next(0, i + 1); // 生成随机索引
                                                 // 交换
                char temp = input[i];
                input[i] = input[j];
                input[j] = temp;
            }
        }
        public static void FisherYates(string[] input)
        {
            // Fisher-Yates 洗牌算法，完全打乱
            for (int i = input.Length - 1; i > 0; i--)
            {
                int j = MyRandom.Next(0, i + 1); // 生成随机索引
                                                 // 交换
                string temp = input[i];
                input[i] = input[j];
                input[j] = temp;
            }
        }
        public static void FisherYates(bool[] input)
        {
            // Fisher-Yates 洗牌算法，完全打乱
            for (int i = input.Length - 1; i > 0; i--)
            {
                int j = MyRandom.Next(0, i + 1); // 生成随机索引
                                                 // 交换
                bool temp = input[i];
                input[i] = input[j];
                input[j] = temp;
            }
        }
        /// <summary>
        /// 洗牌算法
        /// </summary>
        /// <param name="str">需打乱的字符串</param>
        /// <returns>打乱结果</returns>
        public static string ShuffleString(string str, int time = 0)
        {
            if (string.IsNullOrWhiteSpace(str)) return str;



            if (time < 1)
            {
                char[] array = str.ToCharArray(); // 将字符串转换为字符数组
                FisherYates(array);
                return new string(array); // 返回新的乱序字符串
            }
            else
            {
                // 只随机切牌time轮  算法3 - 不均匀切
                time = Math.Min(time, str.Length - 1);
                bool[] cuts = new bool[str.Length - 1];

                var stringBuilder = new StringBuilder();
                for (int i = 0; i < cuts.Length; i++) cuts[i] = (i < time);
                FisherYates(cuts);
                List<string> parts = new List<string>();

                // 切割字符串
                int startIndex = 0;
                for (int i = 1; i < str.Length; i++)
                {
                    if (cuts[i - 1])
                    {
                        parts.Add(str.Substring(startIndex, i - startIndex));
                        startIndex = i;
                    }

                }
                parts.Add(str.Substring(startIndex));
                var pparts = parts.ToArray();
                FisherYates(pparts);
                return string.Concat(pparts);

                //// 只随机切牌time轮  算法2 - 均匀切
                //time = Math.Min(time, str.Length);
                //int partLength = str.Length / time;
                //List<string> parts = new List<string>();

                //// 切割字符串
                //for (int i = 0; i < time; i++)
                //{
                //    // 计算切割的开始和结束索引
                //    int startIndex = i * partLength;
                //    // 处理最后一部分，确保包含所有剩余字符
                //    int length = (i == time - 1) ? str.Length - startIndex : partLength;

                //    // 提取子字符串并添加到列表中
                //    parts.Add(str.Substring(startIndex, length));
                //}

                //// 打乱切割后的部分
                //List<string> shuffledParts = parts.OrderBy(x => rand.Next()).ToList();

                //// 合并打乱后的部分为最终字符串
                //return string.Concat(shuffledParts);

                //// 只随机切牌time轮 算法1 - 切后拼后切
                //for (int i = 0; i < Math.Min(time, str.Length*2); i++)
                //{
                //    int cutPosition = rand.Next(1, str.Length); 
                //    string leftPart = str.Substring(0, cutPosition);
                //    if (rand.Next(0, 2) > 0) leftPart = new string(leftPart.Reverse().ToArray());
                //    string rightPart = str.Substring(cutPosition);
                //    if (rand.Next(0, 2) > 0) rightPart = new string(rightPart.Reverse().ToArray());
                //    str = rightPart + leftPart;
                //}
                //// 合并打乱后的部分为最终字符串
                //return str;
            }
        }


        #endregion



        //public static string Wav2Pcm(string inputFile)
        //{
        //    // 命令行指令
        //    inputFile = Path.GetFullPath(inputFile);
        //    string outputFile = $"{Path.GetDirectoryName(inputFile)}\\{Path.GetFileNameWithoutExtension(inputFile)}.pcm";
        //    string cmd = "ffmpeg";
        //    string param = $" -i {inputFile} -y {outputFile}";
        //    Process process = new Process();
        //    ProcessStartInfo startInfo = new ProcessStartInfo()
        //    {
        //        FileName = cmd,
        //        Arguments = param,
        //        CreateNoWindow = true
        //    };
        //    process.StartInfo = startInfo;

        //    process.Start();
        //    process.WaitForExit();

        //    int exitCode = process.ExitCode;
        //    if (exitCode != 0)
        //    {
        //        Logger.Instance.Log($"语音合成失败。指令：{cmd} {param}");
        //        //throw new Exception($"FFmpeg exited with code {exitCode}");
        //    }
        //    return outputFile;

        //}


        public static string Mp32AmrBase64(string inputFile)
        {
            var f1 = MP32Wav(inputFile);
            if (!string.IsNullOrWhiteSpace(f1))
            {
                var f2 = Wav2Amr(f1, 0);
                if (!string.IsNullOrWhiteSpace(f2))
                {
                    var b64 = ConvertFileToBase64(f2);
                    if(!string.IsNullOrWhiteSpace(b64))
                    {
                        Thread.Sleep(500);
                        System.IO.File.Delete(f1);
                        System.IO.File.Delete(f2);

                        return b64;
                    }
                }
            }

            return null;
        }
       
        public static string MP32Wav(string inputFile)
        {
            // 命令行指令
            inputFile = Path.GetFullPath(inputFile);
            string outputFile = $"{Path.GetDirectoryName(inputFile)}\\{Path.GetFileNameWithoutExtension(inputFile)}.wav";
            string cmd = "D:\\ffmpeg\\bin\\ffmpeg.exe";
            string param = $" -i {inputFile} -acodec libmp3lame -y {outputFile}";
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = cmd,
                Arguments = param,
                CreateNoWindow = true
            };
            process.StartInfo = startInfo;

            process.Start();
            process.WaitForExit();

            int exitCode = process.ExitCode;
            if (exitCode != 0)
            {
                Logger.Instance.Log($"语音合成失败。指令：{cmd} {param}");
                //throw new Exception($"FFmpeg exited with code {exitCode}");
            }
            else
            {
                //var res = ConvertAmrToBase64(outputFile);
                //System.IO.File.Delete(outputFile);
                //return res;
            }

            return outputFile;
        }



        public static string Wav2Amr(string inputFile, int addDB)
        {
            // 命令行指令
            inputFile = Path.GetFullPath(inputFile);
            string outputFile = $"{Path.GetDirectoryName(inputFile)}\\{Path.GetFileNameWithoutExtension(inputFile)}.amr";
            string cmd = "D:\\ffmpeg\\bin\\ffmpeg.exe";
            string param = "";
            if (addDB != 0)
            {
                param = $" -i {inputFile} -acodec amr_wb -ar 16000 -ac 1 -filter:a \"loudnorm=i=-14:tp=0.0\" -y {outputFile}";
                // param = $" -i {inputFile} -acodec amr_wb -ar 16000 -ac 1 -filter:a \"volume={addDB}dB\" -y {outputFile}";
                //param = $" -i {inputFile} -c:a amr_nb -b:a 12.20k -ar 8000 -filter:a \"volume={addDB}dB\" -y {outputFile}";
            }
            else
            {
                    param = $" -i {inputFile} -acodec amr_wb -ar 16000 -ac 1 -y {outputFile}";
                //param = $" -i {inputFile} -acodec amr_nb -ab 12.2k -ar 8000 -ac 1 -y {outputFile}";
            }
            
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = cmd,
                Arguments = param,
                CreateNoWindow = true
            };
            process.StartInfo = startInfo;

            process.Start();
            process.WaitForExit();

            int exitCode = process.ExitCode;
            if (exitCode != 0)
            {
                Logger.Instance.Log($"语音合成失败。指令：{cmd} {param}");
                //throw new Exception($"FFmpeg exited with code {exitCode}");
            }
            else
            {
                //var res = ConvertFileToBase64(outputFile);
                //System.IO.File.Delete(outputFile);
                //return res;
            }

            return outputFile;
        }


        /// <summary>
        /// 从文件读取 .wav 文件并转换为 Base64 字符串
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string ConvertFileToBase64(string filePath)
        {
            // 检查文件是否存在
            if (!System.IO.File.Exists(filePath))
            {
                return null;
                //throw new FileNotFoundException("The specified file does not exist.", filePath);
            }

            // 读取文件的字节内容
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

            // 将字节数组转换为 Base64 字符串
            string base64String = Convert.ToBase64String(fileBytes);

            return base64String;
        }


    }
}
