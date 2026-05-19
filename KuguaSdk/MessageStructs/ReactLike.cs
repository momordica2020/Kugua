namespace KuguaSdk.MessageStructs
{
    /// <summary>
    /// 群消息的动画表情回应
    /// </summary>
    public class ReactLike : Message
    {
        public EmojiTypeInfo emoji;

        public ReactLike(EmojiTypeInfo emoji)
        {
            this.emoji = emoji;
        }
    }







}
