using Kugua.Integrations.NTBot;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Kugua
{


    /// <summary>
    /// 管理员指令
    /// </summary>
    public class ModAdmin : Mod
    {

        public override bool Init(string[] args)
        {
            ModCommands.Add(new ModCommand(new Regex(@"^帮助$"), getWelcomeString));
            ModCommands.Add(new ModCommand(new Regex(@"^(拉黑|屏蔽)(\d+)"), handleBanned));
            ModCommands.Add(new ModCommand(new Regex(@"^解封(\d+)"), handleUnBanned));
            ModCommands.Add(new ModCommand(new Regex(@"^设置(\d*)\+(\S+)"), handleAddTag));
            ModCommands.Add(new ModCommand(new Regex(@"^设置(\d*)\-(\S+)"), handleRemoveTag));
            ModCommands.Add(new ModCommand(new Regex(@"^设置清空(\s*)"), handleClearTag));
            ModCommands.Add(new ModCommand(new Regex(@"^状态$"),handleShowState));
            ModCommands.Add(new ModCommand(new Regex(@"^(存档|保存)$"), handleSave));

            ModCommands.Add(new ModCommand(new Regex(@"^撤回(.*)"), handleRecall));
            ModCommands.Add(new ModCommand(new Regex(@"^帮我撤回(.*)"), handleRecall2));
            ModCommands.Add(new ModCommand(new Regex(@"^群内搜索(.+)"), handleSearch));
            ModCommands.Add(new ModCommand(new Regex(@"^(拍拍|贴贴)"), sendPoke));
            //ModCommands.Add(new ModCommand(new Regex(@"^刷新列表"), refreshList));

            ModCommands.Add(new ModCommand(new Regex(@"^连接本地$"), handleLinkLocal));

            return true;
        }



        //public override Task<bool> HandleMessagesDIY(MessageContext context)
        //{
        //    if(context.recvMessages != null)
        //    {
        //        foreach (var msg in context.recvMessages)
        //        {
        //            if (msg is Poke)
        //            {
        //                context.client?.SendPoke(context.groupId, context.userId);
        //            }
        //        }
        //    }

        //    return base.HandleMessagesDIY(context);
        //}


        private string handleLinkLocal(MessageContext context, string[] param)
        {
            BotHost.Instance.LinkLocal();
            return null;
        }





        /// <summary>
        /// bot的欢迎文本
        /// 帮助
        /// </summary>
        /// <returns></returns>
        private string getWelcomeString(MessageContext context, string[] param)
        {
            return 
                $"想在群里使用，就at我或者打字开头加“{Config.Instance.BotName}”，再加内容。私聊乐我的话直接发内容。\r\n" 
                + BotHost.Instance.ModsDesc();
            //return "" +
            //    $"想在群里使用，就at我或者打字开头加“{Config.Instance.BotName}”，再加内容。私聊乐我的话直接发内容。\r\n" +
            //    "以下是群常用功能。\r\n" +
            //    "~状态查看：“状态”\r\n" +
            //    "~模式更换：“模式列表”、“xx模式on”\r\n" +
            //    "~掷骰子：“rd 成功率”“r3d10 攻击力”\r\n" +
            //    //"~多语翻译：“汉译法译俄 xxxx”\r\n" +
            //    //"~天气预报：“北京明天天气”\r\n" +
            //    //"B站live搜索：“绘画区谁在播”“虚拟区有多少B限”“xxx在播吗”\r\n" +
            //    "~赛马：“赛马介绍”“签到”“个人信息”\r\n" +
            //    "~生成攻受文：“A攻B受”\r\n" +
            //    "~生成笑话：“讽刺 本国=A国，好人=甲，坏人=乙，事件=xx”\r\n" +
            //    "~生成随机汉字：“随机5*4”\r\n" +
            //    "~周易占卜：“占卜 xxx”\r\n";
        }


        private string handleSave(MessageContext context, string[] param)
        {
            if (Config.Instance.UserHasAdminAuthority(context.userId))
            {
                Config.Instance.Save();
                GPT.Instance.AISaveMemory();
                ModRoulette.Instance.Save();
                ModRaceHorse.Instance.Save();
                HistoryManager.Instance.SaveAllToLocal(true);

                return  $"配置文件以存档 {DateTime.Now.ToString("F")}";
            }

            return "";
        }

        /// <summary>
        /// 返回bot的工作状态信息
        /// 状态
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string handleShowState(MessageContext context, string[] param)
        {
            StringBuilder rmsg = new StringBuilder();
            var user = Config.Instance.UserInfo(context.userId);
            var group = Config.Instance.GroupInfo(context.groupId);
            if (Config.Instance.GroupHasAdminAuthority(context.groupId) || Config.Instance.UserHasAdminAuthority(context.userId)) //临时：只有测试群可查详细信息
            {
                DateTime startTime = Config.Instance.StartTime;
                rmsg.AppendLine($"内核版本 - 苦音酱 v{Config.Instance.App.Version}（{StaticUtil.GetBuildDate().ToString("F")}）");
                rmsg.AppendLine($"启动时间：{startTime.ToString("yyyy-MM-dd HH:mm:ss")}(已运行{(DateTime.Now - startTime).TotalDays.ToString("0.00")}天)");
                rmsg.AppendLine($"CPU({Config.Instance.systemInfo.CpuLoad.ToString(".0")}%) 内存({(100.0 - ((double)Config.Instance.systemInfo.MemoryAvailable * 100 / Config.Instance.systemInfo.PhysicalMemory)).ToString(".0")}%)");
                rmsg.AppendLine($"{SystemInfo.GetNvidiaGpuAndMemoryUsage()}");
                rmsg.AppendLine($"数据库有{Config.Instance.groups.Count}个群和{Config.Instance.users.Count}个账户");
                rmsg.AppendLine($"在群里被乐{Config.Instance.UseTimeGroup}次");
                rmsg.AppendLine($"在私聊被乐{Config.Instance.UseTimePrivate}次");
                rmsg.AppendLine($"报错{Config.Instance.ErrorTime}次");
                rmsg.AppendLine($"机主是{Config.Instance.App.Avatar.adminQQ}");

                rmsg.AppendLine("自检信息：" + BotHost.Instance.SelfCheckInfo());
            }
            if (context.isGroup)
            {
                rmsg.AppendLine($"在本群的标签是：{(group.Tags.Count == 0 ? "(暂无标签)" : string.Join(", ", group.Tags))}");
            }
            else
            {
                //私聊查状态
                rmsg.AppendLine($"在私聊的标签是：{(user.Tags.Count == 0 ? "(暂无标签)" : string.Join(", ", user.Tags))}");
            }
            return rmsg.ToString();
        }


        private string handleBanned(MessageContext context, string[] param)
        {
            if (!Config.Instance.UserHasAdminAuthority(context.userId)) return "";
            string message = param[1];
            var targetUserId = 0;
            if (string.IsNullOrWhiteSpace(message) || !int.TryParse(message, out targetUserId))
            {
                return $"请在指令后接用户QQ号码";
            }
            else
            {
                var targetUser = Config.Instance.UserInfo(targetUserId.ToString());
                targetUser.Tags.Add("黑名单"); // 临时性拉黑，没有加type设置
                return $"已全局屏蔽{targetUser.Name}({targetUserId})";
            }
        }



        private string handleUnBanned(MessageContext context, string[] param)
        {
            if (!Config.Instance.UserHasAdminAuthority(context.userId)) return "";
            string message = param[1];
            var targetUserId = 0;
            if (string.IsNullOrWhiteSpace(message) || !int.TryParse(message, out targetUserId))
            {
                return $"请在指令后接用户QQ号码";
            }
            else
            {
                var targetUser = Config.Instance.UserInfo(targetUserId.ToString());
                targetUser.Tags.Remove("黑名单"); // 临时性，没有加type设置
                return $"已解除屏蔽{targetUser.Name}({targetUserId})";
            }
        }

        private string handleAddTag(MessageContext context, string[] param)
        {
            string groupid = param[1];
            string message = param[2];
            if (string.IsNullOrWhiteSpace(groupid)) groupid = context.groupId;
            if (!Config.Instance.UserHasAdminAuthority(context.userId)) return "";

            var user = Config.Instance.UserInfo(context.userId);
            var group = Config.Instance.GroupInfo(groupid);
            if (string.IsNullOrWhiteSpace(message))
            {
                return $"请在指令后接tag名称";
            }
            if (context.isGroup)
            {
                group.Tags.Add(message);
                return $"群{groupid}已添加tag：{message}";
            }
            else
            {
                user.Tags.Add(message);
                return $"私聊已添加tag：{message}";
            }
        }


        private string handleRemoveTag(MessageContext context, string[] param)
        {
            string groupid = param[1];
            string message = param[2];
            if (string.IsNullOrWhiteSpace(groupid)) groupid = context.groupId;
            if (!Config.Instance.UserHasAdminAuthority(context.userId)) return "";

            var user = Config.Instance.UserInfo(context.userId);
            var group = Config.Instance.GroupInfo(groupid);
            if (string.IsNullOrWhiteSpace(message))
            {
                return $"请在指令后接tag名称";
            }
            if (context.isGroup)
            {
                group.Tags.Remove(message);
                return $"群{groupid}已移除tag：{message}";
            }
            else
            {
                user.Tags.Remove(message);
                return $"私聊已移除tag：{message}";
            }
        }


        private string handleClearTag(MessageContext context, string[] param)
        {
            string message = param[1];
            if (!Config.Instance.UserHasAdminAuthority(context.userId)) return "";

            var user = Config.Instance.UserInfo(context.userId);
            var group = Config.Instance.GroupInfo(context.groupId);
            if (string.IsNullOrWhiteSpace(message))
            {
                if (context.isGroup)
                {
                    group.Tags.Clear();
                    return $"本群已清空所有tag";

                }
                else
                {
                    user.Tags.Clear();
                    return $"私聊已清空所有tag";

                }
            }
            else
            {
                if (context.isGroup)
                {
                    group.Tags.RemoveWhere(tag => tag.Contains(message));
                    return  $"本群已删除所有带{message}tag";
                }
                else
                {
                    user.Tags.RemoveWhere(tag => tag.Contains(message));
                    return  $"私聊已删除所有带{message}tag";
                }
            }
        }


        /// <summary>
        /// 让bot拍拍你
        /// 拍拍/贴贴
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string sendPoke(MessageContext context, string[] param)
        {
            context.client?.SendPoke(context.groupId, context.userId);
            //context.SendBack([new Poke { type="1", id="-1"}]);
            return null;
        }




        /// <summary>
        /// 让bot撤回最后n条消息
        /// 撤回/撤回N
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string handleRecall(MessageContext context, string[] param)
        {
            int num = 1;
            if (param.Length >= 2)
            {
                int.TryParse(param[1], out num);
            }
            if (num <= 0) num = 1;
            if (num >= 10) num = 10;
            var historys = HistoryManager.Instance.SearchByUser(Config.Instance.BotQQ, context.groupId);
            for (int i = 0; i < Math.Min(historys.Length, num); i++)
            {
                int hindex = historys.Length - i - 1;
                if (hindex < 0) break;
                string msgid = historys[hindex].MessageId;
                if (string.IsNullOrEmpty(msgid))
                {
                    num++;
                }
                else
                {
                    //Logger.Log($"?{msgid}");
                    context.client?.Send(new delete_msg(msgid));
                    historys[hindex].MessageId = string.Empty;
                }


                //if (clientMirai != null)
                //{
                //    new GroupMessage(context.groupId, [
                //        new Quote(historys[i].messageId,context.groupId,context.userId,context.groupId,
                //            [new Plain(historys[i].message)])]
                //        ).Send(clientMirai);
                //    new Recall(historys[i].messageId).Send(clientMirai);
                //}
            }
            return null;

        }


        /// <summary>
        /// 帮你撤回消息。但bot必须有管理员权限。
        /// 帮我撤回/帮我撤回N
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string handleRecall2(MessageContext context, string[] param)
        {
            int num = 1;
            if (param.Length >= 2)
            {
                int.TryParse(param[1], out num);
            }
            if (num <= 0) num = 1;
            if (num >= 10) num = 10;
            var historys = HistoryManager.Instance.SearchByUser(context.userId, context.groupId);
            for (int i = 0; i < Math.Min(historys.Length, num); i++)
            {
                int hindex = historys.Length - i - 1;
                if (hindex < 0) break;
                string msgid = historys[hindex].MessageId;
                if (string.IsNullOrEmpty(msgid))
                {
                    num++;
                }
                else
                {
                    //Logger.Log($"?{msgid}");
                    context.client?.Send(new delete_msg(msgid));
                    historys[hindex].MessageId = string.Empty;
                }
            }
            return null;

        }


        /// <summary>
        /// 在群内搜索关键词
        /// 群内搜索 早安
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string handleSearch(MessageContext context, string[] param)
        {
            if (!context.isGroup) return "";
            string keyword = param[1].Trim();
            int showMax = 10;
            var historys = HistoryManager.Instance.Search(context.groupId, keyword);
            StringBuilder sb = new StringBuilder();
            if (historys.Length <= 0)
            {
                return "没搜到。";
            }
            sb.AppendLine($"本群搜到{historys.Length}条结果：");
            for (int i = 0; i < Math.Min(historys.Length, showMax); i++)
            {
                sb.AppendLine($"{historys[i].Content}——{Config.Instance.UserInfo(historys[i].UserId).Name},{historys[i].RecvDate.ToString("yyyy.MM.dd HH:mm")}");
                if (sb.Length > 900) break;
            }
            return sb.ToString();
        }

        ///// <summary>
        ///// 刷新好友列表并更新配置文件
        ///// </summary>
        //public void RefreshFriendList()
        //{
        //    try
        //    {
        //        //if (clientMirai != null)
        //        //{
        //        //    var fp = new FriendList().Send(clientMirai);
        //        //    Config.Instance.qqfriends.Clear();
        //        //    if (fp == null)
        //        //    {
        //        //        Logger.Log($"不会吧不会吧不会没有好友吧");

        //        //    }
        //        //    else
        //        //    {
        //        //        foreach (var f in fp)
        //        //        {
        //        //            var friend = Config.Instance.UserInfo(f.id);
        //        //            friend.Name = f.nickname;
        //        //            //friend.Mark = f.remark;
        //        //            friend.Tags.Add("好友");
        //        //            //friend.Type = PlayerType.Normal;
        //        //            Config.Instance.qqfriends.Add(f.id, f);
        //        //        }
        //        //    }




        //        //    var gp = new GroupList().Send(clientMirai);
        //        //    Config.Instance.qqgroups.Clear();
        //        //    Config.Instance.qqgroupMembers.Clear();
        //        //    if (gp == null)
        //        //    {
        //        //        Logger.Log($"不会吧不会吧不会没有群吧");

        //        //    }
        //        //    else
        //        //    {
        //        //        foreach (var g in gp)
        //        //        {
        //        //            var group = Config.Instance.GroupInfo(g.id);
        //        //            group.Name = g.name;
        //        //            var groupMembers = g.GetMemberList(clientMirai);
        //        //            if (groupMembers == null)
        //        //            {
        //        //                Logger.Log($"不会吧不会吧不会{g.id}是鬼群吧");
        //        //                continue;
        //        //            }
        //        //            Config.Instance.qqgroups.Add(g.id, g);
        //        //            Config.Instance.qqgroupMembers.Add(g.id, groupMembers);
        //        //            foreach (var gf in groupMembers)
        //        //            {
        //        //                var member = Config.Instance.UserInfo(gf.id);
        //        //                member.Mark = gf.memberName;    //群昵称？
        //        //            }
        //        //        }
        //        //    }

        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Log(ex);
        //    }

        //}

        ///// <summary>
        ///// bot自刷新好友和群列表（暂不可用）
        ///// </summary>
        ///// <param name="context"></param>
        ///// <param name="param"></param>
        ///// <returns></returns>
        //private string refreshList(MessageContext context, string[] param)
        //{
        //    //if (context.isGroup && Config.Instance.UserHasAdminAuthority(context.userId))
        //    //{
        //    //    Logger.Log($"更新好友列表和群列表...");
        //    //    RefreshFriendList();
        //    //    Logger.Log($"更新完毕，找到{Config.Instance.qqfriends.Count}个好友，{Config.Instance.qqgroups.Count}个群...");

        //    //    return $"更新完毕，找到{Config.Instance.qqfriends.Count}个好友，{Config.Instance.qqgroups.Count}个群...";
        //    //}
        //    return "";
        //}





    }


}
