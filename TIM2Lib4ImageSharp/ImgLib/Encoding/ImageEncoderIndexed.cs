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

using System.Drawing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using TIM2Lib4ImageSharp.ImgLib.Common;
using TIM2Lib4ImageSharp.ImgLib.Filters;
using Color = SixLabors.ImageSharp.Color;

namespace TIM2Lib4ImageSharp.ImgLib.Encoding
{
    public class ImageEncoderIndexed<TPixel> : ImageEncoder<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        private class DefaultColorSorter : IComparer<TPixel>
        {
            public int Compare(TPixel x, TPixel y)
            {
                Rgba32 xVal = Color.FromPixel(x).ToPixel<Rgba32>();
                Rgba32 yVal = Color.FromPixel(y).ToPixel<Rgba32>();
                long result = (long)xVal.PackedValue - (long)yVal.PackedValue;
                return result < 0 ? -1 : result > 0 ? 1 : 0;
            }
        }

        private IList<Image<TPixel>> images;
        private Image<TPixel> referenceImage;

        private int colors;
        private int width, height;
        private IndexCodec? codec;
        private IComparer<TPixel> pixelSorter = new DefaultColorSorter();
        private ColorCodec<TPixel>? colorEncoder;
        private ImageFilter? imageFilter;
        private PaletteFilter<TPixel>? paletteFilter;

        private bool fromReference;

        public ImageEncoderIndexed(IList<TPixel[]?> palettes, Image<TPixel> referenceImage, IndexCodec codec, ColorCodec<TPixel>? encoder = null, ImageFilter? imageFilter = null, PaletteFilter<TPixel>? paletteFilter = null)
        {
            fromReference = true;
            Palettes = palettes;
            this.referenceImage = referenceImage;

            width = referenceImage.Width;
            height = referenceImage.Height;

            if (!IsGreyScale(referenceImage))
            {
                throw new ArgumentException("The reference image must be in grey scale!");
            }

            Init(codec, null, encoder, imageFilter, paletteFilter);


        }

        public ImageEncoderIndexed(Image<TPixel> image, IndexCodec codec, IComparer<TPixel>? pixelComparer = null, ColorCodec<TPixel>? encoder = null, ImageFilter? imageFilter = null, PaletteFilter<TPixel>? paletteFilter = null)
        : this(new List<Image<TPixel>>() { image}, codec,pixelComparer, encoder, imageFilter,paletteFilter)
        {

        }


        private ImageEncoderIndexed(IList<Image<TPixel>> images, IndexCodec codec, IComparer<TPixel>? pixelComparer = null, ColorCodec<TPixel>? encoder = null, ImageFilter? imageFilter = null, PaletteFilter<TPixel>? paletteFilter = null)
        {
            fromReference = false;

            if (images.Count == 0)
            {
                throw new ArgumentException("The image list cannot be empty!");
            }

            width = images.First().Width;
            height = images.First().Height;

            foreach (Image img in images)
            {
                if (img.Width != width || img.Height != height)
                {
                    throw new ArgumentException("The images are not of the same size!");
                }
            }

            this.images = images;

            Init(codec,pixelComparer,encoder,imageFilter,paletteFilter);
        }

        private void Init(IndexCodec? codec, IComparer<TPixel>? pixelComparer, ColorCodec<TPixel>? encoder, ImageFilter? imageFilter, PaletteFilter<TPixel>? paletteFilter)
        {
            this.codec = codec;
            this.colorEncoder = encoder;

            this.imageFilter = imageFilter;
            this.paletteFilter = paletteFilter;
            colors = 1 << codec.BitDepth;

            if (pixelComparer != null)
            {
                pixelSorter = pixelComparer;
            }
        }

        private bool IsGreyScale(Image<TPixel> referenceImage)
        {
            return referenceImage.GetColorArray().All(p =>
            {
                Rgba32 rgba = Color.FromPixel(p).ToPixel<Rgba32>();
                return rgba.R == rgba.G && rgba.R == rgba.B;
            });
        }

        public IList<TPixel[]?> Palettes { get; private set; }
        public IList<byte[]?> EncodedPalettes { get; private set; }

        public byte[]? Encode()
        {
            if (fromReference)
            {
                return EncodeFromReference();
            }
            else
            {
                return EncodeFromImages();
            }
        }

        private byte[]? EncodeFromReference()
        {
            int[] indexes = referenceImage.GetColorArray().Select(p =>
            {
                Rgba32 rgba = Color.FromPixel(p).ToPixel<Rgba32>();
                return (rgba.R >> (8 - codec.BitDepth));
            }).ToArray();

            if (colorEncoder != null)
            {
                EncodedPalettes = new List<byte[]?>(Palettes.Count);
                foreach (TPixel[]? pal in Palettes)
                {
                    EncodedPalettes.Add(colorEncoder.EncodeColors(paletteFilter == null ? pal : paletteFilter.ApplyFilter(pal)));
                }
            }
            return imageFilter == null ? codec.PackIndexes(indexes) : imageFilter.ApplyFilter(codec.PackIndexes(indexes));
        }

        private byte[]? EncodeFromImages()
        {
            IList<Image<TPixel>> bitmaps = null;
            if (images.Count == 1) // We can quantize a single palette image
            {
                Image<TPixel> img = images.First();
                if (img.ColorsCount() > colors)
                {
                    img.Mutate(context => context.Quantize(new WuQuantizer(options: new QuantizerOptions { MaxColors = colors })));
                }
                bitmaps = new List<Image<TPixel>> { img };
            }
            else //for multi palette images, quantization may break the pixel structure of the images. We must trust the work of the graphics editor.
            {
                bitmaps = images;
            }

            var indexes = new int[width * height];


            Palettes = new List<TPixel[]?>();

            for (int i = 0; i < bitmaps.Count; i++)
            {
                Palettes.Add(Enumerable.Repeat(Color.Black, colors).Select(c => c.ToPixel<TPixel>()).ToArray());
            }

            for (int i = 0; i < bitmaps.Count; i++)
            {
                int count = 0;
                List<TPixel> palette = new List<TPixel>();
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        TPixel pixel = bitmaps[i][x, y];
                        if (!palette.Contains(pixel))
                        {
                            if (count >= colors)
                            {
                                throw new ArgumentException("Too many colors! The maximum for this image is " + colors + "!");
                            }
                            palette.Add(pixel);
                            count++;
                        }
                    }
                }

                for (int c = 0; c < colors - count; c++)
                {
                    palette.Add(Color.Black.ToPixel<TPixel>());
                }

                palette.Sort(pixelSorter);
                Palettes[i] = palette.ToArray();
            }


            int k = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    TPixel pixel = bitmaps[0][x, y];
                    int idx = Array.BinarySearch(Palettes[0], pixel, pixelSorter);
                    indexes[k++] = idx;
                }
            }
            if (colorEncoder != null)
            {
                EncodedPalettes = new List<byte[]?>(Palettes.Count);
                foreach (TPixel[]? pal in Palettes)
                {
                    EncodedPalettes.Add(colorEncoder.EncodeColors(paletteFilter == null ? pal : paletteFilter.ApplyFilter(pal)));
                }
            }
            
            return imageFilter == null ? codec.PackIndexes(indexes) : imageFilter.ApplyFilter(codec.PackIndexes(indexes));
        }
    }
}
