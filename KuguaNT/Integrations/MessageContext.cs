
using Kugua.Integrations.NTBot;


namespace Kugua
{
    public class MessageContext
    {
        public string userId { get; set; }
        public string groupId { get; set; }
        public bool isGroup
        {
            get
            {
                return !string.IsNullOrWhiteSpace(groupId);
            }
        }

        public bool isTemp = false;

        public bool isPrivate
        {
            get
            {
                return !isGroup;
            }
        }

        public bool isAskme;

        public NTBot client { get; set; }

        public List<Message> recvMessages;

        public async Task<string> SendBackPlain(string message, bool isAt = false)
        {
            if (isGroup)
            {
                if (isAt) return SendBack([new At(userId), new Text(message)]).Result;
                else return SendBack([new Text(message)]).Result;
            }
            else
            {
                return SendBack([new Text(message)]).Result;
            }
        }

        public async Task<int> SendBackDice()
        {
            if(client!=null && client is not LocalClient)
            {
                Logger.Log("!!");
                var msgid = SendBack([new Dice()]).Result;
                Logger.Log(msgid);
                if (!string.IsNullOrWhiteSpace(msgid))
                {
                    var r = client.Send(new get_msg(msgid)).Result;
                    int rint = 0;
                    if (int.TryParse(r, out rint))
                    {
                        return rint;
                    }
                    
                }
            }

            return 0;
        }
        public async Task<string> SendBack(Message[] _sendMessages)
        {
            if (_sendMessages != null)
            {
                if (client == null) return "";

                // filtered
                List<string> msgStrings = new List<string>();

                List<Message> sendMessagesOthers = new List<Message>();

                foreach (var item in _sendMessages)
                {
                    if (item is Text itemPlain)
                    {
                        itemPlain.text = Filter.Instance.FiltingBySentense(itemPlain.text, FilterType.Normal);

                        int index = 0;
                        int maxlen = 1500;

                        while(index < itemPlain.text.Length)
                        {
                            msgStrings.Add(itemPlain.text.Substring(index, Math.Min(maxlen, itemPlain.text.Length - index)));
                            index += maxlen;
                        }
                    }
                    else
                    {
                        sendMessagesOthers.Add(item);
                    }
                }
                bool firstFrame = true;
                if (msgStrings.Count <= 0) msgStrings.Add("");
                foreach(var s in msgStrings)
                {
                    var pmsg = new List<MessageInfo>();
                    if (firstFrame)
                    {
                        foreach (var item in sendMessagesOthers)
                        {
                            pmsg.Add(new MessageInfo( item));
                        }
                        //pmsg.AddRange(sendMessagesOthers);
                        firstFrame = false;
                    }
                    if(!string.IsNullOrWhiteSpace(s)) pmsg.Add(new MessageInfo(new Text(s)));
                    
                    if (client is LocalClient lc)
                    {
                        lc.HandleMessage(userId, Config.Instance.UserInfo(userId).Name, pmsg);
                    }
                    else
                    {
                        //if (isTemp)
                        //{
                        //    new TempMessage(userId, groupId, pmsg.ToArray()).Send(client);
                        //    Config.Instance.GroupInfo(userId).UseTimes += 1;
                        //}
                        //else
                        if (isGroup)
                        {
                            Config.Instance.GroupInfo(groupId).UseTimes += 1;
                            return client.Send(new send_group_msg(groupId, pmsg)).Result;
                            
                        }
                        else
                        {
                            Config.Instance.UserInfo(userId).UseTimes += 1;
                            return client.Send(new send_private_msg(userId, pmsg)).Result;
                            
                        }
                    }
                }

                
            }
            return "";
        }

    }

    
   







}
