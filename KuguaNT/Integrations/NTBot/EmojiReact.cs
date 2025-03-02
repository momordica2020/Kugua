namespace Kugua.Integrations.NTBot
{
    public class EmojiReact
    {
        #region 单例
        private static readonly Lazy<EmojiReact> instance = new Lazy<EmojiReact>(() => new EmojiReact());

        public static EmojiReact Instance => instance.Value;

        #endregion

        const string emoji_likes_file = "emoji_likes.txt";
        public Dictionary<string, EmojiTypeInfo> emojiTypeInfos = new Dictionary<string, EmojiTypeInfo>();


        private EmojiReact()
        {
            try
            {
                // 读取emoji_likes
                emojiTypeInfos = new Dictionary<string, EmojiTypeInfo>();
                foreach (var line in LocalStorage.ReadResourceLines(emoji_likes_file))
                {
                    var parts = line.Split('\t', StringSplitOptions.TrimEntries);
                    if (parts.Length >= 3)
                    {
                        emojiTypeInfos[parts[1]] = new EmojiTypeInfo { type = parts[0], id = parts[1], name = parts[2] };
                        if (parts.Length >= 4) emojiTypeInfos[parts[1]].desc = parts[3];
                    }
                }
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
            if (emojiTypeInfos.ContainsKey(id)) return emojiTypeInfos[id];
            return null;
        }

    }

    public class EmojiTypeInfo
    {
        public string type;
        public string id;
        public string name;
        public string desc;
    }
}
