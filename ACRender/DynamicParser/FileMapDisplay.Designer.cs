namespace Gawa.ACRender.DynamicParser
{
    partial class FileMapDisplay
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pnlFileChooser = new System.Windows.Forms.Panel();
            this.btnDumpHex = new System.Windows.Forms.Button();
            this.btnReapply = new System.Windows.Forms.Button();
            this.cbFilters = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cbFiles = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbGroups = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tvStructureTree = new System.Windows.Forms.TreeView();
            this.rtbHex = new System.Windows.Forms.RichTextBox();
            this.imgListTree = new System.Windows.Forms.ImageList(this.components);
            this.pnlFileChooser.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlFileChooser
            // 
            this.pnlFileChooser.Controls.Add(this.btnDumpHex);
            this.pnlFileChooser.Controls.Add(this.btnReapply);
            this.pnlFileChooser.Controls.Add(this.cbFilters);
            this.pnlFileChooser.Controls.Add(this.label3);
            this.pnlFileChooser.Controls.Add(this.cbFiles);
            this.pnlFileChooser.Controls.Add(this.label2);
            this.pnlFileChooser.Controls.Add(this.cbGroups);
            this.pnlFileChooser.Controls.Add(this.label1);
            this.pnlFileChooser.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFileChooser.Location = new System.Drawing.Point(0, 0);
            this.pnlFileChooser.Name = "pnlFileChooser";
            this.pnlFileChooser.Size = new System.Drawing.Size(661, 29);
            this.pnlFileChooser.TabIndex = 0;
            // 
            // btnDumpHex
            // 
            this.btnDumpHex.Location = new System.Drawing.Point(341, 2);
            this.btnDumpHex.Name = "btnDumpHex";
            this.btnDumpHex.Size = new System.Drawing.Size(75, 23);
            this.btnDumpHex.TabIndex = 3;
            this.btnDumpHex.Text = "Hex";
            this.btnDumpHex.UseVisualStyleBackColor = true;
            this.btnDumpHex.Click += new System.EventHandler(this.btnDumpHex_Click);
            // 
            // btnReapply
            // 
            this.btnReapply.Location = new System.Drawing.Point(583, 0);
            this.btnReapply.Name = "btnReapply";
            this.btnReapply.Size = new System.Drawing.Size(75, 23);
            this.btnReapply.TabIndex = 2;
            this.btnReapply.Text = "Reapply";
            this.btnReapply.UseVisualStyleBackColor = true;
            this.btnReapply.Click += new System.EventHandler(this.btnReapply_Click);
            // 
            // cbFilters
            // 
            this.cbFilters.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbFilters.FormattingEnabled = true;
            this.cbFilters.Location = new System.Drawing.Point(440, 2);
            this.cbFilters.Name = "cbFilters";
            this.cbFilters.Size = new System.Drawing.Size(137, 21);
            this.cbFilters.TabIndex = 1;
            this.cbFilters.SelectedIndexChanged += new System.EventHandler(this.cbFilters_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(306, 6);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Filter";
            // 
            // cbFiles
            // 
            this.cbFiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbFiles.FormattingEnabled = true;
            this.cbFiles.Location = new System.Drawing.Point(179, 3);
            this.cbFiles.Name = "cbFiles";
            this.cbFiles.Size = new System.Drawing.Size(121, 21);
            this.cbFiles.TabIndex = 1;
            this.cbFiles.SelectedValueChanged += new System.EventHandler(this.cbFiles_SelectedValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(147, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "File:";
            // 
            // cbGroups
            // 
            this.cbGroups.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbGroups.FormattingEnabled = true;
            this.cbGroups.Location = new System.Drawing.Point(48, 3);
            this.cbGroups.Name = "cbGroups";
            this.cbGroups.Size = new System.Drawing.Size(93, 21);
            this.cbGroups.TabIndex = 1;
            this.cbGroups.SelectedValueChanged += new System.EventHandler(this.cbGroups_SelectedValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Group:";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 29);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tvStructureTree);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.rtbHex);
            this.splitContainer1.Size = new System.Drawing.Size(661, 281);
            this.splitContainer1.SplitterDistance = 220;
            this.splitContainer1.TabIndex = 1;
            // 
            // tvStructureTree
            // 
            this.tvStructureTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvStructureTree.Location = new System.Drawing.Point(0, 0);
            this.tvStructureTree.Name = "tvStructureTree";
            this.tvStructureTree.Size = new System.Drawing.Size(220, 281);
            this.tvStructureTree.TabIndex = 0;
            this.tvStructureTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvStructureTree_AfterSelect);
            // 
            // rtbHex
            // 
            this.rtbHex.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbHex.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbHex.Location = new System.Drawing.Point(3, 6);
            this.rtbHex.Name = "rtbHex";
            this.rtbHex.Size = new System.Drawing.Size(431, 236);
            this.rtbHex.TabIndex = 0;
            this.rtbHex.Text = "";
            // 
            // imgListTree
            // 
            this.imgListTree.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imgListTree.ImageSize = new System.Drawing.Size(16, 16);
            this.imgListTree.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // FileMapDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.pnlFileChooser);
            this.Name = "FileMapDisplay";
            this.Size = new System.Drawing.Size(661, 310);
            this.pnlFileChooser.ResumeLayout(false);
            this.pnlFileChooser.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlFileChooser;
        private System.Windows.Forms.ComboBox cbFilters;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbFiles;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbGroups;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView tvStructureTree;
        private System.Windows.Forms.Button btnReapply;
        private System.Windows.Forms.Button btnDumpHex;
        private System.Windows.Forms.RichTextBox rtbHex;
        private System.Windows.Forms.ImageList imgListTree;
    }
}
