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
            this.pictureBoxWorldColor = new System.Windows.Forms.PictureBox();
            this.ctrlCA3 = new CDZNET.GUI.CtrlMMNode();
            this.buttonLearnAndLog = new System.Windows.Forms.Button();
            this.checkBoxPretrainMotor = new System.Windows.Forms.CheckBox();
            this.progressBarCurrentOp = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.pictureBoxWorldOrientation = new System.Windows.Forms.PictureBox();
            this.label3 = new System.Windows.Forms.Label();
            this.pictureBoxEgosphereOrientation = new System.Windows.Forms.PictureBox();
            this.label4 = new System.Windows.Forms.Label();
            this.pictureBoxEgosphereColor = new System.Windows.Forms.PictureBox();
            this.checkBoxEgosphere = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxWorldColor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxWorldOrientation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxEgosphereOrientation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxEgosphereColor)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBoxWorldColor
            // 
            this.pictureBoxWorldColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxWorldColor.Location = new System.Drawing.Point(440, 253);
            this.pictureBoxWorldColor.Name = "pictureBoxWorldColor";
            this.pictureBoxWorldColor.Size = new System.Drawing.Size(250, 248);
            this.pictureBoxWorldColor.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxWorldColor.TabIndex = 1;
            this.pictureBoxWorldColor.TabStop = false;
            // 
            // ctrlCA3
            // 
            this.ctrlCA3.AutoSize = true;
            this.ctrlCA3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ctrlCA3.Location = new System.Drawing.Point(440, 12);
            this.ctrlCA3.Name = "ctrlCA3";
            this.ctrlCA3.Size = new System.Drawing.Size(910, 235);
            this.ctrlCA3.TabIndex = 0;
            // 
            // buttonLearnAndLog
            // 
            this.buttonLearnAndLog.Location = new System.Drawing.Point(10, 73);
            this.buttonLearnAndLog.Name = "buttonLearnAndLog";
            this.buttonLearnAndLog.Size = new System.Drawing.Size(75, 23);
            this.buttonLearnAndLog.TabIndex = 2;
            this.buttonLearnAndLog.Text = "Learn N Log";
            this.buttonLearnAndLog.UseVisualStyleBackColor = true;
            this.buttonLearnAndLog.Click += new System.EventHandler(this.buttonLearnAndLog_Click);
            // 
            // checkBoxPretrainMotor
            // 
            this.checkBoxPretrainMotor.AutoSize = true;
            this.checkBoxPretrainMotor.Location = new System.Drawing.Point(10, 16);
            this.checkBoxPretrainMotor.Name = "checkBoxPretrainMotor";
            this.checkBoxPretrainMotor.Size = new System.Drawing.Size(92, 17);
            this.checkBoxPretrainMotor.TabIndex = 4;
            this.checkBoxPretrainMotor.Text = "Pretrain Motor";
            this.checkBoxPretrainMotor.UseVisualStyleBackColor = true;
            // 
            // progressBarCurrentOp
            // 
            this.progressBarCurrentOp.Location = new System.Drawing.Point(91, 73);
            this.progressBarCurrentOp.Name = "progressBarCurrentOp";
            this.progressBarCurrentOp.Size = new System.Drawing.Size(243, 23);
            this.progressBarCurrentOp.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(531, 504);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "World Color";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(782, 504);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "World Orientation";
            // 
            // pictureBoxWorldOrientation
            // 
            this.pictureBoxWorldOrientation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxWorldOrientation.Location = new System.Drawing.Point(696, 253);
            this.pictureBoxWorldOrientation.Name = "pictureBoxWorldOrientation";
            this.pictureBoxWorldOrientation.Size = new System.Drawing.Size(250, 248);
            this.pictureBoxWorldOrientation.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxWorldOrientation.TabIndex = 7;
            this.pictureBoxWorldOrientation.TabStop = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(782, 786);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(136, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "World Orientation (reconstr)";
            // 
            // pictureBoxEgosphereOrientation
            // 
            this.pictureBoxEgosphereOrientation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxEgosphereOrientation.Location = new System.Drawing.Point(696, 535);
            this.pictureBoxEgosphereOrientation.Name = "pictureBoxEgosphereOrientation";
            this.pictureBoxEgosphereOrientation.Size = new System.Drawing.Size(250, 248);
            this.pictureBoxEgosphereOrientation.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxEgosphereOrientation.TabIndex = 11;
            this.pictureBoxEgosphereOrientation.TabStop = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(531, 786);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(109, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "World Color (reconstr)";
            // 
            // pictureBoxEgosphereColor
            // 
            this.pictureBoxEgosphereColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxEgosphereColor.Location = new System.Drawing.Point(440, 535);
            this.pictureBoxEgosphereColor.Name = "pictureBoxEgosphereColor";
            this.pictureBoxEgosphereColor.Size = new System.Drawing.Size(250, 248);
            this.pictureBoxEgosphereColor.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxEgosphereColor.TabIndex = 9;
            this.pictureBoxEgosphereColor.TabStop = false;
            // 
            // checkBoxEgosphere
            // 
            this.checkBoxEgosphere.AutoSize = true;
            this.checkBoxEgosphere.Location = new System.Drawing.Point(10, 39);
            this.checkBoxEgosphere.Name = "checkBoxEgosphere";
            this.checkBoxEgosphere.Size = new System.Drawing.Size(113, 17);
            this.checkBoxEgosphere.TabIndex = 13;
            this.checkBoxEgosphere.Text = "Predict Egosphere";
            this.checkBoxEgosphere.UseVisualStyleBackColor = true;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(1391, 956);
            this.Controls.Add(this.checkBoxEgosphere);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.pictureBoxEgosphereOrientation);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.pictureBoxEgosphereColor);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pictureBoxWorldOrientation);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.progressBarCurrentOp);
            this.Controls.Add(this.checkBoxPretrainMotor);
            this.Controls.Add(this.buttonLearnAndLog);
            this.Controls.Add(this.pictureBoxWorldColor);
            this.Controls.Add(this.ctrlCA3);
            this.Name = "MainWindow";
            this.Text = "Gui";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxWorldColor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxWorldOrientation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxEgosphereOrientation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxEgosphereColor)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public CDZNET.GUI.CtrlMMNode ctrlCA3;
        private System.Windows.Forms.PictureBox pictureBoxWorldColor;
        private System.Windows.Forms.Button buttonLearnAndLog;
        private System.Windows.Forms.CheckBox checkBoxPretrainMotor;
        private System.Windows.Forms.ProgressBar progressBarCurrentOp;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox pictureBoxWorldOrientation;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.PictureBox pictureBoxEgosphereOrientation;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.PictureBox pictureBoxEgosphereColor;
        private System.Windows.Forms.CheckBox checkBoxEgosphere;

    }
}