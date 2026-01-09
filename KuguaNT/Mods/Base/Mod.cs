using Kugua.Integrations.NTBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Kugua.Mods.Base
{


    public delegate string HandleCommandEvent(MessageContext context, string[] param);


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


        /// <summary>
        /// 匹配模块中正在等待回应的指令
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool HandleWaitedMessage(MessageContext context)
        {
            string uid = $"{context.groupId}_{context.userId}";
            bool isWaiting = false;
            (MessageContext ctx, ModCommand cmd) last;
            lock (WaitingCmdsLock) { isWaiting = WaitingCmds.TryGetValue(uid, out last); }
            if (isWaiting)
            {
                if (context.IsAskme)
                {
                    // 重新at了bot，说明该用户已开启新的对话
                    lock (WaitingCmdsLock) { WaitingCmds.Remove(uid); }
                    return false;
                }
                else
                {
                    //context.recvMessages.AddRange(val.ctx.recvMessages);
                    last.ctx.recvMessages.AddRange(context.recvMessages);    // 合并消息内容
                    if (last.cmd.Handle(last.ctx))
                    {
                        // 只删自己，意思是如果handle内部重新设置了下一条要等待的指令，这里就不必remove了
                        lock (WaitingCmdsLock) { if (WaitingCmds[uid] == last) WaitingCmds.Remove(uid); }
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task<bool> HandleMessages(MessageContext context)
        {
            try
            {
                if (context.recvMessages == null) return false;


                //string message = context.recvMessages.ToTextString().Trim();

                //if (string.IsNullOrWhiteSpace(message)) return false;

                if(HandleWaitedMessage(context))return true;
                

                // 逐个指令处理响应
                foreach (var cmd in ModCommands)
                {
                    if (cmd.Handle(context)) return true;
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
