using Microsoft.AspNetCore.Mvc.TagHelpers;
using Newtonsoft.Json;

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
                data is Text?"text":
                data is Image?"image":
                data is Face ? "face" :
                data is At ? "at" :
                data is Video ? "video" :
                data is  Rps? "rps" :
                data is Dice ? "dice" :
                data is Shake ? "shake" :
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
                "" );


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

    /// <summary>
    /// 图片消息
    /// </summary>
    public class Image : Message
    {
 
        /// <summary>
        /// 图片路径格式：
        /// file:///C:\\Users\Richard\Pictures\1.png
        /// http://i1.piimg.com/567571/fdd6e7b6d93f1ef0.jpg
        /// base64://iVBORw0KGgoAAAANSUhEUgAAABQAAAAVCAIAAADJt1n/AAAAKElEQVQ4EWPk5+RmIBcwkasRpG9UM4mhNxpgowFGMARGEwnBIEJVAAAdBgBNAZf+QAAAAABJRU5ErkJggg==
        /// </summary>
        public string file { get; set; }
        public string type { get; set; }  // 可选项，flash表示闪照
        public string? url { get; set; }   // 图片 URL
        public int? cache { get; set; }   // 图片缓存标志
        public int? proxy { get; set; }   // 是否使用代理
        public int? timeout { get; set; } // 下载超时时间

        public Image()
        {

        }

        public Image(string file)
        {
            this.file = file;
        }

        public Image(string file, string type, int cache=1, int proxy=0, int timeout = 3)
        {
            this.file =file;
            this.type =type;
            this.cache =cache;
            this.proxy =proxy;
            this.timeout =timeout;
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

    /// <summary>
    /// 语音消息
    /// </summary>
    public class Record : Message
    {
        public string file { get; set; }
        public int magic {  get; set; }
        public string? url {  get; set; }

        public int? cache { get; set; }

        public int? proxy { get; set; }

        public int? timeout { get; set; } // 下载超时时间

        public Record()
        {
            
        }

        public Record(string file, int magic = 0)
        {
            this.file = file;
            this.magic = magic;
        }
        public Record(string file, int magic = 0, int cache = 1, int proxy = 0, int timeout = 3)
        {
            this.file = file;
            this.magic = magic;
            this.cache = cache;
            this.proxy = proxy;
            this.timeout = timeout;
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

    /// <summary>
    /// 窗口抖动
    /// </summary>
    public class Shake : Message
    {
        public Shake()
        {

        }

    }

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
            this.type= type;
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
        public Location(string lat, string lon,string title, string content)
        {  
            this.lat = lat; 
            this.lon = lon; 
            this.title= title;
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
    /// 合并转发,需要get_forward_msg 获取节点
    /// </summary>
    public class ForwardMessage : Message
    {
        public string id { get; set; }

        public ForwardMessage()
        {
        }
    }

    public class ForwardNode : Message
    {
        string user_id;
        string nickname;
        Message[] content;
    }

    public class XmlData : Message
    {
        public string data;
    }

    public class JsonData: Message
    {
        public string data;
    }
}
