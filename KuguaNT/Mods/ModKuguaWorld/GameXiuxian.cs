using Kugua.Core;
using Kugua.Integrations.AI;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Numerics;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using static System.Collections.Specialized.BitVector32;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Kugua.Mods{


    public class GameXiuxian
    {
        public static Dictionary<string, XiuxianUser> users = new Dictionary<string, XiuxianUser>();
        public static Dictionary<string, List<string>> names = new Dictionary<string, List<string>>();

        public GameXiuxian()
        {

        }


        public static void Init()
        {
            string upath = Config.Instance.FullPath("xiuxian/user");
            Directory.CreateDirectory(upath);
            foreach (var userfile in Directory.GetFiles(upath))
            {
                string userid = Path.GetFileNameWithoutExtension(userfile);
                users[userid] = new XiuxianUser(userid);
                try
                {
                    users[userid] = JsonConvert.DeserializeObject<XiuxianUser>(LocalStorage.Read(userfile));

                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            }



            string npath = Config.Instance.FullPath("xiuxian/name");
            Directory.CreateDirectory(npath);
            foreach (var namefile in Directory.GetFiles(npath))
            {
                string name = Path.GetFileNameWithoutExtension(namefile);
                names[name] = LocalStorage.ReadLines(namefile).ToList();
            }
        }

        public static void Save()
        {
            try
            {
                string upath = Config.Instance.FullPath("xiuxian/user");
                Directory.CreateDirectory(upath);
                foreach (var data in users.Values)
                {
                    File.WriteAllText($"{upath}/{data.id}.json", JsonConvert.SerializeObject(data, Formatting.Indented));
                }


                string npath = Config.Instance.FullPath("xiuxian/name");
                Directory.CreateDirectory(npath);
                //Logger.Log($"names {names.Count}:{string.Join(" ", names.Keys)}");
                foreach (var name in names)
                {
                    File.WriteAllText($"{npath}/{name.Key}.txt", string.Join("\r\n",name.Value));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public static void Save(string id)
        {
            try
            {
                string path = Config.Instance.FullPath("xiuxian/user");
                Directory.CreateDirectory(path);
                if(users.ContainsKey(id))
                {
                    File.WriteAllText($"{path}/{id}.json", JsonConvert.SerializeObject(users[id], Formatting.Indented));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }



        public static string CreateUser(string id)
        {
            // new user create
            var cuser = Config.Instance.UserInfo(id);

            var user = new XiuxianUser(id);
            users[id] = user;
            user.birthDate = DateTime.Now;

            if (!names.ContainsKey("种族")) names["种族"] = new List<string>();
            user.race = AGword($"昵称{cuser.Name}的玩家在修仙世界的种族是什么？", names["种族"]);
            string area = AGarea($"昵称{cuser.Name}的玩家的出场地");
            if (!names["种族"].Contains(user.race)) names["种族"].Add(user.race);
            user.birthDesc = AGdesc($"{user.race}修炼者{cuser.Name}在{area}加入修仙世界的出场简介");
            user.items = new List<XiuxianItem>();

            Save(id);
            return $"{user.birthDesc}";
        }


        public static void ChangeName(string id, string newName)
        {
            if (string.IsNullOrWhiteSpace(id)) return;
            if (string.IsNullOrWhiteSpace(newName)) return;
            if (!users.ContainsKey(id))
            {
                CreateUser(id);
            }
            var user = users[id];
            user.name = newName;
            Save(id);

        }


        /// <summary>
        /// 玩家的整体信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string Info(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return "";
            string res = "";
            if (!users.ContainsKey(id))
            {
                CreateUser(id);
            }
            var user = users[id];

            res = $"{user.FullName}\r\n"
                + $"种族：{user.race}\r\n"
                + $"境界：{user.LevelName}\r\n"
                + $"加入时间：{user.birthDate.ToString("yyyy-MM-dd HH:mm")}\r\n";
            foreach (var prop in user.prop) if (prop.Key == "境界") continue; else res += $"{prop.Key}：{prop.Value}\r\n";
            
            if (user.items.Count > 0)
            {
                res += $"宝物：";
                foreach (var item in user.items) res += $"[{item.type}]{item.name} x {item.num}：{item.desc}\r\n";
            }

            return res;
        }

        public static BigInteger getThisLevelFloorPower(BigInteger level)
        {
            return 100 + 127 * BigInteger.Pow(level, 2);
        }

        public static string AddLevel(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return "";
            string res = "";

            if (!users.ContainsKey(id))
            {
                res = CreateUser(id);
            }
            else
            {
                var user = users[id];
                var level = user.prop["境界"];
                var power = user.prop["灵力"];
                if (power < getThisLevelFloorPower(level)) return $"突破失败，灵力不足（{power}/{getThisLevelFloorPower(level)}）";
                if(MyRandom.NextDouble < 0.2)
                {
                    // fail
                    res = AGdesc($"{user.race}{user.FullName}突破到{AGlevel(user.race,(int)level+1)}境界失败的过程");
                    return res;
                }
                else
                {
                    // success
                    user.prop["境界"] += 1;
                    res = AGdesc($"{user.race}{user.FullName}突破到{user.LevelName}境界成功的过程");
                }
            }
            Save(id);

            return res;
        }

        public static string Restart(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return "";
            string res = "";
            if (!users.ContainsKey(id))
            {
                res = CreateUser(id);
            }
            else
            {
                var user = users[id];
                string olduser = $"{user.race}{user.FullName}";
                res = CreateUser(id);
                res = AGdesc($"{olduser}转世成{users[id].race}{users[id].LevelName}");
            }
            Save(id);
            return res;
        }

        public static string Action(string id, string action)
        {
            if (string.IsNullOrWhiteSpace(id)) return "";
            if (string.IsNullOrWhiteSpace(action)) return "";
            string res = "";

            try
            {
                if (!users.ContainsKey(id))
                {
                    res = CreateUser(id);
                }
                else
                {
                    var user = users[id];
                    var cooldown = new TimeSpan(0, 0, 5);
                    if (DateTime.Now - user.lastPlayDate < cooldown)
                    {
                        // cooldown
                        res = $"{user.FullName}修养生息，在{(cooldown - (DateTime.Now - user.lastPlayDate)).TotalMinutes:0.0}分钟之内不能轻举妄动";
                        return res;
                    }


                    string powerDesc = "";
                    string enemyDesc = "";
                    string lostitemDesc = "";
                    string newitemDesc = "";


                    //string action = MyRandom.NextString(["战斗", "修炼", "奇遇", "夺宝"]);
                    string area = AGarea($"{user.FullName}，境界是{user.LevelName},触发{action}剧情的场景");

                    if (user.prop["灵力"] >= getThisLevelFloorPower(user.prop["境界"]))
                    {
                        // reach floor.
                        if (action == "修炼")
                        {
                            res = AGdesc($"{user.race}{user.FullName}修炼达到{user.LevelName}瓶颈的情形");
                            return res;
                        }
                        else
                        {
                            // 满灵力时候的格斗或者寻宝
                            if (user.items.Count > 0 && MyRandom.NextDouble < 0.3)
                            {
                                // lost item
                                lostitemDesc = $"，消耗了{getLostItemList(id)}";
                            }
                            // get item
                            newitemDesc = $"，获得了{getNewItemList(id, action,2)}"; 
                        }
                        
                    }
                    else
                    {
                        //res = $"{user.FullName}开始{action}……\r\n";
                        BigInteger addMax = user.prop["境界"] * 130;
                        BigInteger addMin = user.prop["境界"] * -120;
                        BigInteger addBase = MyRandom.Next(addMin, addMax);
                        BigInteger add = addBase;
                        user.prop["灵力"] += add;
                        powerDesc = $"{(add > 0 ? $"获取{add}" : $"失去{-add}")}点灵力";

                        if (user.items.Count > 0 && MyRandom.NextDouble < 0.1)
                        {
                            // lost item
                            lostitemDesc = $"，消耗了{getLostItemList(id)}";
                        }

                        if (MyRandom.NextDouble < 0.2)
                        {
                            // get item
                            newitemDesc = $"，获得了{getNewItemList(id, action,1)}";

                        }
                    }

                    if(action != "修炼")
                    {
                        // 遭遇的敌手列表
                        enemyDesc = $"遇到了{AGgetEnemys($"{area}", MyRandom.Next(1, 2))}";
                    }



                    


                    string desccmd = $"{user.LevelName}境界的{user.race}{user.FullName}在{area}{enemyDesc}通过{action}{powerDesc}{lostitemDesc}{newitemDesc}的过程";
                    Logger.Log(desccmd);
                    res = AGdesc(desccmd);

                    user.lastPlayDate = DateTime.Now;
                }

                
                Save(id);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            
            return res;
        }



        public static string UseItem(string id, string itemName)
        {
            if (string.IsNullOrWhiteSpace(id)) return "";
            string res = "";
            if (!users.ContainsKey(id))
            {
                res = CreateUser(id);
            }
            else
            {
                var user = users[id];
                if (user.GetItem(itemName) is XiuxianItem item)
                {
                    res = AGUseItem(user, item);
                    item.num -= 1;
                    if (item.num <= 0) user.items.Remove(item);
                }
                else
                {
                    res = $"{user.FullName}没有物品{itemName},他手上只有{string.Join(",",user.items.Select(item=>$"({item.type}){item.name} x {item.num}"))}";
                    return res;
                }
            }
            Save(id);
            return res;
        }















        public static string getLostItemList(string id)
        {
            string desc = "";
            var user = users[id];

            var lostitem = user.items[MyRandom.Next(user.items.Count)];
            var lostitemNum = MyRandom.Next(1, (int)(Math.Ceiling(0.5 * ((int)(lostitem.num) + 1))));
            lostitem.num -= lostitemNum;
            if (lostitem.num <= 0) user.items.Remove(lostitem);
            desc = $"{(lostitemNum > 1 ? $"{lostitemNum}个" : "")}{lostitem.name}";

            return desc;

        }

        public static string getNewItemList(string id, string action, int maxnum = 3)
        {
            string desc = "";
            var user = users[id];


            //var newitem = new XiuxianItem();
            var listdesc = AGwordlist($"{user.LevelName}的{user.FullName}在{action}中获得的物品、装备或法宝列表，不超过{maxnum}个");
            //newitem.num += 1;
            foreach (var itemname in listdesc)
            {
                bool exist = false;
                foreach (var item in user.items)
                {
                    if (item.name == itemname)
                    {
                        exist = true;
                        item.num += 1;
                        desc += $"{item.name}({item.type},{item.desc})、";
                        break;
                    }
                }
                if (!exist)
                {
                    var item = new XiuxianItem();
                    item.name = itemname;
                    item.type = AGword($"{item.name}的类型名称");
                    item.num = 1;
                    item.desc = AGdesc($"{item.type}类道具{item.name}的威力或效用介绍",50);
                    //var props = 
                    user.items.Add(item);
                    desc += $"{item.name}({item.type},{item.desc})、";
                }
            }

            return desc;
        }













        public static string AGgetEnemys(string area, int maxnum)
        {
            string res = "";

            var enemylists = AGwordlist($"{area}场景里的{maxnum}个敌人");
            if (enemylists.Where(e => e == area).FirstOrDefault() is string enemyWrongName) enemylists.Remove(enemyWrongName);
            for(int i = 0; i < Math.Min(maxnum, enemylists.Count); i++)
            {
                res += $"{MyRandom.Next(1, 3)}个{enemylists[i]},";
            }

            return res;
        }
        


        public static string AGUseItem(XiuxianUser user, XiuxianItem item)
        {
            string desc = "";
            if (AGjudge(item, $"{user.race}{user.LevelName}{user.FullName}可以使用这个物品吗"))
            {
                var props = AGwordlist($"{user.race}{user.LevelName}{user.FullName}使用{item.type}类物品{item.name}所影响的属性种类或效果，物品描述是{item.desc},你只需要返回属性名称的序列，例如：精力,境界,黑暗抗性");
                List<(string, BigInteger)> pval =new List<(string, BigInteger)> ();
                foreach(var prop in props)
                {
                    
                    BigInteger oldval = 0;
                    if(user.prop.ContainsKey(prop))oldval = user.prop[prop];
                    var newdata = AGsetNum($"{user.race}{user.LevelName}{user.FullName}使用{item.type}类物品{item.name}之后的{prop}值改变量，使用者目前属性列表：{string.Join(",", user.prop.Select(e => $"{e.Key}={e.Value}"))}，物品描述是{item.desc},你只需要输出一个结果的数值，比如-3，不要输出额外解释");
                    user.prop[prop] = newdata;
                    pval.Add((prop, newdata - oldval));
                }
                desc = AGdesc($"{user.race}{user.LevelName}{user.FullName}使用{item.type}类物品{item.name}，影响：{string.Join(",", pval.Select(e => $"{e.Item1}{e.Item2}"))}。物品描述是{item.desc}");
            }
            else
            {

                desc = AGdesc($"{user.race}{user.LevelName}{user.FullName}使用{item.type}类物品{item.name}，失败了。物品描述是{item.desc}");

            }


            return desc;
        }


        public static string AGarea(string desc = "")
        {
            if (!names.ContainsKey("地区")) names["地区"] = new List<string>();
            while (users.Count > names["地区"].Count)
            {
                names["地区"].Add(AGword($"新的修仙地区？", names["地区"]));
            }
            if(MyRandom.NextDouble < 0.1) names["地区"].Add(AGword($"新的修仙地区？", names["地区"]));

            if (string.IsNullOrWhiteSpace(desc)) return MyRandom.NextString(names["地区"]);
            else return AGchooseOne(desc, names["地区"]);
        }


        public static string AGlevel(string race, int level)
        {
            int jlevel = 0;
            List<string> jlavelTopName = new List<string>();// 顶层境界名，比如xx初级和xx圆满只记录一个xx
            List<string> jlavelAllName = new List<string>();

            string levelDataKey = $"{race}-境界";
            if (!names.ContainsKey(levelDataKey))
            {
                names.Add(levelDataKey, new List<string>());
            }
            foreach (var nameItem in names[levelDataKey])
            {
                string name = nameItem.Split('\t')[0];
                int num = int.Parse(nameItem.Split('\t')[1]);
                jlavelTopName.Add(name);
                if (num <= 1) jlavelAllName.Add($"{name}");
                else if (num == 2) jlavelAllName.AddRange([$"{name}初期",$"{name}后期"]);
                else if (num == 3) jlavelAllName.AddRange([$"{name}初期", $"{name}中期", $"{name}后期"]);
                else if (num == 4) jlavelAllName.AddRange([$"半步{name}", $"{name}初期", $"{name}中期", $"{name}后期"]);
            }
            while (level > jlavelAllName.Count)
            {
                jlavelTopName.Add(AGword($"{race}族第{jlavelTopName.Count}级的修仙境界名称是什么？", jlavelTopName));
                string name = jlavelTopName.Last();
                int num = MyRandom.Next(2, 5); 
                if (num == 2) jlavelAllName.AddRange([$"{name}初期", $"{name}后期"]);
                else if (num == 3) jlavelAllName.AddRange([$"{name}初期", $"{name}中期", $"{name}后期"]);
                else if (num == 4) jlavelAllName.AddRange([$"半步{name}", $"{name}初期", $"{name}中期", $"{name}后期"]);
                names[levelDataKey].Add($"{name}\t{num}");
            }
            return jlavelAllName[level - 1];
        }

        public static string AGdesc(string use, int maxlen = 150)
        {
            //string res;
            return LLM.Instance.HSSendSingle($"作为修仙小说撰写器，为情节生成相应的描述语段，你只能返回纯文本，尽量在{maxlen}字以内，必须包含我给出的所有关键词和数值参数。语言风格{ MyRandom.NextString(["口语化","言简意赅","华丽唯美","神秘兮兮","恢弘悠远","淳朴直白"])}。",
                $"{use}");

        }

        public static BigInteger AGsetNum(string use)
        {
            BigInteger res = 0;
            BigInteger.TryParse(LLM.Instance.HSSendSingle($"作为修仙RPG数值计算器模块，你只能返回代表结果的数值，不输出任何额外内容。",
                $"{use}"),out res);
            return res;
        }

        public static List<string> AGwordlist(string use)
        {
            //string res;
            return LLM.Instance.HSSendSingle("作为修仙小说撰写器，生成相应的词语序列，你只能返回纯文本，不同项使用中文逗号隔开。",
                $"{use}").Split([',', '，'], StringSplitOptions.TrimEntries).ToList();

        }

        public static string AGchooseOne(string use, IEnumerable<string> exists)
        {
            //string res;
            return LLM.Instance.HSSendSingle("作为修仙小说撰写帮手，帮我选取一个词汇，你只能返回选择的结果结果词汇，别说其他话。",
                $"{use}{((exists == null || exists.Count() <= 0) ? "" : $" 待选列表：{string.Join("、", exists)}")}");

        }

        public static string AGword(string use, IEnumerable<string> exists = null)
        {
            //string res;
            return LLM.Instance.HSSendSingle("作为修仙小说撰写帮手，帮我生成相应的专有词汇，你只能返回一个结果词汇，不要与已有的重复。",
                $"{use}{((exists == null || exists.Count()<=0) ? "" : $" 已有的包括：{string.Join("、", exists)}")}");

        }


        public static bool AGjudge(XiuxianItem item, string use)
        {
            //string res;
            string res= LLM.Instance.HSSendSingle("作为修仙RPG规则判断者，你只能返回单个数字表示判断结果，1是允许，0是不许。",
                $"物品：{item.name}，描述：{item.desc},请问{use}");
            return res.Trim() == "1";
        }

    }

    public class XiuxianItem
    {
        public string name;
        public string type;
        public string desc;

        public BigInteger num;
        public Dictionary<string, BigInteger> prop;

        //public override string ToString()
        //{
        //    return $"{name}\t{num}\t{desc}";
        //}
    }


    public class XiuxianUser
    {
        public string id;
        public string name;
        public string title;
        public string race;
        public string birthDesc;
        public DateTime birthDate;

        /// <summary>
        /// 记录一个冷却时间
        /// </summary>
        public DateTime lastPlayDate;
        


        public Dictionary<string, BigInteger> prop;
        //public BigInteger level;
        //public BigInteger exp;
        public List<XiuxianItem> items;
        

        public XiuxianUser(string id)
        {
            this.id = id;
            this.prop = new Dictionary<string, BigInteger>();
            prop["境界"] = 1;
            prop["灵力"] = 0;
            prop["肉体"] = 1;
            birthDate = DateTime.Now;
        }


        /// <summary>
        /// 在修仙界展现的全名，包括头衔
        /// </summary>
        [JsonIgnore]
        public string FullName
        {
            get
            {
                var cuser = Config.Instance.UserInfo(id);
                string uname = $"{(string.IsNullOrWhiteSpace(title) ? "" : $"{title}")}{name}{cuser.Name}";
                return uname;
            }
        }


        /// <summary>
        /// 等级的名称
        /// </summary>
        [JsonIgnore]
        public string LevelName
        {
            get
            {
                if (!prop.ContainsKey("境界")) prop["境界"] = 1;
                return GameXiuxian.AGlevel(race, (int)prop["境界"]);
            }
        }

        public XiuxianItem GetItem(string name)
        {
            if(items == null) items = new List<XiuxianItem>();
            return items.Where(t => t.name == name).FirstOrDefault();
        }
    }
}