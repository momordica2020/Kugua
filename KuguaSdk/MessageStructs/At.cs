namespace KuguaSdk.MessageStructs
{
    /// <summary>
    /// @某人
    /// </summary>
    public class At : Message
    {
        public string qq { get; set; }
        public At()
        {

        }
        public At(string qq)
        {
            this.qq = qq;
        }
        public At(long qq)
        {
            this.qq = qq.ToString();
        }

    }







}
