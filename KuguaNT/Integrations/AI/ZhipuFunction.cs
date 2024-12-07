using System.Text.Json.Nodes;
using ZhipuApi.Modules;

namespace Kugua
{
    public partial class GPT
    {
        string speak_sentense(string chatid, string param)
        {
            try
            {
                string speakWords = (string)(JsonObject.Parse(param)["sentense"]);
                if (ChatMessageContext[chatid] != null)
                {
                    AITalk(ChatMessageContext[chatid], speakWords);
                    return "语音发送成功";
                }

            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            return "语音发送失败";
        }
    }
}
