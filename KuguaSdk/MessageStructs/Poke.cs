namespace KuguaSdk.MessageStructs
{
    ///// <summary>
    ///// 窗口抖动
    ///// </summary>
    //public class Shake : Message
    //{
    //    //public object type = null;
    //    public Shake()
    //    {

    //    }

    //}

    /// <summary>
    /// 戳一戳
    /// </summary>
    public class Poke : Message
    {
        public string type { get; set; }

        public string id { get; set; }
        public Poke()
        {

        }

        public Poke(string type, string id)
        {
            this.type = type;
            this.id = id;
        }
    }







}
