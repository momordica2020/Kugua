using HtmlAgilityPack;
using MMDK.Struct;
using MMDK.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MMDK.Plugins
{
    /// <summary>
    /// 直播间信息
    /// </summary>
    public class LiveInfo
    {
        public string uname;
        public int roomid;
        public int uid;
        public int online;
        public string title;
    }

    class BilibiliPlugin : Plugin
    {
        string nameDictName = "namedict.txt";
        string roomDictName = "roomdict.txt";
        string areaDictName = "areadict.txt";
        object searchMutex = new object();
        public Dictionary<string, string> nameDict = new Dictionary<string, string>();
        Dictionary<string, string> roomDict = new Dictionary<string, string>();
        Dictionary<string, int[]> areas = new Dictionary<string, int[]>();

        string shiquName = "shiqu.txt";
        Dictionary<double, string[]> shiqus = new Dictionary<double, string[]>();


        public BilibiliPlugin() : base("Bilibili")
        {
            
        }
        public override void InitSource()
        {
            var lines = FileHelper.readLines(PluginPath + nameDictName);
            nameDict = new Dictionary<string, string>();
            foreach (var line in lines)
            {
                var items = line.Split('\t');
                if (items.Length >= 2)
                {
                    nameDict[items[0]] = items[1];
                }
            }

            var lines2 = FileHelper.readLines(PluginPath + roomDictName);
            roomDict = new Dictionary<string, string>();
            foreach (var line in lines2)
            {
                var items = line.Split('\t');
                if (items.Length >= 2)
                {
                    roomDict[items[0]] = items[1];
                }
            }

            var lines3 = FileHelper.readLines(PluginPath + areaDictName);
            areas = new Dictionary<string, int[]>();
            foreach (var line in lines3)
            {
                var items = line.Split('\t');
                if (items.Length >= 3)
                {
                    try
                    {
                        areas[items[0]] = new int[] { int.Parse(items[1]), int.Parse(items[2]) };
                    }
                    catch { }
                }
            }

            var lines4 = FileHelper.readLines(PluginPath + shiquName);
            foreach (var line in lines4)
            {
                var items1 = line.Trim().Split(':');
                if (items1.Length == 2)
                {
                    var items2 = items1[1].Split('\t');
                    shiqus[double.Parse(items1[0])] = items2;
                }
            }
        }
        public override void HandleMessage(Message msg)
        {
            string cmd = BOT.getAskCmd(msg);

            Regex bsearchreg = new Regex("(\\S+)区有多少(\\S+)");
            var bseatchres = bsearchreg.Match(cmd);
            bool matchSuccess = false;
            if (bseatchres.Success)
            {
                try
                {
                    string barea = bseatchres.Groups[1].ToString().Trim() + "区";
                    string btar = bseatchres.Groups[2].ToString().Trim();

                    string res = getTitleSearch(barea, btar);
                    msg.str = res;
                    BOT.sendBack(msg, true);
                    return;
                }
                catch
                {

                }
            }

            if (cmd.EndsWith("区谁在播") || cmd.EndsWith("区有谁在播") || cmd.EndsWith("区有谁") || cmd.EndsWith("区都有谁"))
            {
                string areaname = cmd.Substring(0, cmd.LastIndexOf('区') + 1);
                msg.str = getLiveNum(areaname);
                matchSuccess = true;
            }
            else if (cmd.EndsWith("区谁最惨"))
            {
                string areaname = cmd.Substring(0, cmd.LastIndexOf('区') + 1);
                msg.str = getPoorLives(areaname);
                matchSuccess = true;
            }
            else if (cmd.Contains("在播吗") || cmd.Contains("播了吗"))
            {
                string test = cmd.Replace("在播吗", "").Replace("播了吗", "");
                string xnq = getLiveInfo(test);
                msg.str = getLiveInfo(test);
                matchSuccess = true;
            }
            else if (cmd.StartsWith("别名"))
            {
                var items = cmd.Substring(2).Trim().Split(new char[] { ' ', ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
                if (items.Length >= 2)
                {
                    setReplaceName(items[0], items[1]);
                    setReplaceName(items[1], items[0]);
                    msg.str = "好";
                    matchSuccess = true;
                }
            }
            else if (cmd.StartsWith("房间号"))
            {
                var items = cmd.Substring(3).Trim().Split(new char[] { ' ', ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
                if (items.Length >= 2)
                {
                    setRoomId(items[0], items[1]);
                    msg.str = "好";
                    matchSuccess = true;
                }
            }

            else if (cmd.StartsWith("查看别名"))
            {
                var item = cmd.Substring(4).Trim();
                if (item.Length > 0)
                {
                    var reslist = getNames(item);
                    msg.str = string.Join("/", reslist);
                    matchSuccess = true;
                }
            }
            if (matchSuccess)
            {
                BOT.sendBack(msg, true);
            }
           
            return;
        }

       

        public string getNowClockCountry(int time)
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            string res = "";

            DateTime nowd = DateTime.UtcNow;
            int hourdiff = time - nowd.Hour;
            if (hourdiff < -12) hourdiff += 24;
            else if (hourdiff > 12) hourdiff -= 24;
            res += shiqus[hourdiff][rand.Next(shiqus[hourdiff].Length)];
            res += $"时间{time.ToString().PadLeft(2, '0')}:{nowd.ToString("mm:ss")}";

            return res;
        }
        public void saveRooms(ICollection<LiveInfo> infos)
        {
            try
            {
                //var infos = getLiveInfos(parea, area);
                lock (searchMutex)
                {
                    try
                    {
                        foreach (var info in infos)
                        {
                            if (!roomDict.ContainsKey(info.uname)) roomDict[info.uname] = info.roomid.ToString();
                        }
                        //if (File.Exists(path + roomDictName)) File.Delete(path + roomDictName);
                        List<string> res = new List<string>();
                        foreach (var key in roomDict.Keys)
                        {
                            res.Add($"{key}\t{roomDict[key]}");
                        }
                        File.WriteAllLines(PluginPath + roomDictName, res.ToArray(), Encoding.UTF8);
                    }
                    catch
                    {

                    }
                }
            }
            catch
            {
            }
        }

        public void setReplaceName(string ori, string tar)
        {
            lock (searchMutex)
            {
                try
                {
                    nameDict[ori] = tar;
                    List<string> res = new List<string>();
                    foreach (var key in nameDict.Keys)
                    {
                        res.Add($"{key}\t{nameDict[key]}");
                    }
                    File.WriteAllLines(PluginPath + nameDictName, res.ToArray(), Encoding.UTF8);
                }
                catch
                {

                }
            }
        }

        public void setRoomId(string ori, string roomid)
        {
            lock (searchMutex)
            {
                try
                {
                    roomDict[ori] = roomid;
                    List<string> res = new List<string>();
                    foreach (var key in roomDict.Keys)
                    {
                        res.Add($"{key}\t{roomDict[key]}");
                    }
                    File.WriteAllLines(PluginPath + roomDictName, res.ToArray(), Encoding.UTF8);
                }
                catch
                {

                }
            }

        }

        /// <summary>
        /// 将给定的毫秒数转换成DateTime
        /// </summary>
        public static DateTime SecondsToDateTime(long seconds)
        {
            DateTime dt_1970 = new DateTime(1970, 1, 1);
            TimeSpan span = TimeSpan.FromSeconds(seconds);
            span = span.Add(new TimeSpan(8, 0, 0));
            return dt_1970 + span;
        }

        public string getRoomInfo(string roomid)
        {
            string url = "https://live.bilibili.com/" + roomid;
            string html = WebHelper.getData(url, Encoding.UTF8);
            HtmlDocument hdoc = new HtmlDocument();
            hdoc.LoadHtml(html);

            //统计数值
            try
            {
                string info = "";
                //string title = hdoc.DocumentNode.SelectSingleNode("//*[@id=\"link-app-title\"]").InnerText;
                if (html.Contains("__NEPTUNE_IS_MY_WAIFU__"))
                {
                    int begin = html.LastIndexOf("__NEPTUNE_IS_MY_WAIFU__") + 24;
                    int end = html.LastIndexOf("}") + 1;
                    //FileIOActor.log("begin "+begin);
                    //FileIOActor.log("end " + end);
                    if (begin < end)
                    {
                        string json = html.Substring(begin, end - begin);
                        //FileIOActor.log(json);
                        try
                        {
                            JObject j = JObject.Parse(json);
                            string status = j["roomInitRes"]["data"]["live_status"].ToString();
                            string title = j["baseInfoRes"]["data"]["title"].ToString();
                            string timeSpendStr = "";
                            long beginTimelong = long.Parse(j["roomInitRes"]["data"]["live_time"].ToString());
                            if (beginTimelong > 1000000000)
                            {
                                //FileIOActor.log("beginTimelong " + beginTimelong);

                                DateTime beginTime = SecondsToDateTime(beginTimelong);
                                //FileIOActor.log("beginTime " + beginTime.ToString("yyyyMMdd HHmmss"));
                                DateTime nowTime = DateTime.Now;
                                // FileIOActor.log("nowTime " + nowTime.ToString("yyyyMMdd HHmmss"));
                                var timespend = nowTime - beginTime;
                                if (timespend.Days > 0) timeSpendStr += $"{timespend.Days}天";
                                if (timespend.Hours > 0) timeSpendStr += $"{timespend.Hours}小时";
                                if (timespend.Minutes > 0) timeSpendStr += $"{timespend.Minutes}分钟";
                                if (timeSpendStr.Length <= 0) timeSpendStr = "刚不到一分钟";
                            }
                            else
                            {
                                timeSpendStr = "不知道多长时间";
                            }
                            //FileIOActor.log("begin ? "+ timeSpendStr);
                            string areaName = j["baseInfoRes"]["data"]["area_name"].ToString();
                            int online = int.Parse(j["baseInfoRes"]["data"]["online"].ToString());

                            if (status == "1")
                            {
                                // live open
                                info = $"正在{areaName}区播 {title},人气{online},播了{timeSpendStr}";
                            }
                            else
                            {
                                // live close
                                info = "没播";
                            }
                        }
                        catch (Exception e1)
                        {
                            FileHelper.Log(e1);
                        }
                    }
                }
                return info;
            }
            catch (Exception e)
            {
                FileHelper.Log(e.Message + "\r\n" + e.StackTrace);
            }
            return "";
        }

        public string[] getNames(string username)
        {
            string[] tail = { "Official", "official", "Channel", "channel", "_Official", "_Channel" };
            List<string> finds = new List<string>();
            List<string> findsnext = new List<string>();
            Dictionary<string, int> names = new Dictionary<string, int>();

            finds.Add(username);
            foreach (var t in tail) finds.Add(username + t);

            bool finish = false;
            while (!finish)
            {
                finish = true;
                foreach (var target in finds)
                {
                    if (!names.ContainsKey(target))
                    {
                        finish = false;
                        names[target] = 0;
                    }
                    if (nameDict.ContainsKey(target))
                    {
                        var next = nameDict[target];
                        if (!names.ContainsKey(next))
                        {
                            finish = false;
                            findsnext.Add(next);
                        }
                    }
                }
                finds = findsnext;
                findsnext = new List<string>();
            }

            return names.Keys.ToArray();
        }

        public string getLiveInfo(string username)
        {
            //readRooms();
            int findmaxtime = 15;

            // Directory<string, string
            while (nameDict.ContainsKey(username) && findmaxtime-- >= 0) username = nameDict[username];
            //FileIOActor.log(username + " <- name");

            foreach (var un in getNames(username))
            {
                //string un = username + t;
                if (roomDict.ContainsKey(un))
                {
                    string roomid = roomDict[un];
                    string info = $"{un}" + getRoomInfo(roomid);
                    return info;
                }
            }
            return $"没找到{username}的直播，他大概没开播8";
        }


        public string getTitleSearch(string areaName, string searchItem)
        {
            if (!areas.ContainsKey(areaName))
            {
                return $"好像没在b站找到这个分区：" + areaName;
            }
            int parea = areas[areaName][0];
            int area = areas[areaName][1];

            //readRooms(parea, area);

            List<LiveInfo> infos = getLiveInfos(parea, area);

            int sum = 0;
            StringBuilder sb = new StringBuilder();
            List<string> livers = new List<string>();
            foreach (var info in infos)
            {
                if (info.title.Contains(searchItem))
                {
                    sum += 1;
                    livers.Add(info.uname);
                }
            }
            if (sum <= 0)
            {
                sb.Append("🈚️");
            }
            else
            {
                sb.Append($"{areaName}现在有{sum}个{searchItem}：");
                int maxshow = 10;
                if (livers.Count <= maxshow) sb.Append(string.Join("，", livers.ToArray()));
                else sb.Append(string.Join("，", livers.ToArray(), 0, maxshow) + " 等");
            }

            return sb.ToString();
        }

        public List<LiveInfo> getLiveInfos(int parea, int area, int maxpage = -1)
        {
            int page = 1;
            string sort = "online";
            int sum = 0;
            int sumindex = 0;
            List<LiveInfo> infos = new List<LiveInfo>();
            do
            {
                string url = $"https://api.live.bilibili.com/room/v3/area/getRoomList?platform=web&parent_area_id={parea}&cate_id=0&area_id={area}&sort_type={sort}&page={page}&page_size=30&tag_version=1";
                string resstr = WebHelper.getData(url, Encoding.UTF8, "", false, false);
                try
                {
                    JObject jo = JObject.Parse(resstr);
                    sum = int.Parse(jo["data"]["count"].ToString());
                    int num = jo["data"]["list"].Count();
                    for (int i = 0; i < num; i++)
                    {
                        try
                        {
                            LiveInfo info = new LiveInfo();
                            info.roomid = int.Parse(jo["data"]["list"][i]["roomid"].ToString());
                            info.uid = int.Parse(jo["data"]["list"][i]["uid"].ToString());
                            info.uname = jo["data"]["list"][i]["uname"].ToString();
                            info.online = int.Parse(jo["data"]["list"][i]["online"].ToString());
                            info.title = jo["data"]["list"][i]["title"].ToString();
                            infos.Add(info);
                        }
                        catch { }
                    }
                    sumindex += num;
                }
                catch (Exception ex)
                {
                    FileHelper.Log(ex);
                }

                page += 1;
                if (page == maxpage) break;
            } while (sumindex < sum);

            return infos;
        }

        public string getPoorLives(string areaName)
        {
            if (!areas.ContainsKey(areaName))
            {
                return $"好像没在b站找到这个分区：" + areaName;
            }
            int parea = areas[areaName][0];
            int area = areas[areaName][1];

            //readRooms(parea,area);

            LiveInfo[] infos = getLiveInfos(parea, area).ToArray();
            Array.Sort(infos, (LiveInfo info1, LiveInfo info2) => {
                if (info1.online > info2.online) return 1;
                else if (info1.online < info2.online) return -1;
                else return 0;
            });
            saveRooms(infos);
            int sum = infos.Length;

            StringBuilder sb = new StringBuilder();
            sb.Append($"{areaName}还有{sum}个人播");
            if (sum > 0)
            {
                sb.Append($"\r\n倒数第一是{infos[0].uname}，{infos[0].online}人气，在播{infos[0].title}");
            }
            if (sum > 1)
            {
                sb.Append($"\r\n倒数第二是{infos[1].uname}，{infos[1].online}人气，在播{infos[1].title}");
            }
            if (sum > 2)
            {
                sb.Append($"\r\n倒数第三是{infos[2].uname}，{infos[2].online}人气，在播{infos[2].title}");
            }
            if (sum <= 0)
            {
                sb.Clear();
                sb.Append($"{areaName}目前无人在播。");
            }
            return sb.ToString();
        }

        public string getLiveNum(string areaName)
        {
            if (!areas.ContainsKey(areaName))
            {
                return $"好像没在b站找到这个分区：" + areaName;
            }
            int parea = areas[areaName][0];
            int area = areas[areaName][1];

            //readRooms(parea,area);

            List<LiveInfo> infos = getLiveInfos(parea, area);
            saveRooms(infos);
            int sum = infos.Count;

            StringBuilder sb = new StringBuilder();
            sb.Append($"{areaName}还有{sum}个人播");
            if (sum > 0)
            {
                sb.Append($"\r\n第一是{infos[0].uname}，{infos[0].online}人气，在播{infos[0].title}");
            }
            if (sum > 1)
            {
                sb.Append($"\r\n第二是{infos[1].uname}，{infos[1].online}人气，在播{infos[1].title}");
            }
            if (sum > 2)
            {
                sb.Append($"\r\n第三是{infos[2].uname}，{infos[2].online}人气，在播{infos[2].title}");
            }
            if (sum <= 0)
            {
                sb.Clear();
                sb.Append($"{areaName}目前无人在播。");
            }
            return sb.ToString();

        }
    }
}
