using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;


namespace Kugua.Core
{
    /// <summary>
    /// 感知插件的连接、断开，并转发消息
    /// </summary>
    public class CommunicationHub : Hub
    {
        // 当有插件连接成功时触发
        public override async Task OnConnectedAsync()
        {
            // 插件连接时，通常会在 URL 参数里带上自己的名字，比如 ?pluginName=MusicPlugin
            var httpContext = Context.GetHttpContext();
            string pluginName = httpContext?.Request.Query["pluginName"] ?? "未知插件";

            // 将连接 ID 与插件名绑定（这里简单打印，实际可用 ConcurrentDictionary 存起来）
            Console.WriteLine($"[感知] 插件【{pluginName}】已上线！连接ID: {Context.ConnectionId}");

            // 通知其他所有在线插件，某个插件上线了
            await Clients.Others.SendAsync("OnPluginStatusChanged", pluginName, "Online");

            await base.OnConnectedAsync();
        }

        // 当有插件断开连接（或崩溃重启）时触发
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine($"[感知] 有插件断开连接。连接ID: {Context.ConnectionId}");

            // 可以在这里通知其他插件或主程序处理善后工作
            await base.OnDisconnectedAsync(exception);
        }

        // 通用方法：插件向主程序发送数据，主程序处理
        public void SendDataToHost(string sender, string dataType, string jsonData)
        {
            Console.WriteLine($"[收到数据] 来自【{sender}】的【{dataType}】: {jsonData}");

            // 在这里可以分发给你的 Kugua Bot 做出响应
            // if(dataType == "QQMessage") { BotHost.Instance.SendGroupMessage(...) }
        }

        // 通用方法：插件 A 想调戏 插件 B，或者主程序群发给所有插件
        public async Task BroadcastToAllPlugins(string sender, string msg)
        {
            await Clients.All.SendAsync("ReceiveBroadcast", sender, msg);
        }
    }
}
