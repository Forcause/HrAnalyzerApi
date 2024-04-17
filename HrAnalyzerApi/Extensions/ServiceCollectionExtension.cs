using HrAnalyzer.Data.Services;

namespace HrAnalyzerApi.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IAnalyzerService, AnalyzerService>();

        return serviceCollection;
    }
}