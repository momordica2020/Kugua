using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageMagick;
using Kugua.Integrations.NTBot;

namespace LiveServer.Util
{
    public class Util
    {
        public static string GetDesc(Message msg)
        {
            if(msg is Text t)
            {
                return t.text;
            }
            else if(msg is ImageBasic img)
            {
                return $"(image)url={img.file}";
            }
            else if(msg is Record r)
            {
                return $"(record)url={r.path}";
            }
            else
            {
                return $"(msg)type={msg.GetType().Name}";
            }
        }



    }
}
