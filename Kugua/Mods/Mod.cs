using MeowMiraiLib;
using MeowMiraiLib.Msg.Sender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MeowMiraiLib.Msg.Type;
using static MeowMiraiLib.Msg.Sender.GroupMessageSender;
using MeowMiraiLib.Msg;


namespace Kugua
{



    public delegate string HandleCommandEvent(MessageContext context, string[] param);
    public abstract class Mod
    {
        protected Dictionary<Regex, HandleCommandEvent> ModCommands = new Dictionary<Regex, HandleCommandEvent>();
        public Client clientMirai;
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

        public async Task<bool> HandleFriendMessage(MessageContext context)
        {
            var c = await HandleMessages(context);
            return c;
        }

        public async Task<bool> HandleGroupMessage(MessageContext context)
        {
            var c = await HandleMessages(context);
            return c;
        }

        private async Task<bool> HandleMessages(MessageContext context)
        {
            try
            {
                if (context.recvMessages == null) return false;
                string message = context.recvMessages.MGetPlainString().Trim();
                //if (string.IsNullOrWhiteSpace(message)) return false;

                if (context.isAskme)
                {
                    //Logger.Instance.Log("!" + message);
                    foreach (var cmd in ModCommands)
                    {
                        var m = cmd.Key.Match(message);
                        if (m.Success)
                        {
                            List<string> ps = new List<string>();
                            for (int i = 0; i < m.Groups.Count; i++)
                            {
                                ps.Add(m.Groups[i].Value.Trim());
                            }
                            string res = cmd.Value(context, ps.ToArray());

                            if (res == null)
                            {
                                // 用null 表明中断后续其他模块对该信息的响应
                                return true;
                            }

                            if (!string.IsNullOrWhiteSpace(res))
                            {
                                var sendMessage = new List<Message>();
                                if (context.isGroup) sendMessage.Add(new At(context.userId, null));
                                sendMessage.Add(new Plain(res));

                                context.SendBack(sendMessage.ToArray());
                                return true;
                            }
                        }
                    }
                }
                

                // 后续处理本模块的继承里，其他的用户自定义的消息处理机制
                var r = await HandleMessagesDIY(context);
                return r;

            }
            catch(Exception ex)
            {
                Logger.Instance.Log(ex);
                
            }
            return false;
        }

        public virtual async Task<bool> HandleMessagesDIY(MessageContext context)
        {
            return false;
        }






    }


}
