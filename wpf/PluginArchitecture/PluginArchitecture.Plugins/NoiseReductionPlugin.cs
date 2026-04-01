using System;
using System.Collections.Generic;
using PluginArchitecture.Core;

namespace PluginArchitecture.Plugins;

public class NoiseReductionPlugin : IAnalysisPlugin
{
    private readonly List<PluginParameter> _parameters;

    public NoiseReductionPlugin()
    {
        _parameters = new List<PluginParameter>
        {
            new PluginParameter
            {
                Key = "kernelSize",
                DisplayName = "卷積核大小",
                Value = 3,
                DefaultValue = 3,
                Type = PluginParameterType.Integer,
                Min = 3,
                Max = 7
            }
        };
    }

    public string Name => "雜訊濾除";
    public string Description => "使用均值模糊濾除影像雜訊";
    public string Version => "1.0.0";
    public bool IsEnabled { get; set; } = true;
    public IReadOnlyList<PluginParameter> Parameters => _parameters;

    public byte[] Process(byte[] inputPixels, int width, int height)
    {
        int kernel = (int)_parameters[0].Value;
        if (kernel % 2 == 0) kernel = 3;
        int half = kernel / 2;

        byte[] output = new byte[inputPixels.Length];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int sumR = 0, sumG = 0, sumB = 0, count = 0;

                for (int ky = -half; ky <= half; ky++)
                {
                    for (int kx = -half; kx <= half; kx++)
                    {
                        int nx = Math.Clamp(x + kx, 0, width - 1);
                        int ny = Math.Clamp(y + ky, 0, height - 1);
                        int nIdx = (ny * width + nx) * 4;
                        sumR += inputPixels[nIdx];
                        sumG += inputPixels[nIdx + 1];
                        sumB += inputPixels[nIdx + 2];
                        count++;
                    }
                }

                int idx = (y * width + x) * 4;
                output[idx] = (byte)(sumR / count);
                output[idx + 1] = (byte)(sumG / count);
                output[idx + 2] = (byte)(sumB / count);
                output[idx + 3] = 255;
            }
        }

        return output;
    }

    public void SetParameter(string key, object value)
    {
        var param = _parameters.Find(p => p.Key == key);
        if (param != null) param.Value = value;
    }
}
