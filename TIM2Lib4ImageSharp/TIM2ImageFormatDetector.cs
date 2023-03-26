using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using SixLabors.ImageSharp.Formats;

namespace TIM2Lib4ImageSharp;

public class TIM2ImageFormatDetector : IImageFormatDetector
{
    private TIM2ImageFormatDetector() { }
    
    public int HeaderSize => TIM2Constants.HeaderBytes.Length;
    
    public static TIM2ImageFormatDetector Instance { get; } = new();
    
    public bool TryDetectFormat(ReadOnlySpan<byte> header, [UnscopedRef] out IImageFormat? format)
    {
        format = IsSupportedFileFormat(header) ? TIM2Format.Instance : null;
        return format is not null;
    }

    private bool IsSupportedFileFormat(ReadOnlySpan<byte> header)
    {
        return header.Length >= HeaderSize && BinaryPrimitives.ReadUInt64BigEndian(header) == TIM2Constants.HeaderValue;
    }
}