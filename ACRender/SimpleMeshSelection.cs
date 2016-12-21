using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Gawa.ACRender
{
    public partial class SimpleMeshSelection : Form
    {
        public SimpleMeshSelection()
        {
            InitializeComponent();

            List<EmbeddedFileEntry> files = MeshProvider.Instance.GetSimpleMeshList();
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

        private void lbMeshSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = (lbMeshSelection.SelectedIndex >= 0);
        }

        public uint SelectedFileId
        {
            get
            {
                return ((EmbeddedFileEntry)lbMeshSelection.SelectedItem).FileID;
            }
            set
            {
                int modelIndex = FindModelIndex(value);
                if (modelIndex >= 0)
                {
                    lbMeshSelection.SelectedIndex = modelIndex;
                }
            }
        }

        private int FindModelIndex(uint id)
        {
            int index = -1;
            foreach (EmbeddedFileEntry file in lbMeshSelection.Items)
            {
                if (file.FileID == id)
                {
                    index = lbMeshSelection.Items.IndexOf(file);
                    break;
                }
            }
            return index;
        }

        public uint FindNext(uint current)
        {
            int currentModelIndex = FindModelIndex(current);
            if (currentModelIndex < 0)
            {
                return current;
            }
            currentModelIndex = (currentModelIndex + 1) % lbMeshSelection.Items.Count;
            return ((EmbeddedFileEntry)lbMeshSelection.Items[currentModelIndex]).FileID;
        }

        public uint FindPrevious(uint current)
        {
            int currentModelIndex = FindModelIndex(current);
            if (currentModelIndex < 0)
            {
                return current;
            }
            currentModelIndex--;
            if (currentModelIndex < 0)
            {
                currentModelIndex += lbMeshSelection.Items.Count;
            }
            return ((EmbeddedFileEntry)lbMeshSelection.Items[currentModelIndex]).FileID;
        }
    }
}