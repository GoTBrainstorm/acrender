using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Gawa.ACRender
{
    /// <summary>
    /// Describes an embedded texture
    /// </summary>
    public class ACTexture
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sourceFile">The source file that contains the
        /// texture.</param>
        public ACTexture(EmbeddedFile sourceFile, uint? overridePaletteId)
        {
            Debug.Assert(sourceFile != null);
            m_sourceFile = sourceFile;
            m_overridePaletteId = overridePaletteId;
        }

        /// <summary>
        /// Reads the texture information from the embedded file.
        /// </summary>
        public void ReadTexture()
        {
            try
            {
                m_sourceFile.PrepareFileForReading();

                // layout (as far as known):
                // [header](16 bytes) [data](height*width bytes) [default palatte](DWORD)
                // header: [id](dword) [flags](dword) [width](dword) [height](dword)

                m_fileId = m_sourceFile.ReadUInt32();
                m_flags = m_sourceFile.ReadUInt32();
                m_width = m_sourceFile.ReadUInt32();
                m_height = m_sourceFile.ReadUInt32();
                m_sourceFile.Seek(m_sourceFile.FileLength - 4);
                m_defaultPalette = m_sourceFile.ReadUInt32();

                // load the palette
                if (m_overridePaletteId.HasValue)
                {
                    m_palette = PaletteLoader.Instance.LoadPalette(m_overridePaletteId.Value);
                }
                else
                {
                    m_palette = PaletteLoader.Instance.LoadPalette(m_defaultPalette);
                }
            }
            finally
            {
                m_sourceFile.FileReadingComplete();
            }
        }

        /// <summary>
        /// Generates a preview of this texture.
        /// </summary>
        /// <returns>Preview image of the texture.</returns>
        public Bitmap GetBitmap()
        {
            if (m_flags != 0x02)
            {
                return null;
            }

            Bitmap bmp = new Bitmap((int)m_width, (int)m_height, PixelFormat.Format8bppIndexed);
            try
            {
                // determine the palette id
                uint paletteId;
                if (m_overridePaletteId.HasValue)
                {
                    paletteId = m_overridePaletteId.Value;
                }
                else
                {
                    paletteId = m_defaultPalette;
                }

                // load the palette
                ACPalette palette = PaletteLoader.Instance.LoadPalette(paletteId);
                bmp.Palette = palette.GetPaletteForPreviewImage(bmp.Palette);

                m_sourceFile.PrepareFileForReading();
                m_sourceFile.Skip(16);                  // skip header
                byte[] textureData = m_sourceFile.ReadBytes((int)(m_width * m_height));

                BitmapData bmpData = null;
                try
                {
                    bmpData = bmp.LockBits(new Rectangle(0, 0, (int)m_width, (int)m_height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                    Marshal.Copy(textureData, 0, bmpData.Scan0, textureData.Length);
                }
                finally
                {
                    if (bmpData != null)
                    {
                        bmp.UnlockBits(bmpData);
                    }
                }

                return bmp;
            }
            catch (Exception)
            {
                bmp.Dispose();
                throw;
            }
            finally
            {
                m_sourceFile.FileReadingComplete();
            }
        }

        public uint ID
        {
            get
            {
                return m_sourceFile.FileId;
            }
        }

        /// <summary>
        /// The palette that needs to be used when displaying this texture.
        /// </summary>
        private ACPalette m_palette;

        /// <summary>
        /// The ID of the palette that should be used instead of the
        /// default palette. If no value is available, the default
        /// palette will be used.
        /// </summary>
        private uint? m_overridePaletteId;

        /// <summary>
        /// The sourcefile
        /// </summary>
        private EmbeddedFile m_sourceFile;

        private uint m_fileId;
        private uint m_flags;
        private uint m_width;
        private uint m_height;
        private uint m_defaultPalette;
    }
}