using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Gawa.ACRender
{
    /// <summary>
    /// Palette class
    /// </summary>
    public class ACPalette
    {
        public ACPalette(EmbeddedFile sourceFile)
        {
            Debug.Assert(sourceFile != null);
            m_sourceFile = sourceFile;
        }

        /// <summary>
        /// Reads the palette
        /// </summary>
        public void ReadPalette()
        {
            try
            {
                m_sourceFile.PrepareFileForReading();

                uint id = m_sourceFile.ReadUInt32();
                Debug.Assert(id == m_sourceFile.FileId);
                m_paletteSize = m_sourceFile.ReadUInt32();
            }
            finally
            {
                m_sourceFile.FileReadingComplete();
            }
        }

        /// <summary>
        /// Get a bitmap for previewing the palette
        /// </summary>
        /// <returns></returns>
        public Bitmap GetPreviewBitmap()
        {
            // calculate height and width
            int width = (int) Math.Sqrt(m_paletteSize);
            int height = ((int)m_paletteSize) / width;
            if (m_paletteSize % width != 0)
            {
                height++;
            }

            Bitmap bmp = new Bitmap(width * PREVIEW_SQUARESIZE,
                height * PREVIEW_SQUARESIZE, PixelFormat.Format32bppArgb);
            try
            {
                m_sourceFile.PrepareFileForReading();

                // skip the first 8 bytes (the header)
                m_sourceFile.Skip(8);

                // loop to read all the palette entries and draw them on the image
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    int cellPtr = 0;
                    int line = 0;
                    for (int i = 0; i < m_paletteSize; i++)
                    {
                        Color c = Color.FromArgb(m_sourceFile.ReadInt32());
                        g.FillRectangle(new SolidBrush(c), cellPtr * PREVIEW_SQUARESIZE,
                            line * PREVIEW_SQUARESIZE, PREVIEW_SQUARESIZE,
                            PREVIEW_SQUARESIZE);

                        cellPtr++;
                        if (cellPtr >= width)
                        {
                            line++;
                            cellPtr = 0;
                        }
                    }
                }
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
            return bmp;
        }

        public ColorPalette GetPaletteForPreviewImage(ColorPalette basePalette)
        {
            try
            {
                m_sourceFile.PrepareFileForReading();

                // skip the first 8 bytes (the header)
                m_sourceFile.Skip(8);

                for (int i = 0; i < m_paletteSize; i++)
                {
                    Color c = Color.FromArgb(m_sourceFile.ReadInt32());
                    basePalette.Entries[i] = c;
                }

                return basePalette;
            }
            finally
            {
                m_sourceFile.FileReadingComplete();
            }
        }

        /// <summary>
        /// Get the palette size
        /// </summary>
        public uint PaletteSize
        {
            get { return m_paletteSize; }
        }

        /// <summary>
        /// The source file
        /// </summary>
        private EmbeddedFile m_sourceFile;
        /// <summary>
        /// Size of the palette
        /// </summary>
        private uint m_paletteSize;
        /// <summary>
        /// Size of a cell in preview mode
        /// </summary>
        private const int PREVIEW_SQUARESIZE = 8;
    }
}
