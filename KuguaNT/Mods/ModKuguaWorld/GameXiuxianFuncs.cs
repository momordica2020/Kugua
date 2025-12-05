using Kugua.Core;
using Kugua.Integrations.AI;
using Newtonsoft.Json;
using System.Data;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Kugua.Mods{


    public partial class GameXiuxian
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
                    var u = JsonConvert.DeserializeObject<XiuxianUser>(LocalStorage.Read(userfile));
                    users[userid] = u;




                    ///TEMP
                    ///重设境界
                    if (u.prop.ContainsKey("境界") && u.prop.ContainsKey("灵力"))
                    {
                        int oldlevel = (int) u.prop["境界"];
                        BigInteger power = u.prop["灵力"];
                        int newlevel = 1;
                        while(GetLevelPower(newlevel) <= power)
                        {
                            newlevel += 1;
                        }
                        u.level = int.Min(oldlevel, newlevel);
                        u.prop.Remove("境界");
                    }
                    if (u.level <= 0) u.level = 1;
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
            user.nick = "";// cuser.Name;
            user.race = AGrace(user,action);
            string area = AGarea(user,action);
            user.birthDesc = AGdesc($"{user.race}修炼者{UserTemplateName}在{area}{action}的出场情形").Replace(UserTemplateName,user.FullName);
            user.items = new List<XiuxianItem>();

            Save(user);
            return user.birthDesc;
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
            var itemname = AGwordSingle($"{user.race}{UserTemplateName}在{action}时获得的{MyRandom.NextString(["物品","装备","法宝"])}");
            //newitem.num += 1;
           // foreach (var itemname in listdesc)
           // {
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
                    item.desc = AGdesc($"{item.name}的威力或效用介绍，这道具{MyRandom.NextString(["挺垃圾的","非常一般","品质稍好","品质很高","是极品"])}",30);
                    //var props = 
                    user.items.Add(item);
                    desc += $"{item.name}({item.desc})、";
                }
           // }
            desc = desc.TrimEnd('、');

            return desc;
        }


        /// <summary>
        /// 本境界的灵力上限
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static BigInteger GetLevelPower(int level)
        {
            return 100 + 127 * BigInteger.Pow(2, level - 1);
        }




        /// <summary>
        /// 要是玩加属性太多，随机选几个让ai参考
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        static string GetUserPropDescRandomNumber(XiuxianUser user, int limitnum = 5)
        {
            var prep = user.prop.ToList();
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
            var prep = user.prop.ToList();
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



    }
}