using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDK.Struct
{
    public enum GroupPermission
    {
        Member,
        Admin,
        Owner,
    }
    public class GroupInfo
    {
        public long id;
        public string name;
        public GroupPermission myPermission;

        public string announcement;
        public bool confessTalk;
        public bool allowMemberInvite;
        public bool autoApprove;
        public bool anonymousChat;

        public List<UserInfo> members = new List<UserInfo>();

        public GroupInfo()
        {

        }
    }
}
