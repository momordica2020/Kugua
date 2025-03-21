﻿
using Kugua.Integrations.NTBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;



namespace Kugua
{
    /// <summary>
    /// 所触发操作的指令。可以用regex匹配文本，也可以用image匹配图片等，反正参数都放入string[]里传
    /// </summary>
    public class ModCommand
    {
        public Regex regex;
        public HandleCommandEvent handle;
        public bool useImage = false;
        public bool needAsk = true;

        public ModCommand(Regex _regex, HandleCommandEvent _handle, bool _needAsk = true, bool _useImage =false)
        {
            regex = _regex;
            handle = _handle;
            needAsk = _needAsk;
            useImage = _useImage;
        }

        public bool Handle(MessageContext context)
        {
            if (needAsk && !context.IsAskme) return false;
            List<string> param = new List<string>();
            if (regex != null)
            {
                string message = context.recvMessages.ToTextString().Trim();
                var m = regex.Match(message);
                if (m.Success)
                {
                    param.AddRange(m.Groups.Values.Select(e => e.Value.Trim()).ToList());
                }
               
            }
            if(useImage)
            {
                int imgNum = 0;
                foreach(var  item in context.recvMessages)
                {
                    if(item is ImageBasic img)
                    {
                        imgNum++;
                        //param.Add(img.url);
                    }
                }
                if(imgNum>0) param.Add(imgNum.ToString());
            }
            if (param.Count > 0)
            {
                string res = handle(context, param.ToArray());

                if (res == null) { return true; } // 用null 表明中断后续其他模块对该信息的响应
                else if (!string.IsNullOrWhiteSpace(res))
                {
                    //var sendMessage = new List<Message>();
                    //if (context.isGroup) sendMessage.Add(new At(context.userId));
                    //sendMessage.Add(new Text(res));

                    context.SendBackText(res, context.IsGroup);
                    return true;
                }                        
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


    public delegate string HandleCommandEvent(MessageContext context, string[] param);



    ///// <summary>
    ///// 模组的可持久化资源，用于自动存取
    ///// </summary>
    //public class ModResource
    //{
        
    //}

    //public enum ModResourceType
    //{
    //    Dictionary
    //}

    /// <summary>
    /// 模组的基础类型，根据正则匹配等方式调用模组内方法
    /// </summary>
    public abstract class Mod
    {
        /// <summary>
        /// 等待新参数的指令。默认每个 群-人 只能有一条待参指令
        /// </summary>
        protected Dictionary<string, (MessageContext ctx, ModCommand cmd)> WaitingCmds = new Dictionary<string, (MessageContext ctx, ModCommand cmd)>();
        object WaitingCmdsLock = new object();

        protected List<ModCommand> ModCommands = new List<ModCommand> ();
        public NTBot clientQQ;
        public LocalClient clientLocal;

        /// <summary>
        /// Mod初始化，只调用一次
        /// </summary>
        /// <param name="args">可选的传入参数</param>
        /// <returns></returns>
        public abstract bool Init(string[] args);


        /// <summary>
        /// Mod退出清理，在bot关闭时调用一次
        /// </summary>
        public virtual void Exit()
        {

        }


        /// <summary>
        /// 模块重新载入，主要是更新配置文件用
        /// </summary>
        public virtual void Reload()
        {

        }

        /// <summary>
        /// 模块保存配置文件到本地用
        /// </summary>
        public virtual void Save()
        {

        }

        //public async Task<bool> HandleFriendMessage(MessageContext context)
        //{
        //    var c = await HandleMessages(context);
        //    return c;
        //}

        //public async Task<bool> HandleGroupMessage(MessageContext context)
        //{
        //    var c = await HandleMessages(context);
        //    return c;
        //}


        /// <summary>
        /// 添加一条等待下次触发的临时指令
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="cmd"></param>
        public void WaitNext(MessageContext ctx, ModCommand cmd)
        {
            lock (WaitingCmdsLock)
            {
                WaitingCmds[$"{ctx.groupId}_{ctx.userId}"] = (ctx, cmd);
            }
        }

        public async Task<bool> HandleMessages(MessageContext context)
        {
            try
            {
                if (context.recvMessages == null) return false;


                //string message = context.recvMessages.ToTextString().Trim();

                //if (string.IsNullOrWhiteSpace(message)) return false;

                // 如果处理模块中存在正在等待回应的指令，进行匹配
                
                string uid = $"{context.groupId}_{context.userId}";
                bool isWaiting = false;
                (MessageContext ctx, ModCommand cmd) val;
                lock (WaitingCmdsLock){  isWaiting = WaitingCmds.TryGetValue(uid, out val);  }
                if (isWaiting)
                {
                    if (context.IsAskme)
                    {
                        // new ask!
                        lock (WaitingCmdsLock) {  WaitingCmds.Remove(uid);  }
                    }
                    else
                    {
                        //context.recvMessages.AddRange(val.ctx.recvMessages);
                        val.ctx.recvMessages.AddRange(context.recvMessages);
                        if (val.cmd.Handle(val.ctx))
                        {
                           // 只删自己，意思是如果handle时增加了新的waitingCmd，则保留
                            lock (WaitingCmdsLock) { if (WaitingCmds.ContainsValue(val)) WaitingCmds.Remove(uid);  }
                            return true;
                        }
                    }
                }
                

                // 逐个指令处理响应
                foreach (var cmd in ModCommands)
                {
                    if(cmd.Handle(context))return true;
                }

                
                

                // 后续处理本模块的继承里，其他的用户自定义的消息处理机制
                var r = await HandleMessagesDIY(context);
                return r;

            }
            catch(Exception ex)
            {
                Logger.Log(ex);
                
            }
            return false;
        }

        public virtual async Task<bool> HandleMessagesDIY(MessageContext context)
        {
            return false;
        }

        /// <summary>
        /// 获取全部指令描述信息
        /// </summary>
        /// <returns></returns>
        public string GetCommandDescriptions()
        {
            StringBuilder res=new StringBuilder();
            int i = 1;
            foreach (var cmd in ModCommands)
            {
                var cmdDesc = cmd.ToString();
                if (!string.IsNullOrWhiteSpace(cmdDesc))
                {
                    res.AppendLine($"{i++} : {cmdDesc}");
                }
            }

            return  res.ToString();
        }



    }


}
