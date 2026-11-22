using DroidBox.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DroidBox.Runtime.Qemu;

public static class DependencyInjection
{
    public static IServiceCollection AddQemuRuntime(this IServiceCollection services)
    {
        services.AddSingleton<QemuProvider>();
        return services;
    }
}
