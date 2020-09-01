using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using MMDK.Plugins;
using MMDK.Struct;
using MMDK.Util;

namespace MMDK.Core
{
    public delegate void processOutputHandler(string str);
    public delegate bool HandleProcessMessage(Message msg);

    public interface IGlobalFunc {
        bool isAskme(Message msg);
        string getAskCmd(Message msg);
        void send(Message msg);
        void sendBack(Message msg, bool beginWithAt = false);
        void log(string msg);
        void accept(EventAddFriend e);
        void deny(EventAddFriend e);
        void accept(EventAddGroup e);
        void deny(EventAddGroup e);
        UserInfo getUserInfo(long qq);
        GroupInfo getGroupInfo(long group);
        void setGroupInfo(GroupInfo info);
        void tick(long group, long qq, string msg);
        void ban(long group, long qq, int time);
        void ban(long group);
        void banCancel(long group, long qq);
        void banCancel(long group);
        void quit(long group);
        int getGroupNum();
        int getFriendNum();
        MoneyManager getMoneyManager();
        HistoryManager GetHistoryManager();

    }
    class MainProcess : IGlobalFunc
    {
        public event processOutputHandler processOutput;
        //public event HandleProcessMessage processMessage;

        public static string ConfigFile = "./config.txt";
        public static string PluginPath = "./plugin/";
        public static string MoneyPath = "./money/";
        public static string HistoryPath;

        MiraiInfo minfo = new MiraiInfo();
        public Config config;
        public MoneyManager money = new MoneyManager();
        public HistoryManager history = new HistoryManager();

        Dictionary<string, Plugin> plugins = new Dictionary<string, Plugin>();
        

        public Dictionary<long, UserInfo> friends = new Dictionary<long, UserInfo>();
        public Dictionary<long, GroupInfo> groups = new Dictionary<long, GroupInfo>();


        public MainProcess()
        {

        }

        /// <summary>
        /// 初始化各组件
        /// </summary>
        /// <param name="_config"></param>
        /// <param name="_minfo"></param>
        public void Init(Config _config, MiraiInfo _minfo)
        {
            minfo = _minfo;
            config = _config;

            money = new MoneyManager();
            money.init(config["money"], MoneyPath);

            
            if(config["historySave"] == "1")
            {
                // 打开历史记录，不会是真的吧
                HistoryPath = Path.GetFullPath(config["historyPath"]);
                if (!Directory.Exists(HistoryPath)) Directory.CreateDirectory(HistoryPath);
                processOutput?.Invoke($"历史记录保存在 {HistoryPath} 里");
                history.init(HistoryPath);
            } else
            {
                processOutput?.Invoke($"历史记录不会有记录");
            }
            

            //LoadConfig();
            LoadMirai();

            LoadGroupAndFriendList();

            LoadPlugins();

        }

        public void LoadGroupAndFriendList()
        {
            try
            {
                // init data
                var us = MiraiHelper.getFriendList();
                foreach (var u in us)
                {
                    friends[u.qq] = u;
                }
                var gs = MiraiHelper.getGroupList();
                foreach (var g in gs)
                {
                    groups[g.id] = g;
                }
            }catch(Exception ex)
            {
                FileHelper.Log(ex);
            }

        }

        public void Exit()
        {
            foreach(var plugin in plugins.Values)
            {
                plugin.Dispose();
            }
            history.run = false;
            MiraiHelper.Exit();
        }


        public void LoadConfig()
        {
            processOutput?.Invoke($"开始加载配置文件");
            config = new Config(ConfigFile);
            config.load();

            processOutput?.Invoke($"配置文件加载完成");
        }

        public void LoadMirai()
        {
            processOutput?.Invoke($"开始启动Mirai http 连接");
            var res = MiraiHelper.Link(minfo.host, minfo.port, minfo.authKey, long.Parse(config["qq"]));
            if (!res)
            {
                processOutput?.Invoke($"连接Mirai 失败");
                return;
            }
            MiraiHelper.handleMiraiMsg += new HandleMiraiMsg(RecvMessage);
            MiraiHelper.handleAddFriend += new HandleEventAddFriend(accept);
            MiraiHelper.handleAddGroup += new HandleEventAddGroup(accept);
            MiraiHelper.handleMiraiRecall += new HandleMiraiMsg(RecvMessage);
            processOutput?.Invoke($"连接成功");

        }

        public void InitPlugin(Plugin p)
        {
            //processMessage += new HandleProcessMessage(p.HandleMessage);
            p.Init(this, config, PluginPath);
            plugins[p.PluginName] = p;
        }

        public void LoadPlugins()
        {
            processOutput?.Invoke($"开始加载插件");

            if (!Directory.Exists(PluginPath)) Directory.CreateDirectory(PluginPath);
            if (!Directory.Exists(MoneyPath)) Directory.CreateDirectory(MoneyPath);
           

            List<Plugin> plist = new List<Plugin>();
            
            plist.Add(new DicePlugin());
            plist.Add(new BilibiliPlugin());
            plist.Add(new DivinationPlugin());
            plist.Add(new TranslatePlugin());
            plist.Add(new RaceHorsePlugin());
            plist.Add(new ModePlugin());


            foreach (var p in plist)
            {
                processOutput?.Invoke($"开始加载插件 ： {p.PluginName}");
                InitPlugin(p);
            }

            processOutput?.Invoke($"插件加载完毕");

        }


        

        public void RecvMessage(Message msg)
        {
            if (config["debug"] == "1")
            {
                processOutput?.Invoke($"{msg.ToString()}");
            }
            if(config["historySave"] == "1")
            {
                history.saveMsg(msg.fromGroup, msg.from, msg.str);
            }
            if(msg.fromGroup <= 0)
            {
                // private
                config.setInt("playtimeprivate", config.getInt("playtimeprivate"));
            }
            else
            {
                // group.
                config.setInt("playtimegroup", config.getInt("playtimegroup"));
            }
            foreach(var plugin in plugins)
            {
                if(plugin.Value.HandleMessage(msg) == true)
                {
                    // finish.
                    break;
                }
            }
        }

        public void send(Message msg)
        {
            if (config["historySave"] == "1")
            {
                history.saveMsg(msg.toGroup, msg.to, msg.str);
            }
            MiraiHelper.sendMessage(msg);
        }

        /// <summary>
        /// 原路发回给消息发送者。
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="beginWithAt"></param>
        public void sendBack(Message msg, bool beginWithAt = false)
        {
            if(msg.fromGroup <= 0)
            {
                // from private.
                if (msg.to <= 0) msg.to = msg.from;
            }
            else
            {
                // from group.
                if (beginWithAt)
                {
                    msg.ats.Clear();
                    MessageAt at = new MessageAt(msg.from, $"@{msg.fromName}");
                }
                if (msg.to <= 0) msg.to = msg.from;
                if (msg.toGroup <= 0) msg.toGroup = msg.fromGroup;
            }
            send(msg);
        }
        public void accept(EventAddFriend e)
        {
            MiraiHelper.responseAddFriend(e, MiraiAddFriendRespose.Allow);
        }

        public void deny(EventAddFriend e)
        {
            MiraiHelper.responseAddFriend(e, MiraiAddFriendRespose.Deny);
        }

        public void accept(EventAddGroup e)
        {
            MiraiHelper.responseAddGroup(e, MiraiAddGroupResponse.Allow);
        }

        public void deny(EventAddGroup e)
        {
            MiraiHelper.responseAddGroup(e, MiraiAddGroupResponse.Deny);
        }



        public UserInfo getUserInfo(long qq)
        {
            if (friends.ContainsKey(qq))
            {
                return friends[qq];
            }
            return null;
        }

        public GroupInfo getGroupInfo(long group)
        {
            if (groups.ContainsKey(group))
            {
                return groups[group];
            }
            return null;
        }

        public void tick(long group, long qq, string msg)
        {
            MiraiHelper.setGroupMemberKick(group, qq, msg);
        }

        public void quit(long group)
        {
            MiraiHelper.setGroupQuit(group);
        }

        public void ban(long group, long qq, int time)
        {
            MiraiHelper.setGroupMemberMute(group, qq, true, time);
        }

        public void banCancel(long group, long qq)
        {
            MiraiHelper.setGroupMemberMute(group, qq, false);
        }

        public void ban(long group)
        {
            MiraiHelper.setGroupMute(group, true);
        }

        public void banCancel(long group)
        {
            MiraiHelper.setGroupMute(group, false);
        }

        public void setGroupInfo(GroupInfo info)
        {
            MiraiHelper.setGroupDetail(info);
        }

        public bool isAskme(Message msg)
        {
            bool res = false;
            if (msg.str.StartsWith(config["askname"]))
            {
                res = true;
            }
            if (msg.isAtMe(config.getInt("qq")))
            {
                res = true;
            }
            if (msg.fromGroup <= 0)
            {
                // private
                res = true;
            }

            if (res)
            {
                config.setInt("playtimegroup", config.getInt("playtimegroup") + 1);
            }

            return res;
        }

        public string getAskCmd(Message msg)
        {
            if (!isAskme(msg))
            {
                return "";
            }
            if (msg.fromGroup <= 0)
            {
                // private
                return msg.str;
            }
            if (msg.str.StartsWith(config["askname"]))
            {
                return msg.str.Substring(config["askname"].Length);
            }
            if (msg.isAtMe(config.getInt("qq")))
            {
                for (int i = 0; i < msg.ats.Count; i++) {
                    if(msg.ats[i].qq == config.getInt("qq"))
                    {
                        msg.ats.RemoveAt(i);
                        break;
                    }
                } 
                return msg.str;
            }

            return "";
        }

        public MoneyManager getMoneyManager()
        {
            return money;
        }

        public HistoryManager GetHistoryManager()
        {
            return history;
        }

        public void log(string msg)
        {
            processOutput?.Invoke($"{DateTime.Now.ToString("G")}:{msg}");
        }

        public int getGroupNum()
        {
            LoadGroupAndFriendList();
            return groups.Count;
        }

        public int getFriendNum()
        {
            
            return friends.Count;
        }
    }
}
