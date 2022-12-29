using MassTransit;

using SaunterTest.Extensions;
using SaunterTest.Handlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(masstransitConfiguration =>
{
    masstransitConfiguration.AddConsumer<TestHandler>();
    masstransitConfiguration.AddConsumer<TestHandler2>();

    masstransitConfiguration.UsingInMemory((context, configurator) =>
    {
        configurator.ReceiveEndpoint(new TemporaryEndpointDefinition(), receiveEndpointConfiguration =>
        {
            receiveEndpointConfiguration.ConfigureConsumer<TestHandler>(context);
            receiveEndpointConfiguration.ConfigureConsumer<TestHandler2>(context);
        });

        configurator.ConfigureEndpoints(context);
    });
});

builder.Services.AddMasstransitWithSaunter();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();

app.MapMasstransitWithSaunter();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

await app.RunAsync();
