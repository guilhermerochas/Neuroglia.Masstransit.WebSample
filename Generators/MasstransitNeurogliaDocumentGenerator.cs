using FluentValidation;

using MasstransitTest.WebApiTest.Exceptions;

using Microsoft.Extensions.Options;

using Neuroglia.AsyncApi.Configuration;
using Neuroglia.AsyncApi.Models;
using Neuroglia.AsyncApi.Services.FluentBuilders;
using Neuroglia.AsyncApi.Services.Generators;

namespace MasstransitTest.WebApiTest.Generators;

public class MasstransitNeurogliaDocumentGenerator : AsyncApiDocumentGenerator
{
    public MasstransitNeurogliaDocumentGenerator(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override Task<IEnumerable<AsyncApiDocument>> GenerateAsync(IEnumerable<Type> markupTypes, AsyncApiDocumentGenerationOptions options)
    {
        var optionsAsyncApiGeneratorOptions = ServiceProvider.GetService<IOptions<AsyncApiGenerationOptions>>();
        var asyncApiDocumentValidators = ServiceProvider.GetServices<IValidator<AsyncApiDocument>>();

        if (optionsAsyncApiGeneratorOptions is null)
        {
            throw new ModuleNotFoundException($"Not able to in the DI Container module of type {nameof(IOptions<AsyncApiGenerationOptions>)}");
        }

        if (asyncApiDocumentValidators is null)
        {
            throw new ModuleNotFoundException($"Not able to in the DI Container modules of type {nameof(IValidator<AsyncApiDocument>)}");
        }


        var defaultDocumentConfiguration = new AsyncApiDocumentBuilder(ServiceProvider, asyncApiDocumentValidators);
        optionsAsyncApiGeneratorOptions.Value.DefaultDocumentConfiguration.Invoke(defaultDocumentConfiguration);

        return Task.FromResult<IEnumerable<AsyncApiDocument>>(new List<AsyncApiDocument>
        {
            defaultDocumentConfiguration.Build()
        });
    }
}
