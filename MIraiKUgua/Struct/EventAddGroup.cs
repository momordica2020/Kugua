using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDK.Struct
{
    public class EventAddGroup
    {
        public long id;
        public long fromqq;
        public long group;
        public string name;
        public string groupName;
        public string desc;

        public EventAddGroup()
        {

        }
        public EventAddGroup(long _id, long _group, string _name, string _desc)
        {
            id = _id;
            group = _group;
            name = _name;
            desc = _desc;
        }


    }
}
