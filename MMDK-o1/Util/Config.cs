
using MeowMiraiLib.GenericModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace MMDK.Util
{
    public interface Mod
    {
        /// <summary>
        /// Mod初始化，只调用一次
        /// </summary>
        /// <param name="args">可选的传入参数</param>
        /// <returns></returns>
        public bool Init(string[] args);


        /// <summary>
        /// Mod退出清理，在bot关闭时调用一次
        /// </summary>
        public void Exit();

        /// <summary>
        /// Mod的文本处理接口
        /// </summary>
        /// <param name="userId">用户QQ</param>
        /// <param name="groupId">群QQ</param>
        /// <param name="message">输入的文本内容</param>
        /// <param name="results">传出返回文本序列</param>
        /// <returns>返回是否已处理，true表示该模块已经对信息进行了处理并截断后续其他模块的处理流程</returns>
        public bool HandleText(long userId, long groupId, string message, List<string> results);

        
    }

    public interface ModWithMirai
    {
        /// <summary>
        /// 直连Mirai，不从函数回传
        /// 实现该模块时，代码需要操作Mirai库的Api
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        public void ReceiveMiraiMessage(MeowMiraiLib.Client client, MeowMiraiLib.Msg.Type.Message msg);
    }

    public class AppConfigs
    {
        public string Version { get; set; }
        public DateTime LateUpdated { get; set; }
        public string ResourcePath { get; set; }

        public AvatarConfigs Avatar { get; set; }
        public IOConfigs IO { get; set; }
        public LogConfigs Log { get; set; }

        public Dictionary<string, Resource> Resources { get; set; }
    }

    public enum ResourceType
    {
        File,
        Path,
    }

    public class Resource
    {
        //public string Name { get; set; }
        public ResourceType Type { get; set; }
        public string Path { get; set; }
    }

    public class IOConfigs
    {
        public bool MiraiRun { get; set; }
        public string MiraiWS { get; set; }
        public int MiraiPort { get; set; }

        public bool BilibiliRun { get; set; }

        public string BKeySecret { get; set; }
        public string BKeyId { get; set; }
        /// <summary>
        /// 应用ID
        /// </summary>
        public string BAppId { get; set; }
        /// <summary>
        /// 身份码
        /// </summary>
        public string BUId { get; set; }



    }

    /// <summary>
    /// 个性项
    /// </summary>
    public class AvatarConfigs
    {
        public long myQQ { get; set; }
        public string myName { get; set; }
        public string askName { get; set; }
        /// <summary>
        /// bot应答方式
        /// 0  全领域静默
        /// 1  仅供测试
        /// 2  仅供私聊和测试
        /// 3  全部开放
        /// </summary>
        public int answerState { get; set; }
        /// <summary>
        /// 测试人员qq
        /// </summary>
        public long adminQQ { get; set; }
        /// <summary>
        /// 测试群
        /// </summary>
        public long adminGroup { get; set; }
    }

    /// <summary>
    /// 运行状态记录
    /// </summary>
    public class LogConfigs
    {
        public DateTime StartTime { get; set; }
        public long playTimePrivate { get; set; }

        public long playTimeGroup { get; set; }

        public long beginTimes { get; set; }

        public long errTimes { get; set; }

        public long numGroup { get; set; }


    }

    public enum PlayerType
    {
        Normal,
        Blacklist,
        Admin,
    }
    public class Player
    {
        //public long Id { get; set; }
        public string Name { get; set; }
        public string Mark { get; set; }
        public PlayerType Type { get; set; }
        public long UseTimes { get; set; }



        public string Tag {  get; set; }



        public long Money { get; set; }
        public DateTime LastSignTime { get; set; }
        public long SignTimes {  get; set; }

        public void SetTag(string tag)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tag)) return;
                if (!Is(tag))
                {
                    if (string.IsNullOrWhiteSpace(Tag))Tag = $"{tag},";
                    else Tag += $"{tag},";
                    
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
            }
        }
        public void DeleteTag(string tag)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tag)) return;
                if (Is(tag)) Tag = Tag.Replace($"{tag},", "");

            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
            }
        }

        public bool Is(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return false;
            if (!string.IsNullOrWhiteSpace(Tag) && Tag.Contains($"{tag},")) return true;
            return false;
        }
    }

    public enum PlaygroupType
    {
        Normal,
        Blacklist,
        Test,
    }

    public class Playgroup
    {
        //public long id { get; set; }
        public string Name { get; set; }
        public PlaygroupType Type {  get; set; }

        public long UseTimes { get; set; }



        public string Tag { get; set; }



        public void SetTag(string tag)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tag)) return;
                tag = tag.Trim();
                if (string.IsNullOrWhiteSpace(Tag)) Tag = $"{tag},";
                else if (!Tag.Contains(tag)) Tag += $"{tag},";


                // Save();
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
            }
        }
        public void DeleteTag(string tag)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tag)) return;
                tag = tag.Trim();
                if (!string.IsNullOrWhiteSpace(Tag) && Tag.Contains(tag)) Tag = Tag.Replace($"{tag},", "");
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
            }
        }
        public bool Is(string tag)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tag)) return false;
                return Tag.Contains($"{tag},");
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
            }
            return false;
        }
    }


    public class Config
    {
        private static readonly Lazy<Config> instance = new Lazy<Config>(() => new Config());

        bool isLoaded;
        string configFile;

        public AppConfigs App;
        public Dictionary<long, Player> players;
        public Dictionary<long, Playgroup> playgroups;
        // 以下两个数据动态从Mirai收集
        public Dictionary<long, QQFriend> friends = new Dictionary<long, QQFriend>();
        public Dictionary<long, QQGroup> groups = new Dictionary<long, QQGroup>();
        public Dictionary<long, QQGroupMember[]> groupMembers = new Dictionary<long, QQGroupMember[]>();

        private Config()
        {
            configFile = $"{Directory.GetCurrentDirectory()}/config.json";
            isLoaded = false;
        }

        public static Config Instance => instance.Value;

        public bool Load()
        {
            if (isLoaded) return true;
            try
            {
                if (!File.Exists(configFile))
                {
                    Logger.Instance.Log($"新建配置文件，路径是{configFile}", LogType.Debug);
                    CreateDefaultConfig();
                }
                string jsonString = File.ReadAllText(configFile);
                App = JsonConvert.DeserializeObject<AppConfigs>(jsonString);
                //SaveConfig();


                string rootPath = Path.GetDirectoryName(App.ResourcePath);
                if (!string.IsNullOrEmpty(rootPath) && !Directory.Exists(rootPath))
                {
                    Logger.Instance.Log($"新建路径{rootPath}", LogType.Debug);
                    Directory.CreateDirectory(rootPath);
                    
                }

                string path = ResourceFullPath("Player");
                if (!File.Exists(path))
                {
                    Logger.Instance.Log($"新建空白用户资料列表，路径是{path}", LogType.Debug);
                    File.WriteAllText(path, "{}");
                }
                jsonString = File.ReadAllText(path);
                players = JsonConvert.DeserializeObject<Dictionary<long, Player>>(jsonString);
                if (players != null)
                {
                    Logger.Instance.Log($"从{path}读取了{players.Count}名用户资料", LogType.Debug);
                }

                path = ResourceFullPath("Playgroup");
                if (!File.Exists(path))
                {
                    Logger.Instance.Log($"新建空白群组资料列表，路径是{path}", LogType.Debug);
                    File.WriteAllText(path, "{}");
                }
                jsonString = File.ReadAllText(path);
                playgroups = JsonConvert.DeserializeObject<Dictionary<long, Playgroup>>(jsonString);
                if (playgroups != null)
                {
                    Logger.Instance.Log($"从{path}读取了{playgroups.Count}个群组资料", LogType.Debug);
                }



            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
                return false;
            }

        

            isLoaded = true;
            return true;
        }

        

        public string ResourceFullPath(string Name)
        {
            try
            {
                if (App != null && App.Resources != null)
                {
                    if (App.Resources.TryGetValue(Name, out Resource res))
                    {
                        return $"{Directory.GetCurrentDirectory()}/{App.ResourcePath}/{res.Path}";
                    }
                    else
                    {
                        Logger.Instance.Log($"未找到键 '{Name}' 的对象。", LogType.Debug);
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.Instance.Log(ex);
            }

            return $"{Directory.GetCurrentDirectory()}/{App.ResourcePath}";
        }

        public bool Save()
        {
            try
            {
                string jsonString = JsonConvert.SerializeObject(App, Formatting.Indented);
                File.WriteAllText(configFile, jsonString);

                string path = ResourceFullPath("Player");
                jsonString = JsonConvert.SerializeObject(players, Formatting.Indented);
                File.WriteAllText(path, jsonString);


                path = ResourceFullPath("Playgroup");
                jsonString = JsonConvert.SerializeObject(playgroups, Formatting.Indented);
                File.WriteAllText(path, jsonString);


                return true;
            }
            catch (Exception ex)
            {
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
                Logger.Instance.Log(ex);
            }
            
        }



        /// <summary>
        /// 看看这句话是不是在at我，也就是句首有无我的昵称。
        /// 如果有，就返回true
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool isAskMe(string msg)
        {
            if (msg != null && msg.Length > 0 && msg.StartsWith(App.Avatar.askName))
            {
                //msg = msg.Substring(appConfig.Avatar.askName.Length);
                return true;
            }
            return false;
        }


        #region 用户Player相关
        public Player GetPlayerInfo(long id)
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
                    Tag= "",
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
                var u = GetPlayerInfo(id);
                if (u.Type == PlayerType.Blacklist || u.Is("黑名单")) return false;
                

                return true;
                
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
            }
            // 默认皆可应答
            return false;
        }
        #endregion





        #region 群组Group相关
        public Playgroup GetGroupInfo(long id)
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
                    Tag = "",
                    UseTimes = 0
                };
                playgroups.Add(id, p);
                return p;
            }
        }


        #endregion


       

    }

}
