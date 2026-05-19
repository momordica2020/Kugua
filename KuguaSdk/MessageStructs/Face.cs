namespace KuguaSdk.MessageStructs
{
    /// <summary>
    /// 表情
    /// </summary>
    public class Face : Message
    {
        public string id { get; set; }

        public Face(string id="")
        {
            this.id = id;
        }


    }







}
