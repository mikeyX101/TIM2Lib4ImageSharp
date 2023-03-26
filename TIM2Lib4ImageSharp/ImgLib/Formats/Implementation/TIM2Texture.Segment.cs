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
using TIM2Lib4ImageSharp.ImgLib.Encoding;
using TIM2Lib4ImageSharp.ImgLib.Encoding.ColorComparers;
using TIM2Lib4ImageSharp.ImgLib.Filters;



namespace TIM2Lib4ImageSharp.ImgLib.Formats.Implementation
{
    public class TIM2SegmentParameters
    {
        //segment parameters
        internal int width, height;
        internal bool swizzled = false;
        internal bool linearPalette;
        internal byte bpp;
        internal int colorSize;
        internal byte mipmapCount;

        //raw header data we don't mind to process (I hope so).
        internal byte format;
        internal byte[] GsTEX0 = new byte[8], GsTEX1 = new byte[8];
        internal uint GsRegs, GsTexClut;
        internal byte[] userdata = Array.Empty<byte>();
    }

    public class TIM2Segment<TPixel> : TextureFormatBase<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {

        #region Members

        private readonly TIM2SegmentParameters parameters;

        private byte[]? imageData;
        private TPixel[]?[] palettes = Array.Empty<TPixel[]>();
        private ImageDecoder<TPixel> decoder;
        private readonly TIM2PaletteFilter<TPixel> paletteFilter;
        private readonly SwizzleFilter swizzleFilter;

        #endregion

        internal static readonly string NAME = "TIM2Segment";

        internal TIM2Segment(byte[]? imageData, byte[]? paletteData, uint colorEntries, TIM2SegmentParameters parameters)
        {
            this.imageData = imageData;
            this.parameters = parameters;
            swizzleFilter = new SwizzleFilter(parameters.width, parameters.height, parameters.bpp);
            paletteFilter = new TIM2PaletteFilter<TPixel>(parameters.bpp);

            if (parameters.swizzled)
            {
                this.imageData = swizzleFilter.Defilter(imageData);
            }

            ConstructPalettes(paletteData, colorEntries);
            CreateImageDecoder(imageData);

            if (!parameters.linearPalette)
            {
                for (int i = 0; i < palettes.Length; i++)
                {
                    palettes[i] = paletteFilter.Defilter(palettes[i]);
                }
            }

        }

        internal TIM2Segment(Image<TPixel> image, IList<TPixel[]?>? palettes, TIM2SegmentParameters parameters)
        {
            this.parameters = parameters;
            swizzleFilter = new SwizzleFilter(parameters.width, parameters.height, parameters.bpp);
            paletteFilter = new TIM2PaletteFilter<TPixel>(parameters.bpp);

            if (parameters.bpp > 8) //true color image
            {
                imageData = GetColorCodec().EncodeColors(image.GetColorArray()); //I love extension methods. Hurray!
            }
            else
            {
                ImageEncoderIndexed<TPixel> encoder;
                if (palettes != null)
                {
                    encoder = new ImageEncoderIndexed<TPixel>(palettes, image, IndexCodec.FromBitPerPixel(parameters.bpp));
                }
                else
                {
                    encoder = new ImageEncoderIndexed<TPixel>(image, IndexCodec.FromBitPerPixel(parameters.bpp), new ARGBColorComparer<TPixel>());
                }

                imageData = encoder.Encode();
                this.palettes = new List<TPixel[]?>(encoder.Palettes).ToArray();
            }
            CreateImageDecoder(imageData);
        }

        #region Properties

        public override string Name => NAME;

        public override int Width => parameters.width;

        public override int Height => parameters.height;

        public override int PalettesCount => palettes.Length;

        public override int FramesCount => 1;

        public int Bpp => parameters.bpp;

        internal bool Swizzled
        {
            get => parameters.swizzled;
            set
            {
                if (parameters.swizzled == value)
                {
                    return;
                }

                parameters.swizzled = value;

                imageData = parameters.swizzled ? swizzleFilter.Defilter(imageData) : swizzleFilter.ApplyFilter(imageData);
                CreateImageDecoder(imageData);
            }
        }

        public bool LinearPalette
        {
            get
            {
                return parameters.linearPalette;
            }
        }

        public int ColorSize => parameters.colorSize;

        #endregion

        #region Non public methods

        protected override Image<TPixel> GetImage(int activeFrame, int activePalette)
        {
            ImageDecoderIndexed<TPixel>? iDecoder = decoder as ImageDecoderIndexed<TPixel>;
            if (iDecoder != null)
            {
                iDecoder.Palette = palettes[activePalette];
            }

            return decoder.DecodeImage();

        }

        protected override TPixel[]? GetPalette(int activePalette)
        {
            return palettes[activePalette];
        }

        private void CreateImageDecoder(byte[]? imageData)
        {
            if (Bpp <= 8) //here we have an Indexed TIM2
            {
                decoder = new ImageDecoderIndexed<TPixel>(imageData,
                                              parameters.width, parameters.height,
                                              IndexCodec.FromBitPerPixel(Bpp),
                                              palettes[SelectedPalette]);
            }
            else //otherwise, we have a true color TIM2
            {
                decoder = new ImageDecoderDirectColor<TPixel>(imageData,
                                              parameters.width, parameters.height,
                                              GetColorCodec());

            }
        }

        private void ConstructPalettes(byte[]? paletteData, uint colorEntries)
        {

            if (parameters.bpp > 8)
                return;

            int numberOfPalettes = paletteData.Length / ((int)colorEntries * parameters.colorSize);
            int singlePaletteSize = paletteData.Length / numberOfPalettes;

            palettes = new TPixel[numberOfPalettes][];

            int start = 0;
            for (int i = 0; i < numberOfPalettes; i++)
            {

                palettes[i] = GetColorCodec().DecodeColors(paletteData, start, singlePaletteSize);
                start += singlePaletteSize;
            }
        }

        private ColorCodec<TPixel> GetColorCodec()
        {
            return ColorCodecs.GetColorCodecByPixelType<TPixel>();
            /*switch (pixelSize)
            {
                case 2:
                    return ColorCodecs.CODEC_16BITLE_ABGR;
                case 3:
                    return ColorCodecs.CODEC_24BIT_RGB;
                case 4:
                    return ColorCodecs.CODEC_32BIT_RGBA;
                default:
                    throw new TextureFormatException("Illegal Pixel size!");
            }*/
        }

        #endregion

        #region Internal methods for serializers

        internal byte[]? GetImageData()
        {
            return parameters.swizzled ? swizzleFilter.ApplyFilter(imageData) : imageData;
        }

        internal byte[] GetPaletteData()
        {

            ColorCodec<TPixel> encoder = GetColorCodec();

            MemoryStream stream = new MemoryStream();
            foreach (TPixel[]? palette in palettes)
            {
                TPixel[]? pal = !parameters.linearPalette ? paletteFilter.ApplyFilter(palette) : palette;

                byte[]? buf = encoder.EncodeColors(pal);
                stream.Write(buf, 0, buf.Length);
            }
            stream.Close();
            return stream.ToArray();
        }

        internal TIM2SegmentParameters GetParameters()
        {
            return parameters;
        }

        #endregion

        public override Image<TPixel>? GetReferenceImage()
        {
            ImageDecoderIndexed<TPixel>? iDecoder = decoder as ImageDecoderIndexed<TPixel>;
            if (iDecoder != null && PalettesCount > 1)
            {
                return iDecoder.ReferenceImage;
            }

            return null;
        }
    }
}