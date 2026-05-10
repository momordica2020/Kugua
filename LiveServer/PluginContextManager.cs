using KuguaSdk;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace KuguaServer
{
    /// <summary>
    /// 插件生命周期与 DI 容器管理器
    /// </summary>
    class PluginContextManager
    {
        private CollectibleAssemblyLoadContext _alc;
        private ServiceProvider _serviceProvider;
        private CancellationTokenSource _pluginRunningCts;
        private readonly Dictionary<string, Delegate> _dynamicEndpoints = new();

        public async Task ReloadPluginsAsync(string sourceFolder, WebApplication app)
        {
            await UnloadAsync(); // 先卸载旧的
            _dynamicEndpoints.Clear(); // 清空旧路由映射


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
                System.Diagnostics.Debug.WriteLine($"[Host] 加载程序集: {assembly.GetName().Name}");
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
            services.AddLogging();
            _serviceProvider = services.BuildServiceProvider();

            // 3. 启动所有需要长期运行的插件 (指令发布者)

            var mediator = _serviceProvider.GetService<IMediator>();
            var entries = _serviceProvider.GetServices<IPluginEntry>();

            if (mediator != null)
            {
                foreach (var entry in entries)
                {
                    // 在后台启动插件逻辑，不阻塞主线程
                    //entry.RegisterRoutes(app);
                    _ = Task.Run(() => entry.StartAsync(mediator, _pluginRunningCts.Token));
                }
            }
        }


        /// <summary>
        /// 卸载所有正在运行的插件
        /// </summary>
        /// <returns></returns>
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

}