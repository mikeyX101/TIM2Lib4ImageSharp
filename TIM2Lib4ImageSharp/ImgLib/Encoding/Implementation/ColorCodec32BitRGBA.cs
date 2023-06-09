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



namespace TIM2Lib4ImageSharp.ImgLib.Encoding.Implementation
{
    /// <summary>
    /// This  ColorDecoder decodes sequences of pixels in 32 bit RGBA format.
    /// </summary>
    public class ColorCodec32BitRGBA : ColorCodec<Rgba32>
    {
        public override Rgba32[] DecodeColors(byte[] palette, int start, int size)
        {
            List<Rgba32> pal = new List<Rgba32>();

            for (int i = 0; i < size / 4; i++)
            {
                pal.Add(new Rgba32(palette[start + i * 4], palette[start + i * 4 + 1], palette[start + i * 4 + 2], palette[start + i * 4 + 3]));
            }

            return pal.ToArray();
        }

        public override byte[] EncodeColors(Rgba32[] colors, int start, int length)
        {

            byte[]? encoded = new byte[length * 4];

            for (int i = 0; i < encoded.Length; i += 4)
            {
                encoded[i]      = colors[start + i / 4].R;
                encoded[i + 1]  = colors[start + i / 4].G;
                encoded[i + 2]  = colors[start + i / 4].B;
                encoded[i + 3]  = colors[start + i / 4].A;
            }

            return encoded;
        }

        public override int BitDepth => 32;
    }
}
