using AspNetCoreBuildTaskRunner;
using AspNetCoreBuildTaskRunner.Abstractions;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

var cts = new CancellationTokenSource();

Console.CancelKeyPress += (s, e) =>
{
    cts.Cancel();
    e.Cancel = true;
};

var assemblyPaths = args[0].Split(';');
var pluginPaths = args[1].Split(';');

foreach (var assemblyPath in assemblyPaths)
{
    var context = new CustomAssemblyLoadContext(assemblyPath);

    if (context.Assembly.EntryPoint is null)
        return 0;

    using var scope = context.EnterContextualReflection();

    var pluginContexts = pluginPaths
        .Select(x => new CustomAssemblyLoadContext(x, context))
        .Select(x => x.Assembly)
        .SelectMany(x => x.ExportedTypes)
        .Where(x => x.GetInterfaces().Any(y => y == typeof(IAspNetCoreBuildTaskRunnerPlugin)))
        .Select(x => (IAspNetCoreBuildTaskRunnerPlugin)Activator.CreateInstance(x)!)
        .ToList();

    var manualReset = new ManualResetEventSlim(false);

    var factory = HostFactoryResolver.ResolveHostFactory(
        context.Assembly,
        Debugger.IsAttached ? Timeout.InfiniteTimeSpan : TimeSpan.FromSeconds(10),
        configureHostBuilder: hb =>
        {
            var hostBuilder = (IHostBuilder)hb;

            hostBuilder.ConfigureServices(sc =>
            {
                sc
                    .AddSingleton<IServer, NullServer>()
                    .AddSingleton<IHostLifetime, NullHostLifetime>()
                    .AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

                for (var i = sc.Count - 1; i >= 0; i--)
                {
                    var service = sc[i];

                    if (service is IHostedService)
                        sc.RemoveAt(i);
                }
            });
        },
        entrypointCompleted: (ex) =>
        {
            manualReset.Set();
        },
        stopApplication: false);

    if (factory is null)
        return 0;

    using var host = (IHost)factory([$"--{HostDefaults.ApplicationKey}={context.Assembly.GetName().FullName}"]);

    if (host is null)
    {
        Console.WriteLine("No host detected.");
        return 1;
    }

    var applicationLifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

    using var registration = applicationLifetime.ApplicationStarted.Register(manualReset.Set);

    manualReset.Wait();

    foreach (var pluginContext in pluginContexts)
    {
        await pluginContext.ExecuteAsync(host.Services, cts.Token);
    }
}

return 0;