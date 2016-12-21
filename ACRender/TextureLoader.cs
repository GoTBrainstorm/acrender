using System;
using System.Collections.Generic;
using System.Text;

namespace Gawa.ACRender
{
    /// <summary>
    /// Helper class for loading textures
    /// </summary>
    public class TextureLoader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        private TextureLoader()
        {
            m_portalReader = DataProvider.Instance.PortalDatReader;
        }

        /// <summary>
        /// Get the singleton instance of the texture loader
        /// </summary>
        public static TextureLoader Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new TextureLoader();
                }
                return m_instance;
            }
        }

        public List<EmbeddedFileEntry> GetTextureList()
        {
            return m_portalReader.GetFileIDs(0x06000000, 0x06FFFFFF);
        }

        public ACTexture LoadTexture(uint id)
        {
            EmbeddedFile file = LoadTextureFile(id);
            if (file == null)
            {
                return null;
            }
            ACTexture texture = new ACTexture(file, null);
            texture.ReadTexture();
            return texture;
        }

        public EmbeddedFile LoadTextureFile(uint id)
        {
            return m_portalReader.LocateFile(id);
        }

        /// <summary>
        /// Singleton instance
        /// </summary>
        private static TextureLoader m_instance;
        /// <summary>
        /// Portal.dat reader
        /// </summary>
        private DatReader m_portalReader;
    }
}
