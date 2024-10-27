﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using MMDK.Util;

namespace MMDK.Mods
{
    /// <summary>
    /// 管理员指令
    /// </summary>
    public class ModAdmin : Mod
    {






        public bool Init(string[] args)
        {

            return true;
        }

        public void Exit()
        {
            
        }

        public bool HandleText(long userId, long groupId, string message, List<string> results)
        {
            if(string.IsNullOrWhiteSpace(message))return false;
            message = message.Trim();

            CommandType cmd = CommandType.None;
            string tag = "";
            bool isGroup = groupId > 0;
            var user = Config.Instance.GetPlayerInfo(userId);
            
            var group = Config.Instance.GetGroupInfo(groupId);
            if (TryReadCommand(ref message, out cmd))
            {
                switch (cmd)
                {
                    case CommandType.Help:
                        results.Add(getWelcomeString());
                        return true;
                    case CommandType.Ban:
                        var targetUserId = 0;
                        if(int.TryParse(message, out targetUserId))
                        {
                            var targetUser = Config.Instance.GetPlayerInfo(targetUserId);
                            targetUser.SetTag("黑名单"); // 临时性拉黑，没有加type设置
                            results.Add($"已全局屏蔽{targetUser.Name}({targetUserId})");
                            return true;
                        }
                        else
                        {
                            results.Add($"请在指令后接用户QQ号码");
                            return true;
                        }
                    case CommandType.UnBan:
                        var targetUserId2 = 0;
                        if (int.TryParse(message, out targetUserId2))
                        {
                            var targetUser = Config.Instance.GetPlayerInfo(targetUserId2);
                            targetUser.DeleteTag("黑名单"); 
                            results.Add($"已解除屏蔽{targetUser.Name}({targetUserId2})");
                            return true;
                        }
                        else
                        {
                            results.Add($"请在指令后接用户QQ号码");
                            return true;
                        }
                    case CommandType.TagAdd:
                        if (string.IsNullOrWhiteSpace(message))
                        {
                            results.Add($"请在指令后接tag名称");
                            return true;
                        }
                        if (isGroup)
                        {
                            group.SetTag(message);
                            results.Add($"本群已添加tag：{message}");
                            return true;
                        }
                        else
                        {
                            user.SetTag(tag);
                            results.Add($"私聊已添加tag：{message}");
                            return true;
                        }
                    case CommandType.TagRemove:
                        if (string.IsNullOrWhiteSpace(message))
                        {
                            results.Add($"请在指令后接tag名称");
                            return true;
                        }
                        if (isGroup)
                        {
                            group.DeleteTag(tag);
                            results.Add($"本群已删掉tag：{message}");
                            return true;
                        }
                        else
                        {
                            user.DeleteTag(tag);
                            results.Add($"私聊已删掉tag：{message}");
                            return true;
                        }
                    case CommandType.CheckState:
                        string rmsg = "";
                        if (isGroup)
                        {
                            if (group.Is("测试")) //临时：只有测试群可查详细信息
                            {
                                DateTime startTime = Config.Instance.App.Log.StartTime;
                                rmsg += $"本次启动时间：{startTime.ToString("yyyy-MM-dd HH:mm:ss")}(已运行{(DateTime.Now - startTime).TotalDays.ToString("0.00")}天)\r\n";
                                rmsg += $"重启了{Config.Instance.App.Log.beginTimes}次\r\n";
                                rmsg += $"加了{Config.Instance.App.Log.numGroup}个群\r\n";
                                rmsg += $"在群里被乐{Config.Instance.App.Log.playTimeGroup}次\r\n";
                                rmsg += $"在私聊被乐{Config.Instance.App.Log.playTimePrivate}次\r\n";
                            }
                            else
                            {
                                rmsg += $"在本群的配置是：{(string.IsNullOrWhiteSpace(group.Tag) ? "*平平无奇*" : group.Tag)}\r\n";
                                //rmsg += $"在本群闲聊配置为：{Config.Instance.GetGroupInfo(groupId).Tag}\r\n";
                            }
                        }
                        else
                        {
                            //私聊查状态
                            rmsg += $"目前闲聊配置为：{user.Tag}\r\n";
                        }
                        results.Add(rmsg);
                        return true;
                    case CommandType.None:
                    default:
                        return false;
                }
            }
            return false;
        }





        bool isAskme(string msg)
        {
            if (msg.StartsWith(Config.Instance.App.Avatar.askName))
            {
                return true;
            }
            return false;
        }

        enum CommandType
        {
            None,
            Help,
            Ban,
            UnBan,
            TagAdd,
            TagRemove,
            CheckState,
        }
        // 存储可匹配的命令
        private static readonly Dictionary<string, CommandType> CommandDict = new Dictionary<string, CommandType>
        {
            { "功能", CommandType.Help},
            { "帮助", CommandType.Help},
            { "菜单", CommandType.Help},
            { "拉黑", CommandType.Ban},
            { "屏蔽", CommandType.Ban},
            { "ban", CommandType.Ban},
            { "封", CommandType.Ban},
            { "解封", CommandType.UnBan},
            { "解开", CommandType.UnBan},
            { "设置+", CommandType.TagAdd},
            { "设置", CommandType.TagAdd},
            //{ "添加", CommandType.TagAdd},
            { "设置-", CommandType.TagRemove},
            { "状态", CommandType.CheckState},
        };
        static bool TryReadCommand(ref string input, out CommandType commandType)
        {
            if (!string.IsNullOrWhiteSpace(input))
            {
                foreach (var command in CommandDict)
                {
                    if (input.StartsWith(command.Key))
                    {
                        commandType = command.Value; // 找到的命令
                        input = input.Substring(command.Key.Length).Trim(); // 截掉命令部分并去掉前导空格
                        return true;
                    }
                }
            }


            commandType = CommandType.None; // 如果没有匹配的命令
            return false;
        }


        /// <summary>
        /// bot的欢迎文本
        /// </summary>
        /// <returns></returns>
        string getWelcomeString()
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
                //"~生成谴责：“A谴责B的C”\r\n" +
                //"~生成笑话：“讽刺 本国=A国，好人=甲，坏人=乙，事件=xx”\r\n" +
                "~生成随机汉字：“随机5*4”\r\n" +
                "~周易占卜：“占卜 xxx”\r\n";
        }



        bool UserHasAdminAuthority(long userId)
        {
            if (userId <= 0) return false;
            if (userId == Config.Instance.App.Avatar.adminQQ) return true;
            var user = Config.Instance.GetPlayerInfo(userId);
            if (user.Is("管理员")) return true;
            if (user.Type == PlayerType.Admin) return true;
            return false;
        }

        bool GroupHasAdminAuthority(long groupId)
        {
            if (groupId <= 0) return false;
            if (groupId == Config.Instance.App.Avatar.adminGroup) return true;
            var group = Config.Instance.GetGroupInfo(groupId);
            if (group.Is("测试")) return true;
            if (group.Type == PlaygroupType.Test) return true;
            return false;
        }


    }


}