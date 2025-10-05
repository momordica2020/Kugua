using Kugua.Core;
using Kugua.Integrations.AI;
using Newtonsoft.Json;
using System.Data;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Kugua.Mods{


    public class GameXiuxian
    {
        public static string UserTemplateName = "刘昊";

        public static string[] NovelScene =
        {
            "耽美言情",
            "惊悚恐怖",
            "都市传说",
            "无厘头搞笑",
            "青春校园",
            "洪荒修仙",
            "欧美哥特",
            "打脸爽文",
        };

        public static string[] NovelStyles =
        {
            "耽美宫斗女频文",
            "轻松搞笑二次元",
            "中式恐怖",
            "洪荒修仙流",
            "哥特式文学の翻译腔",
            "古代文言文",
            "贴吧暴躁老哥",
            "都市青春颓废",
            "歌词",
            "纯对话文体模拟"
        };


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

            if (!names.ContainsKey("地区")) names["地区"] = new List<string>();
            if (!names.ContainsKey("种族")) names["种族"] = new List<string>();
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

        public static void Save(XiuxianUser user)
        {
            try
            {
                string path = Config.Instance.FullPath("xiuxian/user");
                Directory.CreateDirectory(path);
                File.WriteAllText($"{path}/{user.id}.json", JsonConvert.SerializeObject(user, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }



        public static string CreateUser(string id,string action)
        {
            // new user create
            var cuser = Config.Instance.UserInfo(id);

            var user = new XiuxianUser(id);
            users[id] = user;
            user.birthDate = DateTime.Now;
            user.InitProp();
            user.name = "";// cuser.Name;
            user.race = AGrace(user,action);
            string area = AGarea(user,action);
            user.birthDesc = AGdesc($"{user.race}修炼者{UserTemplateName}在{area}{action}的出场情形").Replace(UserTemplateName,user.FullName);
            user.items = new List<XiuxianItem>();

            Save(user);
            return user.birthDesc;
        }


        public static void ChangeName(string id, string newName)
        {
            if (string.IsNullOrWhiteSpace(id)) return;
            if (string.IsNullOrWhiteSpace(newName)) return;
            if (!users.ContainsKey(id))
            {
                CreateUser(id,"初入修仙界");
            }
            var user = users[id];
            user.name = newName;
            Save(user);

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
                CreateUser(id, "初入修仙界");
            }
            var user = users[id];

            res = $"{user.FullName}\r\n"
                + $"种族：{user.race}\r\n"
                + $"境界：{user.LevelName}({user.prop["境界"]})\r\n"
                + $"加入时间：{user.birthDate.ToString("yyyy-MM-dd HH:mm")}\r\n";
            foreach (var prop in user.prop)
            {
                if (prop.Key == "境界" || prop.Value == 0) continue;
                else if (prop.Key == "灵力") res += $"{prop.Key}：{prop.Value} / {getThisLevelFloorPower(user.prop["境界"])}\r\n";
                else res += $"{prop.Key}：{prop.Value}\r\n";
            }
            
            if (user.items.Count > 0)
            {
                res += $"储物袋：\r\n";
                foreach (var item in user.items) res += $"{item.name} x {item.num}：{item.desc}\r\n";
            }

            return res;
        }



        /// <summary>
        /// 本境界的灵力上限
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
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
                res = CreateUser(id, "初入修仙界");
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
                    res = AGdesc($"{user.race}{UserTemplateName}突破到{AGlevel(user.race,(int)level+1)}境界失败的过程");
                    res = res.Replace(UserTemplateName, user.FullName);
                    return res;
                }
                else
                {
                    // success
                    user.prop["境界"] += 1;
                    res = AGdesc($"{user.race}{UserTemplateName}突破到{user.LevelName}境界成功的过程");
                    res = res.Replace(UserTemplateName, user.FullName);
                    Save(user);
                }
                
            }
            
            

            return res;
        }

        public static string Restart(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return "";
            string res = "";
            if (!users.ContainsKey(id))
            {
                res = CreateUser(id, "初入修仙界");
            }
            else
            {
                var user = users[id];
                string olduser = $"{user.race}的{UserTemplateName}";
                string action = MyRandom.NextString(["转生", "转世", "投胎", "坠入下界", "飞升"]);
                res = CreateUser(id, action);
                res = AGdesc($"{olduser}{action}成为{users[id].race}{users[id].LevelName}").Replace(UserTemplateName,user.FullName);
            }

           
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
                    res = CreateUser(id, "初入修仙界");
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
                    user.lastPlayDate = DateTime.Now;

                    string powerDesc = "";
                    string enemyDesc = "";
                    string lostitemDesc = "";
                    string newitemDesc = "";


                    //string action = MyRandom.NextString(["战斗", "修炼", "奇遇", "夺宝"]);
                    string area = AGarea(user,action);

                    if (user.prop["灵力"] >= getThisLevelFloorPower(user.prop["境界"]))
                    {
                        // reach floor.
                        if (action == "修炼")
                        {
                            res = AGdesc($"{user.race}{UserTemplateName}{action}达到{user.LevelName}瓶颈的情形");
                            res = res.Replace(UserTemplateName, user.FullName);
                            return res;
                        }
                        else
                        {
                            // 灵力达到上限时候的格斗或者寻宝
                            if (user.items.Count > 0 && MyRandom.NextDouble < 0.3)
                            {
                                // lost item
                                lostitemDesc = $"，消耗了{getLostItem(user)}";
                            }
                            else if (MyRandom.NextDouble < 0.7)
                            {
                                newitemDesc = $"一无所获";
                            }
                            else
                            {
                                // get item
                                newitemDesc = $"，获得了{getNewItem(user, $"在{area}{action}" )}";
                            }
                        }
                        
                    }
                    else
                    {
                        //res = $"{user.FullName}开始{action}……\r\n";
                        BigInteger addMax = user.prop["境界"] * 130;
                        BigInteger addMin = user.prop["境界"] * -70;
                        BigInteger addBase = MyRandom.Next(addMin, addMax);
                        BigInteger add = addBase;
                        user.prop["灵力"] += add;
                        powerDesc = $"{(add > 0 ? $"获取{add}" : $"失去{-add}")}点灵力";

                        if (user.items.Count > 0 && MyRandom.NextDouble < 0.1)
                        {
                            // lost item
                            lostitemDesc = $"，消耗了{getLostItem(user)}";
                        }

                        if (MyRandom.NextDouble < 0.2)
                        {
                            // get item
                            newitemDesc = $"，获得了{getNewItem(user, $"在{area}{action}")}";

                        }
                    }

                    if(action != "修炼")
                    {
                        // 遭遇的敌手列表
                        enemyDesc = $"遇到了{AGgetEnemys($"{area}", MyRandom.Next(1, 2))}";
                    }

                    string desccmd = $"{user.race}{UserTemplateName}在{area}{enemyDesc}通过{action}{powerDesc}{lostitemDesc}{newitemDesc}的情节";
                    Logger.Log(desccmd);
                    res = AGdesc(desccmd);
                    res = res.Replace(UserTemplateName, $"{user.FullName}");

                    
                    Save(user);
                }


            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            
            return res;
        }


        /// <summary>
        /// itemName是模糊匹配所有
        /// </summary>
        /// <param name="id"></param>
        /// <param name="itemName"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static string UseItem(string id, string itemName,string action)
        {
            if (string.IsNullOrWhiteSpace(id)) return "";
            string res = "";
            if (!users.ContainsKey(id))
            {
                // 没有角色时候不作响应，以防误触
                return "";
            }
            else
            {
                var user = users[id];
                if (user.items.Count <= 0) return "";
                var itemList = user.GetItems(itemName);
                if (itemList.Count <= 0)
                {
                    res = $"{user.FullName}没有{itemName}。储物袋：\r\n{string.Join(",", user.items.Select(item => $"{item.name} x {item.num}"))}";
                    return res;
                }
                else
                {
                    int aiTakeMaxNum = 5;
                    var itemListAiInput = itemList.Take(aiTakeMaxNum).ToList();
                    if (new string[] { "熔炼", "炼化","卖" }.Contains(action))
                    {
                        res = AGSaleItems(user,itemListAiInput, action);
                    }
                    else
                    {
                        res = AGUseItems(user, itemListAiInput, action);
                    }
                    if (itemList.Count > aiTakeMaxNum) res += $"\r\n({action}了{itemList.Count}件：{string.Join(",",itemList.Select(item=>item.name))})";
                    for (int i = itemList.Count - 1; i >= 0; i--) user.items.Remove(itemList[i]);
                    Save(user);
                }
            }
            
            return res;
        }


        public static string getShuangxiu(string id1,string id2, string action)
        {
            if (string.IsNullOrWhiteSpace(id1)) return "";
            if (string.IsNullOrWhiteSpace(id2)) return "";
            if (string.IsNullOrWhiteSpace(action)) return "";
            string res = "";
            if (!users.ContainsKey(id1)) res = CreateUser(id1, "初入修仙界");
            var user1 = users[id1];
            var area = AGarea(user1, "与亲密之人见面");
            XiuxianUser user2 = null;
            if (!Regex.IsMatch(id2, @"^\d+$"))
            {
                // name
                user2 = getUserByName(id2);
            }
            else
            {
                // id
                if (!users.ContainsKey(id2)) res = CreateUser(id2, "初入修仙界");
                user2 = users[id2];
            }
            if (user2 == null) return AGdesc($"{user1.race}{user1.FullName}没在{area}找到{id2}，很{MyRandom.NextString(["悲伤", "尴尬", "释然", "愤怒"])}");

            if (user2.id == user1.id)
            {
                // 不合理的输入，自己打自己
                return $"不是，哥们?";
            }

            string desc = $"{user1.race}{user1.FullName}与{user2.race}{user2.FullName}{action}";
            string end = "";
            //MyRandom.NextString(["共同修炼", "在修炼的同时搞暧昧", "在修炼中相爱相杀", "不打不相识", "激情互动夺宝"])
            if (action == "对战")
            {
                var level1 = user1.prop["境界"];
                var level2 = user2.prop["境界"];
                var power1 = user1.prop["灵力"];
                var power2 = user2.prop["灵力"];
                var props1 = GetUserPropDescRandomNumber(user1, 5);
                var props2 = GetUserPropDescRandomNumber(user2, 5);
                var i1list = user1.items.Select(item => item.name).ToList();
                Util.FisherYates(i1list);
                var items1 = string.Join("、", i1list.Take(5));

                var i2list = user2.items.Select(item => item.name).ToList();
                Util.FisherYates(i2list);
                var items2 = string.Join("、", i2list.Take(5));

                action = MyRandom.NextString(["在修炼中相爱相杀","偶遇并厮杀","作为宿敌互相搏斗", "产生爱欲"]);
                // 计算胜率
                double success = MyRandom.NextDouble;
                if (level2 < level1) success = success * ((double)(level1 - level2 + 1) * 0.7);
                if (level2 > level1) success = success / ((double)(level2 - level1 + 1));
                if(success > 0.5)
                {
                    // fight success
                    end = "赢了";
                    var dpower = power2 / MyRandom.Next(2, 10);
                    if (dpower > 0)
                    {
                        user1.prop["灵力"] += dpower;
                        user2.prop["灵力"] -= dpower;
                        end += $",从{user2.FullName}那里夺取了{dpower}灵力";
                    }
                    if(user2.items.Count>0 && MyRandom.NextDouble < 0.3)
                    {
                        var item = user2.items[MyRandom.Next(user2.items)];
                        user1.items.Add(item);
                        user2.items.Remove(item);
                        end += $",从{user2.FullName}那里夺取了{item.name}";
                    }
                }
                else
                {
                    // fail
                    end = "输了";
                    var dpower = power1 / MyRandom.Next(2, 10);
                    if (dpower > 0)
                    {
                        user1.prop["灵力"] -= dpower;
                        user2.prop["灵力"] += dpower;
                        end += $",被{user2.FullName}夺走了{dpower}灵力";
                    }
                    if (user1.items.Count > 0 && MyRandom.NextDouble < 0.3)
                    {
                        var item = user1.items[MyRandom.Next(user1.items)];
                        user1.items.Remove(item);
                        user2.items.Add(item);                        
                        end += $",被{user2.FullName}夺走了{item.name}";
                    }
                }
                
                res = AGdesc($"{user1.race}{user1.FullName}({props1},{items1})与{user2.race}{user2.FullName}({props2},{items2}){action},{user1.FullName}{end}（不需要列出双方属性值，要描述双方使用法宝对打细节，要写战利品）",300);
            }
            else if (action == "双修")
            {
                action = MyRandom.NextString(["在修炼的同时搞暧昧", "彼此信赖歃血为盟", "交流修炼法门"]);
                var dpower = (user1.prop["灵力"] + user2.prop["灵力"]) / MyRandom.Next(4, 10);
                if (dpower<=0)
                {
                    dpower = MyRandom.Next(5, 100);
                }
                user1.prop["灵力"] += dpower;
                user2.prop["灵力"] += dpower;
                res = AGdesc($"{user1.race}{user1.FullName}与{user2.race}{user2.FullName}在{area}{action},双双获得{dpower}点灵力",200);

            }
            Save(user1);
            Save(user2);


            return res;

        }


























        public static XiuxianUser getUserByName(string name)
        {
            foreach(var user in users)
            {
                if (user.Value.FullName == name) return user.Value;
            }
            return null;
        }

        public static string getLostItem(XiuxianUser user)
        {
            string desc = "";
            var lostitem = user.items[MyRandom.Next(user.items.Count)];
            var lostitemNum = MyRandom.Next(1, (int)(Math.Ceiling(0.5 * ((int)(lostitem.num) + 1))));
            lostitem.num -= lostitemNum;
            if (lostitem.num <= 0) user.items.Remove(lostitem);
            desc = $"{(lostitemNum > 1 ? $"{lostitemNum}个" : "")}{lostitem.name}";

            return desc;

        }

        public static string getNewItem(XiuxianUser user, string action)
        {
            string desc = "";
            //var newitem = new XiuxianItem();
            var listdesc = AGwordlist($"{user.race}的{UserTemplateName}在{action}时获得的1个{MyRandom.NextString(["物品","装备","法宝"])}");
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
                        desc += $"{item.name}({item.desc})、";
                        break;
                    }
                }
                if (!exist)
                {
                    var item = new XiuxianItem();
                    item.name = itemname;
                    //item.type = AGword($"{item.name}的类型名称");
                    item.num = 1;
                    item.desc = AGdesc($"道具{item.name}的威力或效用介绍，这道具{MyRandom.NextString(["挺垃圾的","非常一般般","品质稍好","品质很高","是极品"])}",30);
                    //var props = 
                    user.items.Add(item);
                    desc += $"{item.name}({item.desc})、";
                }
            }
            desc = desc.TrimEnd('、');

            return desc;
        }

























        public static string AGgetEnemys(string area, int maxnum)
        {
            string res = "";

            var enemylists = AGwordlist($"{area}场景里的{maxnum}种敌人");
            if (enemylists.Where(e => e == area).FirstOrDefault() is string enemyWrongName) enemylists.Remove(enemyWrongName);
            for(int i = 0; i < Math.Min(maxnum, enemylists.Count); i++)
            {
                res += $"{MyRandom.Next(1, 3)}个{enemylists[i]},";
            }

            return res;
        }
        
        /// <summary>
        /// 要是玩加属性太多，随机选几个让ai参考
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        static string GetUserPropDescRandomNumber(XiuxianUser user, int limitnum = 5)
        {
            var prep = user.prop.Where(p => p.Key != "境界").ToList();
            if (prep.Count <= limitnum) return $"{string.Join(",", prep.Select(e => $"{e.Key}={e.Value}"))}";
            else
            {
                int[] indexs = new int[prep.Count];
                for (int i = 0; i < prep.Count; i++) indexs[i] = i;
                Util.FisherYates(indexs);
                string res = "";
                for (int i = 0; i < limitnum; i++) res += $"{prep[indexs[i]].Key}={prep[indexs[i]].Value},";
                return res;
            }
        }


        static string GetUserPropNameRandomNumber(XiuxianUser user, int limitnum = 5)
        {
            var prep = user.prop.Where(p => p.Key != "境界").ToList();
            if (prep.Count <= limitnum) return $"{string.Join(",", prep.Select(e => $"{e.Key}"))}";
            else
            {
                int[] indexs = new int[prep.Count];
                for (int i = 0; i < prep.Count; i++) indexs[i] = i;
                Util.FisherYates(indexs);
                string res = "";
                for (int i = 0; i < limitnum; i++) res += $"{prep[indexs[i]].Key},";
                return res;
            }
        }


        public static string AGSaleItems(XiuxianUser user, List<XiuxianItem> items, string action)
        {
            string desc = "";
            string itemsdesc = "";
            if (items.Count <= 1) itemsdesc = $"{items[0].name}({items[0].desc})";
            else itemsdesc = $"{string.Join("、", items.Select(it => it.name))}";

            string propName = "灵石";
            if (action == "炼化") propName = "灵力";
            if (!user.prop.ContainsKey(propName)) user.prop[propName] = 0;
            //var oldval = user.prop[propName];
            var newdata = AGsetNum($"{user.LevelName}{user.race}{action}{itemsdesc}之后增加的{propName}值，你只需要输出变化的数值，比如+3，不要输出额外解释");
            user.prop[propName] += newdata;
            
            desc = AGdesc($"{user.race}{user.FullName}{action}了{itemsdesc}，获得了{newdata}{propName}。");



            return desc;
        }
        public static string AGUseItems(XiuxianUser user, List<XiuxianItem> items,string action)
        {
            string desc = "";
            string propsdesc = GetUserPropDescRandomNumber(user);
            string propnamesdesc = GetUserPropNameRandomNumber(user);
            string itemsdesc = "";
            if (items.Count <= 1) itemsdesc = $"{items[0].name}({items[0].desc})";
            else itemsdesc = $"{string.Join("、", items.Select(it => it.name))}";
  
            var props = AGwordlist($"{user.LevelName}{user.race}{action}{itemsdesc}影响了哪些属性？待选项：{propnamesdesc}");
            List<(string, BigInteger)> pval =new List<(string, BigInteger)> ();
            foreach(var prop in props)
            {
                    
                BigInteger oldval = 0;
                if(user.prop.ContainsKey(prop))oldval = user.prop[prop];
                var newdata = AGsetNum($"{user.LevelName}{user.race}{action}{itemsdesc}之后的{prop}值改变量，使用者目前属性列表：{propsdesc}，,你只需要输出一个结果的数值，比如+3，不要输出额外解释");
                user.prop[prop] = oldval + newdata;
                pval.Add((prop, newdata));
            }
            desc = AGdesc($"{user.race}{user.FullName}{action}{itemsdesc}，影响：{string.Join(",", pval.Select(e => $"{e.Item1}{(e.Item2>0?"+":"")}{e.Item2}"))}。");



            return desc;
        }

        public static string AGrace(XiuxianUser user, string action)
        {
            string race = "";

            var list = names["种族"];
            race = AGword($"昵称{user.FullName}的修炼者在{MyRandom.NextString(NovelStyles)}世界的种族是什么？可以不拘一格", list);
            if (!list.Contains(race)) list.Add(race);

            return race;
        }

        public static string AGarea(XiuxianUser user, string action)
        {
            string area = "";

            
            var list = names["地区"];
            area = AGword($"{user.FullName}在{MyRandom.NextString(NovelStyles)}风格小说里{action}的地区？可以不拘一格");
            if (!list.Contains(area)) list.Add(area);

            return area;
        }
        //public static string AGarea(string desc = "")
        //{
        //    if (!names.ContainsKey("地区")) names["地区"] = new List<string>();
        //    //while (users.Count > names["地区"].Count)
        //    //{
        //    //    names["地区"].Add(AGword($"新的修仙地区？", names["地区"]));
        //    //}
        //    //if(MyRandom.NextDouble < 0.1) names["地区"].Add(AGword($"新的修仙地区？", names["地区"]));

        //    if (string.IsNullOrWhiteSpace(desc)) return MyRandom.NextString(names["地区"]);
        //    else return AGchooseOne(desc, names["地区"]);
        //}


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
            string style = MyRandom.NextString(NovelStyles);
            return LLM.Instance.HSSendSingle($"作为文章修改器，把情节润色为相应{style}风格的段落，你只能返回纯文本，尽量在{maxlen}字以内，必须包含我给出的所有数值参数。必须使用{style}风格",
                $"{use}");

        }

        public static BigInteger AGsetNum(string use)
        {
            BigInteger res = 0;
            BigInteger.TryParse(LLM.Instance.HSSendSingle($"作为{MyRandom.NextString(NovelScene)}RPG数值计算器模块，你只能返回代表结果的数值，如果遇到模糊就随便给个数字即可，不要做任何解释，不输出任何额外内容。",
                $"{use}"),out res);
            return res;
        }

        public static List<string> AGwordlist(string use)
        {
            //string res;
            return LLM.Instance.HSSendSingle($"扮演{MyRandom.NextString(NovelScene)}撰写bot，生成相应的词语序列，你不要做任何解释，如果遇到模糊就随便生成即可，只能返回纯文本，不同项使用中文逗号隔开。",
                $"{use}").Split([',', '，'], StringSplitOptions.TrimEntries).ToList();

        }

        public static string AGchooseOne(string use, IEnumerable<string> exists)
        {
            //string res;
            return LLM.Instance.HSSendSingle($"扮演{MyRandom.NextString(NovelScene)}撰写bot，帮我选取一个词汇，你只能返回选择的结果结果词汇，如果遇到模糊就随便给个词即可，不要做任何解释。",
                $"{use}{((exists == null || exists.Count() <= 0) ? "" : $" 待选列表：{string.Join("、", exists)}")}");

        }




        public static string AGword(string use, IEnumerable<string> exists = null)
        {
            //string res;
            return LLM.Instance.HSSendSingle($"作为{MyRandom.NextString(NovelScene)}小说辅助生成器模块，生成相应的专有词汇，你只能返回一个结果词汇，不要与已有的重复。如果遇到模糊就随便给个词即可，不要做任何解释和额外输出。",
                $"{use}{((exists == null || exists.Count()<=0) ? "" : $" 已有的包括：{string.Join("、", exists)}")}");

        }


        public static bool AGjudge(XiuxianItem item, string use)
        {
            //string res;
            string res= LLM.Instance.HSSendSingle($"作为RPG规则判断者，你只能返回单个数字表示判断结果，1是允许，0是不许。",
                $"物品：{item.name}({item.desc}),{use}");
            return res.Trim() == "1";
        }

    }

    public class XiuxianItem
    {
        public string name;
        //public string type;
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
            prop["气血"] = 0;
            prop["神识"] = 0;
            prop["机缘"] = 0;

            prop["灵石"] = 0;
            birthDate = DateTime.Now;
            items = new List<XiuxianItem>();
        }

        public void InitProp()
        {
            // 初始属性
            prop["灵力"] = GameXiuxian.AGsetNum($"{race}的初始灵力属性值（0~100）");
            prop["气血"] = GameXiuxian.AGsetNum($"{race}的初始气血属性值（0~100）");
            prop["神识"] = GameXiuxian.AGsetNum($"{race}的初始神识属性值（0~100）");
            prop["机缘"] = GameXiuxian.AGsetNum($"{race}的初始机缘属性值（0~100）");
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

        public List<XiuxianItem> GetItems(string name)
        {
            if(items == null) items = new List<XiuxianItem>();
            if (string.IsNullOrWhiteSpace(name)) return items;
            return items.Where(t => t.name.Contains(name)).ToList();
        }
    }
}