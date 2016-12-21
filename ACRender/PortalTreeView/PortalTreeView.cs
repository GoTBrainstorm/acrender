using System;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace Gawa.ACRender.PortalTreeView
{
    public partial class PortalTreeView : Form
    {
        private DatReader m_portalReader = DataProvider.Instance.PortalDatReader;

        public PortalTreeView()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            DirectoryContents contents = m_portalReader.GetRootDirectoryContents();
            ShowSubNodes(contents, treeView1.Nodes[0]);
        }

        public void ShowSubNodes(DirectoryContents contents, TreeNode node)
        {
            node.Tag = contents;

            foreach (uint rootDirPtr in contents.m_subDirLocations)
            {
                TreeNode subNode = new TreeNode(String.Format("0x{0:X8}", rootDirPtr));
                node.Nodes.Add(subNode);
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag == null)
            {
                uint fileId = uint.Parse(e.Node.Text.Substring(2), NumberStyles.HexNumber);
                DirectoryContents contents = m_portalReader.GetDirectoryContents(fileId);
                ShowSubNodes(contents, e.Node);
            }

            if (e.Node.Tag != null && e.Node.Tag is DirectoryContents)
            {
                StringBuilder sb = new StringBuilder();
                DirectoryContents contents = e.Node.Tag as DirectoryContents;
                foreach (uint fileId in contents.m_fileIds)
                {
                    sb.Append(String.Format("0x{0:X8}{1}", fileId, Environment.NewLine));
                }
                textBox1.Text = sb.ToString();
            }
            else
            {
                textBox1.Text = "";
            }
        }

        private void treeView1_AfterExpand(object sender, TreeViewEventArgs e)
        {
        }
    }
}