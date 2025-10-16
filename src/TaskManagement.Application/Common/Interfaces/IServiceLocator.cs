namespace TaskManagement.Application.Common.Interfaces;

/// <summary>
/// Service locator interface for dependency resolution.
/// </summary>
public interface IServiceLocator
{
    /// <summary>
    /// Gets a service of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of service to get.</typeparam>
    /// <returns>The service instance.</returns>
    T GetService<T>() where T : notnull;

    /// <summary>
    /// Gets a required service of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of service to get.</typeparam>
    /// <returns>The service instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the service is not registered.</exception>
    T GetRequiredService<T>() where T : notnull;

    /// <summary>
    /// Gets a required service of the specified type.
    /// </summary>
    /// <param name="serviceType">The type of service to get.</param>
    /// <returns>The service instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the service is not registered.</exception>
    object GetRequiredService(Type serviceType);

    /// <summary>
    /// Gets all services of the specified type.
    /// </summary>
    /// <param name="serviceType">The type of service to get.</param>
    /// <returns>All service instances.</returns>
    IEnumerable<object> GetServices(Type serviceType);
}
