
using MeowMiraiLib.GenericModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace Kugua
{


    /// <summary>
    /// bot的配置文件管理模块
    /// </summary>
    public class Config
    {
        private static readonly Lazy<Config> instance = new Lazy<Config>(() => new Config());

        bool isLoaded;
        string configFile;
        
        public long ErrorTime = 0;
        public long UseTimeGroup
        {
            get
            {
                long sum = 0;
                foreach(var p in playgroups.Values)
                {
                    sum += p.UseTimes;
                }
                return sum;
            }
        }

        public long UseTimePrivate
        {
            get
            {
                long sum = 0;
                foreach (var p in players.Values)
                {
                    sum += p.UseTimes;
                }
                return sum;
            }
        }
        public DateTime StartTime { get; set; }
        
        public SystemInfo systemInfo = new SystemInfo();

        public AppConfigs App;
        public Dictionary<long, Player> players;
        public Dictionary<long, Playgroup> playgroups;

        // 以下数据动态从Mirai收集
        public Dictionary<long, QQFriend> qqfriends = new Dictionary<long, QQFriend>();
        public Dictionary<long, QQGroup> qqgroups = new Dictionary<long, QQGroup>();
        public Dictionary<long, QQGroupMember[]> qqgroupMembers = new Dictionary<long, QQGroupMember[]>();

        private Config()
        {
            
            isLoaded = false;
        }

        public static Config Instance => instance.Value;

        public bool Load()
        {
            if (isLoaded) return true;
            try
            {
                string configFileName = "config.json";
                configFile = $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}{configFileName}";
                Logger.Log($"配置文件路径是{configFile}");
                if (!File.Exists(configFile))
                {

                    Logger.Log($"配置文件不存在:{configFile}");
                    return false;
                    //CreateDefaultConfig();
                }
                string jsonString = File.ReadAllText(configFile);
                App = JsonConvert.DeserializeObject<AppConfigs>(jsonString);
                //SaveConfig();


                //var rootPath = ResourceFullPath(App.ResourcePath);
                if (!string.IsNullOrEmpty(ResourceRootPath) && !Directory.Exists(ResourceRootPath))
                {
                    Logger.Log($"新建路径{ResourceRootPath}");
                    Directory.CreateDirectory(ResourceRootPath);
                    
                }

                string path = ResourceFullPath("Player");
                if (!File.Exists(path))
                {
                    Logger.Log($"新建空白用户资料列表，路径是{path}");
                    File.WriteAllText(path, "{}");
                }
                jsonString = File.ReadAllText(path);
                players = JsonConvert.DeserializeObject<Dictionary<long, Player>>(jsonString);
                if (players != null)
                {
                    Logger.Log($"从{path}读取了{players.Count}名用户资料");
                    foreach(var p in players)
                    {
                        if (p.Value.Tags == null) p.Value.Tags = new HashSet<string>();
                    }
                }

                path = ResourceFullPath("Playgroup");
                if (!File.Exists(path))
                {
                    Logger.Log($"新建空白群组资料列表，路径是{path}");
                    File.WriteAllText(path, "{}");
                }
                jsonString = File.ReadAllText(path);
                playgroups = JsonConvert.DeserializeObject<Dictionary<long, Playgroup>>(jsonString);
                if (playgroups != null)
                {
                    Logger.Log($"从{path}读取了{playgroups.Count}个群组资料");
                    foreach (var p in playgroups)
                    {
                        if (p.Value.Tags == null) p.Value.Tags = new HashSet<string>();
                    }
                }



            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }

        

            isLoaded = true;
            return true;
        }

        /// <summary>
        /// 以分隔符/结尾的资源文件夹绝对路径
        /// </summary>
        public string ResourceRootPath
        {
            get
            {
                if (App != null && !string.IsNullOrWhiteSpace(App.ResourcePath))
                {
                    return $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}{App.ResourcePath}{Path.DirectorySeparatorChar}";
                }
                return $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}";

            }

        }


        public string ResourceFullPath(string Name)
        {
            try
            {
                if (App != null && App.Resources != null)
                {
                    if (App.Resources.TryGetValue(Name, out Resource res))
                    {
                        string fullPath = $"{ResourceRootPath}{res.Path.Replace("/","\\")}";
                        //string fullFilePath = System.IO.Path.GetFullPath(fullPath);
                        if (res.Type == ResourceType.Path && !Directory.Exists(fullPath))
                        {
                            Logger.Log($"新建资源文件夹，路径是{fullPath}");
                            Directory.CreateDirectory(fullPath);
                        }
                        if (res.Type==ResourceType.File && !File.Exists(fullPath))
                        {
                            Logger.Log($"资源文件不存在，路径是{fullPath}");
                        }
                        return fullPath;
                    }
                    else
                    {
                        string tmpFullpath = $"{ResourceRootPath}{Name}";
                        return tmpFullpath;
                        //Logger.Log($"未找到资源 '{Name}' 。请在{configFile} 中配置！已返回{tmpFullpath}");
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.Log(ex);
            }

            return ResourceRootPath;
        }

        public bool Save()
        {
            try
            {
                //string jsonString = JsonConvert.SerializeObject(App, Formatting.Indented);
                //File.WriteAllText(configFile, jsonString);

                string path = ResourceFullPath("Player");
                var jsonString = JsonConvert.SerializeObject(players, Formatting.Indented);
                File.WriteAllText(path, jsonString);


                path = ResourceFullPath("Playgroup");
                jsonString = JsonConvert.SerializeObject(playgroups, Formatting.Indented);
                File.WriteAllText(path, jsonString);

                

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex.Message}\r\n{ex.StackTrace}");
                return false;
            }
        }

        void CreateDefaultConfig()
        {
            try
            {
                AppConfigs defaultSettings = new AppConfigs
                {
                    Version = "0.1.0",
                };

                string jsonString = JsonConvert.SerializeObject(defaultSettings, Formatting.Indented);
                File.WriteAllText(configFile, jsonString);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            
        }


        /// <summary>
        /// 查看我自己的QQ
        /// 这个函数就是为了外面调用起来简短点
        /// </summary>
        /// <returns></returns>
        public long BotQQ
        {
            get
            {
                if (App != null && App.Avatar != null) return App.Avatar.myQQ;

                return 0;
            }
        }

        /// <summary>
        /// 调用bot时喊的名字
        /// 这个函数就是为了外面调用起来简短点
        /// </summary>
        /// <returns></returns>
        public string BotName
        {
            get
            {
                if (App != null && App.Avatar != null) return App.Avatar.askName;

                return null;
            }
            
        }



        #region 用户Player相关
        public Player UserInfo(long id)
        {
            if (players.TryGetValue(id, out Player p))
            {
                return p;
            }
            else
            {
                // create new
                var p2 = new Player
                {
                    Name = "",
                    Type = PlayerType.Normal,
                    UseTimes = 0,
                    Tags = new HashSet<string>(),
                };
                players.Add(id, p2);
                return p2;
            }
        }



        /// <summary>
        /// 判断是否回复特定qq号的消息
        /// 根据personlevel配置来作判断
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool AllowPlayer(long id)
        {
            try
            {
                
                if (id == App.Avatar.myQQ) return false;   // 不许套娃
                var u = UserInfo(id);
                if (u.Type == PlayerType.Blacklist || u.Is("屏蔽")) return false;
                
                // 默认皆可应答
                return true;
                
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            
            return false;
        }

        public bool UserHasAdminAuthority(long userId)
        {
            if (userId <= 0) return false;
            if (userId == App.Avatar.adminQQ) return true;
            var user = UserInfo(userId);
            if (user.Is("管理员")) return true;
            if (user.Type == PlayerType.Admin) return true;
            return false;
        }


        #endregion




        #region 群组Group相关
        public Playgroup GroupInfo(long id)
        {
            if(playgroups.TryGetValue(id, out Playgroup g))
            {
                return g;
            }
            else
            {
                // create new
                var p = new Playgroup
                {
                    Name = "",
                    Type = PlaygroupType.Normal,
                    Tags = new HashSet<string>(),
                    UseTimes = 0
                };
                playgroups.Add(id, p);
                return p;
            }
        }

        /// <summary>
        /// 判断是否回复特定qq号的消息
        /// 根据personlevel配置来作判断
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool AllowGroup(long id)
        {
            try
            {

                var g = GroupInfo(id);
                if (g.Type == PlaygroupType.Blacklist || g.Is("屏蔽")) return false;

                // 默认皆可应答
                return true;

            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            
            return false;
        }

        public bool GroupHasAdminAuthority(long groupId)
        {
            if (groupId <= 0) return false;
            if (groupId == App.Avatar.adminGroup) return true;
            var group = GroupInfo(groupId);
            if (group.Is("测试")) return true;
            if (group.Type == PlaygroupType.Test) return true;
            return false;
        }



        #endregion




    }

}
