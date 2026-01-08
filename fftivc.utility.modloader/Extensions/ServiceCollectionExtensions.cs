using fftivc.utility.modloader.Interfaces.Tables;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a game table as the specified interface type aswell as a <see cref="IFFTOTableManager"/> for enumeration purposes.
    /// </summary>
    /// <typeparam name="TInterface"></typeparam>
    /// <typeparam name="TImpl"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddGameTableSingleton<TInterface, TImpl>(
        this IServiceCollection services)
        where TImpl : class, TInterface, IFFTOTableManager
        where TInterface : class
    {
        services.AddSingleton<TImpl>();
        services.AddSingleton<IFFTOTableManager>(sp => sp.GetRequiredService<TImpl>());
        services.AddSingleton<TInterface>(sp => sp.GetRequiredService<TImpl>());
        return services;
    }
}
