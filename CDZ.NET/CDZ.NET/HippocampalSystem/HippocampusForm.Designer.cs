namespace HippocampalSystem
{
    partial class HippocampusForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.flowLayoutPanelMEC = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanelLEC = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pictureBoxStimulus = new System.Windows.Forms.PictureBox();
            this.ctrlMMNode1 = new CDZNET.GUI.CtrlMMNode();
            this.flowLayoutPanelMEC.SuspendLayout();
            this.flowLayoutPanelLEC.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStimulus)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "MEC Activity";
            // 
            // flowLayoutPanelMEC
            // 
            this.flowLayoutPanelMEC.AutoScroll = true;
            this.flowLayoutPanelMEC.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.flowLayoutPanelMEC.Controls.Add(this.flowLayoutPanel1);
            this.flowLayoutPanelMEC.Location = new System.Drawing.Point(16, 37);
            this.flowLayoutPanelMEC.Name = "flowLayoutPanelMEC";
            this.flowLayoutPanelMEC.Size = new System.Drawing.Size(917, 272);
            this.flowLayoutPanelMEC.TabIndex = 1;
            this.flowLayoutPanelMEC.WrapContents = false;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(0, 0);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // flowLayoutPanelLEC
            // 
            this.flowLayoutPanelLEC.AutoScroll = true;
            this.flowLayoutPanelLEC.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.flowLayoutPanelLEC.Controls.Add(this.flowLayoutPanel3);
            this.flowLayoutPanelLEC.Location = new System.Drawing.Point(16, 328);
            this.flowLayoutPanelLEC.Name = "flowLayoutPanelLEC";
            this.flowLayoutPanelLEC.Size = new System.Drawing.Size(917, 258);
            this.flowLayoutPanelLEC.TabIndex = 3;
            this.flowLayoutPanelLEC.WrapContents = false;
            // 
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.AutoScroll = true;
            this.flowLayoutPanel3.AutoSize = true;
            this.flowLayoutPanel3.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.flowLayoutPanel3.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Size = new System.Drawing.Size(0, 0);
            this.flowLayoutPanel3.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 312);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "LEC Activity";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1616, 24);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // pictureBoxStimulus
            // 
            this.pictureBoxStimulus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxStimulus.Location = new System.Drawing.Point(939, 37);
            this.pictureBoxStimulus.Name = "pictureBoxStimulus";
            this.pictureBoxStimulus.Size = new System.Drawing.Size(665, 549);
            this.pictureBoxStimulus.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxStimulus.TabIndex = 7;
            this.pictureBoxStimulus.TabStop = false;
            // 
            // ctrlMMNode1
            // 
            this.ctrlMMNode1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ctrlMMNode1.Location = new System.Drawing.Point(16, 590);
            this.ctrlMMNode1.Name = "ctrlMMNode1";
            this.ctrlMMNode1.Size = new System.Drawing.Size(917, 223);
            this.ctrlMMNode1.TabIndex = 6;
            // 
            // HippocampusForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1616, 818);
            this.Controls.Add(this.pictureBoxStimulus);
            this.Controls.Add(this.ctrlMMNode1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.flowLayoutPanelLEC);
            this.Controls.Add(this.flowLayoutPanelMEC);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "HippocampusForm";
            this.Text = "HippocampusForm";
            this.flowLayoutPanelMEC.ResumeLayout(false);
            this.flowLayoutPanelMEC.PerformLayout();
            this.flowLayoutPanelLEC.ResumeLayout(false);
            this.flowLayoutPanelLEC.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStimulus)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelMEC;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelLEC;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private CDZNET.GUI.CtrlMMNode ctrlMMNode1;
        private System.Windows.Forms.PictureBox pictureBoxStimulus;
    }
}