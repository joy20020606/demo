namespace PluginArchitecture.Core;

public class PluginMetadata
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string AssemblyPath { get; set; } = string.Empty;
    public bool IsBuiltIn { get; set; }
}
