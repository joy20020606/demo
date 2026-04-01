using System.Collections.Generic;

namespace PluginArchitecture.Core;

public interface IAnalysisPlugin
{
    string Name { get; }
    string Description { get; }
    string Version { get; }
    bool IsEnabled { get; set; }
    IReadOnlyList<PluginParameter> Parameters { get; }
    byte[] Process(byte[] inputPixels, int width, int height);
    void SetParameter(string key, object value);
}

public class PluginParameter
{
    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public object Value { get; set; } = null!;
    public object DefaultValue { get; set; } = null!;
    public PluginParameterType Type { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
}

public enum PluginParameterType
{
    Integer,
    Double,
    Boolean,
    String
}
