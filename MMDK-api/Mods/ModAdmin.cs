using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MeowMiraiLib.Msg.Type;
using MMDK.Util;
using MMDK.Mods;

namespace MMDK.Mods
{


    /// <summary>
    /// 管理员指令
    /// </summary>
    public class ModAdmin : Mod
    {

        Dictionary<Regex, HandleCommandEvent> cmds = new Dictionary<Regex, HandleCommandEvent>();
        


        public bool Init(string[] args)
        {
            cmds.Add(new Regex(@"^帮助$"), getWelcomeString);
            cmds.Add(new Regex(@"^(拉黑|屏蔽)(\d+)"), handleBanned);
            cmds.Add(new Regex(@"^解封(\d+)"), handleUnBanned);
            cmds.Add(new Regex(@"^设置\+(\S+)"), handleAddTag);
            cmds.Add(new Regex(@"^设置\-(\S+)"), handleRemoveTag);
            cmds.Add(new Regex(@"^设置清空(\s*)"), handleClearTag);
            cmds.Add(new Regex(@"^状态$"), handleShowState);
            cmds.Add(new Regex(@"^(存档|保存)$"), handleSave);
            return true;
        }



        public void Exit()
        {
            
        }

        public bool HandleText(long userId, long groupId, string message, List<string> results)
        {
            if(string.IsNullOrWhiteSpace(message))return false;
            message = message.Trim();

            foreach (var cmd in cmds)
            {
                var m = cmd.Key.Match(message);
                if (m.Success)
                {
                    string res = cmd.Value(m, userId, groupId);
                    if (!string.IsNullOrWhiteSpace(res))
                    {
                        results.Add(res);
                        return true;
                    }
                }
            }

            return false;
        }



        /// <summary>
        /// bot的欢迎文本
        /// </summary>
        /// <returns></returns>
        private string getWelcomeString(Match? matchResults = null, long userId = 0, long groupId = 0)
        {
            return "" +
                $"想在群里使用，就at我或者打字开头加“{Config.Instance.App.Avatar.askName}”，再加内容。私聊乐我的话直接发内容。\r\n" +
                "以下是群常用功能。私聊可以闲聊。\r\n" +
                "~状态查看：“状态”\r\n" +
                "~模式更换：“模式列表”、“xx模式on”\r\n" +
                "~掷骰子：“rd 成功率”“r3d10 攻击力”\r\n" +
                //"~多语翻译：“汉译法译俄 xxxx”\r\n" +
                //"~天气预报：“北京明天天气”\r\n" +
                //"B站live搜索：“绘画区谁在播”“虚拟区有多少B限”“xxx在播吗”\r\n" +
                "~赛马：“赛马介绍”“签到”“个人信息”\r\n" +
                "~生成攻受文：“A攻B受”\r\n" +
                //"~生成笑话：“讽刺 本国=A国，好人=甲，坏人=乙，事件=xx”\r\n" +
                "~生成随机汉字：“随机5*4”\r\n" +
                "~周易占卜：“占卜 xxx”\r\n";
        }

        private string handleSave(Match? matchResults = null, long userId = 0, long groupId = 0)
        {
            if (Config.Instance.GroupHasAdminAuthority(groupId)
                || Config.Instance.UserHasAdminAuthority(userId))
            {
                Config.Instance.Save();
                GPT.Instance.AISaveMemory();
                ModRaceHorse.Instance.save();
                return  $"配置文件以存档 {DateTime.Now.ToString("F")}";
            }

            return "";
        }

        private string handleShowState(Match matchResults, long userId, long groupId)
        {
            string rmsg = "";
            var user = Config.Instance.UserInfo(userId);
            var group = Config.Instance.GroupInfo(groupId);
            if (Config.Instance.GroupHasAdminAuthority(groupId) || Config.Instance.UserHasAdminAuthority(userId) || userId == Config.Instance.App.Avatar.adminQQ) //临时：只有测试群可查详细信息
            {
                DateTime startTime = Config.Instance.App.Log.StartTime;
                rmsg += $"内核版本 - 苦音未来v{Config.Instance.App.Version}（{Util.StaticUtil.GetBuildDate().ToString("F")}）\n";
                rmsg += $"启动时间：{startTime.ToString("yyyy-MM-dd HH:mm:ss")}(已运行{(DateTime.Now - startTime).TotalDays.ToString("0.00")}天)\n";
                rmsg += $"CPU({Config.Instance.systemInfo.CpuLoad.ToString(".0")}%) 内存({(100.0 - ((double)Config.Instance.systemInfo.MemoryAvailable * 100 / Config.Instance.systemInfo.PhysicalMemory)).ToString(".0")}%)\n";
                rmsg += $"{SystemInfo.GetNvidiaGpuAndMemoryUsage()}\n";
                rmsg += $"一共重启{Config.Instance.App.Log.beginTimes}次\n";
                rmsg += $"数据库有{Config.Instance.playgroups.Count}个群和{Config.Instance.players.Count}个账户\n";
                rmsg += $"在群里被乐{Config.Instance.App.Log.playTimeGroup}次\n";
                rmsg += $"在私聊被乐{Config.Instance.App.Log.playTimePrivate}次\n";
                rmsg += $"机主是{Config.Instance.App.Avatar.adminQQ}\n";
            }
            if (groupId > 0)
            {
                rmsg += $"在本群的标签是：{(group.Tags.Count == 0 ? "(暂无标签)" : string.Join(", ", group.Tags))}\n";
            }
            else
            {
                //私聊查状态
                rmsg += $"在私聊的标签是：{(user.Tags.Count == 0 ? "(暂无标签)" : string.Join(", ", user.Tags))}\r\n";
            }
            return rmsg;
        }


        private string handleBanned(Match match, long userId, long groupId)
        {
            if (!Config.Instance.UserHasAdminAuthority(userId)) return "";
            string message = match.Groups[1].Value;
            var targetUserId = 0;
            if (string.IsNullOrWhiteSpace(message) || !int.TryParse(message, out targetUserId))
            {
                return $"请在指令后接用户QQ号码";
            }
            else
            {
                var targetUser = Config.Instance.UserInfo(targetUserId);
                targetUser.Tags.Add("黑名单"); // 临时性拉黑，没有加type设置
                return $"已全局屏蔽{targetUser.Name}({targetUserId})";
            }
        }



        private string handleUnBanned(Match match, long userId, long groupId)
        {
            if (!Config.Instance.UserHasAdminAuthority(userId)) return "";
            string message = match.Groups[1].Value;
            var targetUserId = 0;
            if (string.IsNullOrWhiteSpace(message) || !int.TryParse(message, out targetUserId))
            {
                return $"请在指令后接用户QQ号码";
            }
            else
            {
                var targetUser = Config.Instance.UserInfo(targetUserId);
                targetUser.Tags.Remove("黑名单"); // 临时性，没有加type设置
                return $"已解除屏蔽{targetUser.Name}({targetUserId})";
            }
        }

        private string handleAddTag(Match match, long userId, long groupId)
        {
            string message = match.Groups[1].Value;
            if (!Config.Instance.UserHasAdminAuthority(userId)) return "";

            var user = Config.Instance.UserInfo(userId);
            var group = Config.Instance.GroupInfo(groupId);
            if (string.IsNullOrWhiteSpace(message))
            {
                return $"请在指令后接tag名称";
            }
            if (groupId > 0)
            {
                group.Tags.Add(message);
                return $"本群已添加tag：{message}";
            }
            else
            {
                user.Tags.Add(message);
                return $"私聊已添加tag：{message}";
            }
        }

        private string handleRemoveTag(Match match, long userId, long groupId)
        {
            string message = match.Groups[1].Value;
            if (!Config.Instance.UserHasAdminAuthority(userId)) return "";

            var user = Config.Instance.UserInfo(userId);
            var group = Config.Instance.GroupInfo(groupId);
            if (string.IsNullOrWhiteSpace(message))
            {
                return $"请在指令后接tag名称";
            }
            if (groupId > 0)
            {
                group.Tags.Add(message);
                return $"本群已移除tag：{message}";
            }
            else
            {
                user.Tags.Add(message);
                return $"私聊已移除tag：{message}";
            }
        }
        private string handleClearTag(Match match, long userId, long groupId)
        {
            string message = match.Groups[1].Value;
            if (!Config.Instance.UserHasAdminAuthority(userId)) return "";

            var user = Config.Instance.UserInfo(userId);
            var group = Config.Instance.GroupInfo(groupId);
            if (string.IsNullOrWhiteSpace(message))
            {
                if (groupId > 0)
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
                if (groupId > 0)
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
       


        



    }


}
