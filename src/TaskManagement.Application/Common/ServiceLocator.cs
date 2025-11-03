using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Application.Common.Interfaces;

namespace TaskManagement.Application.Common;

/// <summary>
/// Service locator implementation that wraps IServiceProvider.
/// </summary>
public class ServiceLocator : IServiceLocator
{
    private readonly IServiceProvider _serviceProvider;

    public ServiceLocator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public T GetService<T>() where T : notnull
    {
        return _serviceProvider.GetService<T>()!;
    }

    /// <inheritdoc />
    public T GetRequiredService<T>() where T : notnull
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    /// <inheritdoc />
    public object GetRequiredService(Type serviceType)
    {
        return _serviceProvider.GetRequiredService(serviceType);
    }

    /// <inheritdoc />
    public IEnumerable<object> GetServices(Type serviceType)
    {
        return _serviceProvider.GetServices(serviceType).Where(s => s != null)!;
    }
}