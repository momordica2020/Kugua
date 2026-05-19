namespace KuguaSdk.MessageStructs
{
    /// <summary>
    /// 视频消息
    /// </summary>
    public class Video : Message
    {
        public string file { get; set; }
        public string? url { get; set; }

        public int? file_size { get; set; }

        //public int? cache { get; set; }

        //public int? proxy { get; set; }

        //public int? timeout { get; set; } // 下载超时时间
        public Video()
        {

        }
        public Video(string file)
        {
            this.file = file;
        }


    }







}
