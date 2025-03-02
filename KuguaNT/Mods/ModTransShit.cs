using Kugua.Integrations.NTBot;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NvAPIWrapper.DRS.SettingValues;
using System.Data;
using System.Runtime.InteropServices.Marshalling;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Timers;
using System.Xml.Linq;
using ZhipuApi;

namespace Kugua.Mods
{
    public class ModTransShit : Mod
    {
        Dictionary<string, ShitTransGroupInfo> ShitSource = new Dictionary<string, ShitTransGroupInfo>();
        List<ShitTransGroupInfo> ShitTarget = new List<ShitTransGroupInfo>();
        //List<ShitTransGroupInfo> transInfo=new List<ShitTransGroupInfo>();
        //string targetGroup = "833246207";
        string sfile = "ModTransShit/groupinfo.json";
        

        string configfile = "ModTransShit/config.json";
        public ShitConfig config = new ShitConfig();

        string hashfile = "ModTransShit/hash.txt";
        HashSet<string> oldHash = new HashSet<string>();

        /// <summary>
        /// 之前一段时间内的平均用户评分。如果该值太接近0，就增加AI权重。
        /// </summary>
        public double last_loop_ave_score = 1;
        public DateTime lastPublishDate = DateTime.Now;



        Dictionary<string, Shit> shithash = new Dictionary<string, Shit>();
        List<Shit> shits = new List<Shit>();
        public object shitMutex = new object();

        System.Timers.Timer TaskTimer;


        private static readonly Lazy<ModTransShit> instance = new Lazy<ModTransShit>(() => new ModTransShit());
        public static ModTransShit Instance => instance.Value;
        private ModTransShit()
        {


        }


        public override bool Init(string[] args)
        {
            try
            {
                ModCommands.Add(new ModCommand(new Regex(@"^搬史(启动|停止)$", RegexOptions.Singleline), setState));


                ModCommands.Add(new ModCommand(new Regex(@"^转发(\d+)", RegexOptions.Singleline), setTransSource));
                ModCommands.Add(new ModCommand(new Regex(@"^转到(\d+)", RegexOptions.Singleline), setTransTarget));

                ModCommands.Add(new ModCommand(new Regex(@"^别转发(\d*)", RegexOptions.Singleline), setTransSourceStop));
                ModCommands.Add(new ModCommand(new Regex(@"^别转到(\d+)", RegexOptions.Singleline), setTransTargetStop));
                ModCommands.Add(new ModCommand(new Regex(@"^转发情况$", RegexOptions.Singleline), showList));

                // ModCommands.Add(new ModCommand(new Regex(@"^转发每次(\d)条$", RegexOptions.Singleline), showList));

                config = JsonConvert.DeserializeObject<ShitConfig>(LocalStorage.ReadResource(configfile));
                if (config == null || config.once_maxnum <= 0)
                {
                    // need init default
                    config = new ShitConfig
                    {
                        deal_score_span_min = 5,
                        del_old_span_min = 10,
                        once_maxnum = 5,
                        min_score = 1,
                        historyMaxScore = 0,
                        historyMaxScoreDate = DateTime.Now,
                        historyPublished = 0,
                        open=false
                    };
                }

                var transInfo = JsonConvert.DeserializeObject<List<ShitTransGroupInfo>>(LocalStorage.ReadResource(sfile));
                if (transInfo == null)
                {
                    //need init
                    transInfo = new List<ShitTransGroupInfo>();
                }
                foreach (var info in transInfo)
                {
                    if (!string.IsNullOrWhiteSpace(info.sourceId))
                    {
                        ShitSource.Add(info.sourceId, info);
                    }
                    else if (!string.IsNullOrWhiteSpace(info.targetId))
                    {
                        ShitTarget.Add(info);
                    }
                }

                var hashes = LocalStorage.ReadResourceLines(hashfile);
                foreach (var hline in hashes) if (!string.IsNullOrWhiteSpace(hline)) oldHash.Add(hline.Trim());



                TaskTimer = new(1000 * 30); // 30s
                TaskTimer.AutoReset = true;
                TaskTimer.Start();
                TaskTimer.Elapsed += TaskTimer_Elapsed;

            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
            return true;
        }

        private string setState(MessageContext context, string[] param)
        {
            try
            {
                if (!context.IsAdminUser) return "";
                var state = param[1];
                if(state == "启动")
                {
                    config.open = true;
                    Save();
                    return "搬史启动。";
                }else if (state == "停止")
                {
                    config.open = false;
                    Save();
                    return "搬史停止。";
                    
                }
            }catch(Exception ex)
            {
                Logger.Log(ex);
            }
            return null;
        }

        private string showList(MessageContext context, string[] param)
        {
            string sourceList = $"来源{ShitSource.Count}个群：";
            foreach (var s in ShitSource) sourceList = sourceList + $"\r\n{Config.Instance.GroupInfo(s.Value.sourceId)?.Name} {s.Value.sourceId}";
            string targetList = $"发到{ShitTarget.Count}个群：";
            foreach (var s in ShitTarget) targetList = targetList + $"\r\n{Config.Instance.GroupInfo(s.targetId)?.Name} {s.targetId}";
            string storage = $"搬过{config.historyPublished}条，库存{shits.Count}，哈希规模{shithash.Count}";
            string history = $"历史最高分{config.historyMaxScore}，出现于{config.historyMaxScoreDate}";
            var res = new List<Message>
            {
                new ForwardNodeNew
                {
                    user_id = Config.Instance.BotQQ,
                    nickname = Config.Instance.BotName,
                    content=new List<MessageInfo>()
                    {
                        new MessageInfo(new Text(sourceList))
                    }},
                new ForwardNodeNew
                {
                    user_id = Config.Instance.BotQQ,
                    nickname = Config.Instance.BotName,
                    content=new List<MessageInfo>()
                    {
                        new MessageInfo(new Text(targetList))
                    } },
                new ForwardNodeNew
                {  user_id = Config.Instance.BotQQ,
                    nickname = Config.Instance.BotName,
                    content=new List<MessageInfo>()
                    {
                        new MessageInfo(new Text(storage))
                    }  },
                new ForwardNodeNew
                {
                    user_id = Config.Instance.BotQQ,
                    nickname = Config.Instance.BotName,
                    content=new List<MessageInfo>()
                    {
                        new MessageInfo(new Text(history))
                    } },
            };

            //context.SendBack([res]);
            context.client.SendForwardMessageToGroup(context.groupId, res);
            return null;
        }


        /// <summary>
        /// 从shit中取出尚未发表的里面得分最高的一个。传入的是已选元素，以便排除
        /// </summary>
        /// <param name="exists"></param>
        /// <returns></returns>
        private Shit getMaxScoreUnPublicShit(List<Shit> exists)
        {
            long nowBestScore = 0;
            Shit nowBestShit = null;
            foreach (var s in shits)
            {
                // 总分=人工分+（AI分*AI权重）
                var realscore = s.score + Math.Max(0, s.AIscore - 3 + (last_loop_ave_score < 0.1 ? 1 : last_loop_ave_score > 1 ? -1 : 0));
                if (exists.Contains(s)
                    || s.published
                    || realscore < config.min_score
                //|| DateTime.Now - s.createTime < new TimeSpan(0, config.deal_score_span_min, 0) 
                ) continue;

                if (realscore > nowBestScore)
                {
                    nowBestScore = realscore;
                    nowBestShit = s;
                }

                if (realscore > config.historyMaxScore)
                {
                    // history best
                    config.historyMaxScore = realscore;
                    config.historyMaxScoreDate = DateTime.Now;
                }
            }
            return nowBestShit;
        }

        public long getScore(Shit shit)
        {
            long score = 0;

            List<EmojiTypeInfo> goods = new List<EmojiTypeInfo> {
                new EmojiTypeInfo{id="4", type="1"},//得意
                new EmojiTypeInfo{id="5", type="1"},//流泪
              //  new EmojiTypeInfo{id="8", type="1"},//睡
                new EmojiTypeInfo{id="9", type="1"},//大哭
                new EmojiTypeInfo{id="10", type="1"},//尴尬
                new EmojiTypeInfo{id="12", type="1"},//调皮
                new EmojiTypeInfo{id="14", type="1"},//微笑
              //  new EmojiTypeInfo{id="16", type="1"},//酷
                new EmojiTypeInfo{id="21", type="1"},//可爱
                new EmojiTypeInfo{id="23", type="1"},//傲慢
                new EmojiTypeInfo{id="24", type="1"},//饥饿
              //  new EmojiTypeInfo{id="25", type="1"},//困

              //  new EmojiTypeInfo{id="27", type="1"},//流汗
              //  new EmojiTypeInfo{id="28", type="1"},//憨笑
              //  new EmojiTypeInfo{id="29", type="1"},//悠闲
              //  new EmojiTypeInfo{id="30", type="1"},//奋斗
              ////  new EmojiTypeInfo{id="32", type="1"},//疑问
              //  new EmojiTypeInfo{id="33", type="1"},//嘘
              //  new EmojiTypeInfo{id="34", type="1"},//晕

                new EmojiTypeInfo{id="39", type="1"},//再见
              //  new EmojiTypeInfo{id="41", type="1"},//发抖
                new EmojiTypeInfo{id="42", type="1"},//爱情
                new EmojiTypeInfo{id="43", type="1"},//跳跳
                new EmojiTypeInfo{id="49", type="1"},//拥抱
              //  new EmojiTypeInfo{id="53", type="1"},//蛋糕
              //  new EmojiTypeInfo{id="60", type="1"},//咖啡
              //  new EmojiTypeInfo{id="63", type="1"},//玫瑰
                new EmojiTypeInfo{id="66", type="1"},//爱心
                new EmojiTypeInfo{id="74", type="1"},//太阳
               // new EmojiTypeInfo{id="75", type="1"},//月亮
                new EmojiTypeInfo{id="76", type="1"},//赞
              //  new EmojiTypeInfo{id="78", type="1"},//握手
                new EmojiTypeInfo{id="79", type="1"},//胜利
                new EmojiTypeInfo{id="85", type="1"},//飞吻
              //  new EmojiTypeInfo{id="89", type="1"},//西瓜
                new EmojiTypeInfo{id="96", type="1"},//冷汗
                new EmojiTypeInfo{id="97", type="1"},//擦汗
              //  new EmojiTypeInfo{id="98", type="1"},//抠鼻
                new EmojiTypeInfo{id="99", type="1"},//鼓掌
                new EmojiTypeInfo{id="100", type="1"},//糗大了
                new EmojiTypeInfo{id="101", type="1"},//坏笑
              //  new EmojiTypeInfo{id="102", type="1"},//左哼哼
               // new EmojiTypeInfo{id="103", type="1"},//右哼哼
              //  new EmojiTypeInfo{id="104", type="1"},//哈欠
                new EmojiTypeInfo{id="106", type="1"},//委屈
              //  new EmojiTypeInfo{id="109", type="1"},//左亲亲
                new EmojiTypeInfo{id="111", type="1"},//可怜
                new EmojiTypeInfo{id="116", type="1"},//示爱
              //  new EmojiTypeInfo{id="118", type="1"},//抱拳
              //  new EmojiTypeInfo{id="120", type="1"},//拳头
                new EmojiTypeInfo{id="122", type="1"},//爱你
              //  new EmojiTypeInfo{id="123", type="1"},//NO
                new EmojiTypeInfo{id="124", type="1"},//OK
               // new EmojiTypeInfo{id="125", type="1"},//转圈
                new EmojiTypeInfo{id="129", type="1"},//挥手
                new EmojiTypeInfo{id="144", type="1"},//喝彩
              //  new EmojiTypeInfo{id="147", type="1"},//棒棒糖
              //  new EmojiTypeInfo{id="171", type="1"},//茶
                new EmojiTypeInfo{id="173", type="1"},//泪奔
                new EmojiTypeInfo{id="174", type="1"},//无奈
                new EmojiTypeInfo{id="175", type="1"},//卖萌
              //  new EmojiTypeInfo{id="176", type="1"},//小纠结
                new EmojiTypeInfo{id="179", type="1"},//doge
                new EmojiTypeInfo{id="180", type="1"},//惊喜
                new EmojiTypeInfo{id="181", type="1"},//骚扰
                new EmojiTypeInfo{id="182", type="1"},//笑哭
                new EmojiTypeInfo{id="183", type="1"},//我最美
                new EmojiTypeInfo{id="201", type="1"},//点赞
                new EmojiTypeInfo{id="203", type="1"},//托脸
                new EmojiTypeInfo{id="212", type="1"},//托腮
                new EmojiTypeInfo{id="214", type="1"},//啵啵
                new EmojiTypeInfo{id="219", type="1"},//蹭一蹭
                new EmojiTypeInfo{id="222", type="1"},//抱抱
                new EmojiTypeInfo{id="227", type="1"},//拍手
              //  new EmojiTypeInfo{id="232", type="1"},//佛系
              //  new EmojiTypeInfo{id="240", type="1"},//喷脸
              //  new EmojiTypeInfo{id="243", type="1"},//甩头
              //  new EmojiTypeInfo{id="246", type="1"},//加油抱抱
                //new EmojiTypeInfo{id="262", type="1"},//脑阔疼
                //new EmojiTypeInfo{id="264", type="1"},//捂脸
                //new EmojiTypeInfo{id="265", type="1"},//辣眼睛
                //new EmojiTypeInfo{id="266", type="1"},//哦哟
                //new EmojiTypeInfo{id="267", type="1"},//头秃
                //new EmojiTypeInfo{id="268", type="1"},//问号脸
                //new EmojiTypeInfo{id="269", type="1"},//暗中观察
                //new EmojiTypeInfo{id="270", type="1"},//emm
                //new EmojiTypeInfo{id="271", type="1"},//吃瓜
                //new EmojiTypeInfo{id="272", type="1"},//呵呵哒
                //new EmojiTypeInfo{id="273", type="1"},//我酸了
                //new EmojiTypeInfo{id="277", type="1"},//汪汪
                //new EmojiTypeInfo{id="278", type="1"},//汗
                //new EmojiTypeInfo{id="281", type="1"},//无眼笑
                //new EmojiTypeInfo{id="282", type="1"},//敬礼
                //new EmojiTypeInfo{id="284", type="1"},//面无表情
                //new EmojiTypeInfo{id="285", type="1"},//摸鱼
                //new EmojiTypeInfo{id="287", type="1"},//哦
                //new EmojiTypeInfo{id="289", type="1"},//睁眼
              //  new EmojiTypeInfo{id="290", type="1"},//敲开心
                new EmojiTypeInfo{id="293", type="1"},//摸锦鲤
              //  new EmojiTypeInfo{id="294", type="1"},//期待
              //  new EmojiTypeInfo{id="297", type="1"},//拜谢
               // new EmojiTypeInfo{id="298", type="1"},//元宝
                new EmojiTypeInfo{id="299", type="1"},//牛啊
              //  new EmojiTypeInfo{id="305", type="1"},//右亲亲
                new EmojiTypeInfo{id="306", type="1"},//牛气冲天
              //  new EmojiTypeInfo{id="307", type="1"},//喵喵
                new EmojiTypeInfo{id="314", type="1"},//仔细分析
                new EmojiTypeInfo{id="315", type="1"},//加油
                new EmojiTypeInfo{id="318", type="1"},//崇拜
              //  new EmojiTypeInfo{id="319", type="1"},//比心
              //  new EmojiTypeInfo{id="320", type="1"},//庆祝
                
               // new EmojiTypeInfo{id="324", type="1"},//吃糖
                new EmojiTypeInfo{id="326", type="1"},//生气
                new EmojiTypeInfo{id="9728", type="2"},//☀	晴天
              //  new EmojiTypeInfo{id="9749", type="2"},//☕	咖啡
              //  new EmojiTypeInfo{id="9786", type="2"},//☺	可爱
              //  new EmojiTypeInfo{id="10024", type="2"},//✨	闪光
                
                new EmojiTypeInfo{id="10068", type="2"},//❔	问号
              //  new EmojiTypeInfo{id="127801", type="2"},//🌹	玫瑰
              //  new EmojiTypeInfo{id="127817", type="2"},//🍉	西瓜
              //  new EmojiTypeInfo{id="127822", type="2"},//🍎	苹果
               // new EmojiTypeInfo{id="127827", type="2"},//🍓	草莓
              //  new EmojiTypeInfo{id="127836", type="2"},//🍜	拉面
              //  new EmojiTypeInfo{id="127838", type="2"},//🍞	面包
              //  new EmojiTypeInfo{id="127847", type="2"},//🍧	刨冰
                new EmojiTypeInfo{id="127866", type="2"},//🍺	啤酒
                new EmojiTypeInfo{id="127867", type="2"},//🍻	干杯
                new EmojiTypeInfo{id="127881", type="2"},//🎉	庆祝
                new EmojiTypeInfo{id="128027", type="2"},//🐛	虫
                new EmojiTypeInfo{id="128046", type="2"},//🐮	牛
               // new EmojiTypeInfo{id="128051", type="2"},//🐳	鲸鱼
                new EmojiTypeInfo{id="128053", type="2"},//🐵	猴
               // new EmojiTypeInfo{id="128074", type="2"},//👊	拳头
                new EmojiTypeInfo{id="128076", type="2"},//👌	好的
                new EmojiTypeInfo{id="128077", type="2"},//👍	厉害
                new EmojiTypeInfo{id="128079", type="2"},//👏	鼓掌
              //  new EmojiTypeInfo{id="128089", type="2"},//👙	内衣
              //  new EmojiTypeInfo{id="128102", type="2"},//👦	男孩
              //  new EmojiTypeInfo{id="128104", type="2"},//👨	爸爸
                new EmojiTypeInfo{id="128147", type="2"},//💓	爱心
              //  new EmojiTypeInfo{id="128157", type="2"},//💝	礼物
              //  new EmojiTypeInfo{id="128164", type="2"},//💤	睡觉
              //  new EmojiTypeInfo{id="128166", type="2"},//💦	水
              //  new EmojiTypeInfo{id="128168", type="2"},//💨	吹气
              //  new EmojiTypeInfo{id="128170", type="2"},//💪	肌肉
               // new EmojiTypeInfo{id="128235", type="2"},//📫	邮箱
                new EmojiTypeInfo{id="128293", type="2"},//🔥	火
                new EmojiTypeInfo{id="128513", type="2"},//😁	呲牙
                new EmojiTypeInfo{id="128514", type="2"},//😂	激动
                new EmojiTypeInfo{id="128516", type="2"},//😄	高兴
                new EmojiTypeInfo{id="128522", type="2"},//😊	嘿嘿
               // new EmojiTypeInfo{id="128524", type="2"},//😌	羞涩
                new EmojiTypeInfo{id="128527", type="2"},//😏	哼哼
                new EmojiTypeInfo{id="128530", type="2"},//😒	不屑
                new EmojiTypeInfo{id="128531", type="2"},//😓	汗
                new EmojiTypeInfo{id="128532", type="2"},//😔	失落
                new EmojiTypeInfo{id="128536", type="2"},//😘	飞吻
                new EmojiTypeInfo{id="128538", type="2"},//😚	亲亲
               // new EmojiTypeInfo{id="128540", type="2"},//😜	淘气
              //  new EmojiTypeInfo{id="128541", type="2"},//😝	吐舌
                new EmojiTypeInfo{id="128557", type="2"},//😭	大哭
                
                new EmojiTypeInfo{id="128563", type="2"},//😳	瞪眼

            };

            //List<EmojiTypeInfo> bads = new List<EmojiTypeInfo>
            //{
            //    new EmojiTypeInfo{id="26", type="1"},//惊恐
            //    new EmojiTypeInfo{id="38", type="1"},//敲打
            //    new EmojiTypeInfo{id="322", type="1"},//拒绝
            //    new EmojiTypeInfo{id="10060", type="2"},//❌	错误
            //    new EmojiTypeInfo{id="128560", type="2"},//😰	紧张
            //};

            foreach (var emoji in goods) score += shit.contexts.First().client.getEmojiLikeNumber(shit.contexts.First().messageId, emoji.id, emoji.type);




            //foreach (var emoji in bads) score -= context.client.getEmojiLikeNumber(context.messageId, emoji.id, emoji.type);

            return score;
        }



        /// <summary>
        /// 定期巡查发送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaskTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            if (!config.open) return;
            // 检查到期内容是否被点过很多赞，有的话就发送最高的几个
            var span = DateTime.Now - lastPublishDate;
            Logger.Log($"*** shit轮检中，目前{shits.Count}个没发，hash长度{shithash.Count}，均分{last_loop_ave_score:F2}，距上次推送{span.TotalMinutes:F2}min");


            // update score

            for (int i = 0; i < shits.Count; i++)
            {
                var shit = shits[i];
                if (shit.published == false
                    && shit.score == 0
                    && DateTime.Now - shit.createTime > new TimeSpan(0, config.deal_score_span_min, 0)// 足够久远
                )
                {
                    // human score
                    shit.score = 0;
                    foreach (var context in shit.contexts)
                    {
                        shit.score += getScore(shit);
                    }

                    // ai score if nessary
                    if (shit.score < config.min_score
                        && !string.IsNullOrWhiteSpace(shit.imgBase64)
                        && shit.AIscore == 0)
                    {
                        shit.AIscore = GPT.Instance.ZPGetImgScore(shit.imgBase64);
                        if (shit.AIscore == 0) shit.AIscore = -1;//避免重复运算
                    }
                    //shit.score = shit.score + shit.AIscore;

                    //if (shit.score == 0) shit.score = -1;// drop

                }
            }

            // 根据已统计的部分看平均人工打分数值
            // 用来判断当前时段人工打分活跃程度，及需不需要提升AI打分权重
            double allscore = 0;
            double allnum = 1;
            for (int i = 0; i < shits.Count; i++)
            {
                var shit = shits[i];
                if (DateTime.Now - shit.createTime > new TimeSpan(0, config.deal_score_span_min, 0)// 足够久远
                )
                {
                    allscore += shit.score;
                    allnum += 1;
                }
            }
            last_loop_ave_score = allscore / allnum;



            // find bests
            List<Shit> bestShits = new List<Shit>();
            int bestNumMax = config.once_maxnum;
            for (int i = 0; i < bestNumMax; i++)
            {
                var best = getMaxScoreUnPublicShit(bestShits);
                if (best != null)
                {
                    bestShits.Add(best);
                }
            }
            bestShits.Sort((x, y) => (x.createTime > y.createTime ? 1 : -1));
            if (bestShits.Count > 0)
            {
                foreach (var shit in bestShits)
                {
                    string msgid = shit.contexts.First().messageId;
                    Logger.Log($"[{shit.score}分!] shit{msgid}");

                    //var nodes = getNodeTree(shit.context);
                    foreach (var target in ShitTarget)
                    {
                        // 不重构，直接搬运原文
                        Logger.Log($"shit[{msgid}] => {target.targetId}");
                        shit.contexts.First().client.SendForwardToGroupSimply(target.targetId, msgid);
                        //    if (nodes == null)
                        //    {
                        //        // 不重构，直接搬运原文
                        //        context.client.SendForwardToGroupSimply(targetId, context.messageId);
                        //    }
                        //    else
                        //    {
                        //        context.client.SendForwardMessageToGroup(targetId, nodes);
                        //    }

                    }
                    shit.published = true;
                    lastPublishDate = DateTime.Now;
                    config.historyPublished++;
                }
            }


            lock (shitMutex)
            {
                // 将非常旧的扔出队列。当然hash列表保留着
                for (int i = shits.Count - 1; i >= 0; i--)
                {
                    if (shits[i].published == true
                        || DateTime.Now - shits[i].createTime > new TimeSpan(0, config.del_old_span_min, 0))
                    {
                        shits.RemoveAt(i);
                    }
                }


                //foreach(var shit in shits)
                //{
                //    if(DateTime.Now - shit.createTime > new TimeSpan(0, 5, 0) && shit.score > 0)
                //    {

                //    }
                //}

            }



            Save();

        }



        public override void Save()
        {
            try
            {
                var transInfo = new List<ShitTransGroupInfo>(ShitTarget);
                transInfo.AddRange(ShitSource.Values);
                LocalStorage.WriteResource(sfile, JsonConvert.SerializeObject(transInfo));

                LocalStorage.WriteResource(configfile, JsonConvert.SerializeObject(config));

                LocalStorage.WriteResource(hashfile, string.Join("\r\n", oldHash.ToArray()));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }


        }

        private string setTransSourceStop(MessageContext context, string[] param)
        {
            if (!context.IsAdminUser) return "";
            else if (!context.IsGroup) return "";




            //open = false;
            string source = param[1];
            if (string.IsNullOrWhiteSpace(source)) source = context.groupId;
            if (ShitSource.ContainsKey(source))
            {
                ShitSource.Remove(source);
            }
            return $"已中止源于{(source == context.groupId ? "本群" : $"群{source}")}的自动转发行为，，，";
        }

        private string setTransTargetStop(MessageContext context, string[] param)
        {
            if (!context.IsAdminUser) return "";
            else if (!context.IsGroup) return "";




            //open = false;
            string target = param[1];
            if (string.IsNullOrWhiteSpace(target)) target = context.groupId;

            for (int i = ShitTarget.Count - 1; i >= 0; i--)
            {
                if (ShitTarget[i].targetId == target)
                {
                    ShitTarget.RemoveAt(i);
                }
            }
            return $"已中止向{(target == context.groupId ? "本群" : $"群{target}")}的自动转发行为，，，";
        }

        private string setTransTarget(MessageContext context, string[] param)
        {
            try
            {
                if (!context.IsAdminUser) return "";
                else if (!context.IsGroup) return "";

                string tid = param[1];
                string sid = context.groupId;
                bool exist = false;
                foreach (var g in ShitTarget)
                {
                    if (g.targetId == tid)
                    {
                        exist = true;
                        break;
                    }
                }
                if (!exist) ShitTarget.Add(new ShitTransGroupInfo() { targetId = tid });
                return $"已增加转发目标 {tid}";
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
            return "";
        }

        private string setTransSource(MessageContext context, string[] param)
        {
            try
            {
                if (!context.IsAdminUser) return "";
                else if (!context.IsGroup) return "";

                string sid = param[1];
                string tid = context.groupId;

                if (!ShitSource.ContainsKey(sid)) ShitSource.Add(sid, new ShitTransGroupInfo() { sourceId = sid });

                return $"已增加转发来源 {sid}";
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
            return "";
        }

        //public static List<MessageInfo> Msg2Info(List<Message> msgs)
        //{
        //    var infos=new List<MessageInfo>();

        //    if (msgs == null || msgs.Count <= 0) return infos;
        //    foreach (var m in msgs) infos.Add(new MessageInfo(m));

        //    return infos;
        //}

        private List<Message> getNodeTree(ForwardNodeExist forward)
        {
            string user_id_default = "1094950020";
            string nickname_default = "QQ用户";
            List<Message> nodes = new List<Message>();
            if (forward == null) return nodes;
            if (forward.content == null)
            {
                clientQQ?.GetForwardMessage(forward);
            }



            if (forward.content == null || forward.content.Count <= 0) return nodes;
            try
            {
                var pmsg = new List<MessageInfo>();
                foreach (var m in forward.content)
                {
                    List<MessageInfo> infos = new List<MessageInfo>();
                    foreach (var msg in m.message)
                    {
                        if (msg is Text t)
                        {
                            if (t.text.EndsWith("↓")) continue;
                        }
                        else if (msg is JsonData jd) continue;
                        if (msg is ForwardNodeExist)
                        {
                            // 是嵌套结构，所以不拆了，直接套娃转发
                            return null;
                        }
                        //if(msg is Reply r) { continue; }
                        //if (msg is ForwardNodeExist n)
                        //{
                        //    var innerTree = getNodeTree(n.id);
                        //    if (innerTree != null)
                        //    {
                        //        infos.Add(new MessageInfo(new ForwardNodeNew
                        //        {
                        //            user_id = user_id_default,//m.user_id,
                        //            nickname = nickname_default,//m.sender.nickname,
                        //            content = Msg2Info(m.message)
                        //        }));
                        //    }
                        //}


                        infos.Add(new MessageInfo(msg));

                    }
                    var newNode = new ForwardNodeNew
                    {
                        user_id = user_id_default,//m.user_id,
                        nickname = nickname_default,//m.sender.nickname,
                        content = infos
                    };
                    nodes.Add(newNode);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }


            return nodes;
        }

        /// <summary>
        /// 向shit添加新元素。此处会检测是否重复
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Shit addNewShit(MessageContext context)
        {
            var ns = new Shit(context);
            if (string.IsNullOrWhiteSpace(ns.hash))
            {
                // not shit
                //Logger.Log($"[{context.groupId}] not shit.");
                return null;
            }
            else if (shithash.ContainsKey(ns.hash))
            {
                // exist!
                var existShit = shithash[ns.hash];
                var timespan = DateTime.Now - existShit.createTime;
                Logger.Log($"shit发过了，最初来自群{existShit.createGroup}({timespan.TotalMinutes:F2}分钟前)[id={existShit.contexts.First().messageId}]");
                if (!oldHash.Contains(ns.hash)) oldHash.Add(ns.hash);
                existShit.contexts.Add(ns.contexts.First());
                return existShit;
            }
            else if (oldHash.Contains(ns.hash))
            {
                // old shit
                return null;
            }
            else
            {
                // new!
                lock (shitMutex)
                {
                    shithash[ns.hash] = ns;
                    shits.Add(ns);
                    Logger.Log($"shit新鲜的，来自群{ns.createGroup}[id={ns.contexts.First().messageId}]");


                    //if (!string.IsNullOrWhiteSpace(ns.imgBase64))
                    //{
                    //    var score = GPT.Instance.ZPGetImgScore(ns.imgBase64);
                    //}
                    //ns.context.client.SendEmojiLike(ns.context.messageId, 76);
                }

            }


            return ns;
        }



        public override async Task<bool> HandleMessagesDIY(MessageContext context)
        {
            try
            {
                //Logger.Log($"!config.open?{!config.open}");
                if (config.open && ShitSource.ContainsKey(context.groupId))
                {
                    addNewShit(context);
                }
                //bool tranit = false;
                ////ForwardNodeExist forward = null;
                //List<string> targets = new List<string>();
                //foreach (var m in context.recvMessages)
                //{
                //    if (ShitSource.ContainsKey(context.groupId))
                //    {
                //        tranit = true;
                //        break;
                //    }
                //}
                ////Logger.Log($"tranit?{tranit}");
                //if (tranit)
                //{
                //    // add to prepare list
                //    addNewShit(context);
                //    //if (m is ForwardNodeExist fnode)
                //    //{
                //    //    forward = fnode;

                    //    //    break;
                    //    //}
                    //}





                    //var input = context.recvMessages.ToTextString();
                    //if (!context.IsAskme || !input.Contains('译')) return false;
                    //(string text, List<string> langs) = CutLanguages(input);
                    //if (langs.Count > 0 && !string.IsNullOrWhiteSpace(text))
                    //{
                    //    var resAll = getTrans(text, langs);
                    //    if (!string.IsNullOrWhiteSpace(resAll))
                    //    {
                    //        context.SendBackPlain(resAll, true);
                    //        return true;
                    //    }
                    //}

            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return false;
        }
    }

    public class ShitTransGroupInfo
    {
        public string sourceId;
        public string targetId;
        public string tags;
    }

    public class Shit
    {
        public List<MessageContext> contexts;
        public DateTime createTime;
        public string createGroup;
        public string createUser;
        public long score;
        public long AIscore;
        public bool published;
        public bool isForward = false;
        public string imgBase64;

        //string _hashtext;
        string _hash;
        public string hash { get
            {
                return _hash;
            } }
        public Shit(MessageContext _context)
        {
            contexts = new List<MessageContext>();
            contexts.Add(_context);
            createTime = DateTime.Now;
            createGroup = _context.groupId;
            createUser = _context.userId;
            score = 0;
            AIscore = 0;
            published = false;
            isForward = false;
            
            calHash();
        }

        //void downloadImage()
        //{
        //    foreach(var msg in contexts.First().recvMessages)
        //    {
        //        if(msg is Image img)
        //        {
        //            try
        //            {
        //                imgBase64 = Network.ConvertImageUrlToBase64(img.url).Result;
        //                return;

        //            }catch(Exception ex)
        //            {
        //                Logger.Log(ex);
                        
        //            }
                    
        //        }
        //    }
        //}

        void calHashSingleItem(Message item)
        {
            try
            {
                if (item is Image img)
                {
                    //Logger.Log($"<hashimg>{img.file}");
                    _hash = StaticUtil.ComputeHash(_hash + img.file);
                    if (string.IsNullOrWhiteSpace(imgBase64))
                    {
                        imgBase64 = Network.ConvertImageUrlToBase64(img.url).Result;
                    }

                }
                else if (item is Video video)
                {
                    //Logger.Log($"<hashvideo>{video.file}");
                    _hash = StaticUtil.ComputeHash(_hash + video.file);

                }
                else if (item is ForwardNodeExist forward)
                {
                    isForward = true;
                    foreach (var c in forward.content)
                    {
                        //Logger.Log(c.raw_message);
                        //Logger.Log("===1");

                        foreach (var cmsg in c.message)
                        {
                            calHashSingleItem(cmsg);
                        }
                        //Logger.Log("===2");
                        //_hash = StaticUtil.ComputeHash(_hash + c.message);
                    }

                }else if(item is Text text)
                {
                    if (isForward)
                    {
                        // text in forward
                        //Logger.Log($"<hashtext>{text.text}");
                        _hash = StaticUtil.ComputeHash(_hash + text.text);
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
        void calHash()
        {
            _hash = "";
            //bool hasNextForwardLevel = false;
            foreach(var item in contexts.First().recvMessages)
            {
                calHashSingleItem(item);
            }
            //if (string.IsNullOrWhiteSpace(_hashimg))
            //{
            //    _hashimg = StaticUtil.ComputeHash(context.recvMessages.ToTextString());
            //}
            
            //Logger.Log($"<hash> = {_hash}");
        }

        
    }
    public class ShitConfig
    {
        public int deal_score_span_min = 5;//minutes
        public int del_old_span_min = 60;//minutes
        public int once_maxnum = 5;
        public int min_score = 1;
        public bool open = false;

        public long historyPublished = 0;
        public long historyMaxScore = 0;
        public DateTime historyMaxScoreDate;
    }
}
