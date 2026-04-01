using System.Collections.Generic;
using PluginArchitecture.Core;

namespace PluginArchitecture.Plugins;

public class BinarizationPlugin : IAnalysisPlugin
{
    private readonly List<PluginParameter> _parameters;

    public BinarizationPlugin()
    {
        _parameters = new List<PluginParameter>
        {
            new PluginParameter
            {
                Key = "threshold",
                DisplayName = "二值化閾值",
                Value = 128,
                DefaultValue = 128,
                Type = PluginParameterType.Integer,
                Min = 0,
                Max = 255
            }
        };
    }

    public string Name => "二值化";
    public string Description => "依閾值將灰階影像轉換為黑白影像";
    public string Version => "1.0.0";
    public bool IsEnabled { get; set; } = true;
    public IReadOnlyList<PluginParameter> Parameters => _parameters;

    public byte[] Process(byte[] inputPixels, int width, int height)
    {
        int threshold = (int)_parameters[0].Value;
        byte[] output = new byte[inputPixels.Length];

        for (int i = 0; i < inputPixels.Length; i += 4)
        {
            int gray = (inputPixels[i] + inputPixels[i + 1] + inputPixels[i + 2]) / 3;
            byte val = gray >= threshold ? (byte)255 : (byte)0;
            output[i] = val;
            output[i + 1] = val;
            output[i + 2] = val;
            output[i + 3] = 255;
        }

        return output;
    }

    public void SetParameter(string key, object value)
    {
        var param = _parameters.Find(p => p.Key == key);
        if (param != null) param.Value = value;
    }
}
