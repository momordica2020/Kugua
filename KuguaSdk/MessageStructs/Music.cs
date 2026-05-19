namespace KuguaSdk.MessageStructs
{
    /// <summary>
    /// 音乐分享
    /// </summary>
    public class Music : Message
    {
        /// <summary>
        /// type:qq, 163, xm, custom
        /// </summary>
        //[JsonProperty("type")]
        public string type { get; set; }

        // [JsonProperty("id")]
        public string id { get; set; }

        //[JsonProperty("url")]
        public string? url { get; set; }

        // [JsonProperty("audio")]
        public string? audio { get; set; }

        //[JsonProperty("title")]
        public string? title { get; set; }

        //[JsonProperty("content")]
        public string? content { get; set; }

        // [JsonProperty("image")]
        public string? image { get; set; }

        public Music()
        {
        }

        public Music(string type, string id)
        {
            this.type = type;
            this.id = id;
        }

        public Music(string type, string id, string? url, string? audio, string? title, string? content, string? image) : this(type, id)
        {
            this.url = url;
            this.audio = audio;
            this.title = title;
            this.content = content;
            this.image = image;
        }
    }







}
