using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using PluginArchitecture.Core;

namespace PluginArchitecture.App.Services;

public class PluginLoader
{
    private readonly List<IAnalysisPlugin> _loadedPlugins = new();

    public IReadOnlyList<IAnalysisPlugin> LoadedPlugins => _loadedPlugins;

    public void LoadBuiltInPlugins(IEnumerable<IAnalysisPlugin> plugins)
    {
        foreach (var plugin in plugins)
            _loadedPlugins.Add(plugin);
    }

    public IReadOnlyList<IAnalysisPlugin> LoadFromDirectory(string directoryPath)
    {
        var discovered = new List<IAnalysisPlugin>();

        if (!Directory.Exists(directoryPath))
            return discovered;

        foreach (var dllPath in Directory.GetFiles(directoryPath, "*.dll"))
        {
            try
            {
                var assembly = Assembly.LoadFrom(dllPath);
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsClass && !type.IsAbstract && typeof(IAnalysisPlugin).IsAssignableFrom(type))
                    {
                        if (Activator.CreateInstance(type) is IAnalysisPlugin plugin)
                        {
                            discovered.Add(plugin);
                            _loadedPlugins.Add(plugin);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        return discovered;
    }

    public void UnloadPlugin(IAnalysisPlugin plugin)
    {
        _loadedPlugins.Remove(plugin);
    }
}
