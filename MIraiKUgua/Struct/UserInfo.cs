using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDK.Struct
{
    public enum UserType
    {
        Self,
        Master,
        Temp,
        Friend,
        Foreigner,
        Blocked,
    }

    public class UserInfo
    {
        public long qq;
        public string name;
        public string remark;
        public UserType type;

        public Dictionary<long, string> remarkInGroup = new Dictionary<long, string>();
        public Dictionary<long, string> titleInGroup = new Dictionary<long, string>();
        

        public UserInfo()
        {

        }

        public UserInfo(long _qq, string _name, UserType _type)
        {
            qq = _qq;
            name = _name;
            type = _type;
        }
    }
}
