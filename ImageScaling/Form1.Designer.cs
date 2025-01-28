namespace ImageScaling
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
            this.labelInfo1 = new System.Windows.Forms.Label();
            this.btnChooseImage = new System.Windows.Forms.Button();
            this.labelInfo4 = new System.Windows.Forms.Label();
            this.heightTextBox = new System.Windows.Forms.TextBox();
            this.widthTextBox = new System.Windows.Forms.TextBox();
            this.currentHeight = new System.Windows.Forms.Label();
            this.currentWidth = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.labelInfo3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBoxCpp = new System.Windows.Forms.CheckBox();
            this.checkBoxAsm = new System.Windows.Forms.CheckBox();
            this.btnFormatImage = new System.Windows.Forms.Button();
            this.labelInfo2 = new System.Windows.Forms.Label();
            this.btmExit = new System.Windows.Forms.Button();
            this.labelInfo5 = new System.Windows.Forms.Label();
            this.labelInfo6 = new System.Windows.Forms.Label();
            this.labelInfo7 = new System.Windows.Forms.Label();
            this.labelAsmTime = new System.Windows.Forms.Label();
            this.labelCppTime = new System.Windows.Forms.Label();
            this.labelDifferenceTime = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelInfo1
            // 
            this.labelInfo1.AutoSize = true;
            this.labelInfo1.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F);
            this.labelInfo1.Location = new System.Drawing.Point(221, 9);
            this.labelInfo1.Name = "labelInfo1";
            this.labelInfo1.Size = new System.Drawing.Size(245, 22);
            this.labelInfo1.TabIndex = 6;
            this.labelInfo1.Text = "Click button to choose image:";
            this.labelInfo1.Click += new System.EventHandler(this.label3_Click);
            // 
            // btnChooseImage
            // 
            this.btnChooseImage.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.btnChooseImage.Location = new System.Drawing.Point(274, 45);
            this.btnChooseImage.Name = "btnChooseImage";
            this.btnChooseImage.Size = new System.Drawing.Size(131, 32);
            this.btnChooseImage.TabIndex = 7;
            this.btnChooseImage.Text = "choose image";
            this.btnChooseImage.UseVisualStyleBackColor = true;
            this.btnChooseImage.Click += new System.EventHandler(this.btnChoose_Click);
            // 
            // labelInfo4
            // 
            this.labelInfo4.AutoSize = true;
            this.labelInfo4.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F);
            this.labelInfo4.Location = new System.Drawing.Point(212, 264);
            this.labelInfo4.Name = "labelInfo4";
            this.labelInfo4.Size = new System.Drawing.Size(249, 22);
            this.labelInfo4.TabIndex = 33;
            this.labelInfo4.Text = "Check library to main feature: ";
            // 
            // heightTextBox
            // 
            this.heightTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.heightTextBox.Location = new System.Drawing.Point(467, 195);
            this.heightTextBox.Name = "heightTextBox";
            this.heightTextBox.Size = new System.Drawing.Size(100, 24);
            this.heightTextBox.TabIndex = 32;
            this.heightTextBox.Text = "new height";
            // 
            // widthTextBox
            // 
            this.widthTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.widthTextBox.Location = new System.Drawing.Point(467, 164);
            this.widthTextBox.Name = "widthTextBox";
            this.widthTextBox.Size = new System.Drawing.Size(100, 24);
            this.widthTextBox.TabIndex = 31;
            this.widthTextBox.Text = "new width";
            // 
            // currentHeight
            // 
            this.currentHeight.AutoSize = true;
            this.currentHeight.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.currentHeight.Location = new System.Drawing.Point(144, 198);
            this.currentHeight.Name = "currentHeight";
            this.currentHeight.Size = new System.Drawing.Size(100, 18);
            this.currentHeight.TabIndex = 30;
            this.currentHeight.Text = "Current height";
            // 
            // currentWidth
            // 
            this.currentWidth.AutoSize = true;
            this.currentWidth.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.currentWidth.Location = new System.Drawing.Point(144, 167);
            this.currentWidth.Name = "currentWidth";
            this.currentWidth.Size = new System.Drawing.Size(95, 18);
            this.currentWidth.TabIndex = 29;
            this.currentWidth.Text = "Current width";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.label6.Location = new System.Drawing.Point(411, 198);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(55, 18);
            this.label6.TabIndex = 28;
            this.label6.Text = "height: ";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.label5.Location = new System.Drawing.Point(411, 167);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(50, 18);
            this.label5.TabIndex = 27;
            this.label5.Text = "width: ";
            // 
            // labelInfo3
            // 
            this.labelInfo3.AutoSize = true;
            this.labelInfo3.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F);
            this.labelInfo3.Location = new System.Drawing.Point(451, 136);
            this.labelInfo3.Name = "labelInfo3";
            this.labelInfo3.Size = new System.Drawing.Size(82, 22);
            this.labelInfo3.TabIndex = 26;
            this.labelInfo3.Text = "New size";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.label1.Location = new System.Drawing.Point(87, 198);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 18);
            this.label1.TabIndex = 25;
            this.label1.Text = "height:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.label2.Location = new System.Drawing.Point(87, 167);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 18);
            this.label2.TabIndex = 24;
            this.label2.Text = "width: ";
            // 
            // checkBoxCpp
            // 
            this.checkBoxCpp.AutoSize = true;
            this.checkBoxCpp.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.checkBoxCpp.Location = new System.Drawing.Point(389, 299);
            this.checkBoxCpp.Name = "checkBoxCpp";
            this.checkBoxCpp.Size = new System.Drawing.Size(56, 22);
            this.checkBoxCpp.TabIndex = 23;
            this.checkBoxCpp.Text = "C++";
            this.checkBoxCpp.UseVisualStyleBackColor = true;
            this.checkBoxCpp.CheckedChanged += new System.EventHandler(this.checkBox_Cpp_CheckedChanged);
            // 
            // checkBoxAsm
            // 
            this.checkBoxAsm.AutoSize = true;
            this.checkBoxAsm.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.checkBoxAsm.Location = new System.Drawing.Point(216, 299);
            this.checkBoxAsm.Name = "checkBoxAsm";
            this.checkBoxAsm.Size = new System.Drawing.Size(97, 22);
            this.checkBoxAsm.TabIndex = 22;
            this.checkBoxAsm.Text = "Assembler";
            this.checkBoxAsm.UseVisualStyleBackColor = true;
            this.checkBoxAsm.CheckedChanged += new System.EventHandler(this.checkBox_Asm_CheckedChanged);
            // 
            // btnFormatImage
            // 
            this.btnFormatImage.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.btnFormatImage.Location = new System.Drawing.Point(274, 327);
            this.btnFormatImage.Name = "btnFormatImage";
            this.btnFormatImage.Size = new System.Drawing.Size(131, 30);
            this.btnFormatImage.TabIndex = 21;
            this.btnFormatImage.Text = "Format Image";
            this.btnFormatImage.UseVisualStyleBackColor = true;
            this.btnFormatImage.Click += new System.EventHandler(this.btnFormatPhoto_Click);
            // 
            // labelInfo2
            // 
            this.labelInfo2.AutoSize = true;
            this.labelInfo2.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelInfo2.Location = new System.Drawing.Point(108, 136);
            this.labelInfo2.Name = "labelInfo2";
            this.labelInfo2.Size = new System.Drawing.Size(106, 22);
            this.labelInfo2.TabIndex = 20;
            this.labelInfo2.Text = "Current size";
            this.labelInfo2.Click += new System.EventHandler(this.label10_Click);
            // 
            // btmExit
            // 
            this.btmExit.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.btmExit.Location = new System.Drawing.Point(467, 465);
            this.btmExit.Name = "btmExit";
            this.btmExit.Size = new System.Drawing.Size(153, 33);
            this.btmExit.TabIndex = 38;
            this.btmExit.Text = "Exit from program";
            this.btmExit.UseVisualStyleBackColor = true;
            // 
            // labelInfo5
            // 
            this.labelInfo5.AutoSize = true;
            this.labelInfo5.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelInfo5.Location = new System.Drawing.Point(68, 408);
            this.labelInfo5.Name = "labelInfo5";
            this.labelInfo5.Size = new System.Drawing.Size(99, 22);
            this.labelInfo5.TabIndex = 39;
            this.labelInfo5.Text = "Assembler:";
            // 
            // labelInfo6
            // 
            this.labelInfo6.AutoSize = true;
            this.labelInfo6.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelInfo6.Location = new System.Drawing.Point(68, 443);
            this.labelInfo6.Name = "labelInfo6";
            this.labelInfo6.Size = new System.Drawing.Size(48, 22);
            this.labelInfo6.TabIndex = 40;
            this.labelInfo6.Text = "Cpp:";
            this.labelInfo6.Click += new System.EventHandler(this.label12_Click);
            // 
            // labelInfo7
            // 
            this.labelInfo7.AutoSize = true;
            this.labelInfo7.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelInfo7.Location = new System.Drawing.Point(68, 476);
            this.labelInfo7.Name = "labelInfo7";
            this.labelInfo7.Size = new System.Drawing.Size(97, 22);
            this.labelInfo7.TabIndex = 41;
            this.labelInfo7.Text = "Difference:";
            // 
            // labelAsmTime
            // 
            this.labelAsmTime.AutoSize = true;
            this.labelAsmTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelAsmTime.Location = new System.Drawing.Point(171, 408);
            this.labelAsmTime.Name = "labelAsmTime";
            this.labelAsmTime.Size = new System.Drawing.Size(70, 22);
            this.labelAsmTime.TabIndex = 42;
            this.labelAsmTime.Text = "no data";
            // 
            // labelCppTime
            // 
            this.labelCppTime.AutoSize = true;
            this.labelCppTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelCppTime.Location = new System.Drawing.Point(171, 443);
            this.labelCppTime.Name = "labelCppTime";
            this.labelCppTime.Size = new System.Drawing.Size(70, 22);
            this.labelCppTime.TabIndex = 43;
            this.labelCppTime.Text = "no data";
            // 
            // labelDifferenceTime
            // 
            this.labelDifferenceTime.AutoSize = true;
            this.labelDifferenceTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelDifferenceTime.Location = new System.Drawing.Point(171, 476);
            this.labelDifferenceTime.Name = "labelDifferenceTime";
            this.labelDifferenceTime.Size = new System.Drawing.Size(70, 22);
            this.labelDifferenceTime.TabIndex = 44;
            this.labelDifferenceTime.Text = "no data";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(677, 522);
            this.Controls.Add(this.labelDifferenceTime);
            this.Controls.Add(this.labelCppTime);
            this.Controls.Add(this.labelAsmTime);
            this.Controls.Add(this.labelInfo7);
            this.Controls.Add(this.labelInfo6);
            this.Controls.Add(this.labelInfo5);
            this.Controls.Add(this.btmExit);
            this.Controls.Add(this.labelInfo4);
            this.Controls.Add(this.heightTextBox);
            this.Controls.Add(this.widthTextBox);
            this.Controls.Add(this.currentHeight);
            this.Controls.Add(this.currentWidth);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.labelInfo3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.checkBoxCpp);
            this.Controls.Add(this.checkBoxAsm);
            this.Controls.Add(this.btnFormatImage);
            this.Controls.Add(this.labelInfo2);
            this.Controls.Add(this.btnChooseImage);
            this.Controls.Add(this.labelInfo1);
            this.Name = "Form1";
            this.Text = "Program to scaling images";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label labelInfo1;
        private System.Windows.Forms.Button btnChooseImage;
        private System.Windows.Forms.Label labelInfo4;
        private System.Windows.Forms.TextBox heightTextBox;
        private System.Windows.Forms.TextBox widthTextBox;
        private System.Windows.Forms.Label currentHeight;
        private System.Windows.Forms.Label currentWidth;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label labelInfo3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox checkBoxCpp;
        private System.Windows.Forms.CheckBox checkBoxAsm;
        private System.Windows.Forms.Button btnFormatImage;
        private System.Windows.Forms.Label labelInfo2;
        private System.Windows.Forms.Button btmExit;
        private System.Windows.Forms.Label labelInfo5;
        private System.Windows.Forms.Label labelInfo6;
        private System.Windows.Forms.Label labelInfo7;
        private System.Windows.Forms.Label labelAsmTime;
        private System.Windows.Forms.Label labelCppTime;
        private System.Windows.Forms.Label labelDifferenceTime;
    }
}

