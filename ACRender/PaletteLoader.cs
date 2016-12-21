using System;
using System.Collections.Generic;
using System.Text;

namespace Gawa.ACRender
{
    /// <summary>
    /// Helper class for loading palettes
    /// </summary>
    public class PaletteLoader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        private PaletteLoader()
        {
            m_portalReader = DataProvider.Instance.PortalDatReader;
        }

        /// <summary>
        /// Get the singleton instance of the texture loader
        /// </summary>
        public static PaletteLoader Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new PaletteLoader();
                }
                return m_instance;
            }
        }

        public List<EmbeddedFileEntry> GetPaletteList()
        {
            return m_portalReader.GetFileIDs(0x04000000, 0x04FFFFFF);
        }

        public ACPalette LoadPalette(uint id)
        {
            EmbeddedFile file = LoadPaletteFile(id);
            if (file == null)
            {
                return null;
            }
            ACPalette palette = new ACPalette(file);
            palette.ReadPalette();
            return palette;
        }

        public EmbeddedFile LoadPaletteFile(uint id)
        {
            return m_portalReader.LocateFile(id);
        }

        /// <summary>
        /// Singleton instance
        /// </summary>
        private static PaletteLoader m_instance;
        /// <summary>
        /// Portal.dat reader
        /// </summary>
        private DatReader m_portalReader;
    }
}
