using System.Numerics;
using ImageMagick;

namespace ImageDb;

/// <summary>
/// Provides the ability to calculate a perceptual hash for an image.
/// A perceptual hash means input images that are similar will have a
/// similar hash value. Hash distance is calculated using Hamming distance.
/// </summary>
public static class PHash
{
    private const int HashWidth = 32;
    private const int HashHeight = 32;

    /// <summary>
    /// Return the distance between two hashes. This is the Hamming distance
    /// between the two hashes.
    /// The distance is calculated by XORing the values and taking the number
    /// of 1s bits, or pop count.
    /// </summary>
    /// <param name="a">First hash.</param>
    /// <param name="b">Second hash.</param>
    /// <returns>Hash distance.</returns>
    public static int HashDistance(long a, long b) => BitOperations.PopCount((ulong) (a ^ b));

    /// <summary>
    /// Get the brightness value for a pixel.
    /// The pixel is assumed to be in grayscale format with channel 0 corresponding
    /// to the pixel brightness.
    /// </summary>
    /// <param name="pixel">Image pixel</param>
    /// <returns>Brightness in [0, 1]</returns>
    private static double Value(IPixel<byte> pixel)
    {
        const double scale = byte.MaxValue;
        return pixel[0] / scale;
    }

    /// <summary>
    /// Write image data to the given array. The image is assumed to be in grayscale format
    /// with channel 0 corresponding to the pixel brightness.
    /// </summary>
    /// <param name="pixels">Image pixels</param>
    /// <param name="result">Output array</param>
    private static void GetImageData(IPixelCollection<byte> pixels, double[,] result)
    {
        for (var x = 0; x < result.GetLength(0); x++)
        {
            for (var y = 0; y < result.GetLength(1); y++)
            {
                result[x, y] = Value(pixels[x, y]);
            }
        }
    }

    /// <summary>
    /// Perform a Discrete Cosine Transform on the values, with normalization.
    /// This computes a 2D DCT-II and stores the result in the original array.
    /// </summary>
    /// <param name="values">Values</param>
    private static void Dct2(double[,] values)
    {
        var data = (double[,]) values.Clone();
        
        // Compute constant values
        var scale1 = Math.PI / (values.GetLength(0) * 2);
        var scale2 = Math.PI / (values.GetLength(1) * 2);
        // Normalization scalars
        var norm1First = 1 / Math.Sqrt(values.GetLength(0));
        var norm1Else = Math.Sqrt(2d / values.GetLength(0));
        var norm2First = 1 / Math.Sqrt(values.GetLength(1));
        var norm2Else = Math.Sqrt(2d / values.GetLength(1));

        // Returns the 'First' scalar if the index == 0, otherwise the 'Else' scalar
        double Norm1(int i) => i == 0 ? norm1First : norm1Else;
        double Norm2(int i) => i == 0 ? norm2First : norm2Else;
        
        for (var y = 0; y < values.GetLength(1); y++)
        {
            for (var x = 0; x < values.GetLength(0); x++)
            {
                var k1 = x;
                var k2 = y;
                // Perform DCT row- and column-wise.
                values[x, y] = Norm1(x) * Norm2(y) * Enumerable.Range(0, values.GetLength(0))
                    .SelectMany(n1 => Enumerable.Range(0, values.GetLength(1))
                        .Select(n2 => data[n1, n2] * Math.Cos(scale1 * (2 * n1 + 1) * k1) * Math.Cos(scale2 * (2 * n2 + 1) * k2)))
                    .Sum();
            }
        }
    }

    /// <summary>
    /// Scale the given array so that all values are in [0, 1].
    /// </summary>
    /// <param name="values">Values</param>
    private static void Normalize(double[,] values)
    {
        var max = double.MinValue;
        var min = double.MaxValue;
        for (var x = 0; x < values.GetLength(0); x++)
        {
            for (var y = 0; y < values.GetLength(1); y++)
            {
                max = Math.Max(values[x, y], max);
                min = Math.Min(values[x, y], min);
            }
        }
        
        for (var x = 0; x < values.GetLength(0); x++)
        {
            for (var y = 0; y < values.GetLength(1); y++)
            {
                values[x, y] = (values[x, y] - min) / (max - min);
            }
        }
    }

    /// <summary>
    /// Compute a perceptual hash for an image file.
    /// </summary>
    /// <param name="path">Image path</param>
    /// <returns>Image hash</returns>
    public static long ComputeHash(string path)
    {
        using var image = new MagickImage(path);
        return ComputeHash(image);
    }
    
    /// <summary>
    /// Compute a perceptual hash for an image.
    /// </summary>
    /// <param name="image">Image</param>
    /// <returns>Image hash</returns>
    public static long ComputeHash(MagickImage image)
    {
        // Downscale the image and convert to grayscale
        var newSize = new MagickGeometry(HashWidth, HashHeight);
        newSize.IgnoreAspectRatio = true;
        image.Resize(newSize);
        image.Grayscale(PixelIntensityMethod.Average);

        // Compute the Discrete Cosine Transformation for the image
        var pixels = image.GetPixels();
        var data = new double[HashWidth, HashHeight];
        GetImageData(pixels, data);
        Dct2(data);

        // At this point the relevant data is in the upper-left 8x8 square.
        // Calculate the average pixel brightness
        var average = 0d;
        const int smallWidth = HashWidth / 4;
        const int smallHeight = HashHeight / 4;
        for (var x = 0; x < smallWidth; x++)
        {
            for (var y = 0; y < smallHeight; y++)
            {
                // Ignore the first pixel, which is many times brighter than the others
                if (x == 0 && y == 0) continue;
                average += data[x, y];
            }
        }
        const int size = smallWidth * smallHeight;
        average /= smallWidth * smallHeight;

        // Use the average to create a hash value
        var hash = 0L;
        for (var i = 0; i < size; i++)
        {
            var x = i % smallWidth;
            var y = i / smallWidth;
            if (data[x, y] >= average) hash |= 1L << i;
        }
        return hash;
    }
}