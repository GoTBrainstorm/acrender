using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using Microsoft.DirectX.Direct3D;

namespace Gawa.ACRender
{
    /// <summary>
    /// Class for mesh decorations, such as textures and solid colors
    /// </summary>
    public class MeshDecoration
    {
        /// <summary>
        /// Loads the decoration with the specified embedded file.
        /// </summary>
        /// <param name="id">The ID of the decoration</param>
        public MeshDecoration(uint id)
        {
            m_id = id;
            LoadDecoration();
        }

        /// <summary>
        /// Loads the decoration
        /// </summary>
        private void LoadDecoration()
        {
            // if we're dealing with texture info
            if ((m_id & 0xFF000000) == 0x08000000)
            {
                EmbeddedFile sourceFile = DataProvider.Instance.PortalDatReader.LocateFile(m_id);
                try
                {
                    sourceFile.PrepareFileForReading();

                    // determine the type of texture information
                    if (sourceFile.DatType != DatType.Portal_ToD)
                    {
                        uint fileId = sourceFile.ReadUInt32();
                        Debug.Assert(fileId == m_id);
                    }
                    m_typeFlag = sourceFile.ReadUInt32();
                    switch (m_typeFlag)
                    {
                        case 0x01:
                            uint solidColor = sourceFile.ReadUInt32();
                            uint unk01_02 = sourceFile.ReadUInt32();
                            Debug.Assert(unk01_02 == 0x00);
                            LoadAsSolidColor(solidColor);
                            break;
                        case 0x02:
                        case 0x04:
                            uint textureNumber = sourceFile.ReadUInt32();
                            uint paletteNumber = sourceFile.ReadUInt32();
                            uint unk_01 = sourceFile.ReadUInt32();
                            Debug.Assert((textureNumber & 0xFF000000) == 0x05000000);
                            Debug.Assert(paletteNumber == 0x00 || (paletteNumber & 0xFF000000) == 0x04000000);
                            Debug.Assert(unk_01 == 0x00);
                            LoadAsTexture(textureNumber, (paletteNumber != 0x00 ? (uint?)paletteNumber : null));
                            break;
                        case 0x11:
                        case 0x12:
                        case 0x14:
                            m_decorationType = DecorationType.Unknown;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
                finally
                {
                    sourceFile.FileReadingComplete();
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void LoadAsTexture(uint textureId, uint? paletteOverride)
        {
            m_decorationType = DecorationType.Texture;
            EmbeddedFile file = TextureLoader.Instance.LoadTextureFile(textureId);
            m_texture = new ACTexture(file, paletteOverride);
            m_texture.ReadTexture();
        }

        private void LoadAsSolidColor(uint color)
        {
            m_decorationType = DecorationType.SolidColor;
            m_solidColor = Color.FromArgb((int)((color >> 24) & 0xFF),
                (int)((color >> 16) & 0xFF), (int)((color >> 8) & 0xFF),
                (int)(color & 0xFF));
        }

        public override string ToString()
        {
            switch (m_decorationType)
            {
                case DecorationType.Unknown:
                    return base.ToString();
                case DecorationType.Texture:
                    return String.Format("Texture 0x{0:X8}", m_texture.ID);
                case DecorationType.SolidColor:
                    return String.Format("Solidcolor 0x{0:X8}", m_solidColor.ToArgb());
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Get the type of decoration
        /// </summary>
        public DecorationType DecorationType
        {
            get { return m_decorationType; }
        }

        /// <summary>
        /// Get the ac texture, if this decoration is texture
        /// </summary>
        public ACTexture Texture
        {
            get { return m_texture; }
        }

        /// <summary>
        /// Get the color, if this decoration is a solid color
        /// </summary>
        public Color SolidColor
        {
            get { return m_solidColor; }
        }

        /// <summary>
        /// Get the ID of the decoration
        /// </summary>
        public uint Id
        {
            get { return m_id; }
        }

        /// <summary>
        /// Get the type flag
        /// </summary>
        public uint TypeFlag
        {
            get { return m_typeFlag; }
        }

        /// <summary>
        /// Flag that tells about the type of decoration
        /// </summary>
        private uint m_typeFlag;
        /// <summary>
        /// The type of decoration
        /// </summary>
        private DecorationType m_decorationType;
        /// <summary>
        /// The texture, if needed
        /// </summary>
        private ACTexture m_texture;
        /// <summary>
        /// The solid color, if needed
        /// </summary>
        private Color m_solidColor;
        /// <summary>
        /// ID of this decoration
        /// </summary>
        private uint m_id;
    }

    /// <summary>
    /// Defines the available types of decoration
    /// </summary>
    public enum DecorationType
    {
        Unknown = 0,
        Texture,
        SolidColor
    }
}
