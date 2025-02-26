using System.Reflection;
using System.Runtime.Loader;

namespace AspNetCoreBuildTaskRunner;

internal class CustomAssemblyLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;
    private readonly CustomAssemblyLoadContext? _subContext;

    public Assembly Assembly { get; }

    public CustomAssemblyLoadContext(string path, CustomAssemblyLoadContext? subContext = null)
    {
        _resolver = new AssemblyDependencyResolver(path);
        _subContext = subContext;
        Assembly = LoadFromAssemblyPath(path);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);

        if (assemblyPath is not null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        if (_subContext is not null)
        {
            return _subContext.Load(assemblyName);
        }

        return null;
    }

    protected override nint LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);

        if (libraryPath != null)
        {
            return LoadUnmanagedDllFromPath(libraryPath);
        }

        return nint.Zero;
    }
}