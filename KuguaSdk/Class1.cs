using MediatR;
namespace KuguaSdk
{
    /// <summary>
    /// 定义一个请求：传入任务名称，返回处理结果字符串
    /// </summary>
    /// <param name="TaskName"></param>
    public record ProcessTaskCommand(string TaskName) : IRequest<string>;

    /// <summary>
    /// 插件入口接口，主程序加载插件后会调用此接口
    /// </summary>
    public interface IPluginEntry
    {
        /// <summary>
        /// 允许插件直接向 Web 主机注册路由
        /// </summary>
        /// <param name="endpoints"></param>
        //void RegisterRoutes(IEndpointRouteBuilder endpoints);



        /// <summary>
        /// 传入 mediator 用于通信，cancellationToken 用于主程序通知插件卸载
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task StartAsync(IMediator mediator, CancellationToken cancellationToken);
    }

}
