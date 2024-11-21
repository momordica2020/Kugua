using Newtonsoft.Json;
using NvAPIWrapper.Native.GPU;
using System.Security.Cryptography;

namespace Kugua.Integrations.NTBot
{
    public class SenderAPI
    {
        public string action;
        [JsonProperty("params")]
        public SenderData Params;
        public string echo;


    }

    public interface SenderData
    {

    }

    public class SenderReplyAPI
    {

        public string status;
        public int retcode;
        [JsonIgnore]
        public SenderReply data;
        public string echo;
    }

    public interface SenderReply
    {
    }

    public class send_private_msg : SenderData
    {
        public string user_id;
        public object message;
        /// <summary>
        /// 消息内容是否作为纯文本发送（即不解析 CQ 码），只在 message 字段是字符串时有效
        /// </summary>
        public bool auto_escape;

        public send_private_msg(string user_id, List<MessageInfo> message)
        {
            this.user_id = user_id;
            this.message = message;
        }
    }

    public class send_msg_reply : SenderReply
    {
        public int message_id;
    }

    public class send_group_msg : SenderData
    {
        public string group_id;
        public object message;
        /// <summary>
        /// 消息内容是否作为纯文本发送（即不解析 CQ 码），只在 message 字段是字符串时有效
        /// </summary>
        public bool auto_escape;

        public send_group_msg(string group_id, List<MessageInfo> message)
        {
            this.group_id = group_id;
            this.message = message;
        }
    }

    public class send_msg : SenderData
    {
        public string message_type;
        public string user_id;
        public string group_id;
        public object message;
        /// <summary>
        /// 消息内容是否作为纯文本发送（即不解析 CQ 码），只在 message 字段是字符串时有效
        /// </summary>
        public bool auto_escape;

        public send_msg(string message_type, string user_id, string group_id, List<MessageInfo> message)
        {
            this.message_type = message_type;
            this.user_id = user_id;
            this.group_id = group_id;
            this.message = message;
        }
    }

    public class delete_msg : SenderData
    {
        public string message_id;
        
        public delete_msg(string msg_id)
        {
            message_id = msg_id;
        }
    }

    public class get_msg : SenderData
    {
        public string message_id;

        public get_msg(string msg_id)
        {
            message_id = msg_id;
        }
    }

    public class get_forward_msg : SenderData
    {
        public string id;

        public get_forward_msg(string msg_id)
        {
            id = msg_id;
        }
    }

    public class send_like : SenderData
    {
        public string user_id;
        public int times;

        public send_like(string user_id, int times)
        {
            this.user_id = user_id;
            this.times = times;
        }
    }

    public class set_group_kick : SenderData
    {
        public string group_id;
        public string user_id;
        public bool reject_add_request;

        public set_group_kick(string group_id, string user_id,bool reject_add_request)
        {
            this.group_id = group_id;
            this.user_id = user_id;
            this.reject_add_request = reject_add_request;
        }
    }

    public class set_group_ban : SenderData
    {
        public string group_id;
        public string user_id;
        public int duration;

        public set_group_ban(string group_id, string user_id, int duration)
        {
            this.group_id = group_id;
            this.user_id = user_id;
            this.duration = duration;
        }
    }

    public class set_group_anonymous_ban : SenderData
    {
        public string group_id;
        public string user_id;
        public object anonymous;
        public string flag;
        public int duration;
    }

    public class set_group_whole_ban : SenderData
    {
        public string group_id;
        public bool enable;

        public set_group_whole_ban(string group_id, bool enable)
        {
            this.group_id = group_id;
            this.enable = enable;
        }
    }

    public class set_group_admin : SenderData
    {
        public string group_id;
        public string user_id;
        public bool enable;

        public set_group_admin(string group_id, string user_id, bool enable)
        {
            this.group_id = group_id;
            this.user_id = user_id;
            this.enable = enable;
        }
    }

    public class set_group_anonymous : SenderData
    {
        public string group_id;
        public bool enable;
    }

    public class set_group_card : SenderData
    {
        public string group_id;
        public string user_id;
        public string card;
    }

    public class set_group_name : SenderData
    {
        public string group_id;
        public string group_name;
    }

    public class set_group_leave : SenderData
    {
        public string group_id;
        public bool is_dismiss;
    }

    public class set_group_special_title : SenderData
    {
        public string group_id;
        public string user_id;
        public string special_title;
        public int duration;
    }

    public class set_friend_add_request : SenderData
    {
        public string flag;
        public bool approve;
        public string remark;
    }

    public class set_group_add_request : SenderData
    {
        public string flag;
        /// <summary>
        /// add / invite
        /// </summary>
        public string type;

        public bool approve;
        public string reason;
    }

    public class get_login_info : SenderData
    {
        
    }
    public class get_login_info_reply : SenderReply
    {
        public string user_id;
        public string nickname;
    }


    public class get_stranger_info : SenderData
    {
        public string user_id;
        public bool no_cache;

        
    }
    public class get_stranger_info_reply : SenderReply
    {
        public string user_id;
        public string nickname;
        public string sex;
        public int age;
    }
    public class get_friend_list : SenderData
    {
        //[array]
        //public string user_id;
        //public string nickname;
        //public string remark;
    }

    public class get_group_info : SenderData
    {
        public string group_id;
        public bool no_cache;
    }
    public class get_group_info_reply : SenderReply
    {
        public string group_id;
        public string group_name;
        public int member_count;
        public int max_member_count;
    }

    public class get_group_list : SenderData
    {

        //return
        //public string group_id;
        //[get_group_info]
    }

    public class get_group_member_info : SenderData
    {

        public string group_id;
        public string user_id;
        public bool no_cache;
    }
    public class get_group_member_info_reply : SenderReply
    {
        public string group_id;
        public string user_id;
        public string nickname;
        public string card;
        public string sex;
        public int age;
        public string area;
        public int join_time;
        public int last_sent_time;
        public string level;
        public string role;
        public bool unfriendly;
        public string title;
        public int title_expire_time;
        public bool card_changeable;
    }


    public class get_group_member_list : SenderData
    {
        public string group_id;
        //return
        //public string group_id;
        //[get_group_member_info]
    }

    public class get_group_honor_info : SenderData
    {
        public string group_id;
        /// <summary>
        /// talkative,performer,legend,strong_newbie,emotion,all
        /// </summary>
        public string type;
       
        
        //return
        //public string group_id;
        //public array current_talkative;
        //   user_id,nickname,avatar,day_count;
        //public array performer_list;
        //   user_id,nickname,avatar,description;
        //public array legend_list;
        //public array strong_newbie_list;
        //public array emotion_list;
        //[get_group_member_info]
    }


    public class get_cookies: SenderData
    {
        public string domain;

    }
    public class get_cookies_reply : SenderReply
    {
        public string cookies;
    }

    public class get_csrf_token : SenderData
    {
    }
    public class get_csrf_token_reply : SenderReply
    {
        public int token;
    }

    public class get_credentials : SenderData
    {
        public string domain;
    }

    public class get_credentials_reply : SenderReply
    {
        public string cookies;
        public int csrf_token;
    }

    public class get_record : SenderData
    {
        public string file;
        /// <summary>
        /// mp3,amr,wma,m4a,spx,ogg,wav,flac
        /// </summary>
        public string out_format;

    }

    public class get_image :SenderData
    {
        public string file;

    }
    public class get_image_record_reply : SenderReply
    {
        public string file;
    }
    public class can_send_image : SenderData
    {

    }
    public class can_send_record : SenderData
    {

    }
    public class can_send_reply : SenderReply
    {
        public bool yes;
    }
    public class get_status : SenderData
    {
    }
    public class get_status_reply : SenderReply
    {
        public bool online;
        public bool good;
    }
    public class get_version_info : SenderData
    {
    }
    public class get_version_info_reply : SenderReply
    {
        public string app_name;
        public string app_version;
        public string protocol_version;
    }
    public class set_restart : SenderData
    {
        public int delay;
    }

    public class clean_cache : SenderData
    { 

    }
}


