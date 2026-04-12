using Google.Protobuf.WellKnownTypes;
using Plugin.V1;
using Grpc.Core;
using MediatR;

namespace KuguaHost
{
    public class CommandBusService : CommandBus.CommandBusBase
    {
        private readonly ISender _sender;
        private readonly IPublisher _publisher;

        public CommandBusService(ISender sender, IPublisher publisher)
        {
            _sender = sender;
            _publisher = publisher;
        }

        public override async Task<CommandResult> SendCommand(ProcessCommandRequest request, ServerCallContext context)
        {
            var mediatrRequest = new ProcessCommandRequest
            {
                CommandText = request.CommandText,
                Sender = request.Sender
            };

            var result = await _sender.Send(mediatrRequest, context.CancellationToken);
            if (result is not CommandResult domainResult)
            {
                return new CommandResult
                {
                    Success = false,
                    Message = "内部处理失败：返回类型不匹配"
                };
            }
            return new CommandResult
            {
                Success = domainResult.Success,
                Message = domainResult.Message,
                Data = domainResult.Data ?? string.Empty
            };
        }
        public override async Task<Empty> PublishNotification(CommandProcessedNotification request, ServerCallContext context)
        {
            await _publisher.Publish(new CommandProcessedNotification
            {
                CommandText = request.CommandText,
                ResultMessage = request.ResultMessage,
                ProcessorName = request.ProcessorName
            }, context.CancellationToken);

            return new Empty();
        }
    }
}
