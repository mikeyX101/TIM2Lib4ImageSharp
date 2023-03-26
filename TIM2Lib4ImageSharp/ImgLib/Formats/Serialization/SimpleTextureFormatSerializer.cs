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
using TIM2Lib4ImageSharp.ImgLib.Formats.Serialization.Metadata;

namespace TIM2Lib4ImageSharp.ImgLib.Formats.Serialization
{
    /// <summary>
    /// This TextureFormatSerializer takes care of the exporting and import phases.
    /// A class extending this Serializer will only need to implement the Open and Save methods, and some
    /// other minor methods.
    /// This serializer defaults to TextureFormats having a "magic" sequence of bytes as identifier and implements
    /// the method IsValidFormat accordingly. If a more refined identification is needed,
    /// subclasses should override the IsValidFormat method.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SimpleTextureFormatSerializer<TPixel, T> : TextureFormatSerializer<TPixel> where T : TextureFormat<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {

        public abstract string Name { get; }

        public abstract string PreferredFormatExtension { get; }

        public abstract TextureFormat<TPixel> Open(Stream formatData);

        public abstract void Save(TextureFormat<TPixel> texture, Stream outFormatData);

        /// <summary>
        /// Array of bytes with which the TextureFormat starts.
        /// </summary>
        public abstract byte[] MagicID { get; }

        /// <summary>
        /// This is the ID used by this serializer as the name for the root section of the automatically generated
        /// metadata.
        /// </summary>
        public abstract string MetadataID { get; }

        protected virtual void OnExportGeneralTextureMetadata(T texture, MetadataWriter metadata)
        {
            InteropUtils.WriteTo(texture.FormatSpecificData, metadata);
        }

        protected virtual void OnExportFrameMetadata(T texture, int frame, MetadataWriter metadata)
        {
            TextureFormat<TPixel> tFrame = GetTextureFrame(texture, frame);
            InteropUtils.WriteTo(tFrame.FormatSpecificData, metadata);
        }

        /// <summary>
        /// Implementations of this method must return the TextureFormat corresponding to the frame given in the given texture.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        protected abstract TextureFormat<TPixel> GetTextureFrame(T texture, int frame);

        /// <summary>
        /// Implementations of this method must construct the main texture container, according to the
        /// given format specific data.
        /// </summary>
        /// <param name="formatSpecificData"></param>
        /// <returns></returns>
        protected abstract T CreateGeneralTextureFromFormatSpecificData(GenericDictionary formatSpecificData);

        /// <summary>
        /// Implementations of this method must add to the given texture, a new frame (in particular, the frame-th frame).
        /// This frame must be a TextureFormat constructed according to the given format specific data.
        /// Finally, the method must return the constructed frame.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="frame"></param>
        /// <param name="formatSpecificData"></param>
        /// <param name="images"></param>
        /// <param name="referenceImage"></param>
        protected abstract TextureFormat<TPixel> CreateFrameForGeneralTexture(T texture, int frame, GenericDictionary formatSpecificData, IList<Image<TPixel>> images, Image referenceImage, int mipmapsCount);

        private T OnImportGeneralTextureMetadata(MetadataReader metadata)
        {
            GenericDictionary formatSpecific = new GenericDictionary();
            InteropUtils.ReadFrom(metadata, formatSpecific);
            T result = CreateGeneralTextureFromFormatSpecificData(formatSpecific);
            result.FormatSpecificData = formatSpecific;

            return result;
        }

        private void OnImportFrameMetadata(T texture, int frame, MetadataReader metadata, IList<Image<TPixel>> images, Image<TPixel> referenceImage, int mipmapsCount)
        {
            GenericDictionary formatSpecific = new GenericDictionary();
            InteropUtils.ReadFrom(metadata, formatSpecific);
            TextureFormat<TPixel> segment = CreateFrameForGeneralTexture(texture, frame, formatSpecific, images, referenceImage, mipmapsCount);
            segment.FormatSpecificData = formatSpecific;
        }

        public virtual bool IsValidFormat(System.IO.Stream inputFormat)
        {
            long oldPos = inputFormat.Position;
            try
            {
                byte[] magic = new BinaryReader(inputFormat).ReadBytes(MagicID.Length);
                return magic.SequenceEqual(MagicID);
            }
            finally
            {
                inputFormat.Position = oldPos;
            }
        }

        public bool IsValidMetadataFormat(MetadataReader metadata)
        {
            try
            {
                metadata.EnterSection(MetadataID);
            }
            catch (Exception)
            {
                metadata.Rewind();
                return false;
            }
            metadata.ExitSection();
            metadata.Rewind();
            return true;
        }

        public void Export(TextureFormat<TPixel> texture, MetadataWriter metadata, string directory, string basename)
        {
            if(texture.GetType() != typeof(T))
            {
                throw new TextureFormatException("Wrong TextureFormat type, expected: " + typeof(T).FullName);
            }

            metadata.BeginSection(MetadataID);
            metadata.PutAttribute("Textures", texture.FramesCount);
            metadata.PutAttribute("Basename", basename);

                metadata.BeginSection("GeneralMetadata");
                OnExportGeneralTextureMetadata((T)texture,metadata);
                metadata.EndSection();

                int oldSelected=texture.SelectedFrame;
                for(int frame=0;frame<texture.FramesCount;frame++)
                {
                    texture.SelectedFrame = frame;

                    metadata.BeginSection("FrameMetadata");
                    metadata.PutAttribute("PalettesCount", texture.PalettesCount);
                    metadata.PutAttribute("MipmapsCount", texture.MipmapsCount);

                    OnExportFrameMetadata((T)texture, frame, metadata);
                    metadata.EndSection();

                    int i = 0;
                    Image<TPixel> referenceImage;
                    ICollection<Image<TPixel>> list=ConstructImages(texture, out referenceImage);
                    foreach (Image<TPixel> img in list)
                    {
                        string fullPath=Path.Combine(directory, basename + "_layer" + frame + "_" + i++ + ".png");
                        if (img != null)
                        {
                            img.Save(fullPath);
                        }
                        else
                        {
                            File.WriteAllText(fullPath, string.Empty);
                        }
                    }

                    if(referenceImage!=null)
                    {
                        referenceImage.Save(Path.Combine(directory, basename + "_layer" + frame + "_reference.png"));
                    }
                }
                texture.SelectedFrame = oldSelected;

            metadata.EndSection();
        }

        public TextureFormat<TPixel> Import(MetadataReader metadata, string directory)
        {
            metadata.EnterSection(MetadataID);
                int count = metadata.GetAttribute<int>("Textures");
                string basename = metadata.GetAttribute<string>("Basename");

                metadata.EnterSection("GeneralMetadata");
                    T texture=OnImportGeneralTextureMetadata(metadata);
                metadata.ExitSection();

                for (int frame = 0; frame < count;frame++ )
                {
                    metadata.EnterSection("FrameMetadata");
                        int palCount=metadata.GetAttribute<int>("PalettesCount");
                        int mipmapsCount = metadata.GetAttribute<int>("MipmapsCount");

                        IList<Image<TPixel>> images = new List<Image<TPixel>>();
                        Image<TPixel> referenceImage = null;
                        for (int i = 0; i < (palCount == 0 ? 1 : palCount); i++)
                        {
                            Image<TPixel> img = Image.Load<TPixel>(Path.Combine(directory, basename + "_layer" + frame + "_" + i + ".png"));
                            images.Add(img);
                        }
                    
                        if(palCount>1)
                        {
                            referenceImage = Image.Load<TPixel>(Path.Combine(directory, basename + "_layer" + frame + "_reference.png"));
                        }

                        OnImportFrameMetadata(texture, frame, metadata, images, referenceImage, mipmapsCount);

                    metadata.ExitSection();
                }

            metadata.ExitSection();

            return texture;
        }

        private ICollection<Image<TPixel>> ConstructImages(TextureFormat<TPixel> texture, out Image<TPixel> referenceImage)
        {

            var list = new List<Image<TPixel>>();
            int oldSelected = texture.SelectedPalette;
            for (int i = 0; i < (texture.PalettesCount == 0 ? 1 : texture.PalettesCount); i++)
            {
                texture.SelectedPalette = i;
                list.Add(texture.GetImage());
            }
            texture.SelectedPalette = oldSelected;
            referenceImage = texture.GetReferenceImage();
            return list;
        }
    }
}
