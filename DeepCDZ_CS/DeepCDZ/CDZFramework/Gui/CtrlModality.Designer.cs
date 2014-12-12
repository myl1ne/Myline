namespace CDZFramework.Gui
{
    partial class CtrlModality
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.pictureBoxReal = new System.Windows.Forms.PictureBox();
            this.pictureBoxPrediction = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxReal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPrediction)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(55, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Real";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(178, 4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Prediction";
            // 
            // pictureBoxReal
            // 
            this.pictureBoxReal.Location = new System.Drawing.Point(7, 21);
            this.pictureBoxReal.Name = "pictureBoxReal";
            this.pictureBoxReal.Size = new System.Drawing.Size(129, 126);
            this.pictureBoxReal.TabIndex = 2;
            this.pictureBoxReal.TabStop = false;
            // 
            // pictureBoxPrediction
            // 
            this.pictureBoxPrediction.Location = new System.Drawing.Point(142, 21);
            this.pictureBoxPrediction.Name = "pictureBoxPrediction";
            this.pictureBoxPrediction.Size = new System.Drawing.Size(129, 126);
            this.pictureBoxPrediction.TabIndex = 3;
            this.pictureBoxPrediction.TabStop = false;
            // 
            // CtrlModality
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pictureBoxPrediction);
            this.Controls.Add(this.pictureBoxReal);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "CtrlModality";
            this.Size = new System.Drawing.Size(279, 150);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxReal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPrediction)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox pictureBoxReal;
        private System.Windows.Forms.PictureBox pictureBoxPrediction;
    }
}
