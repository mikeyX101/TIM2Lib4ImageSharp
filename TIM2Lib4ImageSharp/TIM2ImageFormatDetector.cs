using System.Diagnostics.CodeAnalysis;
using SixLabors.ImageSharp.Formats;

namespace TIM2Lib4ImageSharp;

public class TIM2FormatDetector : IImageFormatDetector
{
    public bool TryDetectFormat(ReadOnlySpan<byte> header, [UnscopedRef] out IImageFormat? format)
    {
        bool tim2Detected = header.SequenceEqual(TIM2Constants.HeaderBytes);
        if (tim2Detected)
        {
            
        }
    }

    public int HeaderSize => TIM2Constants.HeaderBytes.Length;
}