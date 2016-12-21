using System;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace Gawa.ACRender.DynamicParser
{
    public partial class FileMapDisplay : UserControl, IParserOutput
    {
        /// <summary>
        /// File that is currently being viewed
        /// </summary>
        private EmbeddedFile m_currentFile;

        /// <summary>
        /// File list that provides the file lists
        /// </summary>
        private FileList m_fileList;

        /// <summary>
        /// File parser
        /// </summary>
        private DynamicFileParser m_parser;

        /// <summary>
        /// Constructor
        /// </summary>
        public FileMapDisplay()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Get/set the file list
        /// </summary>
        public FileList FileList
        {
            get { return m_fileList; }
            set
            {
                m_fileList = value;
                CreateFileList();
            }
        }

        /// <summary>
        /// Get/set the file parser
        /// </summary>
        public DynamicFileParser Parser
        {
            get { return m_parser; }
            set { m_parser = value; }
        }

        #region IParserOutput Members

        public void HandleFatalError(string message)
        {
            Console.WriteLine(message);
        }

        public void WriteDebug(string message, params object[] args)
        {
            Console.WriteLine(String.Format(message, args));
        }

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!DesignMode)
            {
                CreateMappingList();
            }
        }

        /// <summary>
        /// Loads the dropdownlists with data from the file list
        /// </summary>
        private void CreateFileList()
        {
            cbGroups.Items.Clear();
            int[] groupIDs = m_fileList.GetFileGroups();
            foreach (int group in groupIDs)
            {
                cbGroups.Items.Add(group);
            }
        }

        private void CreateMappingList()
        {
            cbFilters.Items.Clear();
            string[] filters = m_parser.Mapping.GetFilterList();
            try
            {
                cbFilters.BeginUpdate();
                foreach (string filter in filters)
                {
                    cbFilters.Items.Add(filter);
                }
            }
            finally
            {
                cbFilters.EndUpdate();
            }
        }

        private void cbGroups_SelectedValueChanged(object sender, EventArgs e)
        {
            cbFiles.Items.Clear();
            if (cbGroups.SelectedIndex >= 0)
            {
                int groupId = (int) cbGroups.SelectedItem;
                uint[] contents = m_fileList.GetGroupContents(groupId);
                foreach (uint fileId in contents)
                {
                    cbFiles.Items.Add(fileId.ToString("X8"));
                }
            }
        }

        private void cbFiles_SelectedValueChanged(object sender, EventArgs e)
        {
            // apply the selected filter to the selected file
            ApplyFilterToFile();
        }

        private void cbFilters_SelectedIndexChanged(object sender, EventArgs e)
        {
            // apply the selected filter to the selected file
            ApplyFilterToFile();
        }

        private void btnReapply_Click(object sender, EventArgs e)
        {
            // apply the selected filter to the selected file
            ApplyFilterToFile();
        }

        private void btnDumpHex_Click(object sender, EventArgs e)
        {
            //if (cbFiles.SelectedIndex >= 0)
            //{
            //    uint fileId = UInt32.Parse(cbFiles.SelectedItem.ToString(), System.Globalization.NumberStyles.HexNumber);
            //    EmbeddedFile file = DataProvider.Instance.PortalDatReader.LocateFile(fileId);
            //    try
            //    {
            //        file.PrepareFileForReading();
            //        string text = BitConverter.ToString(file.FileData).Replace("-", "");
            //        tbHex.Text = text;
            //    }
            //    finally
            //    {
            //        file.FileReadingComplete();
            //    }
            //}
        }

        private void ApplyFilterToFile()
        {
            if (cbFilters.SelectedIndex >= 0 && cbFiles.SelectedIndex >= 0)
            {
                uint fileId = UInt32.Parse(cbFiles.SelectedItem.ToString(), NumberStyles.HexNumber);
                string filterName = cbFilters.SelectedItem.ToString();

                m_currentFile = DataProvider.Instance.PortalDatReader.LocateFile(fileId);
                FileGraph graph = m_parser.ApplyFilterToFile(m_currentFile, filterName, this);
                if (graph != null)
                {
                    ShowGraph(graph);
                    ShowFileContents();
                }
            }
            else
            {
                // clear the display
            }
        }

        private void ShowGraph(FileGraph graph)
        {
            try
            {
                tvStructureTree.BeginUpdate();
                tvStructureTree.Nodes.Clear();
                AddDisplayNodeToTree(tvStructureTree.Nodes, graph.RootNode, 0);
                //tvStructureTree.ExpandAll();
            }
            finally
            {
                tvStructureTree.EndUpdate();
            }
        }

        private void AddDisplayNodeToTree(TreeNodeCollection nodes, GraphNode graphNode, int level)
        {
            // create the display for the node
            GraphTreeNode subNode = new GraphTreeNode();
            subNode.Text = String.Format("{0}", graphNode.Name);
            subNode.Data = graphNode;

            switch (graphNode.NodeType)
            {
                case NodeType.Primitive_uint16:
                    subNode.Text = String.Format("{0} - 0x{1:X}", graphNode.Name, (UInt16) graphNode.Value);
                    break;
                case NodeType.Primitive_uint32:
                    subNode.Text = String.Format("{0} - 0x{1:X}", graphNode.Name, (uint) graphNode.Value);
                    break;
                case NodeType.Primitive_single:
                    subNode.Text = String.Format("{0} - {1:0.0000}", graphNode.Name, (float) graphNode.Value);
                    break;
                case NodeType.Empty:
                    subNode.Text = String.Format("{0} - {1} bytes", graphNode.Name, (int) graphNode.Value);
                    break;
                case NodeType.Vector:
                    subNode.Text = String.Format("{0} - vector of {1}", graphNode.Name, graphNode.SubNodes.Count);
                    break;
                case NodeType.Complex:
                    subNode.Text = String.Format("{0} - (complex)", graphNode.Name);
                    break;
                default:
                    throw new NotImplementedException();
            }

            nodes.Add(subNode);

            // show the children of this node
            foreach (GraphNode subGraphNode in graphNode.SubNodes)
            {
                AddDisplayNodeToTree(subNode.Nodes, subGraphNode, level + 1);
            }

            if (level <= 1)
            {
                subNode.Expand();
            }
            else
            {
                subNode.Collapse();
            }
        }

        /// <summary>
        /// Displays the contents of the file
        /// </summary>
        /// <param name="highlightMinPos">Start position to highlight. Zero based.</param>
        /// <param name="highlightMaxPos">End position to highlight. Zero based, upper bound inclusive.</param>
        private void ShowFileContents(int highlightMinPos = -1, int highlightMaxPos = -1)
        {
            // obtain the data of the current file
            byte[] fileData;
            try
            {
                m_currentFile.PrepareFileForReading();
                fileData = m_currentFile.FileData;
            }
            finally
            {
                m_currentFile.FileReadingComplete();
            }

            const int lineSize = 32; // amount of chars per line

            int dataPos = 0;
            int length = Math.Min(lineSize, fileData.Length);
            StringBuilder sb = new StringBuilder();
            int highLightRtfStart = -1;
            int highLightRtfEnd = -1;
            while (length > 0)
            {
                bool doHighLight = false;
                for (int i = dataPos; i < dataPos + length; i++)
                {
                    doHighLight = (i >= highlightMinPos) && (i <= highlightMaxPos);


                    if (doHighLight)
                    {
                        if (highLightRtfStart < 0)
                        {
                            highLightRtfStart = i;
                            highLightRtfEnd = i;
                        }
                        else
                        {
                            highLightRtfEnd++;
                        }
                    }


                    sb.Append(fileData[i].ToString("X2"));
                    sb.Append(' ');
                }
                //sb.Append("\t\t");
                //for (int i = dataPos; i < dataPos + length; i++)
                //{
                //    sb.Append(Convert.ToChar(fileData[i]));
                //}
                dataPos += length;
                length = Math.Min(lineSize, fileData.Length - dataPos);

                // add newline
                //sb.Append(Environment.NewLine);
            }

            // set text
            rtbHex.Clear();
            rtbHex.ResetText();
            rtbHex.Text = sb.ToString();

            // highlight selection
            if (highLightRtfStart >= 0 && highLightRtfEnd > highLightRtfStart)
            {
                // the selection is known as byte poitions. Convert these to text positions
                // each byte is rendered as three characters (hex x 2 + space)
                highLightRtfStart *= 3;
                highLightRtfEnd *= 3;

                rtbHex.Select(highLightRtfStart, highLightRtfEnd - highLightRtfStart - 1);
                rtbHex.SelectionColor = Color.White;
                rtbHex.SelectionBackColor = Color.Black;
            }

            // add line splits
            if (fileData.Length > lineSize)
            {
                const int charsPerByte = 3;
                int textSizePos = (fileData.Length / lineSize) * lineSize;
                while (textSizePos > 0)
                {
                    rtbHex.SelectionStart = (textSizePos * charsPerByte);
                    rtbHex.SelectionLength = 0;
                    rtbHex.SelectedText = Environment.NewLine;

                    textSizePos -= lineSize;
                }
            }
        }

        private void tvStructureTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // we've selected a node, highlight its details in the hex view
            if (tvStructureTree.SelectedNode != null)
            {
                GraphTreeNode node = (GraphTreeNode) tvStructureTree.SelectedNode;
                ShowFileContents(node.Data.FilePositionStart, node.Data.FilePositionEnd);
            }
        }
    }

    public class GraphTreeNode : TreeNode
    {
        /// <summary>
        /// Additional data element in the graph
        /// </summary>
        public GraphNode Data { get; set; }
    }
}