using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.SqlServer.Server;
using MMDK.Struct;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MMDK.Util
{

    enum MiraiReturnCode
    {
        Success = 0,
        AuthError = 1,
        BotNotExist = 2,
        SessionError = 3,
        SessionNotAuth = 4,
        TargetQQNotExist = 5,
        TargetFileNotExist = 6,
        TargetAuthNotExist = 10,
        BotWasBanned = 20,
        MessageTooLong = 30,
        ErrorRequest = 400,
    }

    enum MiraiAddFriendRespose
    {
        Allow = 0,
        Deny = 1,
        DenyAndBlock = 2,
    }

    public enum MiraiAddGroupResponse
    {
        /// <summary>
        /// 同意入群
        /// </summary>
        Allow = 0,
        /// <summary>
        /// 拒绝入群
        /// </summary>
        Deny = 1,
        /// <summary>
        /// 忽略请求
        /// </summary>
        Ignore = 2,
        /// <summary>
        /// 拒绝入群并添加黑名单，不再接收该用户的入群申请
        /// </summary>
        DenyAndBlock = 3,
        /// <summary>
        /// 忽略入群并添加黑名单，不再接收该用户的入群申请
        /// </summary>
        IgnoreAndBlock = 4
    }

    delegate void HandleMiraiMsg(Message msg);
    delegate void HandleEventAddFriend(EventAddFriend e);
    delegate void HandleEventAddGroup(EventAddGroup e);
    class MiraiHelper
    {
        static string HOST = "127.0.0.1/";
        static int PORT = 9999;
        static string SESSION = "";
        static long QQ = 0;
        static bool ISLINK = false;
        public static HandleMiraiMsg handleMiraiMsg;
        public static HandleMiraiMsg handleMiraiRecall;
        public static HandleEventAddFriend handleAddFriend;
        public static HandleEventAddGroup handleAddGroup;
        
        

       // public static WebSocket SOCKET;

        static JObject postAuth(string key)
        {
            string url = $"http://{HOST}:{PORT}/auth";

            JObject json = new JObject();
            json.Add("authKey", key);


            JObject res = WebHelper.postJson(url, json);
            return res;
        }

        static JObject postVerify(string session, long qq)
        {
            string url = $"http://{HOST}:{PORT}/verify";

            JObject json = new JObject();
            json.Add("sessionKey", session);
            json.Add("qq", qq);


            JObject res = WebHelper.postJson(url, json);
            return res;
        }


        static JObject postRelease(string session, long qq)
        {
            string url = $"http://{HOST}:{PORT}/release";

            JObject json = new JObject();
            json.Add("sessionKey", session);
            json.Add("qq", qq);


            JObject res = WebHelper.postJson(url, json);
            return res;
        }

        static MiraiReturnCode GetReturnCode(JObject obj)
        {
            if(obj != null && obj.ContainsKey("code"))
            {
                return (MiraiReturnCode)(int.Parse(obj["code"].ToString()));
            }
            return MiraiReturnCode.ErrorRequest;
        }

        public static Message getMessage(JToken[] tokens)
        {
            Message msg = new Message();

            foreach(var m in tokens)
            {
                switch (m["type"].ToString())
                {
                    case "Source":
                        {
                            msg.time = DateTime.FromFileTime(long.Parse(m["time"]?.ToString()));
                            msg.id = long.Parse(m["id"]?.ToString());
                            break;
                        }
                    case "Face":
                        {
                            msg.faces.Add(new MessageFace(int.Parse(m["faceId"].ToString()), m["name"].ToString()));
                            break;
                        }
                    case "Plain":
                        {
                            msg.plains.Add(m["text"].ToString());
                            break;
                        }
                    case "Image":
                        {
                            msg.imgs.Add(new MessageImage(m["url"].ToString(), m["imageId"].ToString()));
                            break;
                        }
                    case "At":
                        {
                            msg.ats.Add(new MessageAt(long.Parse(m["target"].ToString()), m["display"].ToString()));
                            break;
                        }
                    case "Quote":
                        {
                            var origin = m["origin"].ToArray();
                            msg.quote = getMessage(origin);
                            msg.quote.fromGroup = long.Parse(m["groupId"].ToString());
                            msg.quote.from = long.Parse(m["senderId"].ToString());
                            break;
                        }
                    case "Xml":
                        {
                            msg.xml = m["xml"].ToString();
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }

            msg.str = "";
            foreach(var plain in msg.plains)
            {
                msg.str += plain;
            }

            return msg;
        }

        
        public static GroupPermission getPermission(string str)
        {
            if (str == "OWNER") return GroupPermission.Owner;
            else if (str == "MEMBER") return GroupPermission.Member;
            else return GroupPermission.Admin;
        }

        public static void responseAddFriend(EventAddFriend eaf, MiraiAddFriendRespose response)
        {
            string url = $"http://{HOST}:{PORT}/resp/newFriendRequestEvent";
            JObject json = new JObject();
            json.Add("sessionKey",SESSION);
            json.Add("eventId", eaf.id);
            json.Add("fromId", eaf.qq);
            json.Add("groupId", eaf.fromgroup);
            json.Add("operate", (int)response);
            json.Add("message", eaf.desc);

            JObject res = WebHelper.postJson(url, json);

        }

        public static void responseAddGroup(EventAddGroup eaf, MiraiAddGroupResponse response)
        {
            string url = $"http://{HOST}:{PORT}/resp/memberJoinRequestEvent";
            JObject json = new JObject();
            json.Add("sessionKey", SESSION);
            json.Add("eventId", eaf.id);
            json.Add("fromId", eaf.fromqq);
            json.Add("groupId", eaf.group);
            json.Add("operate", (int)response);
            json.Add("message", eaf.desc);

            JObject res = WebHelper.postJson(url, json);

        }

        public static void sendMessage(Message msg)
        {
            string url;
            JObject json = new JObject();
            json.Add("sessionKey", SESSION);


            JArray chain = new JArray();
            JObject jobj = new JObject();
            if (!string.IsNullOrWhiteSpace(msg.xml))
            {
                jobj = new JObject();
                jobj.Add("type", "Xml");
                jobj.Add("xml", msg.xml);
            }
            foreach(var at in msg.ats)
            {
                jobj = new JObject();
                jobj.Add("type", "At");
                jobj.Add("target", at.qq);
                jobj.Add("display", at.desc);
                chain.Add(jobj);
            }
            foreach(var img in msg.imgs)
            {
                jobj = new JObject();
                jobj.Add("type", "Image");
                jobj.Add("imageId", img.id);
                jobj.Add("url", img.url);
                jobj.Add("path", img.path);
                chain.Add(jobj);
            }
            foreach(var face in msg.faces)
            {
                jobj = new JObject();
                jobj.Add("type", "Face");
                jobj.Add("faceId", face.faceid);
                jobj.Add("name", face.name);
                chain.Add(jobj);
            }
            jobj = new JObject();
            jobj.Add("type", "Plain");
            jobj.Add("text", msg.str);
            chain.Add(jobj);


            if (msg.isRecall)
            {
                // recall
                if(msg.toGroup <= 0)
                {
                    // friend recall
                    url = $"http://{HOST}:{PORT}/resp/memberJoinRequestEvent";
                }
                else
                {
                    // group recall
                    url = $"http://{HOST}:{PORT}/resp/memberJoinRequestEvent";
                }
                
            }
            else if (msg.isTempMsg)
            {
                // temp group.
                url = $"http://{HOST}:{PORT}/sendTempMessage";
                json.Add("qq", msg.to);
                json.Add("group", msg.toGroup);
                json.Add("messageChain", chain);
            } 
            else if(msg.toGroup <= 0)
            {
                // private
                url = $"http://{HOST}:{PORT}/sendFriendMessage";
                json.Add("target", msg.to);
                json.Add("messageChain", chain);
            }
            else
            {
                // group
                url = $"http://{HOST}:{PORT}/sendGroupMessage";
                json.Add("target", msg.toGroup);
                json.Add("messageChain", chain);
            }

            


            JObject res = WebHelper.postJson(url, json);

        }

        public static IEnumerable<UserInfo> getFriendList()
        {
            List<UserInfo> users = new List<UserInfo>();
            string url = $"http://{HOST}:{PORT}/friendList?sessionKey={SESSION}";
            JArray res = WebHelper.getJsonArray(url);
            foreach(JToken token in res)
            {
                UserInfo user = new UserInfo();
                user.qq = long.Parse(token["id"].ToString());
                user.name = token["nickname"].ToString();
                user.remark = token["remark"].ToString();
                users.Add(user);
            }

            return users;
        }

        public static IEnumerable<GroupInfo> getGroupList()
        {
            List<GroupInfo> groups = new List<GroupInfo>();
            string url = $"http://{HOST}:{PORT}/groupList?sessionKey={SESSION}";
            var res = WebHelper.getJsonArray(url);
            foreach (JToken token in res)
            {
                GroupInfo g = new GroupInfo();
                g.id = long.Parse(token["id"].ToString());
                g.name = token["name"].ToString();
                g.myPermission = getPermission(token["permission"].ToString());
                g.members = new List<UserInfo>();
                groups.Add(g);
            }

            return groups;
        }

        public static void getGroupDetail(GroupInfo group)
        {
            string url = $"http://{HOST}:{PORT}/memberList?sessionKey={SESSION}&target={group.id}";
            JObject res = WebHelper.getJson(url);
            group.members = new List<UserInfo>();
            foreach (JToken token in res.Values().ToArray())
            {
                UserInfo user = new UserInfo();
                user.qq = long.Parse(token["id"].ToString());
                user.name = token["memberName"].ToString();
                user.remark = token["remark"].ToString();
                group.members.Add(user);
            }


            url = $"http://{HOST}:{PORT}/groupConfig?sessionKey={SESSION}&target={group.id}";
            res = WebHelper.getJson(url);
            group.announcement = res["announcement"].ToString();
            group.allowMemberInvite = bool.Parse(res["allowMemberInvite"].ToString());
            group.confessTalk = bool.Parse(res["confessTalk"].ToString());
            group.autoApprove = bool.Parse(res["autoApprove"].ToString());
            group.anonymousChat = bool.Parse(res["anonymousChat"].ToString());
        }

        public static void setGroupDetail(GroupInfo group)
        {
            string url = $"http://{HOST}:{PORT}/groupConfig";
            JObject json = new JObject();

            json.Add("sessionKey", SESSION);
            json.Add("target", group.id);
            JObject jconfig = new JObject();
            jconfig.Add("name", group.name);
            jconfig.Add("announcement", group.announcement);
            jconfig.Add("confessTalk", group.confessTalk);
            jconfig.Add("allowMemberInvite", group.allowMemberInvite);
            jconfig.Add("autoApprove", group.autoApprove);
            jconfig.Add("anonymousChat", group.anonymousChat);
            json.Add("config", jconfig);

            JObject res = WebHelper.postJson(url, json);
        }



        public static void getGroupMemberDetail(UserInfo user, long group)
        {
            string url = $"http://{HOST}:{PORT}/memberInfo?sessionKey={SESSION}&target={group}&memberId={user.qq}";
            JObject res = WebHelper.getJson(url);

            user.remarkInGroup[group] = res["name"].ToString();
            user.titleInGroup[group] = res["specialTitle"].ToString();
        }

        public static void setGroupMemberDetail(long group, UserInfo user)
        {
            string url = $"http://{HOST}:{PORT}/memberInfo";
            JObject json = new JObject();

            json.Add("sessionKey", SESSION);
            json.Add("target", group);
            json.Add("memberId", user.qq);
            JObject jconfig = new JObject();
            jconfig.Add("name", user.remarkInGroup[group]);
            jconfig.Add("specialTitle", user.titleInGroup[group]);
            json.Add("info", jconfig);

            JObject res = WebHelper.postJson(url, json);
        }

        public static void setGroupMute(long group, bool isMute)
        {
            string url = "";
            JObject json = new JObject();
            if (isMute)
            {
                url = $"http://{HOST}:{PORT}/muteAll";
            }
            else
            {
                url = $"http://{HOST}:{PORT}/unmuteAll";
            }

            json.Add("sessionKey", SESSION);
            json.Add("target", group);

            JObject res = WebHelper.postJson(url, json);
        }
        public static void setGroupMemberMute(long group, long qq, bool isMute, int seconds = 1)
        {
            string url = "";
            JObject json = new JObject();
            if (isMute)
            {
                url = $"http://{HOST}:{PORT}/mute";
                json.Add("time", seconds);
            }
            else
            {
                url = $"http://{HOST}:{PORT}/unmute";
            }

            json.Add("sessionKey", SESSION);
            json.Add("target", group);
            json.Add("member", qq);
            

            JObject res = WebHelper.postJson(url, json);
        }

        public static void setGroupMemberKick(long group, long qq, string msg = "已踢，，，")
        {
            string url = "";
            JObject json = new JObject();

            url = $"http://{HOST}:{PORT}/kick";

            json.Add("sessionKey", SESSION);
            json.Add("target", group);
            json.Add("member", qq);
            json.Add("msg", msg);

            JObject res = WebHelper.postJson(url, json);
        }

        public static void setGroupQuit(long group)
        {
            string url = "";
            JObject json = new JObject();

            url = $"http://{HOST}:{PORT}/quit";

            json.Add("sessionKey", SESSION);
            json.Add("target", group);

            JObject res = WebHelper.postJson(url, json);
        }

        public async static void workGetEvent()
        {
            ClientWebSocket ws = new ClientWebSocket();
            try
            {
                //CancellationToken token = new CancellationTokenSource().Token;
                await ws.ConnectAsync(new Uri($"ws://{HOST}:{PORT}/all?sessionKey={SESSION}"), CancellationToken.None);
                while (ISLINK)
                {
                    byte[] buffer = new byte[1024];
                    MemoryStream ms = new MemoryStream(1024);
                    try
                    {
                        WebSocketReceiveResult result;
                        while (!(result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None)).EndOfMessage)
                        {
                            ms.Write(buffer, 0, result.Count);
                        }
                        if (result.MessageType == WebSocketMessageType.Close && ms.Length == 0)
                        {
                            Console.WriteLine("receive socket error.");
                            continue;
                            //throw new WebSocketException(10054);
                        }
                        ms.Write(buffer, 0, result.Count);
                        JObject j = JObject.Parse(Encoding.UTF8.GetString(ms.ToArray()));
                        Console.WriteLine(j.ToString());
                        switch (j["type"].ToString())
                        {
                            case "BotOnlineEvent":
                                {
                                    break;
                                }
                            case "BotOfflineEventActive":
                                {
                                    break;
                                }
                            case "BotOfflineEventForce":
                                {
                                    break;
                                }
                            case "BotOfflineEventDropped":
                                {
                                    break;
                                }
                            case "BotReloginEvent":
                                {
                                    break;
                                }
                            case "BotInvitedJoinGroupRequestEvent":
                                {
                                    EventAddGroup eaf = new EventAddGroup();
                                    eaf.name = j["nickName"].ToString();
                                    eaf.group = long.Parse(j["fromId"].ToString());
                                    eaf.desc = j["message"].ToString();
                                    eaf.id = long.Parse(j["eventId"].ToString());
                                    eaf.groupName = j["groupName"].ToString();

                                    handleAddGroup(eaf);
                                    break;
                                }

                            case "FriendMessage":
                                {
                                    var msgs = j["messageChain"].ToArray();
                                    Message msg = getMessage(msgs);
                                    msg.from = long.Parse(j["sender"]["id"].ToString());
                                    msg.fromName = j["sender"]["nickname"].ToString();
                                    msg.fromGroup = 0;

                                    handleMiraiMsg(msg);
                                    break;
                                }
                            case "GroupMessage":
                                {
                                    var msgs = j["messageChain"].ToArray();
                                    Message msg = getMessage(msgs);
                                    msg.from = long.Parse(j["sender"]["id"].ToString());
                                    msg.fromName = j["sender"]["memberName"].ToString();
                                    msg.fromGroup = long.Parse(j["sender"]["group"]["id"].ToString());

                                    msg.fromPermission = getPermission(j["sender"]["permission"].ToString());
                                    msg.fromGroupName = j["sender"]["group"]["name"].ToString();
                                    msg.fromMyPermission = getPermission(j["sender"]["group"]["permission"].ToString());

                                    handleMiraiMsg(msg);
                                    break;
                                }
                            case "TempMessage":
                                {
                                    var msgs = j["messageChain"].ToArray();
                                    Message msg = getMessage(msgs);
                                    msg.from = long.Parse(j["sender"]["id"].ToString());
                                    msg.fromName = j["sender"]["memberName"].ToString();
                                    msg.fromGroup = long.Parse(j["sender"]["group"]["id"].ToString());

                                    msg.fromPermission = getPermission(j["sender"]["permission"].ToString());
                                    msg.fromGroupName = j["sender"]["group"]["name"].ToString();
                                    msg.fromMyPermission = getPermission(j["sender"]["group"]["permission"].ToString());

                                    msg.isTempMsg = true;

                                    handleMiraiMsg(msg);
                                    break;
                                }

                            case "GroupRecallEvent":
                                {
                                    //var msgs = j["messageChain"].ToArray();
                                    Message msg = new Message();
                                    msg.id = long.Parse(j["messageId"].ToString());
                                    msg.to = long.Parse(j["authorid"].ToString());
                                    //msg.time = long.Parse(j["time"].ToString());
                                    msg.from = long.Parse(j["operator"]["id"].ToString());
                                    msg.fromName = j["operator"]["memberName"].ToString();
                                    msg.fromGroup = long.Parse(j["operator"]["group"]["id"].ToString());

                                    msg.fromPermission = getPermission(j["operator"]["permission"].ToString());
                                    msg.fromGroupName = j["operator"]["group"]["name"].ToString();
                                    msg.fromMyPermission = getPermission(j["operator"]["group"]["permission"].ToString());

                                    msg.isRecall = true;

                                    handleMiraiRecall(msg);
                                    break;
                                }
                            case "FriendRecallEvent":
                                {
                                    Message msg = new Message();
                                    msg.id = long.Parse(j["messageId"].ToString());
                                    msg.to = long.Parse(j["authorid"].ToString());
                                    //msg.time = long.Parse(j["time"].ToString());
                                    msg.from = long.Parse(j["operator"]["id"].ToString());
                                    msg.fromName = j["operator"]["memberName"].ToString();

                                    msg.isRecall = true;

                                    handleMiraiRecall(msg);
                                    break;
                                }

                            case "BotGroupPermissionChangeEvent":
                                {
                                    break;
                                }
                            case "BotMuteEvent":
                                {
                                    break;
                                }
                            case "BotUnmuteEvent":
                                {
                                    break;
                                }
                            case "BotJoinGroupEvent":
                                {
                                    break;
                                }
                            case "BotLeaveEventActive":
                                {
                                    break;
                                }
                            case "BotLeaveEventKick":
                                {
                                    break;
                                }

                            case "GroupNameChangeEvent":
                                {
                                    break;
                                }
                            case "GroupEntranceAnnouncementChangeEvent":
                                {
                                    break;
                                }
                            case "GroupMuteAllEvent":
                                {
                                    break;
                                }
                            case "GroupAllowAnonymousChatEvent":
                                {
                                    break;
                                }
                            case "GroupAllowConfessTalkEvent":
                                {
                                    break;
                                }
                            case "GroupAllowMemberInviteEvent":
                                {
                                    break;
                                }

                            case "MemberJoinEvent":
                                {
                                    break;
                                }
                            case "MemberLeaveEventKick":
                                {
                                    break;
                                }
                            case "MemberLeaveEventQuit":
                                {
                                    break;
                                }
                            case "MemberCardChangeEvent":
                                {
                                    break;
                                }
                            case "MemberSpecialTitleChangeEvent":
                                {
                                    break;
                                }
                            case "MemberPermissionChangeEvent":
                                {
                                    break;
                                }
                            case "MemberMuteEvent":
                                {
                                    break;
                                }
                            case "MemberUnmuteEvent":
                                {
                                    break;
                                }
                            case "NewFriendRequestEvent":
                                {
                                    EventAddFriend eaf = new EventAddFriend();
                                    eaf.name = j["nick"].ToString();
                                    eaf.qq = long.Parse(j["fromId"].ToString());
                                    eaf.desc = j["message"].ToString();
                                    eaf.id = long.Parse(j["eventId"].ToString());
                                    eaf.fromgroup = long.Parse(j["groupId"].ToString());

                                    handleAddFriend(eaf);
                                    break;
                                }
                            case "MemberJoinRequestEvent":
                                {
                                    EventAddGroup eaf = new EventAddGroup();
                                    eaf.name = j["nickName"].ToString();
                                    eaf.group = long.Parse(j["fromId"].ToString());
                                    eaf.desc = j["message"].ToString();
                                    eaf.id = long.Parse(j["eventId"].ToString());
                                    eaf.groupName = j["groupName"].ToString();

                                    handleAddGroup(eaf);
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
                        //FileHelper.Log(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
                FileHelper.Log(ex);
            }
        }


        public static bool Link(string host, int port, string key, long qq)
        {
            HOST = host;
            PORT = port;
            JObject res = postAuth(key);
            if(GetReturnCode(res) == MiraiReturnCode.Success)
            {
                // success
                string newSession = res["session"].ToString();
                if (ISLINK)
                {
                    // delete old
                    res = postRelease(SESSION, QQ);
                    Console.Write("delete old session:" + res["code"].ToString());
                    if(GetReturnCode(res) == MiraiReturnCode.Success)
                    {
                        //delete success
                        ISLINK = false;
                    }
                }

                res = postVerify(newSession, qq);
                if (GetReturnCode(res) == MiraiReturnCode.Success)
                {
                    // new link success
                    SESSION = newSession;
                    QQ = qq;
                    ISLINK = true;

                    new Thread(workGetEvent).Start();

                    return true;
                }
                else
                {
                    // error
                    Console.Write("link error:" + res["code"].ToString());
                }
            }
            else
            {
                // auth fail
                Console.Write("auth error:" + res["code"].ToString());
            }
            return false;
        }

        public static bool Exit()
        {
            if (!ISLINK)
            {
                return true;
            }
            else
            {
                var res = postRelease(SESSION, QQ);
                if(GetReturnCode(res) == MiraiReturnCode.Success)
                {
                    // release success
                    return true;
                }
                return false;
            }
        }




        public static bool SendFriendMessage(Message msg)
        {
            string url = $"{HOST}:{PORT}/sendFriendMessage";

            JObject json = new JObject();
            json.Add("sessionKey", SESSION);
            json.Add("target", msg.to);
            JObject messageChain = new JObject();
            JObject textmsg = new JObject();
            textmsg.Add("type", "Plain");
            textmsg.Add("text", msg.str);
            messageChain.Add(textmsg);
            json.Add("messageChain", messageChain);


            JObject res = WebHelper.postJson(url, json);
            if (GetReturnCode(res) == MiraiReturnCode.Success)
            {
                // ok
                return true;
            }
            else
            {
                Console.WriteLine("send private error:" + res["code"].ToString());
                return false;
            }

            //return true;
        }
    }











}
