using MMDK.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace MMDK.Core
{
    public class Config
    {
        public string file;
        public Dictionary<string, string> configs = new Dictionary<string, string>();

        public static string DataPath;
        public static string configSSTVFile = "configsstv.jpg";
        public static string groupLevelListFile = "level_group.txt";
        public static string personLevelListFile = "level_person.txt";
        public Dictionary<long, List<string>> groupLevel;
        public Dictionary<long, List<string>> personLevel;
        public List<string[]> sstvs;

        // online info
        public DateTime thisStartTime = DateTime.Now;

        public string this[string key]
        {
            get
            {
                if (configs.ContainsKey(key)) return configs[key];
                else return "";
            }
            set
            {
                configs[key] = value;
                //if(!configs.ContainsKey(key))
            }
        }

        public Config(string file, string _dataPath ="./")
        {
            this.file = file;
            DataPath = Path.GetFullPath(_dataPath);
            if (!Directory.Exists(DataPath))
            {
                Directory.CreateDirectory(DataPath);
            }
            

            load();
            loadSpecialTag();
        }

        public void load()
        {
            configs = FileHelper.readDict(file, new char[] { '=' });
            
        }

        void loadSpecialTag()
        {
            
            try
            {
                groupLevel = new Dictionary<long, List<string>>();
                var lines = FileHelper.readLines($"{DataPath}{groupLevelListFile}");
                foreach (var line in lines)
                {
                    var items = line.Trim().Split('\t');
                    if (items.Length >= 2)
                    {
                        groupLevel[long.Parse(items[0])] = items[1].Trim().Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    }
                }

                personLevel = new Dictionary<long, List<string>>();
                lines = FileHelper.readLines($"{DataPath}{personLevelListFile}");
                foreach (var line in lines)
                {
                    var items = line.Trim().Split('\t');
                    if (items.Length >= 2)
                    {
                        personLevel[long.Parse(items[0])] = items[1].Trim().Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    }
                }

                sstvs = new List<string[]>();
                lines = FileHelper.readLines($"{DataPath}{configSSTVFile}");
                foreach (var line in lines)
                {
                    var items = line.Split('\t');
                    if (items.Length >= 2)
                    {
                        sstvs.Add(new string[] { items[0], items[1] });
                    }
                    else if (items.Length >= 1)
                    {
                        sstvs.Add(new string[] { items[0], "" });
                    }
                }

            }
            catch (Exception ex)
            {
                FileHelper.Log(ex);
            }
        }

        public void save()
        {
            FileHelper.writeDict(file, configs, '=');


            try
            {
                StringBuilder sb = new StringBuilder();
                foreach (var pair in groupLevel)
                {
                    sb.AppendLine($"{pair.Key}\t{string.Join("，", pair.Value)}");
                }
                FileHelper.writeText(DataPath + groupLevelListFile, sb.ToString());

                sb = new StringBuilder();
                foreach (var pair in personLevel)
                {
                    sb.AppendLine($"{pair.Key}\t{string.Join("，", pair.Value)}");
                }
                FileHelper.writeText(DataPath + personLevelListFile, sb.ToString());
            }
            catch
            {

            }
        }

        public int getInt(string key)
        {
            int res;
            int.TryParse(configs[key], out res);
            return res;
        }

        public void setInt(string key, int val)
        {
            configs[key] = val.ToString();
        }


        public bool groupIs(long group, string state)
        {
            try
            {
                if (groupLevel.ContainsKey(group)) return groupLevel[group].Contains(state);
                else return false;
            }
            catch (Exception ex)
            {
                FileHelper.Log(ex);
            }
            return false;
        }

        public bool groupIsNot(long group, string state)
        {
            return !groupIs(group, state);
        }



        public void groupAddTag(long group, string state)
        {
            try
            {
                if (!groupLevel.ContainsKey(group)) groupLevel[group] = new List<string>();
                if (!groupLevel[group].Contains(state.Trim()))
                {
                    groupLevel[group].Add(state.Trim());
                    save();
                }
            }
            catch (Exception ex)
            {
                FileHelper.Log(ex);
            }

        }

        public void groupDeleteTag(long group, string state)
        {
            try
            {
                if (!groupLevel.ContainsKey(group)) return;
                groupLevel[group].Remove(state.Trim());
                save();
            }
            catch (Exception ex)
            {
                FileHelper.Log(ex);
            }
        }

        public void personAddTag(long user, string state)
        {
            try
            {
                if (!personLevel.ContainsKey(user)) personLevel[user] = new List<string>();
                if (!personLevel[user].Contains(state.Trim()))
                {
                    personLevel[user].Add(state.Trim());
                    save();
                }
            }
            catch (Exception ex)
            {
                FileHelper.Log(ex);
            }

        }

        public void personDeleteTag(long user, string state)
        {
            try
            {
                if (!personLevel.ContainsKey(user)) return;
                personLevel[user].Remove(state.Trim());
                save();
            }
            catch (Exception ex)
            {
                FileHelper.Log(ex);
            }
        }

        public bool personIs(long user, string state)
        {
            try
            {
                if (personLevel.ContainsKey(user)) return personLevel[user].Contains(state);
            }
            catch (Exception ex)
            {
                FileHelper.Log(ex);
            }
            return false;
        }

        /// <summary>
        /// 判断是否回复特定qq号的消息
        /// 根据personlevel配置来作判断
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool allowuser(long user)
        {
            try
            {
                if (!personIs(user, "屏蔽")) return true;
                else
                {
                    if (personLevel.ContainsKey(user))
                    {
                        for (int i = 0; i < personLevel[user].Count; i++)
                        {
                            if (personLevel[user][i].StartsWith("有限："))
                            {
                                try
                                {
                                    var paras = personLevel[user][i].Substring(3).Trim().Split(' ');
                                    if (paras.Length >= 3)
                                    {
                                        long lefttime, fulltime, lastts;
                                        long.TryParse(paras[0], out lefttime);
                                        long.TryParse(paras[1], out fulltime);
                                        long.TryParse(paras[2], out lastts);


                                        if (lefttime <= 0)
                                        {
                                            // try time reset
                                            DateTime lasttime = new DateTime(lastts);
                                            if (DateTime.Now - lasttime > TimeSpan.FromMinutes(60))
                                            {
                                                lastts = DateTime.Now.Ticks;
                                                lefttime = fulltime;
                                            }
                                        }

                                        if (lefttime > 0)
                                        {
                                            lefttime -= 1;
                                            personLevel[user][i] = $"有限：{lefttime} {fulltime} {lastts}";
                                            return true;
                                        }
                                    }

                                }
                                catch (Exception ex)
                                {
                                    FileHelper.Log(ex);
                                }
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FileHelper.Log(ex);
            }

            return false;
        }

        /// <summary>
        /// 去除敏感词
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string replaceSSTV(string str)
        {
            try
            {
                foreach (var w in sstvs)
                {
                    str = str.Replace(w[0], w[1]);
                }
            }
            catch { }
            return str;
        }

    }
}
