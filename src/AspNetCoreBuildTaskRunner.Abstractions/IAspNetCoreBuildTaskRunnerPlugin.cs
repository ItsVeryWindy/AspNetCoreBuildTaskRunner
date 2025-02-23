using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreBuildTaskRunner.Abstractions
{
    public interface IAspNetCoreBuildTaskRunnerPlugin
    {
        Task ExecuteAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken);
    }
}
