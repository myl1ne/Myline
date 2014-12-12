namespace CDZFramework.Gui
{
    partial class CtrlCDZ
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
            this.progressBarConfidence = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // progressBarConfidence
            // 
            this.progressBarConfidence.Location = new System.Drawing.Point(4, 69);
            this.progressBarConfidence.Minimum = 90;
            this.progressBarConfidence.Name = "progressBarConfidence";
            this.progressBarConfidence.Size = new System.Drawing.Size(73, 15);
            this.progressBarConfidence.Step = 1;
            this.progressBarConfidence.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBarConfidence.TabIndex = 0;
            this.progressBarConfidence.Value = 90;
            // 
            // CtrlCDZ
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.progressBarConfidence);
            this.Name = "CtrlCDZ";
            this.Size = new System.Drawing.Size(80, 87);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBarConfidence;
    }
}
