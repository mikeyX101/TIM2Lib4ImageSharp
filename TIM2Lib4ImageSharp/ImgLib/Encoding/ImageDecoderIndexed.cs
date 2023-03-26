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

using TIM2Lib4ImageSharp.ImgLib.Filters;

namespace TIM2Lib4ImageSharp.ImgLib.Encoding
{
    public class ImageDecoderIndexed<TPixel> : ImageDecoder<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {

        protected byte[]? pixelData;

        protected int width, height;

        protected IndexCodec indexCodec;

        protected TPixel[]? grayScale;

        public ImageDecoderIndexed(byte[]? pixelData, int width, int height, IndexCodec codec, TPixel[]? palette = null, ImageFilter? imageFilter = null, PaletteFilter<TPixel>? paletteFilter = null)
        {
            this.pixelData = pixelData;
            if (imageFilter != null)
                this.pixelData = imageFilter.Defilter(pixelData);

            this.width = width;
            this.height = height;
            this.indexCodec = codec;

            grayScale = new TPixel[1 << codec.BitDepth];

            for (int i = 0; i < grayScale.Length; i++)
            {
                grayScale[i] = Color.FromRgba((byte)(i * (256 / grayScale.Length)), (byte)(i * (256 / grayScale.Length)), (byte)(i * (256 / grayScale.Length)), 255).ToPixel<TPixel>();
            }

            if (paletteFilter != null && palette != null)
            {
                Palette = paletteFilter.Defilter(palette);
            }
            else if (palette == null)
            {
                palette = (TPixel[])grayScale.Clone();
                Palette = palette;
            }
            else
            {
                Palette = (TPixel[])palette.Clone();
            }
        }

        public TPixel[]? Palette { get; set; }

        public Image<TPixel> ReferenceImage => DecodeImage(grayScale);

        public Image<TPixel>? DecodeImage()
        {
            return DecodeImage(Palette);
        }

        private Image<TPixel>? DecodeImage(TPixel[]? pal)
        {
            if (width == 0 || height == 0 || pal is null)
            {
                return null;
            }

            Image<TPixel> bmp = new Image<TPixel>(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bmp[x, y] = pal[indexCodec.GetPixelIndex(pixelData, width, height, x, y)];
                }
            }
            return bmp;
        }

    }
}