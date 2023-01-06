namespace MasstransitTest.WebApiTest.Extensions;

public static class IEndpointRouterBuilderExtensions
{
    public static IEndpointRouteBuilder UseNeuroliaMasstransit(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapRazorPages();
        return endpointRouteBuilder;
    }
}
