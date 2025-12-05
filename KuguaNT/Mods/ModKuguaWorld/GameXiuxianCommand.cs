using Kugua.Core;
using Kugua.Integrations.AI;
using Newtonsoft.Json;
using System.Data;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Kugua.Mods
{

    public partial class GameXiuxian
    {

        public static void Csetname(string id, string newName)
        {
            if (string.IsNullOrWhiteSpace(id)) return;
            if (string.IsNullOrWhiteSpace(newName)) return;
            if (!users.ContainsKey(id))
            {
                CreateUser(id,"初入修仙界");
            }
            var user = users[id];
            user.nick = newName;
            Save(user);

        }


        /// <summary>
        /// 玩家的整体信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string Cinfo(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return "";

            string res = "";


            try
            {
                if (!users.ContainsKey(id))
                {
                    CreateUser(id, "初入修仙界");
                }
                var user = users[id];

                res = $"{user.FullName}\r\n"
                    + $"种族：{user.race}\r\n"
                    + $"境界：{user.LevelName}({user.level})\r\n"
                    + $"加入时间：{user.birthDate.ToString("yyyy-MM-dd HH:mm")}\r\n";
                foreach (var prop in user.prop)
                {
                    if (prop.Value == 0) continue;
                    else if (prop.Key == "灵力") res += $"{prop.Key}：{(prop.Value>= GetLevelPower(user.level)?"【可突破】":"")}{prop.Value.ToSci()} / {GetLevelPower(user.level).ToSci()}\r\n";
                    else res += $"{prop.Key}：{prop.Value.ToSci()}\r\n";
                }

                if (user.items.Count > 0)
                {
                    res += $"储物袋：\r\n";
                    foreach (var item in user.items) res += $"{item.name} x {item.num}：{item.desc}\r\n";
                }

            }catch(Exception ex)
            {
                Logger.Log(ex);
            }


            
           
            return res;
        }




        /// <summary>
        /// 突破
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string Ctupo(string id)
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
                var power = user.prop["灵力"];
                if (power < GetLevelPower(user.level)) return $"突破失败，灵力不足（{power.ToSci()}/{GetLevelPower(user.level).ToSci()}）";

                if (user.CheckCooldown is string cooldownDesc)
                {
                    return cooldownDesc;
                }
                else
                {
                    user.lastPlayDate = DateTime.Now;
                }
                int addLevel = 0;

                List<string> newLevels = new List<string>();
                int maxtime = 100;
                while(GetLevelPower(user.level + addLevel) <= power && maxtime-- > 0)
                {
                    if (MyRandom.NextDouble < 0.08)
                    {
                        //fail
                        if (newLevels.Count <= 0)
                        {
                            user.prop["灵力"] = BigInteger.Max(1, user.prop["灵力"] * 9 / 10);
                            res = AGdesc($"{user.race}{user.FullName}突破到{AGlevel(user.race, user.level + 1)}境界失败的过程");
                            //res = res.Replace(UserTemplateName, user.FullName);
                            return res;
                        }
                        else
                        {
                            // half success
                            break;
                        }
                    }
                    else
                    {
                        newLevels.Add(AGlevel(user.race, user.level + addLevel));
                        addLevel += 1;
                        user.level += addLevel;

                        string pname = MyRandom.NextString(["气血", "神识", "机缘"]);
                        if (!user.prop.ContainsKey(pname)) user.prop[pname] = 0;
                        user.prop[pname] += user.prop[pname] * (10 + MyRandom.Next(1,5)) / 10;
                    }
                }
                res = AGdesc($"{user.race}{user.FullName}突破{string.Join("、", newLevels.Take(newLevels.Count-1))}境界,达到{newLevels.Last()}境界的过程");
                Save(user);
                return res;
                
            }
            
            

            return res;
        }
        


        /// <summary>
        /// 重开
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string Crestart(string id)
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
                //user = users[id];
                //res = AGdesc($"{olduser}{action}成为{user.race}{user.LevelName}").Replace(UserTemplateName,user.FullName);
            }

           
            return res;
        }



        /// <summary>
        /// 发布行动指令。包括修炼、夺宝之类
        /// </summary>
        /// <param name="id"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static string Cact(string id, string action)
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
                    if (user.CheckCooldown is string cooldownDesc)
                    {
                        return cooldownDesc;
                    }
                    else
                    {
                        user.lastPlayDate = DateTime.Now;
                    }

                    string powerDesc = "";
                    string enemyDesc = "";
                    string lostitemDesc = "";
                    string newitemDesc = "";


                    //string action = MyRandom.NextString(["战斗", "修炼", "奇遇", "夺宝"]);
                    string area = AGarea(user,action);
                    if (user.items.Count > 0 && MyRandom.NextDouble < 0.3)
                    {
                        // lost item
                        lostitemDesc = $"，消耗了{getLostItem(user)}";
                    }
                    if (user.prop["灵力"] >= GetLevelPower(user.level))
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
                            
                            if (MyRandom.NextDouble < 0.1)
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
                        ;
                        BigInteger addMax = GetLevelPower(user.level) * 100 / 500;
                        BigInteger addMin = GetLevelPower(user.level) * -100 / 5000;
                        BigInteger addBase = MyRandom.Next(addMin, addMax);
                        BigInteger add = addBase;
                        user.prop["灵力"] += add;
                        powerDesc = $"{(add > 0 ? $"获取{add.ToSci()}" : $"失去{(-add).ToSci()}")}点灵力";

                        if (MyRandom.NextDouble < 0.5)
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
        /// 使用物品。其中itemName字段是模糊匹配
        /// </summary>
        /// <param name="id"></param>
        /// <param name="itemName"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static string CuseItem(string id, string itemName,string action)
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


        /// <summary>
        /// 双修或者切磋
        /// </summary>
        /// <param name="id1"></param>
        /// <param name="id2"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static string Cshuangxiu(string id1,string id2, string action)
        {
            if (string.IsNullOrWhiteSpace(id1)) return "";
            if (string.IsNullOrWhiteSpace(id2)) return "";
            if (string.IsNullOrWhiteSpace(action)) return "";
            string res = "";
            if (!users.ContainsKey(id1)) res = CreateUser(id1, "初入修仙界");
            var user1 = users[id1];
           
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

            var area = "";
            if (user2 == null)
            {
                area = AGarea(user1, "与亲密之人见面");
                return AGdesc($"{user1.race}{user1.FullName}没在{area}找到{id2}，很{MyRandom.NextString(["悲伤", "尴尬", "释然", "愤怒"])}");
            }

            if (user2.id == user1.id)
            {
                // 不合理的输入，自己打自己
                return $" 不是，哥们?";
            }

            if (user1.CheckCooldown is string desc1) return desc1;
            else if (user1.CheckShuangxiuCooldown is string desc2) return desc2;
            else if (user2.CheckShuangxiuCooldown is string desc3) return desc3;
            user1.lastPlayDate = DateTime.Now;
            user1.lastShuangxiuDate = DateTime.Now;
            user2.lastShuangxiuDate = DateTime.Now;

            string desc = $"{user1.race}{user1.FullName}与{user2.race}{user2.FullName}{action}";
            string end = "";
            //MyRandom.NextString(["共同修炼", "在修炼的同时搞暧昧", "在修炼中相爱相杀", "不打不相识", "激情互动夺宝"])
            if (action == "对战")
            {
                var level1 = user1.level;
                var level2 = user2.level;
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
                        end += $",从{user2.FullName}那里夺取了{dpower.ToSci()}灵力";
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
                        end += $",被{user2.FullName}夺走了{dpower.ToSci()}灵力";
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
                res = AGdesc($"{user1.race}{user1.FullName}与{user2.race}{user2.FullName}在{area}{action},双双获得{dpower.ToSci()}点灵力",200);

            }
            Save(user1);
            Save(user2);


            return res;

        }



        /// <summary>
        /// 刷新修炼的冷却期
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string CrefreshCD(string id)
        {
            if (!users.ContainsKey(id))
            {
                // 没有角色时候不作响应，以防误触
                return "";
            }
            else
            {
                var user = users[id];
                var cuser = Config.Instance.UserInfo(id);
                if (cuser != null)
                {
                    BigInteger paid = BigInteger.Max(BigInteger.Max(1000, user.level * user.prop["灵力"]), cuser.Money / 200);
                    if (cuser.Money >= paid)
                    {
                        // get
                        cuser.Money -= paid;
                        user.lastPlayDate = user.lastPlayDate.AddDays(-2);
                        user.lastShuangxiuDate = user.lastShuangxiuDate.AddDays(-2);
                        string res = AGdesc($"{user.FullName}充值{paid.ToHans()}{ModBank.unitName},瞬间恢复了行动的活力！", 100);
                        Save(user);
                        return res;
                    }
                    else
                    {
                        return $"{user.FullName}的{ModBank.unitName}不够,帐上只有{cuser.Money.ToHans()}，以他的身份至少得掏{paid.ToHans()}";
                    }
                }


            }
            return "";
        }



        /// <summary>
        /// 助人为乐，无偿奉献
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string Cpaid(string id)
        {
            if (!users.ContainsKey(id))
            {
                // 没有角色时候不作响应，以防误触
                return "";
            }
            else
            {
                var user = users[id];
                var cuser = Config.Instance.UserInfo(id);
                if (cuser != null)
                {

                    BigInteger paid = BigInteger.Max(10, user.prop["灵力"] / 101);
                    if (user.prop["灵力"] >= paid)
                    {
                        string action = MyRandom.NextString(["恢复环境", "反馈社会", "助人为乐", "积累功德", "帮扶同门"]);
                        var helptarget = user;
                        if (MyRandom.NextDouble < 0.2) helptarget = users.Values.ToArray()[MyRandom.Next(users.Values)];
                        string area = AGarea(user, $"{helptarget.race}的{MyRandom.NextString(["宗门","洞府","广场","家里"])}");
                        // get
                        user.prop["灵力"] -= paid;
                        string end = "";
                        var checknum = MyRandom.NextDouble;
                        if (checknum < 0.3)
                        {
                            user.lastPlayDate = user.lastPlayDate.AddDays(-2);
                            user.lastShuangxiuDate = user.lastShuangxiuDate.AddDays(-2);
                            end = "完全恢复了他的活力";
                        }
                        else if(checknum < 0.8)
                        {
                            string pname = MyRandom.NextString(["灵石", "神识", "气血", "机缘"]);
                            if (!helptarget.prop.ContainsKey(pname)) helptarget.prop[pname] = 0;
                            var addval = BigInteger.Max(10, helptarget.prop[pname] *(MyRandom.Next(15)) / 100 * (MyRandom.NextDouble>0.8?1:-1));
                            
                            helptarget.prop[pname] += addval;
                            end = $"{helptarget.FullName}的{pname}{(addval>0?"增加":"减少")}了{addval.ToSci()}";
                        }
                        else
                        {
                            end = MyRandom.NextString(["善哉", "得到夸赞", "被说破坏环境", "得到膜拜", "心里高兴"]);
                        }

                        string res = AGdesc($"{user.race}{user.FullName}为了{action},把{paid.ToSci()}灵力洒向{helptarget.race}{area},{end}", 100);
                        Save(user);
                        return res;
                    }
                    else
                    {
                        return $"{user.FullName}的灵力不够,只有{user.prop["灵力"].ToSci()}，勤奋修炼吧你";
                    }
                }


            }
            return "";
        }














    
    }
}