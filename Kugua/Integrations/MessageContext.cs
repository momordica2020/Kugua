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


        public void SendBack(Message[] sendMessages)
        {
            if (sendMessages != null)
            {
                if (client == null) return;

                // filtered
                foreach (var item in sendMessages)
                {
                    if (item is Plain)
                    {
                        var msg = item as Plain;
                        msg.text = Filter.Instance.Filting(msg.text, FilterType.Normal);
                    }
                }


                if (client is LocalClient)
                {
                    var lc = client as LocalClient;
                    lc.HandleMessage(userId, Config.Instance.UserInfo(userId).Name, sendMessages);
                }
                else
                {
                    if (isGroup)
                    {
                        new GroupMessage(groupId, sendMessages).Send(client);
                    }
                    else
                    {
                        new FriendMessage(userId, sendMessages).Send(client);
                    }
                }
            }
        }

    }

    
   







}
