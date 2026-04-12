using Kugua.Core;
using Kugua.Integrations.AI;
using Kugua.Integrations.NTBot;
using Kugua.Integrations.TiebaClaw;
using Kugua.Mods.Base;
using Microsoft.Extensions.FileSystemGlobbing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;


namespace Kugua.Mods
{


    /// <summary>
    /// 贴吧
    /// </summary>
    public class ModTiebaClaw : Mod
    {
        System.Timers.Timer TaskTimer;
        TiebaClawClient claw;

        private static readonly Lazy<ModTiebaClaw> instance = new Lazy<ModTiebaClaw>(() => new ModTiebaClaw());
        public static ModTiebaClaw Instance => instance.Value;
        private ModTiebaClaw()
        {


        }
        public override bool Init(string[] args)
        {
            ModCommands.Add(new ModCommand(new Regex(@"^[贴|帖]吧看[贴|帖][:：\s]?(.*)"), GetPages));
            ModCommands.Add(new ModCommand(new Regex(@"^[贴|帖]吧看回[帖|贴|复]"), GetReplies));
            ModCommands.Add(new ModCommand(new Regex(@"^[贴|帖]吧点赞[:：\s]*(\d+)(?:[，, \s]+(\d+))?"), PostAgree));
            ModCommands.Add(new ModCommand(new Regex(@"^[贴|帖]吧发[贴|帖](.+?)[:：](.+)", RegexOptions.Singleline), PostPage));
            ModCommands.Add(new ModCommand(new Regex(@"^[贴|帖]吧回[帖|贴|复][:：\s]*(\d+?)(?:[，, \s]+(\d+))?[:：](.+)", RegexOptions.Singleline), PostReply));
            //ModCommands.Add(new ModCommand(new Regex(@"^设置(\d*)\-(\S+)"), handleRemoveTag));

            claw = new TiebaClawClient(Config.Instance.App.AI.TiebaToken);

            TaskTimer = new(1000 * 60); //ms
            TaskTimer.AutoReset = true;
            TaskTimer.Start();
            TaskTimer.Elapsed += TaskTimer_Elapsed;
            return true;
        }

        private void TaskTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {

        }

        string GetPages(MessageContext context, string[] param)
        {
            var res = "";
            if (!string.IsNullOrWhiteSpace(param[1]))
            {
                if (long.TryParse(param[1], out long pageId) && pageId!=0)
                {
                    var root = claw.GetPageDetail(pageId);
                    Logger.Log($"claw detail:\r\n{root.ToString()}");
                    var page = root["page"];
                    res += $"当前页: {page?["current_page"]} / 总页数: {page?["total_page"]}";

                    // --- 2. 解析首楼 (1楼) ---
                    var first = root["first_floor"];
                    string title = first?["title"]?.ToString();
                    // 提取首楼文本内容 (content 是数组)
                    string firstContent = first?["content"]?[0]?["text"]?.ToString();
                    int likes = first?["agree"]?["agree_num"]?.Value<int>() ?? 0;

                    res+= ($"【主题】: {title}\r\n");
                    res+= ($"【楼主】: {firstContent} {(likes > 0 ? $"(👍{likes})" : "")}\r\n");
                    //res+= (new string('-', 10));
                    res += "\r\n\r\n";

                    // --- 3. 遍历楼层列表 (post_list) ---
                    JArray postList = root["post_list"] as JArray;
                    if (postList != null)
                    {
                        foreach (var post in postList)
                        {
                            long postId = post["id"]?.Value<long>() ?? 0;
                            int postLikes = post["agree"]?["agree_num"]?.Value<int>() ?? 0;
                            string postText = post["content"]?[0]?["text"]?.ToString();
                            res += ($"{postId} : {postText}{(postLikes > 0 ? $"(👍{postLikes})" : "")}\r\n");

                            // --- 4. 遍历楼中楼 (sub_post_list) ---
                            // 注意：这里是双层嵌套 key: sub_post_list -> sub_post_list
                            var subPosts = post["sub_post_list"]?["sub_post_list"] as JArray;
                            if (subPosts != null)
                            {
                                foreach (var sub in subPosts)
                                {
                                    string subText = sub["content"]?[0]?["text"]?.ToString();
                                    res+= ($"    |_  : {subText}\r\n");
                                }
                            }
                        }
                    }
                    return res;
                }
                
            }

            var Pages = claw.GetPages();
            Logger.Log($"claw:{Pages.ToString()}");
            if (Pages != null && ((JObject)Pages).ContainsKey("thread_list"))
            {
                if (Pages["thread_list"] is JArray threadList)
                {
                    foreach (JToken thread in threadList)
                    {
                        // 访问内部属性
                        long id = thread["id"]?.Value<long>() ?? 0;
                        string title = thread["title"]?.ToString();
                        int replyNum = thread["reply_num"]?.Value<int>() ?? 0;
                        int viewNum = thread["view_num"]?.Value<int>() ?? 0;
                        int agreeNum = thread["agree_num"]?.Value<int>() ?? 0;
                        string author = thread["author"]?["name"]?.ToString();
                        string contentAbstract = thread["abstract"]?[0]?["text"]?.ToString();
                        string cleanAbstract = string.IsNullOrWhiteSpace(contentAbstract)
                        ? "（无概要）"
                        : contentAbstract.Replace("\n", " ").Replace("\r", "");

                        if (cleanAbstract.Length > 40)
                            cleanAbstract = cleanAbstract.Substring(0, 40) + "...";
                        // 5. 格式化打印
                        res += $"{id,-12} {agreeNum}/{replyNum}/{viewNum,-8}({author,-15}){title}\r\n: {cleanAbstract}\r\n";
                    }
                }
            }
            return res;
        }

        string GetReplies(MessageContext context, string[] param)
        {
            var res = "";
            var Pages = claw.GetReplies();
            if (Pages != null)
            {
                return Pages.ToString();
                if (Pages is JArray threadList)
                {
                    foreach (JToken thread in threadList)
                    {
                        // 访问内部属性
                        long id = thread["id"]?.Value<long>() ?? 0;
                        string title = thread["title"]?.ToString();
                        int replyNum = thread["reply_num"]?.Value<int>() ?? 0;
                        int viewNum = thread["view_num"]?.Value<int>() ?? 0;
                        int agreeNum = thread["agree_num"]?.Value<int>() ?? 0;
                        string author = thread["author"]?["name"]?.ToString();
                        string contentAbstract = thread["abstract"]?[0]?["text"]?.ToString();
                        string cleanAbstract = string.IsNullOrWhiteSpace(contentAbstract) ? "（无）" : contentAbstract;
                        // 5. 格式化打印
                        res += "{id,-12} {agreeNum}/{replyNum}/{viewNum,-8}({author,-15}){title}\r\n概要预览: {cleanAbstract}\r\n";
                    }
                }
            }
            return res;
        }

        string PostPage(MessageContext context, string[] param)
        {
            var title = param[1];
            var content = param[2];
            var res = claw.AddThread(title, content);
            if (res != null)
                return res.ToString();

            return "搞不来";
        }

        string PostReply(MessageContext context, string[] param)
        {
            string content = "";
            long postId = 0;
            long pageId = 0;
            pageId = long.Parse(param[1]);
            if(!string.IsNullOrWhiteSpace(param[2])) postId = long.Parse(param[2]);
            content = param[3];

            var res = claw.AddPost(pageId, content, postId);
            if(res!=null)
            return res.ToString();

            return "搞不来";
            //return "你得输入帖子ID号码";
        }

        string PostAgree(MessageContext context, string[] param)
        {
            long postId = 0;
            long pageId = 0;
            pageId = long.Parse(param[1]);
            if (!string.IsNullOrWhiteSpace(param[2])) postId = long.Parse(param[2]);

            var res = claw.AddAgree(pageId, true, postId);
            if (res != null)
                return res.ToString();

            return "搞不来";
            //return "你得输入帖子ID号码";
        }
    }


}
