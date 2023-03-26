using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.PixelFormats;
using TIM2Lib4ImageSharp.ImgLib.Formats;
using TIM2Lib4ImageSharp.ImgLib.Formats.Implementation;

namespace TIM2Lib4ImageSharp;

public sealed class TIM2Decoder : ImageDecoder 
{
    private TIM2Decoder() { }
    
    public static TIM2Decoder Instance { get; } = new();
    
    private static TIM2Texture<TPixel> GetTim2Texture<TPixel>(Stream stream) where TPixel : unmanaged, IPixel<TPixel>
    {
        TIM2TextureSerializer<TPixel> serializer = new TIM2TextureSerializer<TPixel>();
        if (serializer.Open(stream) is not TIM2Texture<TPixel> tex)
        {
            throw new TextureFormatException("Serializer gave texture in another format");
        }

        return tex;
    }

    protected override Image<TPixel> Decode<TPixel>(DecoderOptions options, Stream stream, CancellationToken cancellationToken)
    {
        TIM2Texture<TPixel> tex = GetTim2Texture<TPixel>(stream);
        
        return tex.GetImage();
    }

    protected override Image Decode(DecoderOptions options, Stream stream, CancellationToken cancellationToken)
    {
        ImageInfo info = Identify(options, stream, cancellationToken);
        stream.Position = 0;
        
        switch (info.PixelType.BitsPerPixel)
        {
            case 2:
                return Decode<Abgr32>(options, stream, cancellationToken);
            case 3:
                return Decode<Rgb24>(options, stream, cancellationToken);
            case 4:
                return Decode<Rgba32>(options, stream, cancellationToken);
            default:
                throw new InvalidImageContentException("Pixel type not supported");
        }
    }

    protected override ImageInfo Identify(DecoderOptions options, Stream stream, CancellationToken cancellationToken)
    {
        List<TIM2SegmentParameters> parameters = TIM2TextureSerializerHelper.OpenSegmentsParameters(stream);
        if (parameters.Count > 1)
        {
            throw new UnknownImageFormatException("TIM2 image with multiple segments not supported");
        }

        TIM2SegmentParameters param = parameters.First();
        return new ImageInfo(new PixelTypeInfo(param.colorSize), new Size(param.width,param.height), null);
    }
}