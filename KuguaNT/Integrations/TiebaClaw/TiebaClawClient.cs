using Kugua.Mods;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NvAPIWrapper.Native.GPU;
using System.Text;

namespace Kugua.Integrations.TiebaClaw
{
    public class TiebaClawClient
    {
        private string _name;
        private readonly string _token;
        static HttpClient _httpClient;


        public long lastPageId;
        public long lastPostId;
        public string lastTriggerGroup;


        public TiebaClawClient(string token)
        {
            _token = token;
        }


        public string Name
        {
            get => _name; 
            set {
                this.SetName(value);
            }
        }

        public JToken Post(string url, string paramString = "")
        {
            return Get(url, true, paramString);
        }
        public JToken Get(string url, bool isPost = false, string paramString = "")
        {
            var handler = new HttpClientHandler()
            {
                Proxy = null, // Use the proxy
            };

            _httpClient = new HttpClient(handler);
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", _token);
            _httpClient.AddUserAgentToHeader();
            try
            {
                HttpResponseMessage response;
                if (isPost)
                {
                    //var sp = System.Web.HttpUtility.UrlPathEncode(paramString);
                    var sp = paramString;
                    response = _httpClient.PostAsync(url, new StringContent(sp, Encoding.UTF8, "application/json")).Result;//"application/x-www-form-urlencoded"));
                                                                                                                           //response = _httpClient.PostAsync(url).Result;

                }
                else
                {
                    response = _httpClient.GetAsync(url).Result;
                }
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    // fail
                    //throw new Exception($"Translate fault with http status code {response.StatusCode}");
                }
                string jsonString = response.Content.ReadAsStringAsync().Result;
                JObject jo = JObject.Parse(jsonString);
                try
                {
                    int errno = 404;
                    if (jo.ContainsKey("no")) errno = int.Parse(jo["no"].ToString());
                    if (jo.ContainsKey("errno")) errno = int.Parse(jo["errno"].ToString());
                    if (jo.ContainsKey("error_code")) errno = int.Parse(jo["error_code"].ToString());

                    if (errno != 0)
                    {
                        string errmsg = "";
                        if (jo.ContainsKey("error_msg")) errmsg = jo["error_msg"].ToString();
                        Console.WriteLine($"Error [{errno}] {errmsg}");
                        return jo;
                    }
                    if (jo.ContainsKey("data")) return jo["data"];
                    else return jo;
                }
                catch (Exception e)
                {

                }
                return jo;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return null;
        }


        /// <summary>
        /// 读取个人回复
        /// </summary>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        public JToken GetReplies(int pageNum = 1)
        {
            var url = $"https://tieba.baidu.com/mo/q/claw/replyme?page={pageNum}";
            var res = Get(url);
            return res;
        }

        /// <summary>
        /// 获取帖子列表
        /// </summary>
        /// <param name="byHot"></param>
        /// <returns></returns>
        public JToken GetPages(bool byHot = false)
        {
            int sortType = byHot ? 3 : 0;
            var url = $"https://tieba.baidu.com/c/f/frs/page_claw?sort_type={sortType}";
            var res = Get(url);

            return res;
        }


        public enum GetDetailSortType
        {
            Default = 0,
            Reverse = 1,
            Hot = 2
        }



        /// <summary>
        /// 获取帖子详细内容
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="pageNum"></param>
        /// <param name="sortType"></param>
        /// <returns></returns>
        public JToken GetPageDetail(long pageId, int pageNum = 1, GetDetailSortType sortType = GetDetailSortType.Default)
        {
            var url = $"https://tieba.baidu.com/c/f/pb/page_claw?pn={pageNum}&kz={pageId}&r={(int)sortType}";
            var res = Get(url);

            return res;
        }

        public enum TabId
        {
            新虾报到 = 4666758,
            硅基哲思 = 4666765,
            赛博摸鱼 = 4666767,
            图灵乐园 = 4666770
        }
        /// <summary>
        /// 发布新帖 (addThread)
        /// </summary>
        public JToken AddThread(string title, string content, TabId tabId = TabId.图灵乐园)
        {
            var url = "https://tieba.baidu.com/c/c/claw/addThread";

            var payload = new
            {
                title = title,
                content = new[] { new { type = "text", content = content } },
                tab_id = tabId
            };

            try
            {
                string jsonString = JsonConvert.SerializeObject(payload);
                return Get(url, true, jsonString);


                //var json = JsonSerializer.Serialize(payload);
                //var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                //var response = WebLinker.PostAsync(url, httpContent).Result;
                //string result = response.Content.ReadAsStringAsync().Result;
                //return WebUtility.HtmlDecode(result);
            }
            catch (Exception ex) { Console.WriteLine(ex.InnerException ?? ex); return null; }
        }

        ///// <summary>
        ///// 回复帖子或楼层 (addPost)
        ///// </summary>
        public JToken AddPost(long threadId, string content, long postId = 0)
        {
            var url = "https://tieba.baidu.com/c/c/claw/addPost";
            if (threadId <= 0) return null;
            long? post_id = postId > 0 ? postId : (long?)null;
            var payload = new
            {
                thread_id = threadId,
                content = content,
                post_id = post_id // 如果回复楼层则传入，否则为 null
            };

            try
            {
                string jsonString = JsonConvert.SerializeObject(payload);
                return Post(url, jsonString);
            }
            catch (Exception ex) { Console.WriteLine(ex.InnerException ?? ex); return null; }
        }


        /// <summary>
        /// 点赞
        /// </summary>
        /// <param name="threadId"></param>
        /// <param name="agree"></param>
        /// <param name="postId"></param>
        /// <returns></returns>
        public JToken AddAgree(long threadId, bool agree=true, long postId = 0)
        {
            var url = "https://tieba.baidu.com/c/c/claw/opAgree";
            if (threadId <= 0) return null;
            long? post_id = postId > 0 ? postId : (long?)null;
            var payload = new
            {
                thread_id = threadId,
                obj_type = postId > 0 ? 1:3,// 必填, 点赞楼层传`1` 楼中楼传`2` 主帖传`3`
                op_type = 0, // 必填, 点赞传`0` 取消点赞传`1`
                post_id = post_id // 如果是回复则传入，否则为 null
            };
            
            try
            {
                string jsonString = JsonConvert.SerializeObject(payload);
                Logger.Log(jsonString);
                return Post(url, jsonString);
            }
            catch (Exception ex) { Console.WriteLine(ex.InnerException ?? ex); return null; }
        }


        public bool SetName(string newName)
        {
            if(string.IsNullOrWhiteSpace(newName)) return false;
            if(newName==this._name) return true;
            string url = "https://tieba.baidu.com/c/c/claw/modifyName";
            var payload = new
            {
                namme = newName,
            };
            string jsonString = JsonConvert.SerializeObject(payload);
            var returnVal = Post(url, jsonString);
            Logger.Log($"修改贴吧昵称返回：{returnVal}");
            if (returnVal.ToString().Contains("success"))
            {
                this._name = newName;
                return true;

            }
            return false;
        }



        ///// <summary>
        ///// 点赞/取消点赞 (opAgree)
        ///// </summary>
        //public string OpAgree(long threadId, int objType, int opType = 0, long? postId = null)
        //{
        //    var url = "https://tieba.baidu.com/c/c/claw/opAgree";
        //    using var client = CreateClient();

        //    var payload = new
        //    {
        //        thread_id = threadId,
        //        obj_type = objType, // 1-楼层, 2-楼中楼, 3-主帖
        //        op_type = opType,   // 0-点赞, 1-取消
        //        post_id = postId
        //    };

        //    try
        //    {
        //        var json = JsonSerializer.Serialize(payload);
        //        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        //        var response = client.PostAsync(url, httpContent).Result;
        //        return WebUtility.HtmlDecode(response.Content.ReadAsStringAsync().Result);
        //    }
        //    catch (Exception ex) { Console.WriteLine(ex.InnerException ?? ex); return ""; }
        //}


    }
}
