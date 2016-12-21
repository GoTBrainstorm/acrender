using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Gawa.ACRender
{
    public partial class UsageList : Form
    {
        public UsageList(uint searchedID)
        {
            InitializeComponent();

            Text = String.Format("Usage for {0:X8}", searchedID);

            List<EmbeddedFileEntry> files = MeshProvider.Instance.GetComplexMeshListBySimpleMeshID(searchedID);
            files.Sort();
            try
            {
                lbMeshSelection.BeginUpdate();
                foreach (EmbeddedFileEntry file in files)
                {
                    lbMeshSelection.Items.Add(file);
                }
            }
            finally
            {
                lbMeshSelection.EndUpdate();
            }
        }
    }
}