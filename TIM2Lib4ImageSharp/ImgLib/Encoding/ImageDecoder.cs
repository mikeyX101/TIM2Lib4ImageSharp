﻿//Copyright (C) 2014+ Marco (Phoenix) Calautti.

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

namespace TIM2Lib4ImageSharp.ImgLib.Encoding
{

    /// <summary>
    /// This interface represents an object that can convert its internal image data encoded with the encoding represented by this object, into an Image object.
    /// </summary>
    public interface ImageDecoder<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        /// <summary>
        /// Constructs an Image object associated to this ImageEncoder. If either the witdh or the height of the image
        /// is zero, null is returned.
        /// </summary>
        /// <returns></returns>
        Image<TPixel>? DecodeImage();
    }
}