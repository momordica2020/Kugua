
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
        #region 单例
        private static readonly Lazy<Config> instance = new Lazy<Config>(() => new Config());
        private Config()
        {

            isLoaded = false;
        }

        public static Config Instance => instance.Value;

        #endregion
        
        
        bool isLoaded;
        object loadMutex = new object();
        
        const string configFileName = "config.json";
        string configFile;

        public long ErrorNum = 0;

        /// <summary>
        /// 用户总共群内调用次数
        /// </summary>
        public long UseTimeGroup
        {
            get
            {
                long sum = 0;
                foreach(var p in groups.Values)
                {
                    sum += p.UseTimes;
                }
                return sum;
            }
        }

        /// <summary>
        /// 用户总共私聊次数
        /// </summary>
        public long UseTimePrivate
        {
            get
            {
                long sum = 0;
                foreach (var p in users.Values)
                {
                    sum += p.UseTimes;
                }
                return sum;
            }
        }


        /// <summary>
        /// bot启动时间
        /// </summary>
        public DateTime StartTime { get; set; }


        /// <summary>
        /// 操作系统信息
        /// </summary>
        public SystemInfo systemInfo = new SystemInfo();

        /// <summary>
        /// 基本配置项信息
        /// </summary>
        public AppConfigs App;

        /// <summary>
        /// 个人（私聊）用户
        /// </summary>
        public Dictionary<string, Player> users;

        /// <summary>
        /// 用户群
        /// </summary>
        public Dictionary<string, Playgroup> groups;

   


        /// <summary>
        /// 自动从配置文件加载
        /// </summary>
        /// <returns></returns>
        public bool Load()
        {
            if (isLoaded) return true;
            lock (loadMutex)
            {
                if (isLoaded) return true;
                try
                {
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
                    if (!string.IsNullOrEmpty(RootPath) && !Directory.Exists(RootPath))
                    {
                        Logger.Log($"新建路径{RootPath}");
                        Directory.CreateDirectory(RootPath);

                    }

                    string path = FullPath("Player");
                    if (!File.Exists(path))
                    {
                        Logger.Log($"新建空白用户资料列表，路径是{path}");
                        File.WriteAllText(path, "{}");
                    }
                    jsonString = File.ReadAllText(path);
                    users = JsonConvert.DeserializeObject<Dictionary<string, Player>>(jsonString);
                    if (users != null)
                    {
                        Logger.Log($"从{path}读取了{users.Count}名用户资料");
                        foreach (var p in users)
                        {
                            if (p.Value.Tags == null) p.Value.Tags = new HashSet<string>();
                        }
                    }
                    else
                    {
                        users = new Dictionary<string, Player>();
                    }

                    path = FullPath("Playgroup");
                    if (!File.Exists(path))
                    {
                        Logger.Log($"新建空白群组资料列表，路径是{path}");
                        File.WriteAllText(path, "{}");
                    }
                    jsonString = File.ReadAllText(path);
                    groups = JsonConvert.DeserializeObject<Dictionary<string, Playgroup>>(jsonString);
                    if (groups != null)
                    {
                        Logger.Log($"从{path}读取了{groups.Count}个群组资料");
                        foreach (var p in groups)
                        {
                            if (p.Value.Tags == null) p.Value.Tags = new HashSet<string>();
                        }
                    }
                    else
                    {
                        groups = new Dictionary<string, Playgroup>();
                    }



                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    return false;
                }

                // 一些合规判断
                if (string.IsNullOrWhiteSpace(App.Version)) App.Version = "v0.0.1";
                if (App.Avatar == null)
                {
                    return false;
                }
                
                if (string.IsNullOrWhiteSpace(App.Avatar.myQQ.ToString())) App.Avatar.myQQ = "";

                StartTime = DateTime.Now;




                isLoaded = true;
                return true;
            }
        }

        /// <summary>
        /// 以分隔符/结尾的资源文件夹绝对路径
        /// </summary>
        public string RootPath
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

        /// <summary>
        /// 读取资源文件的完整路径
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public string FullPath(string Name)
        {
            try
            {
                if (App != null && App.Resources != null)
                {
                    if (App.Resources.TryGetValue(Name, out Resource res))
                    {
                        string fullPath = $"{RootPath}{res.Path.Replace("/","\\")}";
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
                        // 未配置该键值，则返回从资源根路径开始的该名称作为完整路径
                        string tmpFullpath = $"{RootPath}{Name}";
                        return tmpFullpath;
                        //Logger.Log($"未找到资源 '{Name}' 。请在{configFile} 中配置！已返回{tmpFullpath}");
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.Log(ex);
            }

            return RootPath;
        }



        /// <summary>
        /// 储存用户数据。TODO改成sqlite
        /// </summary>
        /// <returns></returns>
        public bool Save()
        {
            try
            {
                //string jsonString = JsonConvert.SerializeObject(App, Formatting.Indented);
                //File.WriteAllText(configFile, jsonString);

                string path = FullPath("Player");
                var jsonString = JsonConvert.SerializeObject(users, Formatting.Indented);
                File.WriteAllText(path, jsonString);


                path = FullPath("Playgroup");
                jsonString = JsonConvert.SerializeObject(groups, Formatting.Indented);
                File.WriteAllText(path, jsonString);

                

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex.Message}\r\n{ex.StackTrace}");
                return false;
            }
        }


        /// <summary>
        /// 查看我自己的QQ
        /// 这个函数就是为了外面调用起来简短点
        /// </summary>
        /// <returns></returns>
        public string BotQQ
        {
            get
            {
                if (App != null && App.Avatar != null) return App.Avatar.myQQ;

                return "";
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
        public Player? UserInfo(string id)
        {
            if (id == null || string.IsNullOrWhiteSpace(id)) return null;
            if (users.TryGetValue(id, out Player p))
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
                users.Add(id, p2);
                return p2;
            }
        }



        /// <summary>
        /// 判断是否允许回复特定qq号的消息
        /// 根据personlevel配置来作判断
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool AllowPlayer(string id)
        {
            try
            {
                
                //if (id == App.Avatar.myQQ) return false;   // 不许套娃
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

        /// <summary>
        /// 判断用户是否有管理员权限
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool UserHasAdminAuthority(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return false;
            if (userId == App.Avatar.adminQQ) return true;
            var user = UserInfo(userId);
            if (user.Is("管理员")) return true;
            if (user.Type == PlayerType.Admin) return true;
            return false;
        }


        #endregion




        #region 群组Group相关


        /// <summary>
        /// 获取用户信息对象。注意，如果id空，返回null。如果id不存在，会创建一个新的对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Playgroup? GroupInfo(string id)
        {
            if (id == null || string.IsNullOrWhiteSpace(id)) return null;
            if (groups.TryGetValue(id, out Playgroup g))
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
                groups.Add(id, p);
                return p;
            }
        }

        /// <summary>
        /// 判断是否回复特定qq号的消息
        /// 根据personlevel配置来作判断
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool AllowGroup(string id)
        {
            try
            {

                var g = GroupInfo(id);
                if (g == null) return true;
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


        /// <summary>
        /// 判断是否处于测试群
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public bool GroupHasAdminAuthority(string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId)) return false;
            if (groupId == App.Avatar.adminGroup) return true;
            var group = GroupInfo(groupId);
            if (group == null) return false;
            if (group.Is("测试")) return true;
            if (group.Type == PlaygroupType.Test) return true;
            return false;
        }



        #endregion




    }

}
