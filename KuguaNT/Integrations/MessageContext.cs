
using ImageMagick;
using Kugua.Integrations.NTBot;
using System.Text;


namespace Kugua
{
    public class MessageContext
    {
        public string userId { get; set; }
        public string groupId { get; set; }
        public string messageId;
        public bool IsGroup
        {
            get
            {
                return !string.IsNullOrWhiteSpace(groupId);
            }
        }

        public bool IsTemp = false;

        public bool IsPrivate
        {
            get
            {
                return !IsGroup;
            }
        }


        public bool IsImage
        {
            get
            {
                if (recvMessages?.Count > 0)
                {
                    foreach (var msg in recvMessages)
                    {
                        if (msg is Image) return true;
                    }
                }
                return false;
            }
        }

        public bool IsAdminUser
        {
            get
            {
                return Config.Instance.UserHasAdminAuthority(userId);
            }
        }

        public bool IsAdminGroup
        {
            get
            {
                return IsGroup && Config.Instance.GroupHasAdminAuthority(groupId);
            }
        }

        public bool Is(string tag)
        {
            if (IsGroup)
            {
                var group = Config.Instance.GroupInfo(groupId);
                if (group == null) return false;
                return group.Is(tag);
            }
            else
            {
                var user = Config.Instance.UserInfo(userId);
                if (user == null) return false;
                return user.Is(tag);
            }
        }

        public bool IsAskme;

        public NTBot client { get; set; }

        public List<Message> recvMessages;

        public string PNG1Base64
        {
            get
            {
                if(recvMessages?.Count > 0 && IsImage)
                {
                    foreach (var it in recvMessages)
                    {
                        if (it is Image img)
                        {
                            var currentFrame = Network.DownloadImage(img.url).First();
                            using (MemoryStream ms = new MemoryStream())
                            {
                                // 将帧转换为 PNG 格式并写入 MemoryStream
                                currentFrame.Format = MagickFormat.Png; // 显式设置输出格式为 PNG
                                currentFrame.Write(ms);
                                var imageBytes = ms.ToArray();
                                var imgBase64 = Convert.ToBase64String(imageBytes);
                                return imgBase64;
                            }


                        }
                    }
                }
               

                return null;
            }
        }

        public List<Image> GetImages()
        {
            List<Image > images = new List<Image>();
            if (recvMessages != null)
            {
                foreach (var it in recvMessages)
                {
                    if (it is Image img) images.Add(img);
                }
            }

            return images;
        }

        public string GetTexts()
        {
            StringBuilder sb = new StringBuilder();
            if (recvMessages != null)
            {
                foreach (var it in recvMessages)
                {
                    if (it is Text text) sb.Append(text.text);
                }
            }


            return sb.ToString();
        }

        /// <summary>
        /// 获取qq市场表情
        /// </summary>
        /// <returns></returns>
        public MFace MarketFace
        {
            get
            {
                if (recvMessages != null)
                {
                    foreach (var it in recvMessages)
                    {
                        if (it is ImageRecvMarketFace img)
                        {
                            return new MFace
                            {
                                emoji_id = img.emoji_id,
                                emoji_package_id = img.emoji_package_id,
                                key = img.key,
                                summary = img.summary
                            };
                        }
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// 消息里是不是有市场表情
        /// </summary>
        /// <returns></returns>
        public bool HasMarketFace
        {
            get
            {
                return MarketFace != null;
            }
        }


        /// <summary>
        /// 是不是内含引用节点
        /// </summary>
        /// <returns></returns>
        public bool HasForward
        {
            get
            {
                if (recvMessages != null)
                {
                    foreach (var it in recvMessages)
                    {
                        if (it is ForwardNodeExist f) return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// 是不是内含嵌套引用
        /// </summary>
        /// <returns></returns>
        public bool HasMultiForward
        {
            get
            {
                try
                {
                    if (recvMessages != null)
                    {
                        foreach (var it in recvMessages)
                        {
                            if (it is ForwardNodeExist f)
                            {
                                foreach (var fnode in f.content)
                                {
                                    foreach (var fnodenode in fnode.message)
                                    {
                                        if (fnodenode is ForwardNodeExist) return true;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }

                return false;

            }
        }


        /// <summary>
        /// 向消息发送表情回应。传入表情的名字或者emoji字符，自动匹配
        /// </summary>
        /// <param name="name"></param>
        public void SendReact(string name)
        {

            try
            {
                var emoji = EmojiReact.Instance.Get(name);

                if (emoji != null) client?.SendEmojiLike(messageId, int.Parse(emoji.id));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public async Task<string> SendBackPlain(string message, bool isAt = false, bool isFilter=false)
        {
            if (IsGroup)
            {
                if (isAt) return SendBack([new At(userId), new Text(message)], isFilter).Result;
                else return SendBack([new Text(message)], isFilter).Result;
            }
            else
            {
                return SendBack([new Text(message)], isFilter).Result;
            }
        }

        public async Task<int> SendBackDice()
        {
            if(client!=null && client is not LocalClient)
            {
                var msgid = SendBack([new Dice()]).Result;
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
        public async Task<string> SendBack(Message[] _sendMessages, bool isFilter = false)
        {
            if (_sendMessages != null)
            {
                if (client == null) return "";

                
                List<string> msgStrings = new List<string>();

                List<Message> msgWithoutText = new List<Message>();

                foreach (var item in _sendMessages)
                {
                    if (item is Text itemPlain)
                    {
                        // filtered
                        if (isFilter)
                        {
                            itemPlain.text = Filter.Instance.FiltingBySentense(itemPlain.text, FilterType.Normal);
                        }
                        
                        

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
                        msgWithoutText.Add(item);
                    }
                }
                bool firstFrame = true;
                if (msgStrings.Count <= 0) msgStrings.Add("");

                List<string> msgIds = new List<string>();
                foreach(var s in msgStrings)
                {
                    var pmsg = new List<MessageInfo>();
                    if (firstFrame)
                    {
                        foreach (var item in msgWithoutText)
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
                        if (IsGroup)
                        {
                            var group = Config.Instance.GroupInfo(groupId);
                            if (group != null) group.UseTimes += 1;
                            var user = Config.Instance.UserInfo(userId);
                            if(user!=null)user.UseTimes += 1;
                            var messageId = client.Send(new send_group_msg(groupId, pmsg)).Result;
                            if (!string.IsNullOrWhiteSpace(messageId)) HistoryManager.Instance.Add(messageId, groupId, Config.Instance.BotQQ, pmsg.ToTextString());
                            msgIds.Add( messageId);
                            
                        }
                        else
                        {
                            var user = Config.Instance.UserInfo(userId);
                            if (user != null) user.UseTimes += 1;
                            var messageId = client.Send(new send_private_msg(userId, pmsg)).Result;
                            if (!string.IsNullOrWhiteSpace(messageId)) HistoryManager.Instance.Add(messageId, "", Config.Instance.BotQQ, pmsg.ToTextString());
                            msgIds.Add( messageId);

                        }
                    }
                }
                if (msgIds.Count > 0) return msgIds.Last();

                
            }
            return "";
        }

        

    }

    
   







}
