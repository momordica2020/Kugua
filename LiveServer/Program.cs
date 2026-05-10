using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KuguaServer
{
    class Program
    {
        private static PluginContextManager _pluginManager = new();
        private static WebApplication _app;
        private static FileSystemWatcher _watcher;
        private static CancellationTokenSource _debounceCts;

        [STAThread]
        static async Task Main(string[] args)
        {
            System.Diagnostics.Debug.WriteLine("主程序已启动，监控 Plugins 目录...");

            var builder = WebApplication.CreateBuilder(args);

            // 1. 注册基础服务
            builder.Services.AddControllers().AddNewtonsoftJson(); // 插件路由支持
            builder.Services.AddSingleton(_pluginManager);
            _app = builder.Build();


            // 注册一个万能代理路由，或者让插件管理器接管
            _app.Map("{*url}", async (HttpContext context, PluginContextManager manager) =>
            {
                // 这里也可以留空，由插件通过 manager 动态注入
            });


            var pluginFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
            Directory.CreateDirectory(pluginFolder);

            // 初始加载
            await _pluginManager.ReloadPluginsAsync(pluginFolder, _app);

            // 设置目录监控
            _watcher = new FileSystemWatcher(pluginFolder, "*.dll")
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                EnableRaisingEvents = true
            };

            _watcher.Changed += OnPluginChanged;
            _watcher.Created += OnPluginChanged;

            System.Diagnostics.Debug.WriteLine("输入 'q' 退出程序。");
            while (Console.ReadLine() != "q") { Thread.Sleep(1000); }

            await _pluginManager.UnloadAsync();
        }

        /// <summary>
        /// 防抖动处理：防止文件复制过程中触发多次加载或读取被占用的文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnPluginChanged(object sender, FileSystemEventArgs e)
        {
            _debounceCts?.Cancel();
            _debounceCts = new CancellationTokenSource();
            var token = _debounceCts.Token;

            Task.Delay(1000, token).ContinueWith(async t =>
            {
                if (!t.IsCanceled)
                {
                    System.Diagnostics.Debug.WriteLine($"\n[Host] 检测到插件变更，正在重新加载...");
                    await _pluginManager.ReloadPluginsAsync(Path.GetDirectoryName(e.FullPath), _app);
                }
            });
        }
    }

}