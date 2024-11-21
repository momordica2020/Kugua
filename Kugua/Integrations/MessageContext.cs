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

        private void tmp_sendAnno(Message[] messages)
        {
            string txt = "";
            string imgurl = "";
            string imgbase64 = "";
            foreach(var item in messages)
            {
                if (item is Plain p) txt += p.text + "\r\n";
                else if(item is Image img)
                {
                    if (img.base64 != null)
                    {
                        imgbase64 = img.base64;
                    }
                    if (img.path != null)
                    {
                        imgurl = img.path;
                    }
                }
            }
            //Logger.Log(imgbase64 + "\r\n" + imgurl);
            //if (string.IsNullOrWhiteSpace(txt)) txt = ",,,";
           if(!string.IsNullOrWhiteSpace(txt) || !string.IsNullOrWhiteSpace(imgurl) ||!string.IsNullOrWhiteSpace(imgbase64))
            {
                new Anno_publish(groupId, txt, false, false, false, true, false, null, string.IsNullOrWhiteSpace(imgurl) ? null : imgurl, string.IsNullOrWhiteSpace(imgbase64) ? null : imgbase64).Send(client);

            }

        }


        public void SendBack(Message[] _sendMessages)
        {
            if (_sendMessages != null)
            {
                if (client == null) return;
                //if (groupId != 0)
                //{
                //    try
                //    {
                //        tmp_sendAnno(_sendMessages);
                //    }
                //    catch (Exception ex)
                //    {

                //    }
                //    return;
                //}
      

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
                if (msgStrings.Count <= 0) msgStrings.Add("");
                foreach(var s in msgStrings)
                {
                    var pmsg = new List<Message>();
                    if (firstFrame)
                    {
                        //foreach (var item in sendMessagesOthers)pmsg.Add(item);
                        pmsg.AddRange(sendMessagesOthers);
                        firstFrame = false;
                    }
                    if(!string.IsNullOrWhiteSpace(s)) pmsg.Add(new Plain(s));
                    
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
