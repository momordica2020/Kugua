using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDK.Struct
{


    public class MessageImage
    {
        public string id;
        public string url;
        public string path;

        public MessageImage()
        {

        }

        public MessageImage(string _url, string _id = "")
        {
            id = _id;
            url = _url;
        }
    }

    public class MessageAt
    {
        public string desc;
        public long qq;

        public MessageAt()
        {

        }

        public MessageAt(long _qq, string _desc = "")
        {
            qq = _qq;
            desc = _desc;
        }
    }

    public class MessageFace
    {
        public int faceid;
        public string name;
        
        public MessageFace()
        {

        }

        public MessageFace(int _faceid, string _name="")
        {
            faceid = _faceid;
            name = _name;
        }
    }



    public class Message
    {
        public long id;

        public string str;
        public long at;

        public long from = 0;
        public string fromName;
        public long fromGroup = 0;
        public string fromGroupName;
        public GroupPermission fromPermission;
        public GroupPermission fromMyPermission;

        public long to = 0;
        public long toGroup = 0;

        
        

        public DateTime time;

        public List<MessageImage> imgs = new List<MessageImage>();
        public List<MessageAt> ats = new List<MessageAt>();
        public List<MessageFace> faces = new List<MessageFace>();
        public List<string> plains = new List<string>();
        public string xml;
        public Message quote = null;

        //public bool isPrivate = false;
        public bool isTempMsg = false;
        public bool isRecall = false;

        public Message()
        {

        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"[fromgroup={fromGroup}][fromqq={from}]{str}");

            return sb.ToString();
        }    
        
        public bool isAtMe(long qq)
        {
            foreach(var at in ats)
            {
                if (at.qq == qq) return true;
            }

            return false;
        }

    }

   

    


}
