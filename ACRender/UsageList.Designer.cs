namespace Gawa.ACRender
{
    partial class UsageList
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lbMeshSelection = new System.Windows.Forms.ListBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbMeshSelection
            // 
            this.lbMeshSelection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbMeshSelection.FormattingEnabled = true;
            this.lbMeshSelection.Location = new System.Drawing.Point(0, 0);
            this.lbMeshSelection.Name = "lbMeshSelection";
            this.lbMeshSelection.Size = new System.Drawing.Size(292, 225);
            this.lbMeshSelection.TabIndex = 3;
            // 
            // btnOk
            // 
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Enabled = false;
            this.btnOk.Location = new System.Drawing.Point(205, 10);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnOk);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 234);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(292, 39);
            this.panel1.TabIndex = 2;
            // 
            // UsageList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.lbMeshSelection);
            this.Controls.Add(this.panel1);
            this.Name = "UsageList";
            this.Text = "UsageList";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lbMeshSelection;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Panel panel1;
    }
}