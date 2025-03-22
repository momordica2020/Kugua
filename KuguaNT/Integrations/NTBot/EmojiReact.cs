namespace Kugua.Integrations.NTBot
{
    public class EmojiReact
    {
        #region 单例
        private static readonly Lazy<EmojiReact> instance = new Lazy<EmojiReact>(() => new EmojiReact());

        public static EmojiReact Instance => instance.Value;

        #endregion

        //const string emoji_likes_file = "emoji_likes.txt";
        


        private EmojiReact()
        {
            try
            {
                //// 读取emoji_likes
                //emojiTypeInfos = new Dictionary<string, EmojiTypeInfo>();
                //foreach (var line in LocalStorage.ReadResourceLines(emoji_likes_file))
                //{
                //    var parts = line.Split('\t', StringSplitOptions.TrimEntries);
                //    if (parts.Length >= 3)
                //    {
                //        emojiTypeInfos[parts[1]] = new EmojiTypeInfo { type = parts[0], id = parts[1], name = parts[2] };
                //        if (parts.Length >= 4) emojiTypeInfos[parts[1]].desc = parts[3];
                //    }
                //}
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }


        public EmojiTypeInfo Get(string name)
        {
            foreach (var f in emojiTypeInfos.Values)
            {
                if(f.name==name || f.desc == name)
                {
                    return f;
                }
            }
            return null;
        }

        public EmojiTypeInfo GetById(string id)
        {
            foreach(var emoji in emojiTypeInfos)
            {
                if (emoji.Value.id == id) return emoji.Value;
            }
            return null;
        }

        public EmojiTypeInfo GetByName(string name)
        {
            if (emojiTypeInfos.ContainsKey(name)) return emojiTypeInfos[name];
            return null;
        }

        public static Dictionary<string, EmojiTypeInfo> emojiTypeInfos = new Dictionary<string, EmojiTypeInfo>
        {
            {"得意", new EmojiTypeInfo{type="1", id="4", name="得意", desc=""}},
            {"流泪", new EmojiTypeInfo{type="1", id="5", name="流泪", desc=""}},
            {"睡", new EmojiTypeInfo{type="1", id="8", name="睡", desc=""}},
            {"大哭", new EmojiTypeInfo{type="1", id="9", name="大哭", desc=""}},
            {"尴尬", new EmojiTypeInfo{type="1", id="10", name="尴尬", desc=""}},
            {"调皮", new EmojiTypeInfo{type="1", id="12", name="调皮", desc=""}},
            {"微笑", new EmojiTypeInfo{type="1", id="14", name="微笑", desc=""}},
            {"酷", new EmojiTypeInfo{type="1", id="16", name="酷", desc=""}},
            {"可爱", new EmojiTypeInfo{type="1", id="21", name="可爱", desc=""}},
            {"傲慢", new EmojiTypeInfo{type="1", id="23", name="傲慢", desc=""}},
            {"饥饿", new EmojiTypeInfo{type="1", id="24", name="饥饿", desc=""}},
            {"困", new EmojiTypeInfo{type="1", id="25", name="困", desc=""}},
            {"惊恐", new EmojiTypeInfo{type="1", id="26", name="惊恐", desc=""}},
            {"流汗", new EmojiTypeInfo{type="1", id="27", name="流汗", desc=""}},
            {"憨笑", new EmojiTypeInfo{type="1", id="28", name="憨笑", desc=""}},
            {"悠闲", new EmojiTypeInfo{type="1", id="29", name="悠闲", desc=""}},
            {"奋斗", new EmojiTypeInfo{type="1", id="30", name="奋斗", desc=""}},
            {"疑问", new EmojiTypeInfo{type="1", id="32", name="疑问", desc=""}},
            {"嘘", new EmojiTypeInfo{type="1", id="33", name="嘘", desc=""}},
            {"晕", new EmojiTypeInfo{type="1", id="34", name="晕", desc=""}},
            {"敲打", new EmojiTypeInfo{type="1", id="38", name="敲打", desc=""}},
            {"再见", new EmojiTypeInfo{type="1", id="39", name="再见", desc=""}},
            {"发抖", new EmojiTypeInfo{type="1", id="41", name="发抖", desc=""}},
            {"爱情", new EmojiTypeInfo{type="1", id="42", name="爱情", desc=""}},
            {"跳跳", new EmojiTypeInfo{type="1", id="43", name="跳跳", desc=""}},
            {"拥抱", new EmojiTypeInfo{type="1", id="49", name="拥抱", desc=""}},
            {"蛋糕", new EmojiTypeInfo{type="1", id="53", name="蛋糕", desc=""}},
            {"咖啡", new EmojiTypeInfo{type="1", id="60", name="咖啡", desc=""}},
            {"玫瑰", new EmojiTypeInfo{type="1", id="63", name="玫瑰", desc=""}},
            {"爱心", new EmojiTypeInfo{type="1", id="66", name="爱心", desc=""}},
            {"太阳", new EmojiTypeInfo{type="1", id="74", name="太阳", desc=""}},
            {"月亮", new EmojiTypeInfo{type="1", id="75", name="月亮", desc=""}},
            {"赞", new EmojiTypeInfo{type="1", id="76", name="赞", desc=""}},
            {"握手", new EmojiTypeInfo{type="1", id="78", name="握手", desc=""}},
            {"胜利", new EmojiTypeInfo{type="1", id="79", name="胜利", desc=""}},
            {"飞吻", new EmojiTypeInfo{type="1", id="85", name="飞吻", desc=""}},
            {"西瓜", new EmojiTypeInfo{type="1", id="89", name="西瓜", desc=""}},
            {"冷汗", new EmojiTypeInfo{type="1", id="96", name="冷汗", desc=""}},
            {"擦汗", new EmojiTypeInfo{type="1", id="97", name="擦汗", desc=""}},
            {"抠鼻", new EmojiTypeInfo{type="1", id="98", name="抠鼻", desc=""}},
            {"鼓掌", new EmojiTypeInfo{type="1", id="99", name="鼓掌", desc=""}},
            {"糗大了", new EmojiTypeInfo{type="1", id="100", name="糗大了", desc=""}},
            {"坏笑", new EmojiTypeInfo{type="1", id="101", name="坏笑", desc=""}},
            {"左哼哼", new EmojiTypeInfo{type="1", id="102", name="左哼哼", desc=""}},
            {"右哼哼", new EmojiTypeInfo{type="1", id="103", name="右哼哼", desc=""}},
            {"哈欠", new EmojiTypeInfo{type="1", id="104", name="哈欠", desc=""}},
            {"委屈", new EmojiTypeInfo{type="1", id="106", name="委屈", desc=""}},
            {"左亲亲", new EmojiTypeInfo{type="1", id="109", name="左亲亲", desc=""}},
            {"可怜", new EmojiTypeInfo{type="1", id="111", name="可怜", desc=""}},
            {"示爱", new EmojiTypeInfo{type="1", id="116", name="示爱", desc=""}},
            {"抱拳", new EmojiTypeInfo{type="1", id="118", name="抱拳", desc=""}},
            {"拳头", new EmojiTypeInfo{type="1", id="120", name="拳头", desc=""}},
            {"爱你", new EmojiTypeInfo{type="1", id="122", name="爱你", desc=""}},
            {"NO", new EmojiTypeInfo{type="1", id="123", name="NO", desc=""}},
            {"OK", new EmojiTypeInfo{type="1", id="124", name="OK", desc=""}},
            {"转圈", new EmojiTypeInfo{type="1", id="125", name="转圈", desc=""}},
            {"挥手", new EmojiTypeInfo{type="1", id="129", name="挥手", desc=""}},
            {"喝彩", new EmojiTypeInfo{type="1", id="144", name="喝彩", desc=""}},
            {"棒棒糖", new EmojiTypeInfo{type="1", id="147", name="棒棒糖", desc=""}},
            {"茶", new EmojiTypeInfo{type="1", id="171", name="茶", desc=""}},
            {"泪奔", new EmojiTypeInfo{type="1", id="173", name="泪奔", desc=""}},
            {"无奈", new EmojiTypeInfo{type="1", id="174", name="无奈", desc=""}},
            {"卖萌", new EmojiTypeInfo{type="1", id="175", name="卖萌", desc=""}},
            {"小纠结", new EmojiTypeInfo{type="1", id="176", name="小纠结", desc=""}},
            {"doge", new EmojiTypeInfo{type="1", id="179", name="doge", desc=""}},
            {"惊喜", new EmojiTypeInfo{type="1", id="180", name="惊喜", desc=""}},
            {"骚扰", new EmojiTypeInfo{type="1", id="181", name="骚扰", desc=""}},
            {"笑哭", new EmojiTypeInfo{type="1", id="182", name="笑哭", desc=""}},
            {"我最美", new EmojiTypeInfo{type="1", id="183", name="我最美", desc=""}},
            {"点赞", new EmojiTypeInfo{type="1", id="201", name="点赞", desc=""}},
            {"托脸", new EmojiTypeInfo{type="1", id="203", name="托脸", desc=""}},
            {"托腮", new EmojiTypeInfo{type="1", id="212", name="托腮", desc=""}},
            {"啵啵", new EmojiTypeInfo{type="1", id="214", name="啵啵", desc=""}},
            {"蹭一蹭", new EmojiTypeInfo{type="1", id="219", name="蹭一蹭", desc=""}},
            {"抱抱", new EmojiTypeInfo{type="1", id="222", name="抱抱", desc=""}},
            {"拍手", new EmojiTypeInfo{type="1", id="227", name="拍手", desc=""}},
            {"佛系", new EmojiTypeInfo{type="1", id="232", name="佛系", desc=""}},
            {"喷脸", new EmojiTypeInfo{type="1", id="240", name="喷脸", desc=""}},
            {"甩头", new EmojiTypeInfo{type="1", id="243", name="甩头", desc=""}},
            {"加油抱抱", new EmojiTypeInfo{type="1", id="246", name="加油抱抱", desc=""}},
            {"脑阔疼", new EmojiTypeInfo{type="1", id="262", name="脑阔疼", desc=""}},
            {"捂脸", new EmojiTypeInfo{type="1", id="264", name="捂脸", desc=""}},
            {"辣眼睛", new EmojiTypeInfo{type="1", id="265", name="辣眼睛", desc=""}},
            {"哦哟", new EmojiTypeInfo{type="1", id="266", name="哦哟", desc=""}},
            {"头秃", new EmojiTypeInfo{type="1", id="267", name="头秃", desc=""}},
            {"问号脸", new EmojiTypeInfo{type="1", id="268", name="问号脸", desc=""}},
            {"暗中观察", new EmojiTypeInfo{type="1", id="269", name="暗中观察", desc=""}},
            {"emm", new EmojiTypeInfo{type="1", id="270", name="emm", desc=""}},
            {"吃瓜", new EmojiTypeInfo{type="1", id="271", name="吃瓜", desc=""}},
            {"呵呵哒", new EmojiTypeInfo{type="1", id="272", name="呵呵哒", desc=""}},
            {"我酸了", new EmojiTypeInfo{type="1", id="273", name="我酸了", desc=""}},
            {"汪汪", new EmojiTypeInfo{type="1", id="277", name="汪汪", desc=""}},
            {"汗", new EmojiTypeInfo{type="1", id="278", name="汗", desc=""}},
            {"无眼笑", new EmojiTypeInfo{type="1", id="281", name="无眼笑", desc=""}},
            {"敬礼", new EmojiTypeInfo{type="1", id="282", name="敬礼", desc=""}},
            {"面无表情", new EmojiTypeInfo{type="1", id="284", name="面无表情", desc=""}},
            {"摸鱼", new EmojiTypeInfo{type="1", id="285", name="摸鱼", desc=""}},
            {"哦", new EmojiTypeInfo{type="1", id="287", name="哦", desc=""}},
            {"睁眼", new EmojiTypeInfo{type="1", id="289", name="睁眼", desc=""}},
            {"敲开心", new EmojiTypeInfo{type="1", id="290", name="敲开心", desc=""}},
            {"摸锦鲤", new EmojiTypeInfo{type="1", id="293", name="摸锦鲤", desc=""}},
            {"期待", new EmojiTypeInfo{type="1", id="294", name="期待", desc=""}},
            {"拜谢", new EmojiTypeInfo{type="1", id="297", name="拜谢", desc=""}},
            {"元宝", new EmojiTypeInfo{type="1", id="298", name="元宝", desc=""}},
            {"牛啊", new EmojiTypeInfo{type="1", id="299", name="牛啊", desc=""}},
            {"右亲亲", new EmojiTypeInfo{type="1", id="305", name="右亲亲", desc=""}},
            {"牛气冲天", new EmojiTypeInfo{type="1", id="306", name="牛气冲天", desc=""}},
            {"喵喵", new EmojiTypeInfo{type="1", id="307", name="喵喵", desc=""}},
            {"仔细分析", new EmojiTypeInfo{type="1", id="314", name="仔细分析", desc=""}},
            {"加油", new EmojiTypeInfo{type="1", id="315", name="加油", desc=""}},
            {"崇拜", new EmojiTypeInfo{type="1", id="318", name="崇拜", desc=""}},
            {"比心", new EmojiTypeInfo{type="1", id="319", name="比心", desc=""}},
            {"庆祝", new EmojiTypeInfo{type="1", id="320", name="庆祝", desc=""}},
            {"拒绝", new EmojiTypeInfo{type="1", id="322", name="拒绝", desc=""}},
            {"吃糖", new EmojiTypeInfo{type="1", id="324", name="吃糖", desc=""}},
            {"生气", new EmojiTypeInfo{type="1", id="326", name="生气", desc=""}},
            {"☀", new EmojiTypeInfo{type="2", id="9728", name="☀", desc="晴天"}},
            {"☕", new EmojiTypeInfo{type="2", id="9749", name="☕", desc="咖啡"}},
            {"☺", new EmojiTypeInfo{type="2", id="9786", name="☺", desc="可爱"}},
            {"✨", new EmojiTypeInfo{type="2", id="10024", name="✨", desc="闪光"}},
            {"❌", new EmojiTypeInfo{type="2", id="10060", name="❌", desc="错误"}},
            {"❔", new EmojiTypeInfo{type="2", id="10068", name="❔", desc="问号"}},
            {"🌹", new EmojiTypeInfo{type="2", id="127801", name="🌹", desc="玫瑰"}},
            {"🍉", new EmojiTypeInfo{type="2", id="127817", name="🍉", desc="西瓜"}},
            {"🍎", new EmojiTypeInfo{type="2", id="127822", name="🍎", desc="苹果"}},
            {"🍓", new EmojiTypeInfo{type="2", id="127827", name="🍓", desc="草莓"}},
            {"🍜", new EmojiTypeInfo{type="2", id="127836", name="🍜", desc="拉面"}},
            {"🍞", new EmojiTypeInfo{type="2", id="127838", name="🍞", desc="面包"}},
            {"🍧", new EmojiTypeInfo{type="2", id="127847", name="🍧", desc="刨冰"}},
            {"🍺", new EmojiTypeInfo{type="2", id="127866", name="🍺", desc="啤酒"}},
            {"🍻", new EmojiTypeInfo{type="2", id="127867", name="🍻", desc="干杯"}},
            {"🎉", new EmojiTypeInfo{type="2", id="127881", name="🎉", desc="庆祝"}},
            {"🐛", new EmojiTypeInfo{type="2", id="128027", name="🐛", desc="虫"}},
            {"🐮", new EmojiTypeInfo{type="2", id="128046", name="🐮", desc="牛"}},
            {"🐳", new EmojiTypeInfo{type="2", id="128051", name="🐳", desc="鲸鱼"}},
            {"🐵", new EmojiTypeInfo{type="2", id="128053", name="🐵", desc="猴"}},
            {"👊", new EmojiTypeInfo{type="2", id="128074", name="👊", desc="拳头"}},
            {"👌", new EmojiTypeInfo{type="2", id="128076", name="👌", desc="好的"}},
            {"👍", new EmojiTypeInfo{type="2", id="128077", name="👍", desc="厉害"}},
            {"👏", new EmojiTypeInfo{type="2", id="128079", name="👏", desc="鼓掌"}},
            {"👙", new EmojiTypeInfo{type="2", id="128089", name="👙", desc="内衣"}},
            {"👦", new EmojiTypeInfo{type="2", id="128102", name="👦", desc="男孩"}},
            {"👨", new EmojiTypeInfo{type="2", id="128104", name="👨", desc="爸爸"}},
            {"💓", new EmojiTypeInfo{type="2", id="128147", name="💓", desc="爱心"}},
            {"💝", new EmojiTypeInfo{type="2", id="128157", name="💝", desc="礼物"}},
            {"💤", new EmojiTypeInfo{type="2", id="128164", name="💤", desc="睡觉"}},
            {"💦", new EmojiTypeInfo{type="2", id="128166", name="💦", desc="水"}},
            {"💨", new EmojiTypeInfo{type="2", id="128168", name="💨", desc="吹气"}},
            {"💪", new EmojiTypeInfo{type="2", id="128170", name="💪", desc="肌肉"}},
            {"📫", new EmojiTypeInfo{type="2", id="128235", name="📫", desc="邮箱"}},
            {"🔥", new EmojiTypeInfo{type="2", id="128293", name="🔥", desc="火"}},
            {"😁", new EmojiTypeInfo{type="2", id="128513", name="😁", desc="呲牙"}},
            {"😂", new EmojiTypeInfo{type="2", id="128514", name="😂", desc="激动"}},
            {"😄", new EmojiTypeInfo{type="2", id="128516", name="😄", desc="高兴"}},
            {"😊", new EmojiTypeInfo{type="2", id="128522", name="😊", desc="嘿嘿"}},
            {"😌", new EmojiTypeInfo{type="2", id="128524", name="😌", desc="羞涩"}},
            {"😏", new EmojiTypeInfo{type="2", id="128527", name="😏", desc="哼哼"}},
            {"😒", new EmojiTypeInfo{type="2", id="128530", name="😒", desc="不屑"}},
            {"😓", new EmojiTypeInfo{type="2", id="128531", name="😓", desc="汗"}},
            {"😔", new EmojiTypeInfo{type="2", id="128532", name="😔", desc="失落"}},
            {"😘", new EmojiTypeInfo{type="2", id="128536", name="😘", desc="飞吻"}},
            {"😚", new EmojiTypeInfo{type="2", id="128538", name="😚", desc="亲亲"}},
            {"😜", new EmojiTypeInfo{type="2", id="128540", name="😜", desc="淘气"}},
            {"😝", new EmojiTypeInfo{type="2", id="128541", name="😝", desc="吐舌"}},
            {"😭", new EmojiTypeInfo{type="2", id="128557", name="😭", desc="大哭"}},
            {"😰", new EmojiTypeInfo{type="2", id="128560", name="😰", desc="紧张"}},
            {"😳", new EmojiTypeInfo{type="2", id="128563", name="😳", desc="瞪眼"}},

        };

    }

    public class EmojiTypeInfo
    {
        public string type;
        public string id;
        public string name;
        public string desc;
    }
}
