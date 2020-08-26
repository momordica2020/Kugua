using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Data.SqlTypes;

namespace MMDK.Util
{
    /// <summary>
    /// 本地文件管理模块
    /// </summary>
    class FileHelper
    {
        public static string logPath = "log.txt";
        public static Encoding encoding = Encoding.UTF8;

        public static string readText(string file)
        {
            try
            {
                return File.ReadAllText(file, encoding);
            }catch(Exception ex)
            {
                Log(ex);
            }
            return "";
        }


        public static string[] readLines(string file)
        {
            List<string> res = new List<string>();

            try
            {
                using(FileStream fs = new FileStream(file, FileMode.OpenOrCreate))
                {
                    using(StreamReader sr=new StreamReader(fs, encoding))
                    {
                        while (!sr.EndOfStream)
                        {
                            string r = sr.ReadLine().Trim();
                            if (!string.IsNullOrWhiteSpace(r))
                            {
                                res.Add(r);
                            }
                            
                        }
                        
                    }
                }
            }
            catch(Exception ex)
            {
                Log(ex);
            }

            return res.ToArray();
        }

        public static Dictionary<string, string> readDict(string file, char[] spliters = null)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            try
            {
                if (spliters == null) spliters = new char[] { ' ', '\t' };
                string[] lines = readLines(file);
                foreach (var line in lines)
                {
                    var items = line.Split(spliters, StringSplitOptions.RemoveEmptyEntries);
                    if (items.Length >= 2)
                    {
                        res[items[0].Trim()] = string.Join(" ", items.Skip(1)).Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex);
            }


            return res;
        }


        public static Dictionary<string, string[]> readDictArray(string file, char[] spliters = null)
        {
            Dictionary<string, string[]> res = new Dictionary<string, string[]>();
            try
            {
                if (spliters == null) spliters = new char[] { ' ', '\t' };
                string[] lines = readLines(file);
                foreach (var line in lines)
                {
                    var items = line.Split(spliters, StringSplitOptions.RemoveEmptyEntries);
                    if (items.Length >= 2)
                    {
                        res[items[0].Trim()] = items.Skip(1).ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex);
            }


            return res;
        }

        public static void writeText(string file, string text)
        {
            try
            {
                File.WriteAllText(file, text, encoding);
            }
            catch (Exception ex)
            {
                Log(ex);
            }
        }

        public static void appendText(string file, string text)
        {
            try
            {
                File.AppendAllText(file, text, encoding);
            }
            catch (Exception ex)
            {
                Log(ex);
            }
        }

        public static void writeLines(string file, IEnumerable<string> lines)
        {
            try
            {
                File.WriteAllLines(file, lines, encoding);
            }
            catch(Exception ex)
            {
                Log(ex);
            }
        }

        public static void writeDict(string file, Dictionary<string, string> dict, char spliter = '\t')
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in dict)
                {
                    sb.Append($"{item.Key}{spliter}{item.Value}\r\n");
                }
                File.WriteAllText(file, sb.ToString(), encoding);
            }
            catch (Exception ex)
            {
                Log(ex);
            }
        }
        
        public static void writeDictArray(string file, Dictionary<string, string[]> dicts, char spliter = '\t')
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in dicts)
                {
                    sb.Append($"{item.Key}{spliter}{string.Join($"{spliter}", item.Value)}\r\n");
                }
                File.WriteAllText(file, sb.ToString(), encoding);
            }
            catch (Exception ex)
            {
                Log(ex);
            }

        }


        public static void Log(string text)
        {
            try
            {
                File.AppendAllText(logPath, $"[{DateTime.Now:G}]{text}\r\n");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        public static void Log(Exception ex)
        {
            Log(ex.Message + "\r\n" + ex.StackTrace);
        }








    }
}
