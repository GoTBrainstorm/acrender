namespace Gawa.ACRender
{
    partial class TexturePreview
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
            this.lbTextureList = new System.Windows.Forms.ListBox();
            this.pbPreview = new System.Windows.Forms.PictureBox();
            this.cbSubSelection = new System.Windows.Forms.ComboBox();
            this.cbAlphaMap = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbPreview)).BeginInit();
            this.SuspendLayout();
            // 
            // lbTextureList
            // 
            this.lbTextureList.FormattingEnabled = true;
            this.lbTextureList.Location = new System.Drawing.Point(12, 64);
            this.lbTextureList.Name = "lbTextureList";
            this.lbTextureList.Size = new System.Drawing.Size(178, 212);
            this.lbTextureList.TabIndex = 0;
            this.lbTextureList.SelectedIndexChanged += new System.EventHandler(this.lbTextureList_SelectedIndexChanged);
            // 
            // pbPreview
            // 
            this.pbPreview.Location = new System.Drawing.Point(196, 12);
            this.pbPreview.Name = "pbPreview";
            this.pbPreview.Size = new System.Drawing.Size(260, 260);
            this.pbPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbPreview.TabIndex = 1;
            this.pbPreview.TabStop = false;
            // 
            // cbSubSelection
            // 
            this.cbSubSelection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSubSelection.FormattingEnabled = true;
            this.cbSubSelection.Items.AddRange(new object[] {
            "Palettes (0x04)",
            "Textures (0x05)",
            "Texture info (0x08)"});
            this.cbSubSelection.Location = new System.Drawing.Point(12, 12);
            this.cbSubSelection.Name = "cbSubSelection";
            this.cbSubSelection.Size = new System.Drawing.Size(178, 21);
            this.cbSubSelection.TabIndex = 2;
            this.cbSubSelection.SelectedIndexChanged += new System.EventHandler(this.cbSubSelection_SelectedIndexChanged);
            // 
            // cbAlphaMap
            // 
            this.cbAlphaMap.AutoSize = true;
            this.cbAlphaMap.Location = new System.Drawing.Point(12, 41);
            this.cbAlphaMap.Name = "cbAlphaMap";
            this.cbAlphaMap.Size = new System.Drawing.Size(107, 17);
            this.cbAlphaMap.TabIndex = 3;
            this.cbAlphaMap.Text = "Show Alpha Map";
            this.cbAlphaMap.UseVisualStyleBackColor = true;
            // 
            // TexturePreview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(464, 284);
            this.Controls.Add(this.cbAlphaMap);
            this.Controls.Add(this.cbSubSelection);
            this.Controls.Add(this.pbPreview);
            this.Controls.Add(this.lbTextureList);
            this.Name = "TexturePreview";
            this.Text = "TexturePreview";
            ((System.ComponentModel.ISupportInitialize)(this.pbPreview)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lbTextureList;
        private System.Windows.Forms.PictureBox pbPreview;
        private System.Windows.Forms.ComboBox cbSubSelection;
        private System.Windows.Forms.CheckBox cbAlphaMap;
    }
}