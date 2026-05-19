namespace KuguaSdk.MessageStructs
{
    /// <summary>
    /// 匿名消息
    /// </summary>
    public class AnonymousMesssage : Message
    {
        public bool ignore { get; set; }

        public AnonymousMesssage()
        {
        }

        public AnonymousMesssage(bool ignore)
        {
            this.ignore = ignore;
        }
    }







}
