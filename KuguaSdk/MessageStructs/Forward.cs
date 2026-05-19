using KuguaSdk.Onebot11;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KuguaSdk.MessageStructs
{





    ///// <summary>
    ///// 接收到的转发消息详情
    ///// </summary>

    //public class ForwardContent
    //{
    //    public string status;
    //    public int retcode;

    //    public string message;
    //    public string wording;
    //    public string echo;

    //    [JsonIgnore]
    //    // 该名称与协议中不同，此处为省略嵌套后的处理结果。在协议json中为data->messages
    //    public List<forward_message_node> datas;
    //    public List<Message> messages;
    //    [JsonIgnore]
    //    public string group_id { get; set; }
    //    [JsonIgnore]
    //    public string user_id { get; set; }
    //    [JsonIgnore]
    //    public message_sender sender { get; set; }
    //}











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
        public List<Message> content;
    }



    public class ForwardSenderSingle
    {
        public string group_id;
        public string message_id;
    }

    /// <summary>
    /// 合并转发已有节点，直接发messageid即可
    /// </summary>
    [JsonConverter(typeof(ForwardConverter))]
    public class Forward : Message
    {
        public string id { get; set; }
        
        public List<forward_message_node> content;

    }

    internal class ForwardConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(Forward);


        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            
            JObject jo = JObject.Load(reader);
            if (jo["data"] == null) return null;
            var forwardData = JsonConvert.DeserializeObject<Forward>(jo["data"].ToString());
            //forwardData.content = new List<forward_message_node>();
            //Logger.Log($"看到了{sender.user_id}的转发喵。id={d.id}", LogType.Debug);
            //if (sender.user_id == Config.Instance.BotQQ)
            //{
            //    //Logger.Log($"是我自己喵。");
            //}
            //foreach (var nodej in jo["data"]["content"].ToArray())
            //{
            //    var node = JsonConvert.DeserializeObject<forward_message_node>(nodej.ToString());
            //    forwardData.content.Add(node);
            //}
            return forwardData;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            var localSerializer = new JsonSerializer();
            //localSerializer.ContractResolver = serializer.ContractResolver;
            //localSerializer.DateFormatString = serializer.DateFormatString;
            JObject jo = new JObject();
            jo["type"] = "node";
            // 1. 获取当前对象的合同配置（Contract）
            var contract = serializer.ContractResolver.ResolveContract(value.GetType()) as Newtonsoft.Json.Serialization.JsonObjectContract;

            JsonConverter currentConverter = null;
            if (contract != null)
            {
                currentConverter = contract.Converter; // 备份当前特性的 Converter
                contract.Converter = null;             // 临时置空，假装没有贴过这个特性标签
            }

            try
            {
                // 2. 此时调用 FromObject，Newtonsoft 就找不到特性了，会按照普通对象完美序列化
                jo["data"] = JObject.FromObject(value, serializer);
            }
            finally
            {
                // 3. 必须在 finally 里还原，否则会影响下一次其他同类对象的序列化
                if (contract != null)
                {
                    contract.Converter = currentConverter;
                }
            }
            jo.WriteTo(writer);
        }
    }




}
