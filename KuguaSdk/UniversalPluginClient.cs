using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Data;
using System.Threading.Tasks;

namespace KuguaSdk
{
    /// <summary>
    /// 插件客户端通信接口
    /// </summary>
    public class UniversalPluginClient
    {
        private HubConnection _connection;
        private readonly string _pluginName;
        private readonly string _pluginUrl;
        private readonly string _hubUrl;
        private readonly string _sdkName = "KuguaSdk";
        public UniversalPluginClient(string pluginName, string pluginUrl, string hubUrl)
        {
            _pluginName = pluginName;
            _pluginUrl = pluginUrl;
            _hubUrl = hubUrl;
        }


        /// <summary>
        /// 监听器启动
        /// </summary>
        /// <returns></returns>
        public async Task StartAsync()
        {
            // 1. 配置连接地址，并带上自动重连机制（WithAutomaticReconnect）
            // WithAutomaticReconnect 是灵魂：主程序重启时，插件会自动不断重试连接，直到主程序复活！
            _connection = new HubConnectionBuilder()
                .WithUrl($"{_hubUrl}?pluginName={_pluginName}")
                .WithAutomaticReconnect()
                .Build();

            // 2. 注册监听【主程序生死状态】的事件
            _connection.Closed += (error) =>
            {
                Console.WriteLine($"[{_sdkName}] 主程序已离线或崩溃！正在等待自动重连...");
                return Task.CompletedTask;
            };

            _connection.Reconnecting += (error) =>
            {
                Console.WriteLine($"[{_sdkName}] 正在尝试重新连接到主程序...");
                return Task.CompletedTask;
            };

            _connection.Reconnected += (connectionId) =>
            {
                Console.WriteLine($"[{_sdkName}] 重新连接主程序成功！已恢复双向感知。");
                return Task.CompletedTask;
            };

            // 3. 注册接收来自主程序（或其他插件）的指令
            _connection.On<string, string>("HostCommand", (cmdType, data) =>
            {
                Console.WriteLine($"[{_sdkName}] 类型: {cmdType}, 数据: {data}");
                // 在这里写插件的业务逻辑
            });

            _connection.On<string, string>("OnPluginStatusChanged", (name, status) =>
            {
                Console.WriteLine($"[{_sdkName}] 插件【{name}】状态变更为: {status}");
            });

            // 4. 正式启动连接
            try
            {
                await _connection.StartAsync();
                Console.WriteLine($"[{_sdkName}]【{_pluginName}】启动成功，已成功连接到主程序中心！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{_sdkName}]初始连接失败: {ex.Message}。将依赖自动重连机制...");
            }
        }

        /// <summary>
        /// 插件上报数据
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        public async Task ReportDataAsync(string dataType, string jsonData)
        {
            if (_connection.State == HubConnectionState.Connected)
            {
                await _connection.InvokeAsync("SendDataToHost", _pluginName, dataType, jsonData);
            }
        }
    }
}
