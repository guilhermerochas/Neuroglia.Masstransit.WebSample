using System.Reflection;

using MassTransit;
using MassTransit.Configuration;
using MassTransit.DependencyInjection;
using MassTransit.Transports;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using Saunter;
using Saunter.AsyncApiSchema.v2;
using Saunter.Generation;
using Saunter.Serialization;
using Saunter.UI;

namespace SaunterTest.Extensions;

public static class SaunterMasstransitExtensions
{
    private static readonly IEnumerable<string> AsyncApiAvailableServerProtocols = new string[12]
    {
        ServerProtocol.Amqp,
        ServerProtocol.Amqps,
        ServerProtocol.Http,
        ServerProtocol.Https,
        ServerProtocol.Jms,
        ServerProtocol.Kafka,
        ServerProtocol.KafkaSecure,
        ServerProtocol.Mqtt,
        ServerProtocol.SecureMqtt,
        ServerProtocol.Stomp,
        ServerProtocol.Ws,
        ServerProtocol.Wss
    };

    public static IServiceCollection AddMasstransitWithSaunter(this IServiceCollection services)
    {
        services.TryAddTransient<IAsyncApiDocumentProvider, AsyncApiDocumentProvider>();
        services.TryAddTransient<IDocumentGenerator, DocumentGenerator>();
        services.TryAddTransient<IAsyncApiDocumentSerializer, NewtonsoftAsyncApiDocumentSerializer>();

        return services;
    }

    public static IEndpointConventionBuilder MapMasstransitWithSaunter(this IEndpointRouteBuilder endpointBuilder)
    {
        var asyncApiOptions = endpointBuilder.CreateMasstransitAsyncApiOptions();
        var optionsAsyncApiOptions = Options.Create(asyncApiOptions);

        var asyncApiMiddleware = endpointBuilder.CreateApplicationBuilder().UseMiddleware<AsyncApiMiddleware>(optionsAsyncApiOptions).Build();

        var asyncApiUiMiddleware = endpointBuilder.CreateApplicationBuilder()
            .Use((context, next) =>
            {
                context.SetEndpoint(null);
                return next();
            })
            .UseMiddleware<AsyncApiUiMiddleware>(optionsAsyncApiOptions).Build();

        var route = asyncApiOptions.Middleware.Route;
        var uiBaseRoute = asyncApiOptions.Middleware.UiBaseRoute + "{*wildcard}";

        endpointBuilder.MapGet(route, asyncApiMiddleware);
        return endpointBuilder.MapGet(uiBaseRoute, asyncApiUiMiddleware);
    }

    private static AsyncApiOptions CreateMasstransitAsyncApiOptions(this IEndpointRouteBuilder endpointBuilder)
    {
        var consumerRegistration = endpointBuilder.ServiceProvider.GetServices<IConsumerRegistration>();
        var registrationContext = endpointBuilder.ServiceProvider.GetService<IBusRegistrationContext>();

        var masstransitEndpointFormatter = registrationContext?.GetService<IEndpointNameFormatter>() ?? DefaultEndpointNameFormatter.Instance;
        var bindBusWithBusInstances = registrationContext?.GetServices<Bind<IBus, IBusInstance>>();

        var consumerDefinitions = consumerRegistration.Select(x => x.GetDefinition(registrationContext));

        var currentlyAssembly = Assembly.GetExecutingAssembly().GetName();
        var version = currentlyAssembly.Version?.ToString();

        return new AsyncApiOptions
        {
            AsyncApi = new AsyncApiDocument
            {
                Info = new Info(currentlyAssembly.Name, version),
                Servers = bindBusWithBusInstances?.Select(busWithBusInstanceBind => busWithBusInstanceBind.Value)
                    .Where(busInstance => AsyncApiAvailableServerProtocols.Contains(busInstance.HostConfiguration.HostAddress.Scheme))
                    .ToDictionary(busInstance => busInstance.Name, busInstance =>
                    {
                        var uriString = busInstance.HostConfiguration.HostAddress.ToString();
                        var protocolScheme = busInstance.HostConfiguration.HostAddress.Scheme;

                        return new Server(uriString, protocolScheme);
                    }),
            },
        };
    }
}