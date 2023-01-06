using MassTransit;

using MasstransitTest.WebApiTest.Extensions;
using MasstransitTest.WebApiTest.Handlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(masstransitConfiguration =>
{
    masstransitConfiguration.AddConsumer<FirstCommandHandler>().Endpoint(endpoint => endpoint.Name = $"{nameof(FirstCommandHandler)}.Endpoint");
    masstransitConfiguration.AddConsumer<SecondCommandHandler>().Endpoint(endpoint => endpoint.Name = $"{nameof(SecondCommandHandler)}.Endpoint");

    masstransitConfiguration.UsingInMemory((context, configurator) =>
    {
        configurator.ReceiveEndpoint(new TemporaryEndpointDefinition(), receiveEndpointConfiguration =>
        {
            receiveEndpointConfiguration.ConfigureConsumer<FirstCommandHandler>(context);
            receiveEndpointConfiguration.ConfigureConsumer<SecondCommandHandler>(context);
        });
    });
});

builder.Services.AddMasstransitAsyncApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseNeuroliaMasstransit();
app.MapControllers();

app.Run();
