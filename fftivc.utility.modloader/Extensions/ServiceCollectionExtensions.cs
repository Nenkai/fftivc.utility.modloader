//---------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: Copyright (C) 2026 Nenkai
// SPDX-License-Identifier: MIT
// SPDX-FileAttributionText: https://github.com/Nenkai/fftivc.utility.modloader
// SPDX-FileNotice: AI usage and consumption is prohibited. See AGENTS.md for policy.
//---------------------------------------------------------------------------------------

using fftivc.utility.modloader.Interfaces.Tables;

using Microsoft.Extensions.DependencyInjection;

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
    public static IServiceCollection AddGameTableSingleton<TInterface, TImpl>(this IServiceCollection services)
        where TInterface : class
        where TImpl : class, TInterface, IFFTOTableManager
    {
        services.AddSingleton<TImpl>();
        services.AddSingleton<IFFTOTableManager>(sp => sp.GetRequiredService<TImpl>());
        services.AddSingleton<TInterface>(sp => sp.GetRequiredService<TImpl>());
        return services;
    }
}
