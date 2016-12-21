using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Gawa.ACRender
{
    public partial class ComplexMeshSelection : Form
    {
        public ComplexMeshSelection()
        {
            InitializeComponent();
        }

        private void lbMeshSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = (lbMeshSelection.SelectedIndex >= 0);
            if (lbMeshSelection.SelectedIndex >= 0)
            {
                DumpMesh(((EmbeddedFileEntry)lbMeshSelection.SelectedItem).FileID);
            }
        }

        private void DumpMesh(uint id)
        {
            EmbeddedFile file = DataProvider.Instance.PortalDatReader.LocateFile(id);
            ComplexMesh mesh = new ComplexMesh(file);
            mesh.ReadComplexMesh();
            try
            {
                file.PrepareFileForReading();
                rtbMeshDump.Clear();
                rtbMeshDump.AppendText(String.Format("Flags: 0x{1:X2}  Count: {2}{0}{0}", Environment.NewLine, mesh.Flags, mesh.SimpleMeshCount));
                WriteLineNumber(0, 0);
                for (int i = 0; i < file.m_debugData.Length; i++)
                {
                    if (i != 0 && i % 8 == 0)
                    {
                        // line break;
                        rtbMeshDump.AppendText(Environment.NewLine);
                        WriteLineNumber(i / 8, i * 4);
                    }
                    rtbMeshDump.AppendText(String.Format("{0:X8} ", file.m_debugData[i]));
                }
                rtbMeshDump.AppendText(String.Format("{0}{0}", Environment.NewLine));
                WriteLineNumber(0, 0);
                for (int i = 0; i < file.m_debugDataFloats.Length; i++)
                {
                    if (i != 0 && i % 8 == 0)
                    {
                        // line break;
                        rtbMeshDump.AppendText(Environment.NewLine);
                        WriteLineNumber(i / 8, i * 4);
                    }
                    rtbMeshDump.AppendText(String.Format("{0,8:0.00000} ", file.m_debugDataFloats[i]));
                }
            }
            finally
            {
                file.FileReadingComplete();
            }
        }

        private void WriteLineNumber(int line, int bytePos)
        {
            rtbMeshDump.AppendText(String.Format("[{0:00}] {1:X4}\t", line, bytePos));
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (lbMeshSelection.Items.Count == 0)
            {
                List<EmbeddedFileEntry> files = MeshProvider.Instance.GetComplexMeshList();
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

        public uint SelectedFileId
        {
            get
            {
                return ((EmbeddedFileEntry)lbMeshSelection.SelectedItem).FileID;
            }
        }
    }
}