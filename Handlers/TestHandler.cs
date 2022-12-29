using MassTransit;

namespace SaunterTest.Handlers;

public class MyCommmand
{
    public string? Value { get; set; }
}

public class MyCommmand2
{
    public string? Value { get; set; }
}


public class TestHandler : IConsumer<MyCommmand>
{
    public Task Consume(ConsumeContext<MyCommmand> context)
    {
        Console.WriteLine(context.Message.Value);
        return Task.CompletedTask;
    }
}

public class TestHandler2 : IConsumer<MyCommmand2>
{
    public Task Consume(ConsumeContext<MyCommmand2> context)
    {
        Console.WriteLine(context.Message.Value);
        return Task.CompletedTask;
    }
}

