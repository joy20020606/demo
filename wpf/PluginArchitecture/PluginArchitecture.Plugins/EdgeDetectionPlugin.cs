using System;
using System.Collections.Generic;
using PluginArchitecture.Core;

namespace PluginArchitecture.Plugins;

public class EdgeDetectionPlugin : IAnalysisPlugin
{
    private readonly List<PluginParameter> _parameters;

    public EdgeDetectionPlugin()
    {
        _parameters = new List<PluginParameter>
        {
            new PluginParameter
            {
                Key = "threshold",
                DisplayName = "邊緣閾值",
                Value = 128,
                DefaultValue = 128,
                Type = PluginParameterType.Integer,
                Min = 0,
                Max = 255
            }
        };
    }

    public string Name => "邊緣偵測";
    public string Description => "使用 Sobel 運算子偵測影像邊緣";
    public string Version => "1.0.0";
    public bool IsEnabled { get; set; } = true;
    public IReadOnlyList<PluginParameter> Parameters => _parameters;

    public byte[] Process(byte[] inputPixels, int width, int height)
    {
        int threshold = (int)_parameters[0].Value;
        byte[] output = new byte[inputPixels.Length];

        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                int idx = (y * width + x) * 4;

                int gxR = GetGx(inputPixels, width, x, y, 0);
                int gxG = GetGx(inputPixels, width, x, y, 1);
                int gxB = GetGx(inputPixels, width, x, y, 2);

                int gyR = GetGy(inputPixels, width, x, y, 0);
                int gyG = GetGy(inputPixels, width, x, y, 1);
                int gyB = GetGy(inputPixels, width, x, y, 2);

                int magR = (int)Math.Sqrt(gxR * gxR + gyR * gyR);
                int magG = (int)Math.Sqrt(gxG * gxG + gyG * gyG);
                int magB = (int)Math.Sqrt(gxB * gxB + gyB * gyB);

                int mag = (int)((magR + magG + magB) / 3.0);

                byte val = mag > threshold ? (byte)255 : (byte)0;
                output[idx] = val;
                output[idx + 1] = val;
                output[idx + 2] = val;
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

    private int GetPixel(byte[] pixels, int width, int x, int y, int channel)
    {
        return pixels[(y * width + x) * 4 + channel];
    }

    private int GetGx(byte[] pixels, int width, int x, int y, int ch)
    {
        return -GetPixel(pixels, width, x - 1, y - 1, ch)
               + GetPixel(pixels, width, x + 1, y - 1, ch)
               - 2 * GetPixel(pixels, width, x - 1, y, ch)
               + 2 * GetPixel(pixels, width, x + 1, y, ch)
               - GetPixel(pixels, width, x - 1, y + 1, ch)
               + GetPixel(pixels, width, x + 1, y + 1, ch);
    }

    private int GetGy(byte[] pixels, int width, int x, int y, int ch)
    {
        return GetPixel(pixels, width, x - 1, y - 1, ch)
               + 2 * GetPixel(pixels, width, x, y - 1, ch)
               + GetPixel(pixels, width, x + 1, y - 1, ch)
               - GetPixel(pixels, width, x - 1, y + 1, ch)
               - 2 * GetPixel(pixels, width, x, y + 1, ch)
               - GetPixel(pixels, width, x + 1, y + 1, ch);
    }
}
