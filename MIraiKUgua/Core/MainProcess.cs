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
    public delegate void HandleProcessMessage(Message msg);

    public interface IGlobalFunc {
        bool isAskme(Message msg);
        string getAskCmd(Message msg);
        void send(Message msg);
        void sendBack(Message msg, bool beginWithAt = false);
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

    }
    class MainProcess : IGlobalFunc
    {
        public event processOutputHandler processOutput;
        public event HandleProcessMessage processMessage;

        public static string ConfigFile = "./config.txt";
        public static string PluginPath = "./plugin/";
        public static string MoneyPath = "./money/";

        public Config config;
        public MoneyManager money;
        List<Plugin> plugins = new List<Plugin>();

       


        public Dictionary<long, UserInfo> users = new Dictionary<long, UserInfo>();
        public Dictionary<long, GroupInfo> groups = new Dictionary<long, GroupInfo>();


        public MainProcess()
        {

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
            var res = MiraiHelper.Link(config["host"], int.Parse(config["port"]), config["key"], long.Parse(config["qq"]));
            MiraiHelper.handleMiraiMsg += new HandleMiraiMsg(RecvMessage);
            MiraiHelper.handleAddFriend += new HandleEventAddFriend(accept);
            MiraiHelper.handleAddGroup += new HandleEventAddGroup(accept);
            MiraiHelper.handleMiraiRecall += new HandleMiraiMsg(RecvMessage);
            processOutput?.Invoke($"连接成功");

        }

        public void InitPlugin(Plugin p)
        {
            processMessage += new HandleProcessMessage(p.HandleMessage);
            p.Init(this, config, PluginPath);
        }

        public void LoadPlugins()
        {
            processOutput?.Invoke($"开始加载插件");

            if (!Directory.Exists(PluginPath)) Directory.CreateDirectory(PluginPath);
            if (!Directory.Exists(MoneyPath)) Directory.CreateDirectory(MoneyPath);
            


            InitPlugin(new ModePlugin());

            InitPlugin(new DicePlugin());

            InitPlugin(new BilibiliPlugin());



            processOutput?.Invoke($"插件加载完毕");

        }


        public void Init(Config _config)
        {

            config = _config;
            money = new MoneyManager();
            money.init(config["money"], MoneyPath);
            //LoadConfig();
            LoadMirai();


            // init data
            var us = MiraiHelper.getFriendList();
            foreach(var u in us)
            {
                users[u.qq] = u;
            }
            var gs = MiraiHelper.getGroupList();
            foreach(var g in gs)
            {
                groups[g.id] = g;
            }

            LoadPlugins();

        }

        public void RecvMessage(Message msg)
        {
            processOutput?.Invoke($"{msg.ToString()}");
            processMessage(msg);
        }

        public void send(Message msg)
        {
            MiraiHelper.sendMessage(msg);
        }
        public void sendBack(Message msg, bool beginWithAt = false)
        {
            if (beginWithAt)
            {
                msg.ats.Clear();
                MessageAt at = new MessageAt(msg.from, $"@{msg.fromName}");
            }
            if (msg.to == 0) msg.to = msg.from;
            if (msg.toGroup == 0) msg.toGroup = msg.fromGroup;

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
            if (users.ContainsKey(qq))
            {
                return users[qq];
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
    }
}
