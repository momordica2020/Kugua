
using ImageMagick;
using Kugua.Core;
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

        public Player User {
            get
            {
                return Config.Instance.UserInfo(userId);
            }
        }

        public Playgroup Group
        {
            get
            {
                if (!IsGroup) return null;
                return Config.Instance.GroupInfo(groupId);
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
                return this.Images.Count > 0;
            }
        }
        public bool IsAudio
        {
            get
            {
                return this.Audios.Count > 0;
            }
        }

        public bool OnlyImage
        {
            get
            {
                bool hasImage = false;
                if (recvMessages?.Count > 0)
                {
                    foreach (var msg in recvMessages)
                    {
                        if (msg is ImageBasic)
                        {
                            hasImage = true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                return hasImage;
            }
        }

        

        public bool OnlyVideo
        {
            get
            {
                bool hasVideo = false;
                if (recvMessages?.Count > 0)
                {
                    foreach (var msg in recvMessages)
                    {
                        if (msg is Video)
                        {
                            hasVideo = true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                return hasVideo;
            }
        }

        public bool OnlyText
        {
            get
            {
                bool hasText = false;
                if (recvMessages?.Count > 0)
                {
                    foreach (var msg in recvMessages)
                    {
                        if (msg is Text)
                        {
                            hasText = true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                return hasText;
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

        public List<At> Ats {             
            get
            {
                List<At> ats = new List<At>();
                if (recvMessages != null)
                {
                    foreach (var it in recvMessages)
                    {
                        if (it is At at) ats.Add(at);
                    }
                }
                return ats;
            }
        }

        public NTBot client { get; set; }

        public List<Message> recvMessages;

        public string PNG1Base64
        {
            get
            {
                if(IsImage)
                {
                    foreach (var it in Images)
                    {
                        if (it is ImageBasic img)
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
        public List<string> PNGBase64s
        {
            get
            {
                if (IsImage)
                {
                    var res = new List<string>();
                    foreach(var it in Images)
                    {
                        if (it is ImageBasic img)
                        {
                            var currentFrame = Network.DownloadImage(img.url).First();
                            using (MemoryStream ms = new MemoryStream())
                            {
                                // 将帧转换为 PNG 格式并写入 MemoryStream
                                currentFrame.Format = MagickFormat.Png; // 显式设置输出格式为 PNG
                                currentFrame.Write(ms);
                                var imageBytes = ms.ToArray();
                                var imgBase64 = Convert.ToBase64String(imageBytes);
                                res.Add( imgBase64);
                            }


                        }
                    }
                    return res;


                }


                return null;
            }
        }
        public List<ImageBasic> Images
        {
            get
            {
                List<ImageBasic> images = new List<ImageBasic>();
                if (recvMessages != null)
                {
                    foreach (var it in recvMessages)
                    {
                        if (it is ImageBasic img) images.Add(img);
                        else if(it is ForwardNodeExist forward)
                        {
                            images.AddRange(getImageFromForward(forward));
                        }
                    }
                }

                return images;
            }
        }

        public List<Record> Audios
        {
            get
            {
                List<Record> audios = new List<Record>();
                if (recvMessages != null)
                {
                    foreach (var it in recvMessages)
                    {
                        if (it is Record rec) audios.Add(rec);
                    }
                }

                return audios;
            }
        }

        List<ImageBasic> getImageFromForward(ForwardNodeExist node)
        {
            List<ImageBasic> res = new List<ImageBasic>();

            if (node == null) return res;
            foreach(var n in node.content)
            {
                foreach(var msg in n.message)
                {
                    if(msg is ImageBasic img)
                    {
                        res.Add(img);
                    }
                    else if(msg is ForwardNodeExist forward)
                    {
                        res.AddRange(getImageFromForward(forward));
                    }
                }
                
            }

            return res;
        }

        public string Texts
        {
            get
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

        public ReactLike React
        {
            get
            {
                foreach(var msg in recvMessages)
                {
                    if (msg is ReactLike like) return like;
                }
                return null;
            }
        }

        public bool IsReact { get { return React != null; } }


        /// <summary>
        /// 根据系统整体配置来决定是否可以输出
        /// </summary>
        /// <param name="targetId"></param>
        /// <param name="isGroup"></param>
        /// <returns></returns>
        public bool CanSendout(string targetId,bool isGroup)
        {
            if(client is NTBot)
            {
                switch (Config.Instance.App.Avatar.answerState)
                {
                    case 0:
                        return false;
                    case 1:
                        if (isGroup && Config.Instance.GroupHasAdminAuthority(targetId)) return true;
                        return false;
                    case 2:
                        if (isGroup && Config.Instance.GroupHasAdminAuthority(targetId)) return true;
                        else if (Config.Instance.UserHasAdminAuthority(targetId)) return true;
                        return false;
                    case 3:
                    default:
                        if (!isGroup && Config.Instance.AllowPlayer(targetId)) return true;
                        if (isGroup && Config.Instance.AllowGroup(targetId)) return true;
                        return false;
                        break;
                }
            }
            return true;
            
        }









        /// <summary>
        /// 向消息发送表情回应。传入表情的名字或者emoji字符，自动匹配
        /// </summary>
        /// <param name="name">emoji或qq自带表情的描述名，如“爱心”</param>
        public void SendReact(string name, string msgId = "")
        {

            try
            {
                if (string.IsNullOrWhiteSpace(msgId)) msgId = this.messageId;
                var emoji = EmojiReact.Instance.Get(name);

                if (emoji != null) client?.SendEmojiLike(msgId, int.Parse(emoji.id));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public async Task<string> SendBackText(string text, bool isAt=false, bool isFilter = false, bool isDelay=true)
        {
            return await SendText(text,  IsGroup ? groupId : userId, IsGroup, isAt, isFilter);
        }

        public async Task<string> SendText(string message, string targetId, bool isGroup, bool isAt = false,  bool isFilter=false, bool isDelay = true)
        {
            if (IsGroup && isAt)
            {
                return Send([new At(userId), new Text(message)], targetId,isGroup, isFilter, isDelay).Result;
            }
            else
            {
                return Send([new Text(message)], targetId, isGroup, isFilter, isDelay).Result;
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



        //public async Task<string> SendImageBase64(string base64, string targetId = "", bool isGroup = false, string desc = "")
        //{
        //    List<Message> msgs = new List<Message>();
        //    if (!string.IsNullOrWhiteSpace(base64)) msgs.Add(new ImageSend($"base64://{base64}"));
        //    if (!string.IsNullOrWhiteSpace(desc)) msgs.Add(new Text(desc));
        //    if (msgs.Count > 0) return await Send(msgs.ToArray(), targetId, isGroup);
        //    else return "";
        //}

        //public async Task<string> SendImage(string imagePath, string targetId = "", bool isGroup = false, string desc = "")
        //{
        //    List<Message> msgs = new List<Message>();
        //    if (!string.IsNullOrWhiteSpace(imagePath)) msgs.Add(new ImageSend($"file://{imagePath}"));
        //    if (!string.IsNullOrWhiteSpace(desc)) msgs.Add(new Text(desc));
        //    if (msgs.Count > 0) return await Send(msgs.ToArray(), targetId, isGroup);
        //    else return "";
        //}


        public async Task<string> SendBackImage(MagickImageCollection images,  string desc = "")
        {
            return await SendImage(images, IsGroup ? groupId : userId, IsGroup, desc);
        }

        public async Task<string> SendBackImage(MagickImage image, string desc = "")
        {
            return await SendImage(image, IsGroup ? groupId : userId, IsGroup, desc);
        }

        public async Task<string> SendBackImages(List<MagickImageCollection> images, string desc = "")
        {
            return await SendImages(images, IsGroup ? groupId : userId, IsGroup, desc);
        }


        public async Task<string> SendImage(MagickImageCollection image, string targetId, bool isGroup, string desc = "")
        {
            
            List<Message> msgs = new List<Message>();
            if (image!=null) msgs.Add(new ImageSend(image));
            if (!string.IsNullOrWhiteSpace(desc)) msgs.Add(new Text(desc));
            if (msgs.Count > 0) return await Send(msgs.ToArray(), targetId, isGroup);
            else return "";
        }

        public async Task<string> SendImages(List<MagickImageCollection> images, string targetId, bool isGroup, string desc = "")
        {

            List<Message> msgs = new List<Message>();
            
            if (images != null)foreach(var image in images) msgs.Add(new ImageSend(image));
            if (!string.IsNullOrWhiteSpace(desc)) msgs.Add(new Text(desc));
            if (msgs.Count > 0) return await Send(msgs.ToArray(), targetId, isGroup);
            else return "";
        }


        public async Task<string> SendImage(MagickImage image, string targetId, bool isGroup,  string desc = "")
        {
            List<Message> msgs = new List<Message>();
            if (image != null) msgs.Add(new ImageSend(image));
            if (!string.IsNullOrWhiteSpace(desc)) msgs.Add(new Text(desc));
            if (msgs.Count > 0) return await Send(msgs.ToArray(), targetId, isGroup);
            else return "";
        }


        public async Task<string> SendBack(Message[] _sendMessages,  bool isFilter = false)
        {
            return await Send(_sendMessages, IsGroup ? groupId : userId, IsGroup, isFilter);
        }

        public async Task<string> Send(Message[] _sendMessages, string targetId, bool isGroup, bool isFilter = false, bool isDealy = true)
        {
            if (client == null) return string.Empty;
            if (_sendMessages == null && _sendMessages.Length <= 0) return string.Empty;
            if (string.IsNullOrWhiteSpace(targetId)) targetId = (isGroup ? groupId : userId);
                

                
            List<string> msgStrings = new List<string>();

            List<Message> msgWithoutText = new List<Message>();

            foreach (var item in _sendMessages)
            {
                if (item is Text itemPlain)
                {
                    // filtered
                    if (isFilter)
                    {
                        itemPlain.text = Filter.Instance.FiltingBySentense(itemPlain.text, Kugua.Core.FilterType.Normal);
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
                    if (isGroup)
                    {
                        var delay = 0;
                        if (Config.Instance.GroupInfo(targetId) is Playgroup group)
                        {
                            group.UseTimes += 1;
                            delay = group.delayMs;
                        }
                        if (Config.Instance.UserInfo(targetId) is Player user)
                        {
                            user.UseTimes += 1;
                        }
                        //Logger.Log($"delay={delay}ms");
                        if(isDealy)   await Task.Delay(delay);
                        var messageId = client.Send(new send_group_msg(targetId, pmsg)).Result;
                        if (!string.IsNullOrWhiteSpace(messageId)) HistoryManager.Instance.Add(messageId, targetId, Config.Instance.BotQQ, pmsg.ToTextString());
                        msgIds.Add( messageId);
                            
                    }
                    else
                    {
                        var delay = 0;
                        if (Config.Instance.UserInfo(targetId) is Player user)
                        {
                            user.UseTimes += 1;
                            delay = user.delayMs;
                        }
                        //Logger.Log($"delay={delay}ms");
                        if (isDealy) await Task.Delay(delay);
                        var messageId = client.Send(new send_private_msg(targetId, pmsg)).Result;
                        if (!string.IsNullOrWhiteSpace(messageId)) HistoryManager.Instance.Add(messageId, "", Config.Instance.BotQQ, pmsg.ToTextString());
                        msgIds.Add( messageId);

                    }
                }
            }
            if (msgIds.Count > 0) return msgIds.Last();

                
            
            return "";
        }

        /// <summary>
        /// 消息长度，用于计算转发消息是否超过5MB限制
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public static int MessageSize(Message[] messages)
        {
            int res = 0;

            foreach (var msg in messages)
            {
                if (msg is Text text)
                {
                    res += Encoding.UTF8.GetByteCount(text.text);
                }
                else if (msg is ImageSend img)
                {
                    res += Encoding.UTF8.GetByteCount(img.file);
                }
            }

                return res;
        }



        /// <summary>
        /// 发送伪造的转发消息
        /// </summary>
        /// <param name="_messages"></param>
        /// <param name="group"></param>
        public void SendForward(Message[] _messages, string group = "")
        {
            if (string.IsNullOrWhiteSpace(group)) group = groupId;
            if (!CanSendout(group, isGroup:true)) return;

            if (_messages == null || _messages.Length == 0) return;
            // 每组最多10条
            const int BatchSize = 10;                    
            var nodes = new List<Message>();
            const int MaxMessageSize = 5 * 1024 * 1024; // 5MB
            const int MaxMessageCount = 100;
            int currentMessageSize = 0;
            int currentMessageCount = 0;


            for (int i = 0; i < _messages.Length; i += BatchSize)
            {
                var batch = _messages.Skip(i)
                                     .Take(BatchSize)
                                     .ToArray();
                currentMessageSize += MessageSize(batch);
                currentMessageCount += batch.Length;
                if (currentMessageSize >= MaxMessageSize || currentMessageCount > MaxMessageCount)
                {
                    client.SendForwardMessageToGroup(group, nodes);
                    nodes.Clear();
                    currentMessageSize = 0;
                    currentMessageCount = 0;
                    continue;
                }
                var contentList = batch.Select(msg => new MessageInfo(msg)).ToList();

                nodes.Add(new ForwardNodeNew
                {
                    user_id = Config.Instance.BotQQ,
                    nickname = Config.Instance.BotName,
                    content = contentList
                });
            }

            client.SendForwardMessageToGroup(group, nodes);
        }

        

    }

    
   







}
