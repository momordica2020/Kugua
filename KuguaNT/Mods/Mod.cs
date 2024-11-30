
using Kugua.Integrations.NTBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;



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
            if (needAsk && !context.isAskme) return false;
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
                foreach(var  item in context.recvMessages)
                {
                    if(item is Image img)
                    {
                        param.Add(img.url);
                    }
                }
            }
            if (param.Count > 0)
            {
                string res = handle(context, param.ToArray());

                if (res == null) { return true; } // 用null 表明中断后续其他模块对该信息的响应
                else if (string.IsNullOrWhiteSpace(res)) { return false; }
                else
                {
                    //var sendMessage = new List<Message>();
                    //if (context.isGroup) sendMessage.Add(new At(context.userId));
                    //sendMessage.Add(new Text(res));

                    context.SendBackPlain(res, context.isGroup);
                    return true;
                }
            }
            else
            {
                return false;
            }

           
        }

    }


    public delegate string HandleCommandEvent(MessageContext context, string[] param);
    public abstract class Mod
    {
        /// <summary>
        /// 等待新参数的指令。默认每个 群-人 只能有一条待参指令
        /// </summary>
        protected Dictionary<string, (MessageContext ctx, ModCommand cmd)> WaitingCmds = new Dictionary<string, (MessageContext ctx, ModCommand cmd)>();
        protected object WaitingCmdsLock = new object();

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

        public async Task<bool> HandleMessages(MessageContext context)
        {
            try
            {
                if (context.recvMessages == null) return false;


                //string message = context.recvMessages.ToTextString().Trim();

                //if (string.IsNullOrWhiteSpace(message)) return false;
                
                // 处理模块中正在等待回应的指令
                lock (WaitingCmdsLock)
                {
                    string uid = $"{context.groupId}_{context.userId}";
                    if (WaitingCmds.TryGetValue(uid, out var val))
                    {
                        context.recvMessages.AddRange(val.ctx.recvMessages);
                        if (val.cmd.Handle(context))
                        {
                            WaitingCmds.Remove(uid);
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






    }


}
