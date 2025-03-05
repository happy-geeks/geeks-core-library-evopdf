using GeeksCoreLibrary.Modules.GclConverters.EvoPdf.Services;
using GeeksCoreLibrary.Modules.GclConverters.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GeeksCoreLibrary.Modules.GclConverters.EvoPdf.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Extension method to add our <see cref="EvoPdfHtmlToPdfConverterService"/> implementation of <see cref="IHtmlToPdfConverterService"/> to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection.</returns>
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IServiceCollection AddEvoPdfHtmlToPdfConverterService(this IServiceCollection services)
    {
        return services.AddTransient<IHtmlToPdfConverterService, EvoPdfHtmlToPdfConverterService>();
    }
}