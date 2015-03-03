namespace CDZNET.GUI
{
    partial class CtrlSignal
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
            this.pictureBoxReal = new System.Windows.Forms.PictureBox();
            this.pictureBoxPrediction = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.labelLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxReal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPrediction)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBoxReal
            // 
            this.pictureBoxReal.Location = new System.Drawing.Point(4, 4);
            this.pictureBoxReal.Name = "pictureBoxReal";
            this.pictureBoxReal.Size = new System.Drawing.Size(138, 143);
            this.pictureBoxReal.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxReal.TabIndex = 0;
            this.pictureBoxReal.TabStop = false;
            // 
            // pictureBoxPrediction
            // 
            this.pictureBoxPrediction.Location = new System.Drawing.Point(148, 4);
            this.pictureBoxPrediction.Name = "pictureBoxPrediction";
            this.pictureBoxPrediction.Size = new System.Drawing.Size(138, 143);
            this.pictureBoxPrediction.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxPrediction.TabIndex = 1;
            this.pictureBoxPrediction.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(52, 150);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Reality";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(191, 150);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Prediction";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(113, 167);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Dimensions";
            // 
            // labelLabel
            // 
            this.labelLabel.AutoSize = true;
            this.labelLabel.Location = new System.Drawing.Point(136, 150);
            this.labelLabel.Name = "labelLabel";
            this.labelLabel.Size = new System.Drawing.Size(16, 13);
            this.labelLabel.TabIndex = 5;
            this.labelLabel.Text = "---";
            this.labelLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // CtrlSignal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelLabel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBoxPrediction);
            this.Controls.Add(this.pictureBoxReal);
            this.Name = "CtrlSignal";
            this.Size = new System.Drawing.Size(291, 188);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxReal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPrediction)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxReal;
        private System.Windows.Forms.PictureBox pictureBoxPrediction;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label labelLabel;
    }
}
