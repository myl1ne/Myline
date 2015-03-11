namespace VowelWorldModel
{
    partial class Saccades
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
            this.buttonLearnAndLog = new System.Windows.Forms.Button();
            this.progressBarCurrentOp = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBoxEndPoint = new System.Windows.Forms.PictureBox();
            this.pictureBoxPredictedEndpoint = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.ctrlCA3 = new CDZNET.GUI.CtrlMMNode();
            this.buttonPlusOneStep = new System.Windows.Forms.Button();
            this.buttonRdmz = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxWorldColor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxEndPoint)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPredictedEndpoint)).BeginInit();
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
            // buttonLearnAndLog
            // 
            this.buttonLearnAndLog.Location = new System.Drawing.Point(12, 496);
            this.buttonLearnAndLog.Name = "buttonLearnAndLog";
            this.buttonLearnAndLog.Size = new System.Drawing.Size(75, 23);
            this.buttonLearnAndLog.TabIndex = 2;
            this.buttonLearnAndLog.Text = "Learn N Log";
            this.buttonLearnAndLog.UseVisualStyleBackColor = true;
            this.buttonLearnAndLog.Click += new System.EventHandler(this.buttonLearnAndLog_Click);
            // 
            // progressBarCurrentOp
            // 
            this.progressBarCurrentOp.Location = new System.Drawing.Point(93, 496);
            this.progressBarCurrentOp.Name = "progressBarCurrentOp";
            this.progressBarCurrentOp.Size = new System.Drawing.Size(243, 23);
            this.progressBarCurrentOp.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(90, 263);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "World Color";
            // 
            // pictureBoxEndPoint
            // 
            this.pictureBoxEndPoint.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxEndPoint.Location = new System.Drawing.Point(22, 377);
            this.pictureBoxEndPoint.Name = "pictureBoxEndPoint";
            this.pictureBoxEndPoint.Size = new System.Drawing.Size(116, 113);
            this.pictureBoxEndPoint.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxEndPoint.TabIndex = 7;
            this.pictureBoxEndPoint.TabStop = false;
            // 
            // pictureBoxPredictedEndpoint
            // 
            this.pictureBoxPredictedEndpoint.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxPredictedEndpoint.Location = new System.Drawing.Point(156, 377);
            this.pictureBoxPredictedEndpoint.Name = "pictureBoxPredictedEndpoint";
            this.pictureBoxPredictedEndpoint.Size = new System.Drawing.Size(116, 113);
            this.pictureBoxPredictedEndpoint.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxPredictedEndpoint.TabIndex = 8;
            this.pictureBoxPredictedEndpoint.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(52, 361);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "End Point";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(166, 361);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(97, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Predicted Endpoint";
            // 
            // ctrlCA3
            // 
            this.ctrlCA3.AutoSize = true;
            this.ctrlCA3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ctrlCA3.Location = new System.Drawing.Point(278, 12);
            this.ctrlCA3.Name = "ctrlCA3";
            this.ctrlCA3.Size = new System.Drawing.Size(910, 235);
            this.ctrlCA3.TabIndex = 0;
            // 
            // buttonPlusOneStep
            // 
            this.buttonPlusOneStep.Location = new System.Drawing.Point(377, 496);
            this.buttonPlusOneStep.Name = "buttonPlusOneStep";
            this.buttonPlusOneStep.Size = new System.Drawing.Size(75, 23);
            this.buttonPlusOneStep.TabIndex = 11;
            this.buttonPlusOneStep.Text = "+1 Step";
            this.buttonPlusOneStep.UseVisualStyleBackColor = true;
            this.buttonPlusOneStep.Click += new System.EventHandler(this.button1_Click);
            // 
            // buttonRdmz
            // 
            this.buttonRdmz.Location = new System.Drawing.Point(93, 279);
            this.buttonRdmz.Name = "buttonRdmz";
            this.buttonRdmz.Size = new System.Drawing.Size(75, 23);
            this.buttonRdmz.TabIndex = 12;
            this.buttonRdmz.Text = "Randomize";
            this.buttonRdmz.UseVisualStyleBackColor = true;
            this.buttonRdmz.Click += new System.EventHandler(this.buttonRdmz_Click);
            // 
            // Saccades
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(1391, 531);
            this.Controls.Add(this.buttonRdmz);
            this.Controls.Add(this.buttonPlusOneStep);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pictureBoxPredictedEndpoint);
            this.Controls.Add(this.pictureBoxEndPoint);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.progressBarCurrentOp);
            this.Controls.Add(this.buttonLearnAndLog);
            this.Controls.Add(this.pictureBoxWorldColor);
            this.Controls.Add(this.ctrlCA3);
            this.Name = "Saccades";
            this.Text = "SACCADES";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxWorldColor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxEndPoint)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPredictedEndpoint)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public CDZNET.GUI.CtrlMMNode ctrlCA3;
        private System.Windows.Forms.PictureBox pictureBoxWorldColor;
        private System.Windows.Forms.Button buttonLearnAndLog;
        private System.Windows.Forms.ProgressBar progressBarCurrentOp;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pictureBoxEndPoint;
        private System.Windows.Forms.PictureBox pictureBoxPredictedEndpoint;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonPlusOneStep;
        private System.Windows.Forms.Button buttonRdmz;

    }
}