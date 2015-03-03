namespace VowelWorldModel
{
    partial class MainWindow
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
            this.pictureBoxWorld = new System.Windows.Forms.PictureBox();
            this.ctrlCA3 = new CDZNET.GUI.CtrlMMNode();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxWorld)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBoxWorld
            // 
            this.pictureBoxWorld.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxWorld.Location = new System.Drawing.Point(13, 257);
            this.pictureBoxWorld.Name = "pictureBoxWorld";
            this.pictureBoxWorld.Size = new System.Drawing.Size(422, 372);
            this.pictureBoxWorld.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxWorld.TabIndex = 1;
            this.pictureBoxWorld.TabStop = false;
            // 
            // ctrlCA3
            // 
            this.ctrlCA3.AutoSize = true;
            this.ctrlCA3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ctrlCA3.Location = new System.Drawing.Point(12, 12);
            this.ctrlCA3.Name = "ctrlCA3";
            this.ctrlCA3.Size = new System.Drawing.Size(910, 235);
            this.ctrlCA3.TabIndex = 0;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(1105, 641);
            this.Controls.Add(this.pictureBoxWorld);
            this.Controls.Add(this.ctrlCA3);
            this.Name = "MainWindow";
            this.Text = "Gui";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxWorld)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public CDZNET.GUI.CtrlMMNode ctrlCA3;
        private System.Windows.Forms.PictureBox pictureBoxWorld;

    }
}