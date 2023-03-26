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
    public class ImageDecoderDirectColor<TPixel> : ImageDecoder<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        protected byte[]? pixelData;
        protected int width, height;

        protected ColorCodec<TPixel> decoder;

        public ImageDecoderDirectColor(byte[]? pixelData, int width, int height, ColorCodec<TPixel> decoder, ImageFilter? imageFilter = null)
        {
            this.pixelData = pixelData;
            if (imageFilter != null)
            {
                this.pixelData = imageFilter.Defilter(pixelData);
            }

            this.width = width;
            this.height = height;
            this.decoder = decoder;
        }

        public Image<TPixel>? DecodeImage()
        {
            if (width == 0 || height == 0 || pixelData is null)
            {
                return null;
            }

            TPixel[] colors = decoder.DecodeColors(pixelData);
            Image<TPixel> bmp = new Image<TPixel>(width, height);

            Image.LoadPixelData<TPixel>(colors, width, height);

            return bmp;
        }

    }
}