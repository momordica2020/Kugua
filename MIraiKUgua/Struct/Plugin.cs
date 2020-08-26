using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using MMDK.Util;
using MMDK.Core;

namespace MMDK.Struct
{
    public delegate string GetNickNameHandler(long qq);
    public delegate void SendMessageHandler(Message msg);

    public abstract class Plugin
    {
        protected string PluginName = "";
        protected string PluginPath = "";

        protected Config config;
        protected IGlobalFunc BOT;


        public Plugin(string _name)
        {
            PluginName = _name;
            
        }

        public void Init(IGlobalFunc _func, Config _config, string _path)
        {
            BOT = _func;
            config = _config;
            PluginPath = $"{_path}{PluginName}/";
            
            if(!string.IsNullOrWhiteSpace(PluginPath))
            {
                if (!Directory.Exists(PluginPath))
                {
                    Directory.CreateDirectory(PluginPath);
                }
            }

            InitSource();
        }

        public abstract void InitSource();

        public abstract void HandleMessage(Message msg);
        
        



    }
}
