using MMDK.Util;
using System;
using System.Collections.Generic;
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

        public Config(string file)
        {
            this.file = file;


            load();
        }

        public void load()
        {
            configs = FileHelper.readDict(file, new char[] { '=' });
            
        }

        public void save()
        {
            FileHelper.writeDict(file, configs, '=');
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
    }
}
