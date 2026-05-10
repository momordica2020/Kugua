using System.Runtime.Loader;

namespace KuguaServer
{
    /// <summary>
    /// 可回收的加载上下文
    /// </summary>
    class CollectibleAssemblyLoadContext : AssemblyLoadContext
    {
        public CollectibleAssemblyLoadContext() : base(isCollectible: true) { }
    }

}