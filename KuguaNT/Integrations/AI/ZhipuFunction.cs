using System.Text.Json.Nodes;
using ZhipuApi.Modules;

namespace Kugua.Integrations.AI
{
    public class ZhipuFunction
    {
        public string speak_sentense(string chatid, string param)
        {
            try
            {
                string speakWords = (string)(JsonObject.Parse(param)["sentense"]);
                LLM.Instance.Speech(LLM.Instance.GetChatContext(chatid).LastMessageContext, speakWords);
                return "语音发送成功";
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            return "语音发送失败";
        }
    }
}
