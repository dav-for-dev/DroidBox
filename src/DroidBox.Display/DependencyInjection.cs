using Microsoft.Extensions.DependencyInjection;

namespace DroidBox.Display;

public static class DependencyInjection
{
    public static IServiceCollection AddDisplayServices(this IServiceCollection services)
    {
        services.AddSingleton<IDisplayRenderer, ScreenStreamRenderer>();
        services.AddSingleton<DisplayConfiguration>();
        return services;
    }
}
