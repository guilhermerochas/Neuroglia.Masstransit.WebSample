using MassTransit;

using MasstransitTest.WebApiTest.Commands;

namespace MasstransitTest.WebApiTest.Handlers;

public class SecondCommandHandler : IConsumer<SecondCommand>
{
    private readonly ILogger<SecondCommandHandler> _logger;

    public SecondCommandHandler(ILogger<SecondCommandHandler> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<SecondCommand> context)
    {
        _logger.LogInformation($"Teste do segundo comand exibindo o valor {context.Message.Value}");
        return Task.CompletedTask;
    }
}
