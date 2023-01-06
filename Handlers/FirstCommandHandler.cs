using MassTransit;

using MasstransitTest.WebApiTest.Commands;

namespace MasstransitTest.WebApiTest.Handlers;

public class FirstCommandHandler : IConsumer<FirstCommand>
{
    private readonly ILogger<FirstCommandHandler> _logger;

    public FirstCommandHandler(ILogger<FirstCommandHandler> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<FirstCommand> context)
    {
        _logger.LogInformation($"Teste do primeiro comand exibindo o valor {context.Message.Value}");
        return Task.CompletedTask;
    }
}
