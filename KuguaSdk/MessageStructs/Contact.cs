namespace KuguaSdk.MessageStructs
{
    /// <summary>
    /// 推荐好友/推荐群
    /// </summary>
    public class Contact : Message
    {
        public string type { get; set; }
        public string id { get; set; }

        public Contact()
        {
        }

        public Contact(string type, string id)
        {
            this.type = type;
            this.id = id;
        }
    }







}
