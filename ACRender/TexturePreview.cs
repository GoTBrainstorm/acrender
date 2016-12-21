using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;

namespace Gawa.ACRender
{
    public partial class TexturePreview : Form
    {
        public TexturePreview()
        {
            InitializeComponent();
            cbSubSelection_SelectedIndexChanged(null, null);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        private void cbSubSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            lbTextureList.Items.Clear();
            if (cbSubSelection.SelectedIndex >= 0)
            {
                uint min, max;
                switch (cbSubSelection.SelectedIndex)
                {
                    case 0:
                        min = 0x04000000;
                        max = 0x04FFFFFF;
                        break;
                    case 1:
                        min = 0x05000000;
                        max = 0x05FFFFFF;
                        break;
                    case 2:
                        min = 0x08000000;
                        max = 0x08FFFFFF;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                List<EmbeddedFileEntry> files = DataProvider.Instance.PortalDatReader.GetFileIDs(min, max);
                files.Sort();
                try
                {
                    lbTextureList.BeginUpdate();
                    foreach (EmbeddedFileEntry file in files)
                    {
                        lbTextureList.Items.Add(file);
                    }
                }
                finally
                {
                    lbTextureList.EndUpdate();
                }
            }
        }

        private void lbTextureList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbTextureList.SelectedIndex >= 0)
            {
                uint fileId = ((EmbeddedFileEntry)lbTextureList.SelectedItem).FileID;

                if ((fileId & 0xFF000000) == 0x04000000)
                {
                    ACPalette palette = PaletteLoader.Instance.LoadPalette(fileId);
                    SetPreviewBitmap(palette.GetPreviewBitmap());
                }
                else if ((fileId & 0xFF000000) == 0x05000000)
                {
                    ACTexture texture = TextureLoader.Instance.LoadTexture(fileId);
                    if (cbAlphaMap.Checked)
                    {
                        SetPreviewBitmap(ConvertToAlphaMap(texture.GetBitmap()));
                    }
                    else
                    {
                        SetPreviewBitmap(texture.GetBitmap());
                    }
                }
                else if ((fileId & 0xFF000000) == 0x08000000)
                {
                    MeshDecoration decoration = new MeshDecoration(fileId);
                    Bitmap bmp = new Bitmap(pbPreview.Width, pbPreview.Height, PixelFormat.Format24bppRgb);
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.Clear(Color.White);
                        int height = 10;
                        Font font = new Font("Tahoma", height);
                        g.DrawString(String.Format("Decoration 0x{0:X8}", fileId), font, Brushes.Black, 5, 5 + (0 * height));
                        if (decoration.DecorationType == DecorationType.Texture)
                        {
                            g.DrawString(String.Format("Texture, texture id: 0x{0:X8}", decoration.Texture.ID), font, Brushes.Black, 5, 5 + (1 * height));
                        }
                        else if (decoration.DecorationType == DecorationType.SolidColor)
                        {
                            g.DrawString(String.Format("Solid, color: 0x{0:X8}", decoration.SolidColor.ToArgb()), font, Brushes.Black, 5, 5 + (1 * height));
                        }
                        else if (decoration.DecorationType == DecorationType.Unknown)
                        {
                            g.DrawString(String.Format("Unknown decoration type."), font, Brushes.Black, 5, 5 + (1 * height));
                        }
                        g.DrawString(String.Format("Type flag: {0:X}", decoration.TypeFlag), font, Brushes.Black, 5, 5 + (2 * height));
                    }
                    SetPreviewBitmap(bmp);
                }
            }
        }

        private void SetPreviewBitmap(Bitmap bmp)
        {
            if (m_previewBitmap != null)
            {
                pbPreview.Image = null;
                m_previewBitmap.Dispose();
            } if (bmp != null)
            {
                m_previewBitmap = bmp;
                pbPreview.Image = m_previewBitmap;
            }
        }

        /// <summary>
        /// Creates a n alpha map from the specified image.
        /// </summary>
        /// <param name="source">Source image</param>
        /// <returns>Alpha map</returns>
        private Bitmap ConvertToAlphaMap(Bitmap source)
        {
            using (source)
            {
                Bitmap dest = new Bitmap(source.Width, source.Height, PixelFormat.Format24bppRgb);
                for (int i = 0; i < source.Height; i++)
                {
                    for (int j = 0; j < source.Height; j++)
                    {
                        Color pixel = source.GetPixel(i, j);
                        pixel = Color.FromArgb(pixel.A, pixel.A, pixel.A, pixel.A);
                        dest.SetPixel(i, j, pixel);
                    }
                }
                return dest;
            }
        }

        /// <summary>
        /// bitmap used for previewing
        /// </summary>
        private Bitmap m_previewBitmap;
    }
}