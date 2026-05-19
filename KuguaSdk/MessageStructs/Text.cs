namespace KuguaSdk.MessageStructs
{
    /// <summary>
    /// 文本消息
    /// </summary>
    public class Text : Message
    {
        public string text { get; set; }

        public Text(string text="")
        {
            this.text = text;
        }


    }

}
