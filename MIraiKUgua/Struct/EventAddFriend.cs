using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDK.Struct
{
    public class EventAddFriend
    {
        public long id;
        public long qq;
        public long fromgroup;
        public string name;
        public string desc;

        public EventAddFriend()
        {

        }
        public EventAddFriend(long _id, long _qq, string _name, string _desc)
        {
            id = _id;
            qq = _qq;
            name = _name;
            desc = _desc;
        }
    }
}
