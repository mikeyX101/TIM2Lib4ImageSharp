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

using TIM2Lib4ImageSharp.ImgLib.Common;
using TIM2Lib4ImageSharp.ImgLib.Filters;

namespace TIM2Lib4ImageSharp.ImgLib.Encoding
{
    public class ImageEncoderDirectColor<TPixel> : ImageEncoder<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        private Image<TPixel> image;
        private ImageFilter? filter;
        private ColorCodec<TPixel> codec;

        public ImageEncoderDirectColor(Image<TPixel> image, ColorCodec<TPixel> codec, ImageFilter? filter = null)
        {
            this.image = image;
            this.codec = codec;
            this.filter = filter;
        }

        public byte[]? Encode()
        {
            byte[]? data = codec.EncodeColors(image.GetColorArray());

            if (filter != null)
            {
                data = filter.ApplyFilter(data);
            }

            return data;
        }
    }
}

