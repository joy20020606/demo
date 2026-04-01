using System.Collections.Generic;

namespace PluginArchitecture.Core;

public interface IPluginFactory
{
    IAnalysisPlugin CreatePlugin(string pluginName);
    IReadOnlyList<string> AvailablePlugins { get; }
}
