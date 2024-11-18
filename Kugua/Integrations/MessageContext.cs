using MeowMiraiLib.Msg.Type;
using MeowMiraiLib.Msg;
using MeowMiraiLib;


namespace Kugua
{
    public class MessageContext
    {
        public long userId { get; set; }
        public long groupId { get; set; }
        public bool isGroup
        {
            get
            {
                return groupId > 0;
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

        public Client client { get; set; }

        public Message[] recvMessages;

        public void SendBackPlain(string message, bool isAt = false)
        {
            if (isGroup)
            {
                if (isAt) SendBack([new At(userId, null), new Plain(message)]);
                else SendBack([new Plain(message)]);
            }
            else
            {
                SendBack([new Plain(message)]);
            }
        }


        public void SendBack(Message[] _sendMessages)
        {
            if (_sendMessages != null)
            {
                if (client == null) return;

                // filtered
                List<string> msgStrings = new List<string>();

                List<Message> sendMessagesOthers = new List<Message>();

                foreach (var item in _sendMessages)
                {
                    if (item is Plain itemPlain)
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
                foreach(var s in msgStrings)
                {
                    var pmsg = new List<Message>();
                    if (firstFrame)
                    {
                        pmsg.AddRange(sendMessagesOthers);
                        firstFrame = false;
                    }
                    pmsg.Add(new Plain(s));
                    
                    if (client is LocalClient)
                    {
                        var lc = client as LocalClient;
                        lc.HandleMessage(userId, Config.Instance.UserInfo(userId).Name, pmsg.ToArray());
                    }
                    else
                    {
                        if (isTemp)
                        {
                            new TempMessage(userId, groupId, pmsg.ToArray()).Send(client);
                            Config.Instance.GroupInfo(userId).UseTimes += 1;
                        }
                        else if (isGroup)
                        {
                            new GroupMessage(groupId, pmsg.ToArray()).Send(client);
                            Config.Instance.GroupInfo(groupId).UseTimes += 1;
                        }
                        else
                        {
                            new FriendMessage(userId, pmsg.ToArray()).Send(client);
                            Config.Instance.UserInfo(userId).UseTimes += 1;
                        }
                    }
                }

                
            }
        }

    }

    
   







}
