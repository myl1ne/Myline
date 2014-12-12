namespace CDZFramework.Gui
{
    partial class CtrlCDZ_ESOM
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
            this.labelNeurons = new System.Windows.Forms.Label();
            this.labelConnections = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Neurons:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Connections:";
            // 
            // labelNeurons
            // 
            this.labelNeurons.AutoSize = true;
            this.labelNeurons.Location = new System.Drawing.Point(70, 22);
            this.labelNeurons.Name = "labelNeurons";
            this.labelNeurons.Size = new System.Drawing.Size(13, 13);
            this.labelNeurons.TabIndex = 2;
            this.labelNeurons.Text = "0";
            // 
            // labelConnections
            // 
            this.labelConnections.AutoSize = true;
            this.labelConnections.Location = new System.Drawing.Point(70, 35);
            this.labelConnections.Name = "labelConnections";
            this.labelConnections.Size = new System.Drawing.Size(13, 13);
            this.labelConnections.TabIndex = 3;
            this.labelConnections.Text = "0";
            // 
            // CtrlCDZ_ESOM
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.Controls.Add(this.labelConnections);
            this.Controls.Add(this.labelNeurons);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "CtrlCDZ_ESOM";
            this.Size = new System.Drawing.Size(86, 87);
            this.Controls.SetChildIndex(this.label1, 0);
            this.Controls.SetChildIndex(this.label2, 0);
            this.Controls.SetChildIndex(this.labelNeurons, 0);
            this.Controls.SetChildIndex(this.labelConnections, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label labelNeurons;
        private System.Windows.Forms.Label labelConnections;
    }
}
