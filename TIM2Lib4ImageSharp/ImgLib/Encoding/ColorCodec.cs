//Copyright (C) 2014+ Marco (Phoenix) Calautti.

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, version 2.0.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License 2.0 for more details.

//A copy of the GPL 2.0 should have been included with the program.
//If not, see http://www.gnu.org/licenses/

//Official repository and contact information can be found at
//http://github.com/marco-calautti/Rainbow

using TIM2Lib4ImageSharp.ImgLib.Encoding.Implementation;
using TIM2Lib4ImageSharp.ImgLib.Filters;


// TODO: Fix interface of ColorCodec, supporting matrices/width x height inputs.
namespace TIM2Lib4ImageSharp.ImgLib.Encoding
{

    public static class ColorCodecs
    {
        public static readonly ColorCodec<Rgb24> CODEC_24BIT_RGB = new ColorCodec24BitRGB();
        public static readonly ColorCodec<Rgba32> CODEC_32BIT_RGBA = new ColorCodec32BitRGBA();
        public static readonly ColorCodec<Abgr32> CODEC_16BITLE_ABGR = new ColorCodec16BitLEABGR();

        public static ColorCodec<TPixel> GetColorCodecByPixelType<TPixel>() where TPixel : unmanaged, IPixel<TPixel>
        {
            Type pixelType = typeof(TPixel);
            if (pixelType == typeof(Rgb24))
            {
                return (CODEC_24BIT_RGB as ColorCodec<TPixel>)!;
            }
            else if (pixelType == typeof(Rgba32))
            {
                return (CODEC_32BIT_RGBA as ColorCodec<TPixel>)!;
            }
            else if (pixelType == typeof(Abgr32))
            {
                return (CODEC_16BITLE_ABGR as ColorCodec<TPixel>)!;
            }
            
            throw new ArgumentException("Illegal Pixel type!");
        }
    }
    
    /// <summary>
    /// Base class for implementing color decoders. A color decoder is an object that converts a byte array of raw color data into a sequence of Colors.
    /// One example of implementation is ColorDecoder32BitRGBA, that converts quadruples of bytes representing the four components RGBA.
    /// </summary>
    public abstract class ColorCodec<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        /// <summary>
        /// Decodes an array of bytes, representing a sequence of color data in some format,
        /// into an array of Colors objects.
        /// </summary>
        /// <param name="colors">The encoded byte array representing the sequence of colors to decode.</param>
        /// <param name="start">the position in the byte array from which to start.</param>
        /// <param name="length">How many bytes have to be considered. If the length is such that the last pixel cannot be completely decoded, then this pixel is discarded.</param>
        /// <returns></returns>
        public abstract TPixel[] DecodeColors(byte[] colors, int start, int length);

        /// <summary>
        /// See Color[] DecodeColors(byte[] colors, int start, int length) documentation.
        /// </summary>
        public virtual TPixel[] DecodeColors(byte[] colors)
        {
            return DecodeColors(colors, 0, colors.Length);
        }

        /// <summary>
        /// Encodes an array of colors into an array of bytes, following the encoding of this object.
        /// </summary>
        /// <param name="colors">The array of colors to be encoded.</param>
        /// <param name="start">The position of the first color to be encoded.</param>
        /// <param name="length">How many colors need to be encoded.</param>
        public abstract byte[] EncodeColors(TPixel[] colors, int start, int length);

        /// <summary>
        /// See byte[] EncodeColors(Color[] colors, int start, int length) documentation.
        /// </summary>
        /// <param name="colors"></param>
        /// <returns></returns>
        public virtual byte[] EncodeColors(TPixel[] colors)
        {
            return EncodeColors(colors, 0, colors.Length);
        }

        /// <summary>
        /// Returns the size in bit of one color encoded in the format implemented by this ColorDecoder.
        /// </summary>
        public abstract int BitDepth { get; }
    }

}