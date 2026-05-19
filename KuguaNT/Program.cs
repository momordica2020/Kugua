using Kugua;
using Kugua.Core;
using KuguaSdk.Protocol;
using Microsoft.AspNetCore.SignalR;
class Program
{
    static void Main(string[] args)
    {
        // 机器人初始化
        BotHost.Instance.Start();



        // 初始化本地 WebAPI 服务器
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSignalR();
        //int localPort = 5000;
        //int.TryParse(Config.Instance.App.Net.LocalWS, out localPort );
        builder.WebHost.ConfigureKestrel(options => options.ListenLocalhost(SdkConfig.HUB_PORT));

        var app = builder.Build();

        // 2. 映射通信路由地址（所有插件都连这个地址）
        app.MapHub<CommunicationHub>(SdkConfig.HUB_SUBNAME);

        // 3. 让主程序持有一个 HubContext，以便主程序可以随时主动“勾搭”插件
        var hubContext = app.Services.GetRequiredService<IHubContext<CommunicationHub>>();


        app.Run();

        // 5. 当程序被关闭（如按下 Ctrl+C 或关闭窗口）时触发
        System.Diagnostics.Debug.WriteLine("-- BOT EXIT --");
        Logger.Log("正在停止Kugua...");
        BotHost.Instance.Stop();
    }
}