using AspNetCoreBuildTaskRunner.Abstractions;
using AspNetCoreBuildTaskRunner.ExampleSharedLibrary;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreBuildTaskRunner.ExamplePlugin
{
    public class PrintSharedClassPlugin : IAspNetCoreBuildTaskRunnerPlugin
    {
        public Task ExecuteAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            Console.WriteLine("I did something");

            var cls = (SharedClass)serviceProvider.GetService(typeof(SharedClass))!;

            foreach (var c in cls.Lines)
            {
                Console.WriteLine(c);
            }

            return Task.CompletedTask;
        }
    }
}
