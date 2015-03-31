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
            this.buttonTrain = new System.Windows.Forms.Button();
            this.buttonTest = new System.Windows.Forms.Button();
            this.listBoxAlgo = new System.Windows.Forms.ListBox();
            this.labelError = new System.Windows.Forms.Label();
            this.numericUpDownSamples = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxWorldColor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSamples)).BeginInit();
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
            this.buttonGenerate.Enabled = false;
            this.buttonGenerate.Location = new System.Drawing.Point(148, 357);
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
            // buttonTrain
            // 
            this.buttonTrain.Location = new System.Drawing.Point(197, 433);
            this.buttonTrain.Name = "buttonTrain";
            this.buttonTrain.Size = new System.Drawing.Size(75, 23);
            this.buttonTrain.TabIndex = 14;
            this.buttonTrain.Text = "Train";
            this.buttonTrain.UseVisualStyleBackColor = true;
            this.buttonTrain.Click += new System.EventHandler(this.buttonTrain_Click);
            // 
            // buttonTest
            // 
            this.buttonTest.Enabled = false;
            this.buttonTest.Location = new System.Drawing.Point(197, 462);
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.Size = new System.Drawing.Size(75, 23);
            this.buttonTest.TabIndex = 15;
            this.buttonTest.Text = "Test";
            this.buttonTest.UseVisualStyleBackColor = true;
            this.buttonTest.Click += new System.EventHandler(this.buttonTest_Click);
            // 
            // listBoxAlgo
            // 
            this.listBoxAlgo.Enabled = false;
            this.listBoxAlgo.FormattingEnabled = true;
            this.listBoxAlgo.Location = new System.Drawing.Point(22, 416);
            this.listBoxAlgo.Name = "listBoxAlgo";
            this.listBoxAlgo.Size = new System.Drawing.Size(169, 69);
            this.listBoxAlgo.TabIndex = 16;
            // 
            // labelError
            // 
            this.labelError.AutoSize = true;
            this.labelError.Location = new System.Drawing.Point(197, 412);
            this.labelError.Name = "labelError";
            this.labelError.Size = new System.Drawing.Size(35, 13);
            this.labelError.TabIndex = 17;
            this.labelError.Text = "label1";
            // 
            // numericUpDownSamples
            // 
            this.numericUpDownSamples.Increment = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.numericUpDownSamples.Location = new System.Drawing.Point(22, 360);
            this.numericUpDownSamples.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDownSamples.Minimum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.numericUpDownSamples.Name = "numericUpDownSamples";
            this.numericUpDownSamples.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownSamples.TabIndex = 18;
            this.numericUpDownSamples.ThousandsSeparator = true;
            this.numericUpDownSamples.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            // 
            // DatasetGenerator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(290, 497);
            this.Controls.Add(this.numericUpDownSamples);
            this.Controls.Add(this.labelError);
            this.Controls.Add(this.listBoxAlgo);
            this.Controls.Add(this.buttonTest);
            this.Controls.Add(this.buttonTrain);
            this.Controls.Add(this.textBoxFileName);
            this.Controls.Add(this.buttonRdmz);
            this.Controls.Add(this.progressBarCurrentOp);
            this.Controls.Add(this.buttonGenerate);
            this.Controls.Add(this.pictureBoxWorldColor);
            this.Name = "DatasetGenerator";
            this.Text = "Dataset Generator";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxWorldColor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSamples)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxWorldColor;
        private System.Windows.Forms.Button buttonGenerate;
        private System.Windows.Forms.ProgressBar progressBarCurrentOp;
        private System.Windows.Forms.Button buttonRdmz;
        private System.Windows.Forms.TextBox textBoxFileName;
        private System.Windows.Forms.Button buttonTrain;
        private System.Windows.Forms.Button buttonTest;
        private System.Windows.Forms.ListBox listBoxAlgo;
        private System.Windows.Forms.Label labelError;
        private System.Windows.Forms.NumericUpDown numericUpDownSamples;

    }
}