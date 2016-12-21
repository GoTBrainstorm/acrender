using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Gawa.ACRender
{
    public partial class DungeonBlockSelection : Form
    {
        public DungeonBlockSelection()
        {
            InitializeComponent();

            List<EmbeddedFileEntry> files = DungeonProvider.Instance.GetDungeonBlockList();
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
            btnParse.Enabled = (lbMeshSelection.SelectedIndex >= 0);
            if (lbMeshSelection.SelectedIndex >= 0)
            {
                DumpMesh(((EmbeddedFileEntry)lbMeshSelection.SelectedItem).FileID);
            }
        }

        private void DumpMesh(uint id)
        {
            EmbeddedFile file = DataProvider.Instance.PortalDatReader.LocateFile(id);
            DungeonBlock block = new DungeonBlock(file);
            block.ReadDungeonBlock();
            try
            {
                file.PrepareFileForReading();
                rtbMeshDump.Clear();
                rtbMeshDump.AppendText(String.Format("Flags: 0x{1:X2}  Count: {2}{0}{0}", Environment.NewLine, block.Flags, block.VertexCount));
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
                string textBuffer = "\t";
                for (int i = 0; i < file.FileData.Length; i++)
                {
                    if (i != 0 && i % 28 == 0)
                    {
                        // line break;
                        rtbMeshDump.AppendText(textBuffer);
                        rtbMeshDump.AppendText(Environment.NewLine);
                        WriteLineNumber(i / 28, i);
                        textBuffer = "\t";
                    }
                    else if (i != 0 && i % 2 == 0)
                    {
                        rtbMeshDump.AppendText(" ");
                    }
                    rtbMeshDump.AppendText(String.Format("{0:X2}", file.FileData[i]));
                    textBuffer += GetDisplayChar((char)file.FileData[i]);
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
                    string val = file.m_debugDataFloats[i].ToString("0.000000");
                    if (val.Length > 8)
                    {
                        val = val.Substring(0, 8);
                    }
                    rtbMeshDump.AppendText(String.Format("{0,8} ", val));
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

        private char GetDisplayChar(char val)
        {
            if (20 > val)
            {
                return (char)0xB7;
            }
            return val;
        }

        public uint SelectedFileId
        {
            get
            {
                return ((EmbeddedFileEntry)lbMeshSelection.SelectedItem).FileID;
            }
        }

        private void btnParse_Click(object sender, EventArgs e)
        {
            EmbeddedFileEntry file = (EmbeddedFileEntry)lbMeshSelection.SelectedItem;
            try
            {
                DungeonBlock block = DungeonProvider.Instance.GetDungeonBlock(file.FileID);
                block.ReadDungeonBlock();
            }
            catch (Exception)
            {
                System.Diagnostics.Debugger.Break();
            }
        }
    }
}