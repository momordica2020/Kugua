using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMDK.Core;
using MMDK.Struct;

namespace MMDK.Plugins
{
    class ModePlugin : Plugin
    {
        public ModePlugin() : base("Mode")
        {

        }

        public override void HandleMessage(Message msg)
        {
            //msg.toGroup = 735545947;// msg.fromGroup;


            
            //sendMessage(msg);
        }

        public override void InitSource()
        {
            //throw new NotImplementedException();
        }
    }
}
