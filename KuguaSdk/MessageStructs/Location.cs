namespace KuguaSdk.MessageStructs
{
    /// <summary>
    /// 位置信息
    /// </summary>
    public class Location : Message
    {
        public string lat { get; set; }
        public string lon { get; set; }
        public string title { get; set; }
        public string? content { get; set; }
        public Location()
        {
        }
        public Location(string lat, string lon, string title, string content)
        {
            this.lat = lat;
            this.lon = lon;
            this.title = title;
            this.content = content;
        }
    }







}
