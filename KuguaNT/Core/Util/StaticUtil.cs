using Microsoft.AspNetCore.Components.Forms;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Kugua
{
    public delegate void SendMsgDelegate(string msg);
    public delegate void SendLogDelegate(LogInfo info);



    /// <summary>
    /// 全局功能
    /// </summary>
    public static class StaticUtil
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
        #endregion

        /// <summary>
        /// 计算基尼系数
        /// </summary>
        /// <param name="incomes"></param>
        /// <returns></returns>
        public static double CalculateGiniCoefficient(List<BigInteger> incomes)
        {
            var sortedIncomes = incomes.OrderBy(income => income).ToList();

            int count = sortedIncomes.Count;
            if (count == 0) return 0.0;

            BigInteger totalIncome = sortedIncomes.Aggregate(BigInteger.Zero, (acc, income) => acc + income);
            if (totalIncome == BigInteger.Zero) return 0.0;

            // Calculate cumulative proportions
            BigInteger cumulativeIncome = BigInteger.Zero;
            BigInteger cumulativeProportionSum = BigInteger.Zero;

            for (int i = 0; i < count; i++)
            {
                cumulativeIncome += sortedIncomes[i];
                cumulativeProportionSum += cumulativeIncome * 2;
            }

            // Gini coefficient formula
            BigInteger totalPopulation = new BigInteger(count);
            BigInteger numerator = totalPopulation * cumulativeProportionSum;
            BigInteger denominator = totalIncome * totalPopulation;

            BigInteger giniNumerator = denominator - numerator;
            double giniCoefficient = (double)giniNumerator / (double)denominator;

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

        #region 数字转换
        /// <summary>
        /// 将科学计数法字符串解析为 BigInteger
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        private static BigInteger ParseScientificNotation(string input)
        {
            var match = Regex.Match(input, @"^([+-]?\d+(\.\d+)?)[eE]([+-]?\d+)$");
            if (!match.Success)
            {
                throw new FormatException($"Invalid scientific notation: {input}");
            }

            // 提取科学计数法部分
            string baseValueStr = match.Groups[1].Value;
            int exponent = int.Parse(match.Groups[3].Value);

            // 分离小数部分
            string[] baseParts = baseValueStr.Split('.');
            BigInteger integerPart = BigInteger.Parse(baseParts[0]);
            BigInteger fractionalPart = baseParts.Length > 1 ? BigInteger.Parse(baseParts[1]) : BigInteger.Zero;
            int fractionalLength = baseParts.Length > 1 ? baseParts[1].Length : 0;

            // 调整指数
            exponent -= fractionalLength;

            // 构造 BigInteger
            BigInteger result = integerPart * BigInteger.Pow(10, Math.Max(0, exponent));
            if (fractionalPart > 0)
            {
                result += fractionalPart * BigInteger.Pow(10, Math.Max(0, exponent - fractionalLength));
            }

            if (exponent < 0)
            {
                throw new InvalidOperationException("Result is too small for BigInteger; consider a different type.");
            }

            return result;
        }


        /// <summary>
        /// 中文数字映射表
        /// </summary>
        private static readonly Dictionary<char, int> ChineseDigitMap = new()
        {
            { '〇', 0 },
            { '零', 0 }, 
            { '一', 1 }, 
            { '二', 2 }, { '两', 2 },
            { '三', 3 }, 
            { '四', 4 },
            { '五', 5 }, 
            { '六', 6 }, 
            { '七', 7 }, 
            { '八', 8 }, 
            { '九', 9 },

        };

            /// <summary>
            /// 中文单位映射表
            /// </summary>
            private static readonly Dictionary<string, BigInteger> ChineseUnitMap = new()
        {
            { "十", 10 }, 
            { "百", 100 },
            { "千", 1000 }, 
            { "万", 10_000 },
            { "亿", 100_000_000 }, 
            { "兆", BigInteger.Pow(10, 12) },
            { "京", BigInteger.Pow(10, 16) }, 
            { "垓", BigInteger.Pow(10, 20) },
            { "秭", BigInteger.Pow(10, 24) }, 
            { "穰", BigInteger.Pow(10, 28) },
            { "沟", BigInteger.Pow(10, 32) }, 
            { "涧", BigInteger.Pow(10, 36) },
            { "正", BigInteger.Pow(10, 40) }, 
            { "载", BigInteger.Pow(10, 44) },
            { "极", BigInteger.Pow(10, 48) }, 
            { "无量大数", BigInteger.Pow(10, 52) },
            { "恒河沙", BigInteger.Pow(10, 56) },
            { "阿僧祇", BigInteger.Pow(10, 60) },
            { "那由他", BigInteger.Pow(10, 64) }, 
            { "不可思议", BigInteger.Pow(10, 68) },
            { "无量数", BigInteger.Pow(10, 72) }, 
            { "大数", BigInteger.Pow(10, 76) }
        };



        /// <summary>
        /// 将汉字写的数字字符串转化成int，兼容本来就是阿拉伯数字的串
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static int ConvertToNumber(string input)
        {
            int result = 0;
            int tempNumber = 0;
            bool hasTen = false;

            
            if(int.TryParse(input, out result))
            {
                return result;
            }
            Logger.Log("??+" + input);
            Dictionary<char, int> ChineseDigitMap2 = new()
            {   
                // 中文常见数字
                { '〇', 0 }, { '零', 0 },
                { '一', 1 }, { '壹', 1 }, { '壱', 1 }, { '〡', 1 }, // 异体
                { '二', 2 }, { '贰', 2 }, { '弐', 2 }, { '貮', 2 }, { '两', 2 }, // 异体
                { '三', 3 }, { '叁', 3 }, { '参', 3 }, { '仨', 3 }, // 异体和俗称
                { '四', 4 }, { '肆', 4 }, { '〤', 4 }, // 异体
                { '五', 5 }, { '伍', 5 }, { '〥', 5 }, // 异体
                { '六', 6 }, { '陆', 6 }, { '〦', 6 }, // 异体
                { '七', 7 }, { '柒', 7 }, { '〧', 7 }, // 异体
                { '八', 8 }, { '捌', 8 }, { '〨', 8 }, // 异体
                { '九', 9 }, { '玖', 9 }, { '〩', 9 }, // 异体
                { '十', 10 }, { '拾', 10 }, { '廿', 20 }, { '卅', 30 }, { '卌', 40 }, // 异体和简写
            };
            foreach (char ch in input)
            {
                if (ChineseDigitMap2.TryGetValue(ch, out int value))
                {
                    if (value == 10) // 遇到“十”
                    {
                        if (tempNumber == 0)
                        {
                            tempNumber = 10; // “十”单独出现表示10
                        }
                        else
                        {
                            tempNumber *= 10; // 如“二十”表示2*10
                        }
                        hasTen = true;
                    }
                    else
                    {
                        if (hasTen) // 如果前面是“十”
                        {
                            tempNumber += value; // 累加个位数字，如“二十八”
                            result += tempNumber; // 累计到结果
                            tempNumber = 0; // 重置临时值
                            hasTen = false;
                        }
                        else
                        {
                            tempNumber = value; // 普通数字直接赋值
                        }
                    }
                }
                else
                {
                    // 如果无法匹配，返回 0 表示无效
                    return 0;
                }
            }

            // 最后一部分累加
            result += tempNumber;

            return result;
        }



        /// <summary>
        /// 替换中文数字字符为对应阿拉伯数字，一一对应
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ConvertChineseDigitsToArabic(string input)
        {
            foreach (var pair in ChineseDigitMap)
            {
                input = input.Replace(pair.Key.ToString(), pair.Value.ToString());
            }
            return input;
        }
        
        public static string ToHans(this BigInteger number)
        {
            return ConvertToChinese(number);
        }
        /// <summary>
        /// 解析字符串为BigInteger
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static BigInteger ConvertToBigInteger(string input)
        {
            if (input == null) return BigInteger.Zero;


            // 检查是否是科学计数法
            if (Regex.IsMatch(input, @"^[+-]?\d+(\.\d+)?[eE][+-]?\d+$"))
            {
                return ParseScientificNotation(input);
            }

            BigInteger result = 0;
            BigInteger currentValue = 0;
            input = ConvertChineseDigitsToArabic(input.Trim());

            // 检查正负号
            bool isNegative = input.StartsWith("负");
            input = isNegative ? input.Substring(1) : input;

            var regex = new Regex(@"(\d+)(十|百|千|万|亿|兆|京|垓|秭|穰|沟|涧|正|载|极|无量大数|恒河沙|阿僧祇|那由他|不可思议|无量数|大数)?");
            var matches = regex.Matches(input);

            foreach (Match match in matches)
            {
                string numberStr = match.Groups[1].Value;
                string unit = match.Groups[2].Value;

                BigInteger value = BigInteger.Parse(numberStr);

                if (ChineseUnitMap.ContainsKey(unit))
                {
                    currentValue += value * ChineseUnitMap[unit];
                }
                else
                {
                    currentValue += value;
                }

                if (ChineseUnitMap.ContainsKey(unit))
                {
                    result += currentValue;
                    currentValue = 0;
                }
            }

            result += currentValue;

            return isNegative ? -result : result;
        }

        /// <summary>
        /// 转换 BigInteger 为中文自然语言表示
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string ConvertToChinese(BigInteger number)
        {
            if (number == 0) return "零";
            var unitMap = new List<(BigInteger Threshold, string Unit)>
            {
                (BigInteger.Pow(10, 76), "大数"),
                (BigInteger.Pow(10, 72), "无量数"),
                (BigInteger.Pow(10, 68), "不可思议"),
                (BigInteger.Pow(10, 64), "那由他"),
                (BigInteger.Pow(10, 60), "阿僧祇"),
                (BigInteger.Pow(10, 56), "恒河沙"),
                (BigInteger.Pow(10, 48), "极"),
                (BigInteger.Pow(10, 44), "载"),
                (BigInteger.Pow(10, 40), "正"),
                (BigInteger.Pow(10, 36), "涧"),
                (BigInteger.Pow(10, 32), "沟"),
                (BigInteger.Pow(10, 28), "穰"),
                (BigInteger.Pow(10, 24), "秭"),
                (BigInteger.Pow(10, 20), "垓"),
                (BigInteger.Pow(10, 16), "京"),
                (BigInteger.Pow(10, 12), "兆"),
                (100_000_000, "亿"),
                (10_000, "万"),
                (1_000, "千"),
                //(100, "百"),
                //(10, "十")
            };
            
            // 处理负数
            bool isNegative = number < 0;
            number = BigInteger.Abs(number);

            var result = new List<string>();
            foreach (var (threshold, unit) in unitMap)
            {
                if (number >= threshold)
                {
                    var value = number / threshold;
                    result.Add($"{value}{unit}");
                    number %= threshold;
                }
            }

            if (number > 0)
            {
                if (result.Count > 0 && result[^1] != "零")
                {
                    result.Add("零"); // 添加“零”连接符
                }
                result.Add(number.ToString());
            }

            return isNegative ? "负" + string.Join("", result) : string.Join("", result);
        }

        #endregion 数字转换


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

        public static void FisherYates(List<bool> input)
        {
            // Fisher-Yates 洗牌算法，完全打乱
            for (int i = input.Count - 1; i > 0; i--)
            {
                int j = MyRandom.Next(0, i + 1); // 生成随机索引
                                                 // 交换
                var temp = input[i];
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
        //        Logger.Log($"语音合成失败。指令：{cmd} {param}");
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
            string param = $" -i \"{inputFile}\" -acodec libmp3lame -y \"{outputFile}\"";
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
                Logger.Log($"语音合成失败。指令：{cmd} {param}");
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


        /// <summary>
        /// wav增强
        /// </summary>
        /// <param name="inputFile"></param>
        /// <returns></returns>
        public static string WavInc(string inputFile)
        {
            inputFile = Path.GetFullPath(inputFile);
            string outputFile = $"{Path.GetDirectoryName(inputFile)}\\{Path.GetFileNameWithoutExtension(inputFile)}_Inc.wav";
            string cmd = "ffmpeg";
            string param = "";

            param = $" -i \"{inputFile}\" -af \"volume=3.0,highpass=f=200,lowpass=f=3000,afftdn=nr=50\" -y \"{outputFile}\"";


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
                Logger.Log($"语音合成失败。指令：{cmd} {param}");
                //throw new Exception($"FFmpeg exited with code {exitCode}");
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
                //param = $" -i {inputFile} -acodec amr_wb -ar 16000 -ac 1 -filter:a \"loudnorm=i=-14:tp=0.0\" -y {outputFile}";
                //param = $" -i {inputFile} -acodec amr_wb -ar 16000 -ac 1 -filter:a \"volume={addDB}dB\" -y {outputFile}";
                param = $" -i \"{inputFile}\" -c:a amr_nb -b:a 12.20k -ar 8000 -filter:a \"loudnorm=i=-14:tp=0.0\" -y \"{outputFile}\"";
                // param = $" -i {inputFile} -c:a amr_nb -b:a 12.20k -ar 8000 -filter:a \"volume={addDB}dB\" -y {outputFile}";
            }
            else
            {
                    param = $" -i \"{inputFile}\" -acodec amr_wb -ar 16000 -ac 1 -y \"{outputFile}\"";
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
                Logger.Log($"语音合成失败。指令：{cmd} {param}");
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
