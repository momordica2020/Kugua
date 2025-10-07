using Newtonsoft.Json;
using System.Data;
using System.Numerics;

namespace Kugua.Mods{
    public class XiuxianUser
    {
        public string id;
        public string nick;
        public string title;
        public string race;

        public string birthDesc;
        public DateTime birthDate;

        /// <summary>
        /// 记录一个冷却时间
        /// </summary>
        public DateTime lastPlayDate;

        /// <summary>
        /// 互动的冷却
        /// </summary>
        public DateTime lastShuangxiuDate;

        public int level;


        public Dictionary<string, BigInteger> prop;
        //public BigInteger level;
        //public BigInteger exp;
        public List<XiuxianItem> items;
        

        public XiuxianUser(string id)
        {
            this.id = id;
            this.prop = new Dictionary<string, BigInteger>();
            level = 1;
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
                string uname = $"{(string.IsNullOrWhiteSpace(title) ? "" : $"{title}")}{nick}{cuser.Name}";
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
                return GameXiuxian.AGlevel(race, level);
            }
        }

        public List<XiuxianItem> GetItems(string name)
        {
            if(items == null) items = new List<XiuxianItem>();
            if (string.IsNullOrWhiteSpace(name)) return items;
            return items.Where(t => t.name.Contains(name)).ToList();
        }



        /// <summary>
        /// 检查用户是否在活动冷却期，如果返回值不为null则说明需要冷却
        /// </summary>
        /// <returns></returns>
        [JsonIgnore]
        public string CheckCooldown
        {
            get
            {
                var cooldown = new TimeSpan(0, 5, 0);
                if (DateTime.Now - lastPlayDate < cooldown)
                {
                    // cooldown
                    string res = $"{FullName}修养生息，在{(cooldown - (DateTime.Now - lastPlayDate)).TotalMinutes:0.0}分钟之内不能轻举妄动";
                    return res;
                }
                
                return null;
            }
        }


        /// <summary>
        /// 检查用户是否在双修和pk活动冷却期，如果返回值不为null则说明需要冷却
        /// </summary>
        /// <returns></returns>
        [JsonIgnore]
        public string CheckShuangxiuCooldown
        {
            get
            {
                var cooldown = new TimeSpan(6, 0, 0);
                if (DateTime.Now - lastShuangxiuDate < cooldown)
                {
                    // cooldown
                    string res = $"{FullName}在{(cooldown - (DateTime.Now - lastShuangxiuDate)).TotalMinutes:0.0}分钟之内需要调息静养";
                    return res;
                }
                
                return null;
            }

        }
    }
}