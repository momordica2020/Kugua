using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using OpenBLive.Client;
using OpenBLive.Client.Data;
using OpenBLive.Runtime;
using OpenBLive.Runtime.Data;
using OpenBLive.Runtime.Utilities;
using Kugua.Integrations.NTBot;
using LiveServer.NTBot;

namespace LiveServer.Blive
{
    
    internal class BServer
    {//初始化于测试的参数
        public const string AccessKeyId = "2RepcPyOcfrFOPbX2A2QF2Jo";//填入你的accessKeyId，可以在直播创作者服务中心【个人资料】页面获取(https://open-live.bilibili.com/open-manage)
        public const string AccessKeySecret = "IaOhYdtKFEVn54Gi0g49pun0ehDYUR";//填入你的accessKeySecret，可以在直播创作者服务中心【个人资料】页面获取(https://open-live.bilibili.com/open-manage)
        public const string AppId = "1733080868639";//填入你的appId，可以在直播创作者服务中心【我的项目】页面创建应用后获取(https://open-live.bilibili.com/open-manage)
        public const string Code = "E1PWQLBP3E9S0";//填入你的主播身份码Code，可以在互动玩法首页，右下角【身份码】处获取(互玩首页：https://play-live.bilibili.com/)


        public static IBApiClient bApiClient = new BApiClient();
        public static string game_id = string.Empty;
        public static bool IsLive = false;
        static string gameId;

        public static async Task Run()
        {
            //是否为测试环境（一般用户可无视，给专业对接测试使用）
            BApi.isTestEnv = false;

            SignUtility.accessKeyId = AccessKeyId;
            SignUtility.accessKeySecret = AccessKeySecret;
            var appId = AppId;
            var code = Code;
            

            var startInfo = new AppStartInfo();

            var closeTimeStr = "30";
            

            if (!string.IsNullOrEmpty(appId))
            {
                startInfo = await bApiClient.StartInteractivePlay(code, appId);
                if (startInfo?.Code != 0)
                {
                    FormMonitor.SendMsgEvent(startInfo?.Message);
                    return;
                }

                gameId = startInfo?.Data?.GameInfo?.GameId;
                if (gameId != null)
                {
                    game_id = gameId;
                    IsLive = true;
                    FormMonitor.SendMsgEvent("B站连接成功开启，开始心跳，场次ID: " + gameId);
                    InteractivePlayHeartBeat m_PlayHeartBeat = new InteractivePlayHeartBeat(gameId);
                    m_PlayHeartBeat.HeartBeatError += M_PlayHeartBeat_HeartBeatError;
                    m_PlayHeartBeat.HeartBeatSucceed += M_PlayHeartBeat_HeartBeatSucceed;
                    m_PlayHeartBeat.Start();
                    //长链接
                    WebSocketBLiveClient m_WebSocketBLiveClient;
                    m_WebSocketBLiveClient = new WebSocketBLiveClient(startInfo.GetWssLink(), startInfo.GetAuthBody());
                    m_WebSocketBLiveClient.OnDanmaku += WebSocketBLiveClientOnDanmaku;
                    m_WebSocketBLiveClient.OnGift += WebSocketBLiveClientOnGift;
                    m_WebSocketBLiveClient.OnGuardBuy += WebSocketBLiveClientOnGuardBuy;
                    m_WebSocketBLiveClient.OnSuperChat += WebSocketBLiveClientOnSuperChat;
                    m_WebSocketBLiveClient.OnLike += M_WebSocketBLiveClient_OnLike;
                    m_WebSocketBLiveClient.OnEnter += M_WebSocketBLiveClient_OnEnter;
                    m_WebSocketBLiveClient.OnLiveStart += M_WebSocketBLiveClient_OnLiveStart;
                    m_WebSocketBLiveClient.OnLiveEnd += M_WebSocketBLiveClient_OnLiveEnd;
                    //m_WebSocketBLiveClient.Connect();
                    m_WebSocketBLiveClient.Connect(TimeSpan.FromSeconds(30));
                }
                else
                {
                    FormMonitor.SendMsgEvent("开启玩法错误: " + startInfo.ToString());
                }
                await Task.Run(async () =>
                {
                    if(IsLive)
                    {
                        await Task.Delay(1000);
                    }
                    
                    
                    return;
                });
            }
        }

        public static async void Stop()
        {
            var ret = await bApiClient.EndInteractivePlay(AppId, gameId);
            FormMonitor.SendMsgEvent("关闭玩法: " + ret.ToString());
        }

        private static void M_WebSocketBLiveClient_OnLiveEnd(LiveEnd liveEnd)
        {
            StringBuilder sb = new StringBuilder($"直播间[{liveEnd.room_id}]直播结束");
            FormMonitor.SendMsgEvent(sb.ToString());
        }

        private static void M_WebSocketBLiveClient_OnLiveStart(LiveStart liveStart)
        {
            StringBuilder sb = new StringBuilder($"直播间[{liveStart.room_id}]开始直播");
            FormMonitor.SendMsgEvent(sb.ToString());
        }

        private static void M_WebSocketBLiveClient_OnEnter(Enter enter)
        {
            StringBuilder sb = new StringBuilder($"用户[{enter.uname}]进入房间");
            FormMonitor.SendMsgEvent(sb.ToString());
        }

        private static void M_WebSocketBLiveClient_OnLike(Like like)
        {
            StringBuilder sb = new StringBuilder($"用户[{like.uname}]点赞了{like.unamelike_count}次");
            FormMonitor.SendMsgEvent(sb.ToString());
        }

        private static void M_PlayHeartBeat_HeartBeatSucceed()
        {
            FormMonitor.SendMsgEvent("心跳成功");
        }

        private static void M_PlayHeartBeat_HeartBeatError(string json)
        {
            JsonConvert.DeserializeObject<EmptyInfo>(json);
            FormMonitor.SendMsgEvent("心跳失败" + json);
        }

        private static void WebSocketBLiveClientOnSuperChat(SuperChat superChat)
        {
            StringBuilder sb = new StringBuilder($"用户[{superChat.userName}]发送了{superChat.rmb}元的醒目留言内容：{superChat.message}");
            FormMonitor.SendMsgEvent(sb.ToString());
        }

        private static void WebSocketBLiveClientOnGuardBuy(Guard guard)
        {
            StringBuilder sb = new StringBuilder($"用户[{guard.userInfo.userName}]充值了{guard.guardNum}个月[{(guard.guardLevel == 1 ? "总督" : guard.guardLevel == 2 ? "提督" : "舰长")}]大航海");
            FormMonitor.SendMsgEvent(sb.ToString());
        }

        private static void WebSocketBLiveClientOnGift(SendGift sendGift)
        {
            StringBuilder sb = new StringBuilder($"用户[{sendGift.userName}]赠送了{sendGift.giftNum}个[{sendGift.giftName}]");
            FormMonitor.SendMsgEvent(sb.ToString());
        }

        private static void WebSocketBLiveClientOnDanmaku(Dm dm)
        {
            FormMonitor.SendMsgEvent($"用户[{dm.uid}][{dm.open_id}][{dm.userName}]发送弹幕:{dm.msg}");

            LBotRequest msg = new LBotRequest
            {
                userId = dm.uid,
                userName = dm.userName,
                type = "danmu",
                messages = { new MessageInfo(new Text(dm.msg)) }
            };
            WebSocketServer.SendMessageAsync(msg);
        }

    }
}
