using Kugua.Core;
using Kugua.Core.Images;
using Kugua.Integrations.NTBot;

namespace Kugua.Mods.ModTransShits
{
    public class Shit
    {
        public List<MessageContext> contexts;
        public DateTime createTime;
        public string createGroup;
        public string createUser;

        public long score;
        //public bool published;

        //public long AIscore;
        //public bool publishedAI;

        public bool isForward = false;
        public string imgBase64;
        public string imgType;
        public bool isVideo = false;

        //string _hashtext;
        //string _hash;
        public List<string> _hashs = new List<string>();
        //public string hash { get
        //    {
        //        return _hash;
        //    } }
        public Shit(MessageContext _context)
        {
            contexts = new List<MessageContext>();
            contexts.Add(_context);
            createTime = DateTime.Now;
            createGroup = _context.groupId;
            createUser = _context.userId;
            score = 0;
            isForward = false;
            isVideo = false;

            calHash();
        }


        void calHashSingleItem(Message item)
        {
            try
            {
                if (item is ImageBasic img && !string.IsNullOrWhiteSpace(img.url))
                {
                    //Logger.Log($"<hashimg>{img.file}");
                    
                    var imgb64 = Network.DownloadImageUrlToBase64(img.url).Result;
                    var imghash = ImageSimilar.GetHashFromBase64(imgb64);
                    //StaticUtil.ComputeHash(imgb64)
                    _hashs.Add(imghash);
                    //_hash = Util.ComputeHash(_hash + imghash);
                    if (string.IsNullOrWhiteSpace(imgBase64))
                    {
                        imgBase64 = imgb64;
                        imgType = img.ext;
                    }

                }
                else if (item is Video video)
                {

                    //string localPath = $"Temp/{video.file.Replace(".mp4",".jpg")}";
                    //if (ImageUtil.CaptureFirstFrame(video.url, Config.Instance.FullPath(localPath)))
                    //{
                    //    var base64 = Convert.ToBase64String(File.ReadAllBytes(localPath));
                    //    var hash = ImageSimilar.GetHashFromBase64(base64);
                    //    Logger.Log($"VIDEO HASH={hash}");
                    //    //break;

                    //}
                    
                    string localVideoPath = Config.Instance.FullPath($"Temp\\{video.file}");
                    Network.Download(video.url, localVideoPath);
                    //Logger.Log($"{video.url} |||||||||| {localVideoPath}");
                    string localFramePath = localVideoPath.Replace("mp4", "jpg");
                    if (ImageHandler.CaptureFirstFrame(localVideoPath, localFramePath))
                    {
                        var base64 = Convert.ToBase64String(File.ReadAllBytes(localFramePath));
                        var hash = ImageSimilar.GetHashFromBase64(base64);
                        Logger.Log($"   VIDEO HASH    ={hash}");
                        //break;
                        
                        
                        isVideo = true;
                        _hashs.Add(Util.ComputeHash(hash));
                        File.Delete(localFramePath);
                        File.Delete(localVideoPath);

                    }

                    
                    //_hash = Util.ComputeHash(_hash + video.file);

                }
                else if (item is ForwardNodeExist forward)
                {
                    isForward = true;
                    foreach (var c in forward.content)
                    {
                        //Logger.Log(c.raw_message);
                        //Logger.Log("===1");

                        foreach (var cmsg in c.message)
                        {
                            calHashSingleItem(cmsg);
                        }
                        //Logger.Log("===2");
                        //_hash = StaticUtil.ComputeHash(_hash + c.message);
                    }

                }else if(isForward && item is Text text)
                {

                    // text in forward
                    //Logger.Log($"<hashtext>{text.text}");
                    _hashs.Add(Util.ComputeHash(text.text));
                    //_hash = Util.ComputeHash(_hash + text.text);
                    
                }

            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }


        /// <summary>
        /// 计算本消息的hash，只取决于图片/视频/转发messageid
        /// </summary>
        void calHash()
        {
            _hashs = new List<string>();
            //_hash = "";
            //bool hasNextForwardLevel = false;
            var context = contexts.First();
            //if(context.IsOnlyImage)
            foreach(var item in contexts.First().recvMessages)
            {
                calHashSingleItem(item);
            }
            //if (string.IsNullOrWhiteSpace(_hashimg))
            //{
            //    _hashimg = StaticUtil.ComputeHash(context.recvMessages.ToTextString());
            //}
            
            //Logger.Log($"<hash> = {_hash}");
        }

        
    }
}
