namespace CDZFrameworkPlayground
{
    partial class Form1
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
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.buttonRunFolder = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.pictureBoxPresented = new System.Windows.Forms.PictureBox();
            this.pictureBoxReal = new System.Windows.Forms.PictureBox();
            this.pictureBoxReconstructed = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.pictureBoxReceptiveField = new System.Windows.Forms.PictureBox();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPresented)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxReal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxReconstructed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxReceptiveField)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonRunFolder
            // 
            this.buttonRunFolder.Location = new System.Drawing.Point(98, 12);
            this.buttonRunFolder.Name = "buttonRunFolder";
            this.buttonRunFolder.Size = new System.Drawing.Size(75, 23);
            this.buttonRunFolder.TabIndex = 0;
            this.buttonRunFolder.Text = "Run folder";
            this.buttonRunFolder.UseVisualStyleBackColor = true;
            this.buttonRunFolder.Click += new System.EventHandler(this.buttonRunFolder_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(12, 16);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(68, 17);
            this.checkBox1.TabIndex = 1;
            this.checkBox1.Text = "rnd order";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // pictureBoxPresented
            // 
            this.pictureBoxPresented.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.pictureBoxPresented.Location = new System.Drawing.Point(214, 77);
            this.pictureBoxPresented.Name = "pictureBoxPresented";
            this.pictureBoxPresented.Size = new System.Drawing.Size(202, 194);
            this.pictureBoxPresented.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxPresented.TabIndex = 2;
            this.pictureBoxPresented.TabStop = false;
            // 
            // pictureBoxReal
            // 
            this.pictureBoxReal.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.pictureBoxReal.Location = new System.Drawing.Point(6, 77);
            this.pictureBoxReal.Name = "pictureBoxReal";
            this.pictureBoxReal.Size = new System.Drawing.Size(202, 194);
            this.pictureBoxReal.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxReal.TabIndex = 3;
            this.pictureBoxReal.TabStop = false;
            // 
            // pictureBoxReconstructed
            // 
            this.pictureBoxReconstructed.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.pictureBoxReconstructed.Location = new System.Drawing.Point(422, 77);
            this.pictureBoxReconstructed.Name = "pictureBoxReconstructed";
            this.pictureBoxReconstructed.Size = new System.Drawing.Size(202, 194);
            this.pictureBoxReconstructed.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxReconstructed.TabIndex = 4;
            this.pictureBoxReconstructed.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(92, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Real";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(303, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Presented";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(492, 54);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Reconstructed";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(6, 288);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(860, 255);
            this.flowLayoutPanel1.TabIndex = 8;
            // 
            // pictureBoxReceptiveField
            // 
            this.pictureBoxReceptiveField.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.pictureBoxReceptiveField.Location = new System.Drawing.Point(683, 77);
            this.pictureBoxReceptiveField.Name = "pictureBoxReceptiveField";
            this.pictureBoxReceptiveField.Size = new System.Drawing.Size(183, 194);
            this.pictureBoxReceptiveField.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxReceptiveField.TabIndex = 9;
            this.pictureBoxReceptiveField.TabStop = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(737, 54);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(81, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Receptive Field";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(878, 804);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.pictureBoxReceptiveField);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBoxReconstructed);
            this.Controls.Add(this.pictureBoxReal);
            this.Controls.Add(this.pictureBoxPresented);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.buttonRunFolder);
            this.Name = "Form1";
            this.Text = "Hypocampus";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPresented)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxReal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxReconstructed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxReceptiveField)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button buttonRunFolder;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.PictureBox pictureBoxPresented;
        private System.Windows.Forms.PictureBox pictureBoxReal;
        private System.Windows.Forms.PictureBox pictureBoxReconstructed;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.PictureBox pictureBoxReceptiveField;
        private System.Windows.Forms.Label label4;
    }
}

