namespace VowelWorldModel
{
    partial class DatasetGenerator
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
            this.pictureBoxWorldColor = new System.Windows.Forms.PictureBox();
            this.buttonGenerate = new System.Windows.Forms.Button();
            this.progressBarCurrentOp = new System.Windows.Forms.ProgressBar();
            this.buttonRdmz = new System.Windows.Forms.Button();
            this.textBoxFileName = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxWorldColor)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBoxWorldColor
            // 
            this.pictureBoxWorldColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxWorldColor.Location = new System.Drawing.Point(22, 12);
            this.pictureBoxWorldColor.Name = "pictureBoxWorldColor";
            this.pictureBoxWorldColor.Size = new System.Drawing.Size(250, 248);
            this.pictureBoxWorldColor.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxWorldColor.TabIndex = 1;
            this.pictureBoxWorldColor.TabStop = false;
            // 
            // buttonGenerate
            // 
            this.buttonGenerate.Location = new System.Drawing.Point(104, 357);
            this.buttonGenerate.Name = "buttonGenerate";
            this.buttonGenerate.Size = new System.Drawing.Size(75, 23);
            this.buttonGenerate.TabIndex = 2;
            this.buttonGenerate.Text = "Generate";
            this.buttonGenerate.UseVisualStyleBackColor = true;
            this.buttonGenerate.Click += new System.EventHandler(this.buttonGenerate_Click);
            // 
            // progressBarCurrentOp
            // 
            this.progressBarCurrentOp.Location = new System.Drawing.Point(22, 386);
            this.progressBarCurrentOp.Name = "progressBarCurrentOp";
            this.progressBarCurrentOp.Size = new System.Drawing.Size(250, 23);
            this.progressBarCurrentOp.TabIndex = 5;
            // 
            // buttonRdmz
            // 
            this.buttonRdmz.Location = new System.Drawing.Point(104, 279);
            this.buttonRdmz.Name = "buttonRdmz";
            this.buttonRdmz.Size = new System.Drawing.Size(75, 23);
            this.buttonRdmz.TabIndex = 12;
            this.buttonRdmz.Text = "Randomize";
            this.buttonRdmz.UseVisualStyleBackColor = true;
            this.buttonRdmz.Click += new System.EventHandler(this.buttonRdmz_Click);
            // 
            // textBoxFileName
            // 
            this.textBoxFileName.Location = new System.Drawing.Point(22, 319);
            this.textBoxFileName.Name = "textBoxFileName";
            this.textBoxFileName.Size = new System.Drawing.Size(250, 20);
            this.textBoxFileName.TabIndex = 13;
            this.textBoxFileName.Text = "newDataset.vww";
            // 
            // DatasetGenerator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(290, 421);
            this.Controls.Add(this.textBoxFileName);
            this.Controls.Add(this.buttonRdmz);
            this.Controls.Add(this.progressBarCurrentOp);
            this.Controls.Add(this.buttonGenerate);
            this.Controls.Add(this.pictureBoxWorldColor);
            this.Name = "DatasetGenerator";
            this.Text = "Dataset Generator";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxWorldColor)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxWorldColor;
        private System.Windows.Forms.Button buttonGenerate;
        private System.Windows.Forms.ProgressBar progressBarCurrentOp;
        private System.Windows.Forms.Button buttonRdmz;
        private System.Windows.Forms.TextBox textBoxFileName;

    }
}