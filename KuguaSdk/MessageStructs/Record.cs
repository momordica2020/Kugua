namespace KuguaSdk.MessageStructs
{
    /// <summary>
    /// 语音消息
    /// </summary>
    public class Record : Message
    {
        public string file { get; set; }
        //public int magic { get; set; }
        public string? url { get; set; }
        public string? path { get; set; }
        public int? file_size { get; set; }
        //public int? cache { get; set; }

        //public int? proxy { get; set; }

        //public int? timeout { get; set; } // 下载超时时间

        public Record()
        {

        }

        //public Record(string file, int magic = 0)
        //{
        //    this.file = file;
        //    this.magic = magic;
        //}
        public Record(string file)//, int magic = 0, int cache = 1, int proxy = 0, int timeout = 3)
        {
            this.file = file;
            //this.magic = magic;
            //this.cache = cache;
            //this.proxy = proxy;
            //this.timeout = timeout;
        }
    }







}
