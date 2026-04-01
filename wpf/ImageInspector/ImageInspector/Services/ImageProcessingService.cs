using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageInspector.Models;

namespace ImageInspector.Services;

public class ImageProcessingService
{
    public WriteableBitmap ApplyAdjustments(BitmapSource source, ImageAdjustment adjustment)
    {
        int width = source.PixelWidth;
        int height = source.PixelHeight;
        int stride = width * 4;
        byte[] pixels = new byte[height * stride];

        var formatted = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
        formatted.CopyPixels(pixels, stride, 0);

        byte[] lut = BuildLut(adjustment);

        for (int i = 0; i < pixels.Length; i += 4)
        {
            pixels[i]     = lut[pixels[i]];
            pixels[i + 1] = lut[pixels[i + 1]];
            pixels[i + 2] = lut[pixels[i + 2]];
        }

        var result = new WriteableBitmap(width, height, source.DpiX, source.DpiY, PixelFormats.Bgra32, null);
        result.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
        return result;
    }

    private static byte[] BuildLut(ImageAdjustment adj)
    {
        byte[] lut = new byte[256];
        for (int i = 0; i < 256; i++)
        {
            double v = i / 255.0;
            v = Math.Pow(v, 1.0 / adj.Gamma);
            v = (v - 0.5) * adj.Contrast + 0.5;
            v += adj.Brightness / 255.0;
            v = Math.Clamp(v, 0.0, 1.0);
            lut[i] = (byte)(v * 255.0);
        }
        return lut;
    }
}
