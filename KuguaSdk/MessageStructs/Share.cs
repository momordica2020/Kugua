namespace KuguaSdk.MessageStructs
{
    /// <summary>
    /// 链接分享
    /// </summary>
    public class Share : Message
    {
        public string url { get; set; }
        public string title { get; set; }
        public string? content { get; set; }
        public string? image { get; set; }

        public Share()
        {
        }

        public Share(string url, string title, string? content, string? image)
        {
            this.url = url;
            this.title = title;
            this.content = content;
            this.image = image;
        }
    }







}
