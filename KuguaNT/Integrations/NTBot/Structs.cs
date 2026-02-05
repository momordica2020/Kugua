using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Kugua.Integrations.NTBot
{
    public class event_base
    {
        public long time { get; set; }

        public string self_id { get; set; }

        public string post_type { get; set; }
    }

    #region message
    public class group_message_event : event_base
    {
        public string message_type { get; set; }

        public string sub_type { get; set; }

        public string message_id { get; set; }

        public string group_id { get; set; }

        public string user_id { get; set; }

        public group_message_anonymous anonymous { get; set; }
        [JsonIgnore]
        public List<Message> message { get; set; }

        public string raw_message { get; set; }

        public int font { get; set; }

        public message_sender sender { get; set; }
    }

    public class group_message_anonymous
    {
        public string id { get; set; }

        public string name { get; set; }

        public string flag { get; set; }
    }

    public class private_message_event : event_base
    {
        public string message_type { get; set; }

        public string sub_type { get; set; }

        public string message_id { get; set; }

        public string user_id { get; set; }
        [JsonIgnore]
        public List<Message> message { get; set; }

        public string raw_message { get; set; }

        public int font { get; set; }

        public message_sender sender { get; set; } // 发送人信息
    }

    /// <summary>
    /// 转发消息的内部子节点获取到的结构
    /// </summary>
    public class forward_message_node : private_message_event
    {
        // = message_id
        public string message_seq { get; set; }

        // = message_id
        public string real_id { get; set; }

        // array
        public string message_format { get; set; }  
    }

    public class message_sender
    {
        public string user_id { get; set; }

        public string? nickname { get; set; }

        public string? card { get; set; }

        public string? sex { get; set; }

        public string? age { get; set; }

        public string? area { get; set; }

        public string? level { get; set; }

        public string? role { get; set; }

        public string? title { get; set; }
    }

    public static class MessageUtil
    {
        public static string ToTextString(this List<MessageInfo> array)
        {
            StringBuilder sb = new();
            foreach (var i in array)
            {
                if (i.data is Text)
                {
                    sb.Append((i.data as Text).text);
                }
                else if (i.data is Dice d)
                {
                    if (d.result != 0) sb.Append(d.result.ToString());
                }
            }
            return sb.ToString();
        }

        public static string ToTextString(this List<Message> array)
        {
            StringBuilder sb = new();
            foreach (var i in array)
            {
                if (i is Text)
                {
                    sb.Append((i as Text).text);
                }
                else if(i is Dice d)
                {
                    if(d.result!=0)sb.Append(d.result.ToString());
                }
            }
            return sb.ToString();
        }
    }

    #endregion message





    #region notice
    /// <summary>
    /// 群上传文件事件
    /// </summary>
    public class group_upload_event : event_base
    {
        /// <summary>
        /// group_upload
        /// </summary>
        public string notice_type { get; set; }

        public string group_id { get; set; }

        public string user_id { get; set; }

        public group_upload_file file { get; set; }
    }
    /// <summary>
    /// 群上传文件
    /// </summary>
    public class group_upload_file
    {
        public string id { get; set; }

        public string name { get; set; }
        /// <summary>
        /// 文件大小（字节数）
        /// </summary>
        public long size { get; set; }
        /// <summary>
        /// 暂时无用
        /// </summary>
        public long busid { get; set; }
    }
    /// <summary>
    /// 群管理员变动
    /// </summary>
    public class group_admin_event : event_base
    {
        /// <summary>
        /// group_admin
        /// </summary>
        public string notice_type { get; set; }
        /// <summary>
        /// set / unset
        /// </summary>
        public string sub_type { get; set; }

        public string group_id { get; set; }

        public string user_id { get; set; }
    }
    public class group_decrease_event : event_base
    {
        /// <summary>
        /// group_decrease
        /// </summary>
        public string notice_type { get; set; }
        /// <summary>
        /// leave / kick / kick_me
        /// </summary>
        public string sub_type { get; set; }

        public string group_id { get; set; }
        /// <summary>
        /// 操作者qq号（如果是主动退群，则和 user_id 相同）
        /// </summary>
        public string operator_id { get; set; }
        /// <summary>
        /// 离开者qq号
        /// </summary>
        public string user_id;
    }
    /// <summary>
    /// 群成员增加
    /// </summary>
    public class group_increase_event : event_base
    {
        /// <summary>
        /// group_increase
        /// </summary>
        public string notice_type { get; set; }
        /// <summary>
        /// approve  /  invite
        /// </summary>
        public string sub_type { get; set; }

        public string group_id { get; set; }
        /// <summary>
        /// 操作者qq号
        /// </summary>
        public string operator_id { get; set; }
        /// <summary>
        /// 目标qq号
        /// </summary>
        public string user_id;
    }

    /// <summary>
    /// 群禁言
    /// </summary>
    public class group_ban_event : event_base
    {
        /// <summary>
        /// group_ban
        /// </summary>
        public string notice_type { get; set; }
        /// <summary>
        /// ban  /  lift_ban  禁言和解除禁言
        /// </summary>
        public string sub_type { get; set; }

        public string group_id { get; set; }
        /// <summary>
        /// 操作者qq号
        /// </summary>
        public string operator_id { get; set; }
        /// <summary>
        /// 目标qq号
        /// </summary>
        public string user_id;
        /// <summary>
        /// 禁言时长，单位是秒
        /// </summary>
        public int duration;
    }
    /// <summary>
    /// 好友添加
    /// </summary>
    public class friend_add_event : event_base
    {
        /// <summary>
        /// friend_add
        /// </summary>
        public string notice_type { get; set; }
        /// <summary>
        /// 目标qq号
        /// </summary>
        public string user_id;
    }
    /// <summary>
    /// 群消息撤回
    /// </summary>
    public class group_recall_event : event_base
    {
        /// <summary>
        /// group_recall
        /// </summary>
        public string notice_type { get; set; }
        public string group_id { get; set; }
        /// <summary>
        /// 目标qq号
        /// </summary>
        public string user_id;
        /// <summary>
        /// 操作者qq号
        /// </summary>
        public string operator_id { get; set; }
        /// <summary>
        /// 被撤回的消息id
        /// </summary>
        public string message_id;
    }
    /// <summary>
    /// 好友消息撤回
    /// </summary>
    public class friend_recall_event : event_base
    {
        /// <summary>
        /// friend_recall
        /// </summary>
        public string notice_type { get; set; }
        /// <summary>
        /// 目标qq号
        /// </summary>
        public string user_id;
        /// <summary>
        /// 被撤回的消息id
        /// </summary>
        public string message_id;
    }
    /// <summary>
    /// 群内戳一戳 / 运气王
    /// </summary>
    public class notify_event : event_base
    {
        /// <summary>
        /// notify
        /// </summary>
        public string notice_type { get; set; }
        /// <summary>
        /// poke / lucky_king
        /// </summary>
        public string sub_type { get; set; }
        public string group_id;
        public string user_id;
        public string target_id;
    }
    /// <summary>
    /// 群内荣耀变更
    /// </summary>
    public class notify_honor_event : event_base
    {
        /// <summary>
        /// notify
        /// </summary>
        public string notice_type { get; set; }
        /// <summary>
        /// honor
        /// </summary>
        public string sub_type { get; set; }
        public string group_id;
        public string user_id;
        /// <summary>
        /// talkative / performer / emotion
        /// </summary>
        public string honor_type;
    }


    public class notify_group_msg_emoji_like : event_base
    {
        public string group_id;
        public string user_id;
        public string notice_type;
        public string message_id;
        //public JToken[] likes;

    }
    #endregion notice




    #region request

    public class friend_request_event : event_base
    {
        /// <summary>
        /// friend
        /// </summary>
        public string request_type { get; set; }

        public string user_id { get; set; }
        /// <summary>
        /// 验证信息
        /// </summary>
        public string comment { get; set; }
        /// <summary>
        /// 请求 flag，在调用处理请求的 API 时需要传入
        /// </summary>
        public string flag { get; set; }

        //public bool? approve { get; set; }
        /////
        //public string? remark { get; set; }
    }
    public class group_request_event : event_base
    {
        /// <summary>
        /// group
        /// </summary>
        public string request_type { get; set; }
        /// <summary>
        /// add、invite 分别表示加群请求、邀请登录号入群
        /// </summary>
        public string sub_type { get; set; }

        public string group_id { get; set; }

        public string user_id { get; set; }
        /// <summary>
        /// 验证信息
        /// </summary>
        public string comment { get; set; }
        /// <summary>
        /// 请求 flag，在调用处理请求的 API 时需要传入
        /// </summary>
        public string flag { get; set; }
    }

    #endregion request


    #region meta


    public class lifecycle_event : event_base
    {
        /// <summary>
        /// livecycle
        /// </summary>
        public string meta_event_type { get; set; }

        /// <summary>
        /// enable、disable、connect
        /// </summary>
        public string sub_type { get; set; }
    }


    public class heartbeat_event : event_base
    {
        /// <summary>
        /// heartbeat
        /// </summary>
        public string meta_event_type { get; set; }

        public dynamic status { get; set; } // 状态信息，根据 get_status 接口的定义
        /// <summary>
        /// 到下次心跳的间隔，单位毫秒
        /// </summary>
        public long interval { get; set; } // 到下次心跳的间隔，单位毫秒
    }



    public class login_info_response
    {
        public long user_id { get; set; }
        public string nickname { get; set; }
    }
    #endregion meta






    #region legacy

    //    /// <summary>
    //    /// 信息类的公开定义
    //    /// </summary>
    //    public class Message
    //{
    //    /// <summary>
    //    /// 信息类型
    //    /// </summary>
    //    public string type { get; protected set; }


    //}
    ///// <summary>
    ///// 源类型(永远为信息链的第一个元素)
    ///// </summary>
    //public sealed class Source : Message
    //{
    //    /// <summary>
    //    /// 消息的识别号，用于引用回复
    //    /// </summary>
    //    public long id { get; }
    //    /// <summary>
    //    /// 时间戳
    //    /// </summary>
    //    public long time { get; }
    //    /// <summary>
    //    /// 源类型(永远为信息链的第一个元素)
    //    /// </summary>
    //    /// <param name="id"></param>
    //    /// <param name="time"></param>
    //    public Source(long id, long time)
    //    {
    //        this.id = id;
    //        this.time = time;
    //        this.type = nameof(Source);
    //    }
    //}
    ///// <summary>
    ///// 回复类信息
    ///// </summary>
    //public sealed class Quote : Message
    //{
    //    /// <summary>
    //    /// 被引用回复的原消息的messageId
    //    /// </summary>
    //    public long id { get; }
    //    /// <summary>
    //    /// 被引用回复的原消息所接收的群号，当为好友消息时为0
    //    /// </summary>
    //    public long groupId { get; }
    //    /// <summary>
    //    /// 被引用回复的原消息的发送者的QQ号
    //    /// </summary>
    //    public long senderId { get; }
    //    /// <summary>
    //    /// 被引用回复的原消息的接收者者的QQ号（或群号）
    //    /// </summary>
    //    public long targetId { get; }
    //    /// <summary>
    //    /// 被引用回复的原消息的消息链对象
    //    /// </summary>
    //    public Message[] origin { get; }
    //    /// <summary>
    //    /// 回复类信息
    //    /// </summary>
    //    /// <param name="id"></param>
    //    /// <param name="groupId"></param>
    //    /// <param name="senderId"></param>
    //    /// <param name="targetId"></param>
    //    /// <param name="origin"></param>
    //    public Quote(long id, long groupId, long senderId, long targetId, Message[] origin)
    //    {
    //        this.id = id;
    //        this.groupId = groupId;
    //        this.senderId = senderId;
    //        this.targetId = targetId;
    //        this.origin = origin;
    //        this.type = nameof(Quote);
    //    }
    //}
    ///// <summary>
    ///// @类信息
    ///// </summary>
    //public sealed class At : Message
    //{
    //    /// <summary>
    //    /// 群员QQ号
    //    /// </summary>
    //    public long target { get; }
    //    /// <summary>
    //    /// At时显示的文字，发送消息时无效，自动使用群名片
    //    /// </summary>
    //    public string display { get; }
    //    /// <summary>
    //    /// @类信息
    //    /// </summary>
    //    /// <param name="target"></param>
    //    /// <param name="display"></param>
    //    public At(long target, string display)
    //    {
    //        this.target = target;
    //        this.display = display;
    //        this.type = nameof(At);
    //    }
    //}
    ///// <summary>
    ///// @全体类信息
    ///// </summary>
    //public sealed class AtAll : Message
    //{
    //    /// <summary>
    //    /// @全体类信息
    //    /// </summary>
    //    public AtAll()
    //    {
    //        this.type = nameof(AtAll);
    //    }
    //}
    ///// <summary>
    ///// 小图脸
    ///// </summary>
    //public sealed class Face : Message
    //{
    //    /// <summary>
    //    /// QQ表情编号，可选，优先高于name
    //    /// </summary>
    //    public long faceId { get; }
    //    /// <summary>
    //    /// QQ表情拼音，可选
    //    /// </summary>
    //    public string name { get; }
    //    /// <summary>
    //    /// 小图脸
    //    /// </summary>
    //    /// <param name="faceId">QQ表情编号，可选，优先高于name</param>
    //    /// <param name="name">QQ表情拼音，可选</param>
    //    public Face(long faceId, string name)
    //    {
    //        this.faceId = faceId;
    //        this.name = name;
    //        this.type = nameof(Face);
    //    }
    //}
    ///// <summary>
    ///// 文字消息
    ///// </summary>
    //public sealed class Plain : Message
    //{
    //    /// <summary>
    //    /// 文字
    //    /// </summary>
    //    public string text;
    //    /// <summary>
    //    /// 文字消息
    //    /// </summary>
    //    /// <param name="text">文字</param>
    //    public Plain(string text)
    //    {
    //        this.text = text;
    //        this.type = nameof(Plain);
    //    }
    //}
    ///// <summary>
    ///// 图片信息
    ///// </summary>
    //public class Image : Message
    //{
    //    /// <summary>
    //    /// 图片的imageId，群图片与好友图片格式不同。不为空时将忽略url属性
    //    /// </summary>
    //    public string? imageId { get; } = null;
    //    /// <summary>
    //    /// 图片的URL，发送时可作网络图片的链接；接收时为腾讯图片服务器的链接，可用于图片下载
    //    /// </summary>
    //    public string? url { get; } = null;
    //    /// <summary>
    //    /// 图片的路径，发送本地图片，路径相对于 JVM 工作路径（默认是当前路径，可通过 -Duser.dir=...指定），也可传入绝对路径。
    //    /// </summary>
    //    public string? path { get; } = null;
    //    /// <summary>
    //    /// 图片的 Base64 编码
    //    /// </summary>
    //    public string? base64 { get; } = null;
    //    /// <summary>
    //    /// 图片信息(构造参数任选其一，出现多个参数时，按照imageId > url > path > base64的优先级)
    //    /// </summary>
    //    /// <param name="imageId">图片的imageId，群图片与好友图片格式不同。不为空时将忽略url属性</param>
    //    /// <param name="url">图片的URL，发送时可作网络图片的链接；接收时为腾讯图片服务器的链接，可用于图片下载</param>
    //    /// <param name="path">图片的路径，发送本地图片，路径相对于 JVM 工作路径（默认是当前路径，可通过 -Duser.dir=...指定），也可传入绝对路径。</param>
    //    /// <param name="base64">图片的 Base64 编码</param>
    //    public Image(string imageId = null, string url = null, string path = null, string base64 = null)
    //    {
    //        this.imageId = imageId;
    //        this.url = url;
    //        this.path = path;
    //        this.base64 = base64;
    //        this.type = nameof(Image);
    //    }
    //}
    ///// <summary>
    ///// 闪照图片信息
    ///// </summary>
    //public sealed class FlashImage : Image
    //{
    //    /// <summary>
    //    /// 闪照图片(构造参数任选其一，出现多个参数时，按照imageId > url > path > base64的优先级)
    //    /// </summary>
    //    /// <param name="imageId">图片的imageId，群图片与好友图片格式不同。不为空时将忽略url属性</param>
    //    /// <param name="url">图片的URL，发送时可作网络图片的链接；接收时为腾讯图片服务器的链接，可用于图片下载</param>
    //    /// <param name="path">图片的路径，发送本地图片，路径相对于 JVM 工作路径（默认是当前路径，可通过 -Duser.dir=...指定），也可传入绝对路径。</param>
    //    /// <param name="base64">图片的 Base64 编码</param>
    //    public FlashImage(string imageId = null, string url = null, string path = null, string base64 = null) : base(imageId, url, path, base64)
    //    {
    //        this.type = nameof(FlashImage);
    //    }
    //}
    ///// <summary>
    ///// 语音信息
    ///// </summary>
    //public sealed class Voice : Message
    //{
    //    /// <summary>
    //    /// 语音的voiceId，不为空时将忽略url属性
    //    /// </summary>
    //    public string voiceId { get; } = null;
    //    /// <summary>
    //    /// 语音的URL，发送时可作网络语音的链接；接收时为腾讯语音服务器的链接，可用于语音下载
    //    /// </summary>
    //    public string url { get; } = null;
    //    /// <summary>
    //    /// 语音的路径，发送本地语音，路径相对于 JVM 工作路径（默认是当前路径，可通过 -Duser.dir=...指定），也可传入绝对路径。
    //    /// </summary>
    //    public string path { get; } = null;
    //    /// <summary>
    //    /// 语音的 Base64 编码
    //    /// </summary>
    //    public string base64 { get; } = null;
    //    /// <summary>
    //    /// 返回的语音长度, 发送消息时可以不传
    //    /// </summary>
    //    public long length { get; }
    //    /// <summary>
    //    /// 语音信息(构造参数任选其一，出现多个参数时，按照voiceId > url > path > base64的优先级)
    //    /// </summary>
    //    /// <param name="voiceId">语音的voiceId，不为空时将忽略url属性</param>
    //    /// <param name="url">语音的URL，发送时可作网络语音的链接；接收时为腾讯语音服务器的链接，可用于语音下载</param>
    //    /// <param name="path">语音的路径，发送本地语音，路径相对于 JVM 工作路径（默认是当前路径，可通过 -Duser.dir=...指定），也可传入绝对路径。</param>
    //    /// <param name="base64">语音的 Base64 编码</param>
    //    /// <param name="length">返回的语音长度, 发送消息时可以不传</param>
    //    public Voice(string voiceId = null, string url = null, string path = null, string base64 = null, long length = 0)
    //    {
    //        this.voiceId = voiceId;
    //        this.url = url;
    //        this.path = path;
    //        this.base64 = base64;
    //        this.length = length;
    //        this.type = nameof(Voice);
    //    }
    //}
    ///// <summary>
    ///// XML信息
    ///// </summary>
    //public sealed class Xml : Message
    //{
    //    /// <summary>
    //    /// XML文本
    //    /// </summary>
    //    public string xml { get; }
    //    /// <summary>
    //    /// XML信息
    //    /// </summary>
    //    /// <param name="xml">XML文本</param>
    //    public Xml(string xml)
    //    {
    //        this.xml = xml;
    //        this.type = nameof(Xml);
    //    }
    //}
    ///// <summary>
    ///// Json信息
    ///// </summary>
    //public sealed class Json : Message
    //{
    //    /// <summary>
    //    /// Json文本
    //    /// </summary>
    //    public string json { get; }
    //    /// <summary>
    //    /// Json信息
    //    /// </summary>
    //    /// <param name="json">Json文本</param>
    //    public Json(string json)
    //    {
    //        this.json = json;
    //        this.type = nameof(Json);
    //    }
    //}
    ///// <summary>
    ///// 应用消息
    ///// </summary>
    //public sealed class App : Message
    //{
    //    /// <summary>
    //    /// 内容
    //    /// </summary>
    //    public string content { get; }
    //    /// <summary>
    //    /// 应用消息
    //    /// </summary>
    //    /// <param name="content">内容</param>
    //    public App(string content)
    //    {
    //        this.content = content;
    //        this.type = nameof(App);
    //    }
    //}
    ///// <summary>
    ///// 窗口震动(戳一戳)
    ///// </summary>
    //public sealed class Poke : Message
    //{
    //    /// <summary>
    //    /// 戳一戳的类型
    //    /// "Poke": 戳一戳 "ShowLove": 比心 "Like": 点赞 "Heartbroken": 心碎 "SixSixSix": 666 "FangDaZhao": 放大招
    //    /// </summary>
    //    public string name { get; }
    //    /// <summary>
    //    /// 窗口震动(戳一戳)
    //    /// </summary>
    //    /// <param name="name">戳一戳的类型 {"Poke": 戳一戳 "ShowLove": 比心 "Like": 点赞 "Heartbroken": 心碎 "SixSixSix": 666 "FangDaZhao": 放大招}</param>
    //    public Poke(string name)
    //    {
    //        this.name = name;
    //        this.type = nameof(Poke);
    //    }
    //}
    ///// <summary>
    ///// 骰子信息
    ///// </summary>
    //public sealed class Dice : Message
    //{
    //    /// <summary>
    //    /// 数值
    //    /// </summary>
    //    public int value { get; }
    //    /// <summary>
    //    /// 骰子信息
    //    /// </summary>
    //    /// <param name="value">数值</param>
    //    public Dice(int value)
    //    {
    //        this.value = value;
    //        this.type = nameof(Dice);
    //    }
    //}
    ///// <summary>
    ///// 音乐分享信息
    ///// </summary>
    //public sealed class MusicShare : Message
    //{
    //    /// <summary>
    //    /// 类型
    //    /// </summary>
    //    public string kind { get; }
    //    /// <summary>
    //    /// 标题
    //    /// </summary>
    //    public string title { get; }
    //    /// <summary>
    //    /// 概括
    //    /// </summary>
    //    public string summary { get; }
    //    /// <summary>
    //    /// 跳转路径
    //    /// </summary>
    //    public string jumpUrl { get; }
    //    /// <summary>
    //    /// 封面路径
    //    /// </summary>
    //    public string pictureUrl { get; }
    //    /// <summary>
    //    /// 音乐路径
    //    /// </summary>
    //    public string musicUrl { get; }
    //    /// <summary>
    //    /// 简介
    //    /// </summary>
    //    public string brief { get; }
    //    /// <summary>
    //    /// 生成音乐分享
    //    /// </summary>
    //    /// <param name="kind">类型</param>
    //    /// <param name="title">标题</param>
    //    /// <param name="summary">概括</param>
    //    /// <param name="jumpUrl">跳转路径</param>
    //    /// <param name="pictureUrl">封面路径</param>
    //    /// <param name="musicUrl">音乐路径</param>
    //    /// <param name="brief">简介</param>
    //    public MusicShare(string kind, string title, string summary, string jumpUrl, string pictureUrl, string musicUrl, string brief)
    //    {
    //        this.kind = kind;
    //        this.title = title;
    //        this.summary = summary;
    //        this.jumpUrl = jumpUrl;
    //        this.pictureUrl = pictureUrl;
    //        this.musicUrl = musicUrl;
    //        this.brief = brief;
    //        this.type = nameof(MusicShare);
    //    }
    //}
    ///// <summary>
    ///// 转发信息
    ///// </summary>
    //public sealed class ForwardMessage : Message
    //{
    //    /// <summary>
    //    /// 转发信息节点
    //    /// </summary>
    //    public class Node
    //    {
    //        /// <summary>
    //        /// 引用缓存中其他对话上下文的消息作为节点
    //        /// </summary>
    //        public struct MessageReferenceNode
    //        {
    //            /// <summary>
    //            /// 引用的 messageId
    //            /// </summary>
    //            public long messageId { get; }
    //            /// <summary>
    //            /// 引用的上下文目标，群号、好友账号
    //            /// </summary>
    //            public long target { get; }
    //            /// <summary>
    //            /// 引用缓存中其他对话上下文的消息作为节点
    //            /// </summary>
    //            /// <param name="messageId">引用的 messageId</param>
    //            /// <param name="target">引用的上下文目标，群号、好友账号</param>
    //            public MessageReferenceNode(long messageId, long target)
    //            {
    //                this.messageId = messageId;
    //                this.target = target;
    //            }
    //        }

    //        /// <summary>
    //        /// 发送人QQ号
    //        /// </summary>
    //        public long senderId { get; }
    //        /// <summary>
    //        /// 时间
    //        /// </summary>
    //        public long time { get; }
    //        /// <summary>
    //        /// 显示名称
    //        /// </summary>
    //        public string senderName { get; }
    //        /// <summary>
    //        /// 转发的消息数组
    //        /// </summary>
    //        public Message[] messageChain { get; }
    //        /// <summary>
    //        /// 转发的消息数组
    //        /// </summary>
    //        public long messageId { get; }
    //        /// <summary>
    //        /// 引用缓存中其他对话上下文的消息作为节点
    //        /// </summary>
    //        public MessageReferenceNode messageRef { get; }
    //        /// <summary>
    //        /// 源ID
    //        /// </summary>
    //        public long sourceId { get; }


    //        /// <summary>
    //        /// 转发消息节点
    //        /// </summary>
    //        /// <param name="senderId">发送人QQ号</param>
    //        /// <param name="time">时间</param>
    //        /// <param name="senderName">显示名称</param>
    //        /// <param name="messageChain">转发的消息数组</param>
    //        /// <param name="sourceId">源ID</param>
    //        public Node(long senderId, long time, string senderName, Message[] messageChain, long sourceId)
    //        {
    //            this.senderId = senderId;
    //            this.time = time;
    //            this.senderName = senderName;
    //            this.messageChain = messageChain;
    //            this.sourceId = sourceId;
    //        }
    //        /// <summary>
    //        /// 转发消息节点(只使用消息messageId)
    //        /// </summary>
    //        /// <param name="senderId">发送人QQ号</param>
    //        /// <param name="time">时间</param>
    //        /// <param name="senderName">显示名称</param>
    //        /// <param name="messageId">消息messageId</param>
    //        /// <param name="sourceId">源ID</param>
    //        public Node(long senderId, long time, string senderName, long messageId, long sourceId)
    //        {
    //            this.senderId = senderId;
    //            this.time = time;
    //            this.senderName = senderName;
    //            this.messageId = messageId;
    //            this.sourceId = sourceId;
    //        }
    //        /// <summary>
    //        /// 转发消息节点(使用其他对话上下文的消息作为节点)
    //        /// </summary>
    //        /// <param name="senderId">发送人QQ号</param>
    //        /// <param name="time">时间</param>
    //        /// <param name="senderName">显示名称</param>
    //        /// <param name="messageRef">引用缓存中其他对话上下文的消息作为节点</param>
    //        /// <param name="sourceId">源ID</param>
    //        public Node(long senderId, long time, string senderName, MessageReferenceNode messageRef, long sourceId)
    //        {
    //            this.senderId = senderId;
    //            this.time = time;
    //            this.senderName = senderName;
    //            this.messageRef = messageRef;
    //            this.sourceId = sourceId;
    //        }
    //    }
    //    /// <summary>
    //    /// 消息节点列表
    //    /// </summary>
    //    public Node[] nodeList { get; }
    //    /// <summary>
    //    /// 转发消息
    //    /// </summary>
    //    /// <param name="nodeList">转发的节点</param>
    //    public ForwardMessage(Node[] nodeList)
    //    {
    //        this.nodeList = nodeList;
    //        this.type = nameof(ForwardMessage);
    //    }
    //}
    ///// <summary>
    ///// 文件信息
    ///// </summary>
    //public sealed class File : Message
    //{
    //    /// <summary>
    //    /// 文件识别ID
    //    /// </summary>
    //    public string id { get; }
    //    /// <summary>
    //    /// 文件名
    //    /// </summary>
    //    public string name { get; }
    //    /// <summary>
    //    /// 文件大小
    //    /// </summary>
    //    public long size { get; }
    //    /// <summary>
    //    /// 文件信息
    //    /// </summary>
    //    /// <param name="id">文件识别ID</param>
    //    /// <param name="name">文件名</param>
    //    /// <param name="size">文件大小</param>
    //    public File(string id, string name, long size)
    //    {
    //        this.id = id;
    //        this.name = name;
    //        this.size = size;
    //        this.type = nameof(File);
    //    }
    //}
    ///// <summary>
    ///// Mirai代码
    ///// </summary>
    //public sealed class MiraiCode : Message
    //{
    //    /// <summary>
    //    /// Mirai代码
    //    /// </summary>
    //    public string code { get; }
    //    /// <summary>
    //    /// Mirai码
    //    /// <para>关于Mirai码的对照: https://github.com/mamoe/mirai/blob/dev/docs/Messages.md</para>
    //    /// </summary>
    //    /// <param name="code">代码</param>
    //    public MiraiCode(string code)
    //    {
    //        this.code = code;
    //    }
    //}
    ///// <summary>
    ///// 商城表情
    ///// </summary>
    //public sealed class MarketFace : Message
    //{
    //    /// <summary>
    //    /// 商城表情唯一标识
    //    /// </summary>
    //    public int id { get; }
    //    /// <summary>
    //    /// 表情显示名称
    //    /// </summary>
    //    public string name { get; }
    //    /// <summary>
    //    /// 构造一个商城表情
    //    /// <para>目前商城表情仅支持接收和转发,不支持构造发送</para>
    //    /// </summary>
    //    /// <param name="id">商城表情唯一标识</param>
    //    /// <param name="name">表情显示名称</param>
    //    public MarketFace(int id, string name)
    //    {
    //        this.id = id;
    //        this.name = name;
    //    }
    //}

    #endregion

}
