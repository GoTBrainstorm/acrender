using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Gawa.ACRender;

namespace Gawa.ACRender.DynamicParser
{
    public partial class FileDisplay : Form
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public FileDisplay()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            tcFiles.TabPages.Clear();

            if (!DesignMode)
            {
                m_parser = new DynamicFileParser();
                m_parser.RefreshTypeMappings();

                m_cellFileList = new FileList(DataProvider.Instance.CellDatReader);
                //m_cellFileList.LoadList(false);
                m_portalFileList = new FileList(DataProvider.Instance.PortalDatReader);
                m_portalFileList.LoadList(false);

                AddFileMappingTab(FileListType.Portal);
            }
        }

        /// <summary>
        /// Adds a new file mapping tab
        /// </summary>
        /// <param name="listType">Type of list that will provide data to
        /// the new tab.</param>
        private void AddFileMappingTab(FileListType listType)
        {
            TabPage tp = new TabPage("-new-");
            FileMapDisplay display = new FileMapDisplay();
            switch (listType)
            {
                case FileListType.Cell:
                    display.FileList = m_cellFileList;
                    break;
                case FileListType.Portal:
                    display.FileList = m_portalFileList;
                    break;
                default:
                    throw new ArgumentException("Unknown file list type");
            }
            display.Parser = m_parser;
            tp.Controls.Add(display);
            display.Dock = DockStyle.Fill;
            tcFiles.TabPages.Add(tp);
            tcFiles.SelectedTab = tp;
        }

        /// <summary>
        /// The filelist that we're using for the cell.dat
        /// </summary>
        private FileList m_cellFileList;

        /// <summary>
        /// The filelist that we're using for the cell.dat
        /// </summary>
        private FileList m_portalFileList;

        /// <summary>
        /// File parser
        /// </summary>
        private DynamicFileParser m_parser;
    }
}