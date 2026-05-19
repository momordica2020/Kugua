using ImageMagick;
using Newtonsoft.Json;

namespace KuguaSdk.MessageStructs
{
    public class Image : Message
    {
        public string file;
        //public string file_id;
        public string url;
        //public string file_unique;
        public string summary;

        /// <summary>
        /// [动画表情]值为1，其他0
        /// </summary>
        public int sub_type;

        // marked face
        public string key;
        public string emoji_id;
        public string emoji_package_id;

        // normal image
        /// <summary>
        /// 单位是byte
        /// </summary>
        public string file_size;


        /// <summary>
        /// 该图片是否属于市场表情
        /// </summary>
        [JsonIgnore]
        public bool IsMarketFace
        {
            get
            {
                return this.sub_type==1;
            }
        }

        /// <summary>
        /// 用于在线api的文件后缀名，默认是jpeg
        /// </summary>
        [JsonIgnore]
        public string ext
        {
            get
            {
                if (string.IsNullOrWhiteSpace(file)) return "jpeg";
                else
                {
                    try
                    {
                        switch (file.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Last())
                        {
                            case "jpg":
                            case "jpeg": return "jpeg";
                            case "png": return "png";
                            case "gif": return "gif";
                            case "webp": return "webp";
                            default: return "jpeg";
                        }
                    }
                    catch (Exception) { }
                }
                return "jpeg";
            }
        }







        /// <summary>
        /// 图片路径格式：
        /// file://C:\\Users\Richard\Pictures\1.png
        /// http://i1.piimg.com/567571/fdd6e7b6d93f1ef0.jpg
        /// base64://iVBORw0KGgoAAAANSUhEUgAAABQAAAAVCAIAAADJt1n/AAAAKElEQVQ4EWPk5+RmIBcwkasRpG9UM4mhNxpgowFGMARGEwnBIEJVAAAdBgBNAZf+QAAAAABJRU5ErkJggg==
        /// </summary>
        //public string file { get; set; }
        //public string type { get; set; }  // 可选项，flash表示闪照
        ////public string? url { get; set; }   // 图片 URL
        //public int? cache { get; set; }   // 图片缓存标志
        //public int? proxy { get; set; }   // 是否使用代理
        //public int? timeout { get; set; } // 下载超时时间

        public Image()
        {

        }

        public Image(string file)
        {
            this.file = file;
        }

        //public ImageBasic(string file, string type, int cache = 1, int proxy = 0, int timeout = 3)
        //{
        //    this.file = file;
        //    this.type = type;
        //    this.cache = cache;
        //    this.proxy = proxy;
        //    this.timeout = timeout;
        //}

        public Image(MagickImage image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Write(ms);
                this.file = $"base64://{Convert.ToBase64String(ms.ToArray())}";
            }
        }

        public Image(MagickImageCollection images)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                images.Write(ms);
                this.file = $"base64://{Convert.ToBase64String(ms.ToArray())}";
            }
        }
    }



}
