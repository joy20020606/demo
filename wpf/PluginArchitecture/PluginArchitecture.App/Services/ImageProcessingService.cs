using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PluginArchitecture.Core;

namespace PluginArchitecture.App.Services;

public class ImageProcessingService
{
    public BitmapSource GenerateSampleImage(int width, int height)
    {
        byte[] pixels = new byte[width * height * 4];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int idx = (y * width + x) * 4;
                double dx = x - width / 2.0;
                double dy = y - height / 2.0;
                double dist = Math.Sqrt(dx * dx + dy * dy);
                byte val = (byte)(128 + 127 * Math.Sin(dist / 10.0));

                pixels[idx] = val;
                pixels[idx + 1] = (byte)(val * 0.8);
                pixels[idx + 2] = (byte)(val * 0.6);
                pixels[idx + 3] = 255;
            }
        }

        return BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, pixels, width * 4);
    }

    public BitmapSource ApplyPlugin(BitmapSource source, IAnalysisPlugin plugin)
    {
        int width = source.PixelWidth;
        int height = source.PixelHeight;
        int stride = width * 4;

        var converted = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
        byte[] inputPixels = new byte[height * stride];
        converted.CopyPixels(inputPixels, stride, 0);

        byte[] outputPixels = plugin.Process(inputPixels, width, height);

        return BitmapSource.Create(width, height, source.DpiX, source.DpiY,
            PixelFormats.Bgra32, null, outputPixels, stride);
    }
}
