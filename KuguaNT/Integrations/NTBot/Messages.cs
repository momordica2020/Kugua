using ImageMagick;
using ImageMagick.Formats;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NvAPIWrapper.Native.GPU;

namespace Kugua.Integrations.NTBot
{

    public class MessageInfo
    {
        public string type;
        public Message data;

        public MessageInfo() { }

        public MessageInfo(Message data)
        {
            this.type = (
                data is Text ? "text" :
                data is ImageBasic ? "image" :
                data is Face ? "face" :
                data is At ? "at" :
                data is Video ? "video" :
                data is Rps ? "rps" :
                data is Dice ? "dice" :
                //data is Shake ? "shake" :
                data is Poke ? "poke" :
                data is AnonymousMesssage ? "anonymous" :
                data is Share ? "share" :
                data is Contact ? "contact" :
                data is Location ? "location" :
                data is Music ? "music" :
                data is Reply ? "reply" :
                data is Record ? "record" :
                data is XmlData ? "xml" :
                data is JsonData ? "json" :
                data is ForwardNodeExist ? "node" :
                data is ForwardNodeNew ? "node" :
                data is MFace ? "mface" :
                "");


            this.data = data;
        }
    }

















    public interface Message
    {

    }



    /// <summary>
    /// 文本消息
    /// </summary>
    public class Text : Message
    {
        public string text { get; set; }

        public Text(string text)
        {
            this.text = text;
        }
    }

    public class ImageBasic : Message
    {
        public string file;
        //public string file_id;
        public string url;
        //public string file_unique;
        public string summary;

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
    }

    public class ImageRecvNormal : ImageBasic
    {
        /// <summary>
        /// [动画表情]值为1，其他0
        /// </summary>
        public int sub_type;
        /// <summary>
        /// 单位是byte
        /// </summary>
        public string file_size;
    }

    public class ImageRecvMarketFace : ImageBasic
    {
        //public string path;
        public string key;
        public string emoji_id;
        public string emoji_package_id;
    }

    /// <summary>
    /// 图片消息
    /// </summary>
    public class ImageSend : ImageBasic
    {

        /// <summary>
        /// 图片路径格式：
        /// file://C:\\Users\Richard\Pictures\1.png
        /// http://i1.piimg.com/567571/fdd6e7b6d93f1ef0.jpg
        /// base64://iVBORw0KGgoAAAANSUhEUgAAABQAAAAVCAIAAADJt1n/AAAAKElEQVQ4EWPk5+RmIBcwkasRpG9UM4mhNxpgowFGMARGEwnBIEJVAAAdBgBNAZf+QAAAAABJRU5ErkJggg==
        /// </summary>
        //public string file { get; set; }
        public string type { get; set; }  // 可选项，flash表示闪照
        //public string? url { get; set; }   // 图片 URL
        public int? cache { get; set; }   // 图片缓存标志
        public int? proxy { get; set; }   // 是否使用代理
        public int? timeout { get; set; } // 下载超时时间

        public ImageSend()
        {

        }

        public ImageSend(string file)
        {
            this.file = file;
        }

        public ImageSend(string file, string type, int cache = 1, int proxy = 0, int timeout = 3)
        {
            this.file = file;
            this.type = type;
            this.cache = cache;
            this.proxy = proxy;
            this.timeout = timeout;
        }

        public ImageSend(MagickImage image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Write(ms);
                this.file = $"base64://{Convert.ToBase64String(ms.ToArray())}";
            }
        }

        public ImageSend(MagickImageCollection images)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                images.Write(ms);
                this.file = $"base64://{Convert.ToBase64String(ms.ToArray())}";
            }
        }
    }

    /// <summary>
    /// 表情
    /// </summary>
    public class Face : Message
    {
        public string id { get; set; }

        public Face(string id)
        {
            this.id = id;
        }
    }

    public class MFace : Message
    {
        public string emoji_id;
        public string emoji_package_id;
        public string key;
        public string summary;
    }

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

    /// <summary>
    /// 视频消息
    /// </summary>
    public class Video : Message
    {
        public string file { get; set; }
        public string? url { get; set; }

        public int? cache { get; set; }

        public int? proxy { get; set; }

        public int? timeout { get; set; } // 下载超时时间
        public Video()
        {

        }
        public Video(string file)
        {
            this.file = file;
        }

        public Video(string file, int cache, int proxy, int timeout = 3)
        {
            this.file = file;
            this.cache = cache;
            this.proxy = proxy;
            this.timeout = timeout;
        }
    }

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

    /// <summary>
    /// 猜拳
    /// </summary>
    public class Rps : Message
    {
        public int? result;
        public Rps()
        {

        }

    }

    /// <summary>
    /// 骰子
    /// </summary>
    public class Dice : Message
    {
        public int? result;
        public Dice()
        {

        }

    }

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

    /// <summary>
    /// 匿名消息
    /// </summary>
    public class AnonymousMesssage : Message
    {
        public bool ignore { get; set; }

        public AnonymousMesssage()
        {
        }

        public AnonymousMesssage(bool ignore)
        {
            this.ignore = ignore;
        }
    }

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


    /// <summary>
    /// 回复
    /// </summary>
    public class Reply : Message
    {
        public string id { get; set; }

        public Reply()
        {
        }
    }

    /// <summary>
    /// 合并转发已有节点，直接发messageid即可
    /// </summary>
    public class ForwardNodeExist : Message
    {
        public string id { get; set; }
        [JsonIgnore]
        public List<forward_message_node> content;

    }

    /// <summary>
    /// 合并转发的新造节点
    /// </summary>
    public class ForwardNodeNew : Message
    {
        public string user_id;
        public string nickname;

        /// <summary>
        /// id content 二选一
        /// </summary>
        //public string id;
        /// <summary>
        /// content
        /// </summary>
        public List<MessageInfo> content;
    }

    /// <summary>
    /// 接收到的转发消息详情
    /// </summary>
    public class ForwardContent
    {
        public string status;
        public int retcode;

        public string message;
        public string wording;
        public string echo;

        [JsonIgnore]
        // 该名称与协议中不同，此处为省略嵌套后的处理结果。在协议json中为data->messages
        public List<forward_message_node> datas;

        [JsonIgnore]
        public string group_id { get; set; }
        [JsonIgnore]
        public string user_id { get; set; }
        [JsonIgnore]
        public message_sender sender { get; set; }
    }



    public class ForwardSenderSingle
    {
        public string group_id;
        public string message_id;
    }

    public class XmlData : Message
    {
        public string data;
    }

    public class JsonData : Message
    {
        public string data;
    }

    /// <summary>
    /// 群消息的动画表情回应
    /// </summary>
    public class ReactLike : Message
    {
        public EmojiTypeInfo emoji;

        public ReactLike(EmojiTypeInfo emoji)
        {
            this.emoji = emoji;
        }
    }
}
