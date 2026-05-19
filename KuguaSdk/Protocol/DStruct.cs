using KuguaSdk.MessageStructs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace KuguaSdk.Protocol
{
    public class DSHeader
    {
        public string SourceId { get; set; }
        public string SourceName { get; set; }
        public string Type { get; set; }
        public List<Message> messages { get; set; }
    }

}
