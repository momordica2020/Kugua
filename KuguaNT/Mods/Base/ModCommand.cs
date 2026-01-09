using Kugua.Integrations.NTBot;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;



namespace Kugua.Mods.Base
{
    /// <summary>
    /// 所触发操作的指令。可以用regex匹配文本，也可以用image匹配图片等，反正参数都放入string[]里传
    /// </summary>
    public class ModCommand
    {
        public Regex regex;
        public HandleCommandEvent handle;
        public bool useImage = false;   // 匹配图片
        public bool useAudio = false;   // 匹配音频
        public bool useAny = false;     // 任何内容皆可触发
        public bool needAsk = true;     // （在群里）需要前缀

        public ModCommand(Regex _regex, HandleCommandEvent _handle, bool _useImage=false,bool _useAudio=false, bool _useAny=false, bool _needAsk=true)
        {
            regex = _regex;
            handle = _handle;
            useAny = _useAny;
            useAudio = _useAudio;
            useImage = _useImage;
            needAsk = _needAsk;
        }

        public bool Handle(MessageContext context)
        {
            if (needAsk && !context.IsAskme) return false;
            List<string> param = new List<string>();
            if (regex != null)
            {
                string message = context.recvMessages.ToTextString().Trim();
                var match = regex.Match(message);
                if (match.Success)
                {
                    param.AddRange(match.Groups.Values.Select(e => e.Value.Trim()).ToList());
                }
                else
                {
                    return false;
                }
            }
            else
            {
                // 用空regex表示非文本的匹配，或者任意匹配。
                // 采用其他标志来决定是否进入处理
                if(useImage && context.IsImage)
                {
                    // ok
                }
                else if(useAudio && context.IsAudio)
                {
                    // ok
                }
                else
                {
                    return false;
                }
            }

            try
            {
                string res = handle(context, param.ToArray());
                if (res == null) { return true; } // 用null 表明中断后续其他模块对该信息的响应
                else if (!string.IsNullOrWhiteSpace(res))
                {
                    //var sendMessage = new List<Message>();
                    //if (context.isGroup) sendMessage.Add(new At(context.userId));
                    //sendMessage.Add(new Text(res));

                    context.SendBackText(res, true);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"ModCommand {handle.Method.Name} ERROR:{ex.Message}");
            } 
            
            return false;
        }

        public override string ToString()
        {
            MethodInfo method = handle.Method;
            string xmlFilePath = $"{Directory.GetCurrentDirectory()}/KuguaNT.xml";  // 确保路径正确
            string desc = method.Name;
            if (File.Exists(xmlFilePath))
            {
                XElement xmlDoc = XElement.Load(xmlFilePath);
                string xmlName = $".{method.Name}(Kugua.MessageContext,System.String[])";
                //Logger.Log(xmlName);
                try
                {
                    var summaryElement = xmlDoc.Descendants("member")
                    .FirstOrDefault(m => m.Attribute("name").Value.Contains(xmlName));
                    if (summaryElement != null)
                    {
                        // 获取函数的 <summary> 信息
                        desc = summaryElement.Elements("summary").FirstOrDefault()?.Value.Trim();
                        var desclines = desc.Split('\n', StringSplitOptions.TrimEntries);
                        if (desclines.Length > 1)
                        {
                            var cmd = desclines.Last();
                            desc = desc.Substring(0, desc.Length - cmd.Length).Trim();
                            return $"{desc}  格式：{cmd}";
                        }
                    }
                }catch(Exception ex)
                {
                    Logger.Log(ex);
                }

            }
            //return $"{desc} 匹配格式: {regex?.ToString()}";//,{(useImage ? "发图" : "发文字")}";
            return "";
        }

    }


}
