using KuguaSdk.Onebot11;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KuguaSdk.MessageStructs
{
    /// <summary>
    /// 消息基类
    /// </summary>
    [JsonConverter(typeof(MessageConverter))]
    public class Message
    {
        
    }




    public class MessageConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(Message);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            //Console.WriteLine($"[MessageConverter] - ReadJson - {jo}");
            string msgType = jo["type"]?.Value<string>();

            Message target = msgType switch
            {
                "text" => new Text(),
                "image" => new Image(),
                "video" => new Video(),
                "face" => new Face(),
                "at" => new At(),
                "rps" => new Rps(),
                "dice" => new Dice(),
                "poke" => new Poke(),
                "anonymous" => new AnonymousMesssage(),
                "share" => new Share(),
                "contact" => new Contact(),
                "location" => new Location(),
                "music" => new Music(),
                "reply" => new Reply(),
                "record" => new Record(),
                "xml" => new XmlData(),
                "json" => new JsonData(),
                "forward" => new Forward(),

                _ => new Text()
            };





            serializer.Populate(jo["data"].CreateReader(), target);
            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            string type = value switch
            {
                MessageStructs.Text => "text",
                MessageStructs.Image => "image",
                Face => "face",
                At => "at",
                Video => "video",
                Rps => "rps",
                Dice => "dice",
                // Shake => "shake",
                Poke => "poke",
                AnonymousMesssage => "anonymous",
                Share => "share",
                Contact => "contact",
                Location => "location",
                Music => "music",
                Reply => "reply",
                Record => "record",
                XmlData => "xml",
                JsonData => "json",
                Forward => "node",
                ForwardNodeNew => "node",
                MFace => "mface",
                _ => "text",
            };
            var localSerializer = new JsonSerializer();
            //localSerializer.ContractResolver = serializer.ContractResolver;
            //localSerializer.DateFormatString = serializer.DateFormatString;
            JObject jo = new JObject();
            jo["type"] = type;
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

    public static class MessageUtil
    {
        public static string ToTextString(this List<Message> array)
        {
            StringBuilder sb = new();
            foreach (var i in array)
            {
                if (i is Text text)
                {
                    sb.Append(text.text);
                }
                else if (i is Dice d)
                {
                    if (d.result != 0) sb.Append(d.result.ToString());
                }
            }
            return sb.ToString();
        }

     
    }

}
