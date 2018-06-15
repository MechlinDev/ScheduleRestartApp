namespace ScheduleRestartApp
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
            this.chkAutoLogOn = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // chkAutoLogOn
            // 
            this.chkAutoLogOn.AutoSize = true;
            this.chkAutoLogOn.Location = new System.Drawing.Point(12, 39);
            this.chkAutoLogOn.Name = "chkAutoLogOn";
            this.chkAutoLogOn.Size = new System.Drawing.Size(161, 17);
            this.chkAutoLogOn.TabIndex = 0;
            this.chkAutoLogOn.Text = "Auto Logon For Current User";
            this.chkAutoLogOn.UseVisualStyleBackColor = true;
            this.chkAutoLogOn.CheckedChanged += new System.EventHandler(this.chkAutoLogOn_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.SystemColors.Control;
            this.label1.ForeColor = System.Drawing.SystemColors.Desktop;
            this.label1.Location = new System.Drawing.Point(193, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(150, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Scheduled Restart Application";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(552, 278);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.chkAutoLogOn);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkAutoLogOn;
        private System.Windows.Forms.Label label1;
    }
}

