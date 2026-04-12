using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MediatR;
using KuguaSdk;
using Microsoft.Extensions.DependencyInjection;

namespace KuguaServer
{
    class Program
    {
        private static PluginContextManager _pluginManager = new();
        private static FileSystemWatcher _watcher;
        private static CancellationTokenSource _debounceCts;

        static async Task Main(string[] args)
        {
            Console.WriteLine("主程序已启动，监控 Plugins 目录...");

            var pluginFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
            Directory.CreateDirectory(pluginFolder);

            // 初始加载
            await _pluginManager.ReloadPluginsAsync(pluginFolder);

            // 设置目录监控
            _watcher = new FileSystemWatcher(pluginFolder, "*.dll")
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                EnableRaisingEvents = true
            };

            _watcher.Changed += OnPluginChanged;
            _watcher.Created += OnPluginChanged;

            Console.WriteLine("输入 'q' 退出程序。");
            while (Console.ReadLine() != "q") { }

            await _pluginManager.UnloadAsync();
        }

        // 防抖动处理：防止文件复制过程中触发多次加载或读取被占用的文件
        private static void OnPluginChanged(object sender, FileSystemEventArgs e)
        {
            _debounceCts?.Cancel();
            _debounceCts = new CancellationTokenSource();
            var token = _debounceCts.Token;

            Task.Delay(1000, token).ContinueWith(async t =>
            {
                if (!t.IsCanceled)
                {
                    Console.WriteLine($"\n[Host] 检测到插件变更，正在重新加载...");
                    await _pluginManager.ReloadPluginsAsync(Path.GetDirectoryName(e.FullPath));
                }
            });
        }
    }

    // 可回收的加载上下文
    class CollectibleAssemblyLoadContext : AssemblyLoadContext
    {
        public CollectibleAssemblyLoadContext() : base(isCollectible: true) { }
    }

    // 插件生命周期与 DI 容器管理器
    class PluginContextManager
    {
        private CollectibleAssemblyLoadContext _alc;
        private ServiceProvider _serviceProvider;
        private CancellationTokenSource _pluginRunningCts;

        public async Task ReloadPluginsAsync(string sourceFolder)
        {
            await UnloadAsync(); // 先卸载旧的

            _alc = new CollectibleAssemblyLoadContext();
            _pluginRunningCts = new CancellationTokenSource();

            var tempFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TempPlugins");
            if (Directory.Exists(tempFolder))
                Directory.Delete(tempFolder, true);
            Directory.CreateDirectory(tempFolder);

            var loadedAssemblies = new List<Assembly>();

            // 1. Shadow Copy 并加载到 ALC
            foreach (var file in Directory.GetFiles(sourceFolder, "*.dll"))
            {
                var tempFile = Path.Combine(tempFolder, Path.GetFileName(file));
                File.Copy(file, tempFile, true);
                var assembly = _alc.LoadFromAssemblyPath(tempFile);
                loadedAssemblies.Add(assembly);
                Console.WriteLine($"[Host] 加载程序集: {assembly.GetName().Name}");
            }

            // 2. 重建 DI 容器与 MediatR
            var services = new ServiceCollection();

            if (loadedAssemblies.Any())
            {
                // 将加载的程序集交给 MediatR 扫描 Handler
                services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(loadedAssemblies.ToArray()));

                // 注册所有实现了 IPluginEntry 的插件启动类
                var entryTypes = loadedAssemblies.SelectMany(a => a.GetTypes())
                    .Where(t => typeof(KuguaSdk.IPluginEntry).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                foreach (var type in entryTypes)
                {
                    services.AddTransient(typeof(IPluginEntry), type);
                }
            }

            _serviceProvider = services.BuildServiceProvider();

            // 3. 启动所有需要长期运行的插件 (指令发布者)
            var mediator = _serviceProvider.GetService<IMediator>();
            var entries = _serviceProvider.GetServices<IPluginEntry>();

            if (mediator != null)
            {
                foreach (var entry in entries)
                {
                    // 在后台启动插件逻辑，不阻塞主线程
                    _ = Task.Run(() => entry.StartAsync(mediator, _pluginRunningCts.Token));
                }
            }
        }

        public async Task UnloadAsync()
        {
            // 通知旧插件停止运行
            if (_pluginRunningCts != null)
            {
                _pluginRunningCts.Cancel();
                _pluginRunningCts.Dispose();
            }

            // 销毁 DI 容器
            if (_serviceProvider != null)
            {
                await _serviceProvider.DisposeAsync();
            }

            // 卸载程序集上下文
            if (_alc != null)
            {
                _alc.Unload();
                // 强制垃圾回收以彻底清理卸载的程序集
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }
    //static class Program
    //{
    //    /// <summary>
    //    /// 应用程序的主入口点。
    //    /// </summary>
    //    [STAThread]
    //    static void Main()
    //    {
    //        Application.EnableVisualStyles();
    //        Application.SetCompatibleTextRenderingDefault(false);
    //        Application.Run(new FormMonitor());
    //    }
    //}
}
