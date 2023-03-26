using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace Tim2Lib;

public sealed class Tim2Decoder : ImageDecoder
{
    protected override Image<TPixel> Decode<TPixel>(DecoderOptions options, Stream stream, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override Image Decode(DecoderOptions options, Stream stream, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override ImageInfo Identify(DecoderOptions options, Stream stream, CancellationToken cancellationToken)
    {
        int colorDepth;
        
        
        new ImageInfo(new PixelTypeInfo(), new Size());
    }
}