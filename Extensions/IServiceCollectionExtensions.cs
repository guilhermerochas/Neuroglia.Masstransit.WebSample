using System.Reflection;

using FluentValidation;

using MassTransit;
using MassTransit.Definition;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using MassTransit.Registration;

using MasstransitTest.WebApiTest.Generators;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using Neuroglia.AsyncApi;
using Neuroglia.AsyncApi.Configuration;
using Neuroglia.AsyncApi.Services;
using Neuroglia.AsyncApi.Services.FluentBuilders;
using Neuroglia.AsyncApi.Services.Generators;
using Neuroglia.AsyncApi.Services.IO;
using Neuroglia.AsyncApi.Services.Validation;
using Neuroglia.Serialization;

using Newtonsoft.Json;

using YamlDotNet.Serialization;

namespace MasstransitTest.WebApiTest.Extensions;

public static class IServiceCollectionExtensions
{
    private const string DefaultAsyncApiVersion = "2.1.0";

    public static IServiceCollection AddMasstransitAsyncApi(this IServiceCollection services)
    {
        services.AddHttpClient();

        services.AddNewtonsoftJsonSerializer(options => options.NullValueHandling = NullValueHandling.Ignore);

        services.AddYamlDotNetSerializer(serializer =>
            serializer.IncludeNonPublicProperties()
                .WithTypeConverter(new StringEnumSerializer())
                .WithEmissionPhaseObjectGraphVisitor(args => new ChainedObjectGraphVisitor(args.InnerVisitor)));

        services.TryAddSingleton<IAsyncApiDocumentReader, AsyncApiDocumentReader>();
        services.TryAddSingleton<IAsyncApiDocumentWriter, AsyncApiDocumentWriter>();
        services.TryAddTransient<IAsyncApiDocumentBuilder, AsyncApiDocumentBuilder>();
        services.AddValidatorsFromAssemblyContaining<AsyncApiDocumentValidator>(ServiceLifetime.Transient);

        services.TryAddSingleton(provider =>
        {
            var asyncApiGenerationOptions = new AsyncApiGenerationOptions
            {
                DefaultDocumentConfiguration = asyncApiConfiguration =>
                {
                    var consumerRegistration = provider.GetServices<IConsumerRegistration>();
                    var registrationContext = provider.GetService<IBusRegistrationContext>();

                    var masstransitEndpointFormatter = registrationContext?.GetService<IEndpointNameFormatter>() ?? DefaultEndpointNameFormatter.Instance;
                    var bindBusWithBusInstances = registrationContext?.GetServices<Bind<IBus, IBusInstance>>();

                    var currentlyAssembly = Assembly.GetExecutingAssembly().GetName();
                    var version = currentlyAssembly.Version?.ToString(3);

                    asyncApiConfiguration
                            .UseAsyncApi(DefaultAsyncApiVersion)
                            .WithTitle(currentlyAssembly.Name)
                            .WithVersion(version)
                            .WithDescription("The Smartylighting Streetlights API allows you to remotely manage the city lights.")
                            .WithLicense("Apache 2.0", new Uri("https://www.apache.org/licenses/LICENSE-2.0"));

                    var busInstances = bindBusWithBusInstances?.Select(busWithBusInstanceBind => busWithBusInstanceBind.Value);

                    if (busInstances is not null)
                    {
                        foreach (var busInstance in busInstances)
                        {
                            asyncApiConfiguration
                                .UseServer(busInstance.Name, server => server
                                    .WithUrl(busInstance.HostConfiguration.HostAddress)
                                    .WithProtocol(AsyncApiProtocols.Https));
                        }
                    }

                    if (registrationContext is BusRegistrationContext busRegistrationContext)
                    {
                        var consumers = busRegistrationContext?.GetType()
                             ?.GetField("Consumers", BindingFlags.NonPublic | BindingFlags.Instance)
                             ?.GetValue(busRegistrationContext) as IRegistrationCache<IConsumerRegistration>;

                        if (consumers is null)
                        {
                            throw new Exception($"Not able to find property of type {nameof(IRegistrationCache<IConsumerRegistration>)}");
                        }

                        foreach (var consumer in consumers)
                        {
                            var messageType = consumer.Key.GetInterfaces()
                                .Where(i => i.IsGenericType)
                                .FirstOrDefault(@interface => @interface.GetGenericTypeDefinition() == typeof(IConsumer<>))?.GetGenericArguments()[0];

                            if (messageType is not null)
                            {
                                asyncApiConfiguration
                                    .UseChannel(consumer.Key.Name, channel => channel
                                    .DefinePublishOperation(operation => operation
                                        .WithSummary(string.Empty)
                                        .WithOperationId($"On{messageType.Name}")
                                        .UseMessage(message => message
                                            .WithName(messageType.Name)
                                            .OfType(messageType))
                                        )
                                    );
                            }
                        }
                    }
                }
            };

            return Options.Create(asyncApiGenerationOptions);
        });

        services.TryAddTransient<IAsyncApiDocumentGenerator, MasstransitNeurogliaDocumentGenerator>();
        services.AddSingleton<AsyncApiDocumentProvider>();
        services.TryAddSingleton<IAsyncApiDocumentProvider>(provider => provider.GetRequiredService<AsyncApiDocumentProvider>());
        services.TryAddSingleton<IHostedService>(provider => provider.GetRequiredService<AsyncApiDocumentProvider>());

        services.AddAsyncApiUI();
        services.AddRazorPages().AddRazorRuntimeCompilation();

        return services;
    }
}
