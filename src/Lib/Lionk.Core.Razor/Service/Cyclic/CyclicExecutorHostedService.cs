﻿// Copyright © 2024 Lionk Project

using Lionk.Core.Component;
using Microsoft.Extensions.Hosting;

namespace Lionk.Core.Razor.Service;

/// <summary>
/// This class is used to nest a CyclicExecutorService into a hosted service.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CyclicExecutorHostedService"/> class.
/// </remarks>
/// <param name="cyclicExecutorService">The cyclicExecutorService.</param>
public class CyclicExecutorHostedService(ICyclicExecutorService cyclicExecutorService) : IHostedService
{
    private readonly ICyclicExecutorService _cyclicExecutorService = cyclicExecutorService;

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cyclicExecutorService.Start();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cyclicExecutorService.Stop();
        return Task.CompletedTask;
    }
}
