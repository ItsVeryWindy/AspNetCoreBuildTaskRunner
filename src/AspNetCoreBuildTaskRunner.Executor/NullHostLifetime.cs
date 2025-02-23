using Microsoft.Extensions.Hosting;

namespace AspNetCoreBuildTaskRunner.Executor;

internal class NullHostLifetime : IHostLifetime
{
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task WaitForStartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}