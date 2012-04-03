namespace owp
{
    partial class FortsMicexCommissionUserControl
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
            this.forF = new System.Windows.Forms.NumericUpDown();
            this.forM = new System.Windows.Forms.NumericUpDown();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.forF)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.forM)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(158, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Сколько рублей за контракт?";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(179, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Какой процент от суммы сделки?";
            // 
            // forF
            // 
            this.forF.DecimalPlaces = 2;
            this.forF.Location = new System.Drawing.Point(17, 32);
            this.forF.Name = "forF";
            this.forF.Size = new System.Drawing.Size(120, 20);
            this.forF.TabIndex = 3;
            // 
            // forM
            // 
            this.forM.DecimalPlaces = 2;
            this.forM.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.forM.Location = new System.Drawing.Point(17, 85);
            this.forM.Name = "forM";
            this.forM.Size = new System.Drawing.Size(120, 20);
            this.forM.TabIndex = 4;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(17, 132);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(190, 13);
            this.linkLabel1.TabIndex = 5;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Русские библиотеки для Wealth-Lab";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // FortsMicexCommissionUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.forM);
            this.Controls.Add(this.forF);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "FortsMicexCommissionUserControl";
            this.Size = new System.Drawing.Size(271, 148);
            ((System.ComponentModel.ISupportInitialize)(this.forF)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.forM)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown forF;
        private System.Windows.Forms.NumericUpDown forM;
        private System.Windows.Forms.LinkLabel linkLabel1;
    }
}
