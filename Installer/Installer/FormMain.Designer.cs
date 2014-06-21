namespace SharpKit.Installer
{
    partial class FormMain
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.tbLog = new System.Windows.Forms.TextBox();
            this.butInstall = new System.Windows.Forms.Button();
            this.cbWebInstaller = new System.Windows.Forms.CheckBox();
            this.radioInstall = new System.Windows.Forms.RadioButton();
            this.radioUninstall = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbLog
            // 
            this.tbLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbLog.BackColor = System.Drawing.Color.White;
            this.tbLog.Location = new System.Drawing.Point(68, 201);
            this.tbLog.Multiline = true;
            this.tbLog.Name = "tbLog";
            this.tbLog.ReadOnly = true;
            this.tbLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbLog.Size = new System.Drawing.Size(459, 109);
            this.tbLog.TabIndex = 4;
            this.tbLog.WordWrap = false;
            // 
            // butInstall
            // 
            this.butInstall.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.butInstall.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.butInstall.Location = new System.Drawing.Point(438, 119);
            this.butInstall.Name = "butInstall";
            this.butInstall.Size = new System.Drawing.Size(75, 23);
            this.butInstall.TabIndex = 0;
            this.butInstall.Text = "Next";
            this.butInstall.UseVisualStyleBackColor = true;
            this.butInstall.Click += new System.EventHandler(this.butInstall_Click);
            // 
            // cbWebInstaller
            // 
            this.cbWebInstaller.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbWebInstaller.AutoSize = true;
            this.cbWebInstaller.BackColor = System.Drawing.Color.Transparent;
            this.cbWebInstaller.Checked = true;
            this.cbWebInstaller.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbWebInstaller.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.cbWebInstaller.Location = new System.Drawing.Point(68, 152);
            this.cbWebInstaller.Name = "cbWebInstaller";
            this.cbWebInstaller.Size = new System.Drawing.Size(142, 21);
            this.cbWebInstaller.TabIndex = 1;
            this.cbWebInstaller.Text = "Check for updates";
            this.cbWebInstaller.UseVisualStyleBackColor = false;
            this.cbWebInstaller.CheckedChanged += new System.EventHandler(this.cbWebInstaller_CheckedChanged);
            // 
            // radioInstall
            // 
            this.radioInstall.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.radioInstall.AutoSize = true;
            this.radioInstall.Checked = true;
            this.radioInstall.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.radioInstall.Location = new System.Drawing.Point(68, 106);
            this.radioInstall.Name = "radioInstall";
            this.radioInstall.Size = new System.Drawing.Size(116, 21);
            this.radioInstall.TabIndex = 2;
            this.radioInstall.TabStop = true;
            this.radioInstall.Text = "Install / Repair";
            this.radioInstall.UseVisualStyleBackColor = true;
            // 
            // radioUninstall
            // 
            this.radioUninstall.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.radioUninstall.AutoSize = true;
            this.radioUninstall.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.radioUninstall.Location = new System.Drawing.Point(68, 129);
            this.radioUninstall.Name = "radioUninstall";
            this.radioUninstall.Size = new System.Drawing.Size(80, 21);
            this.radioUninstall.TabIndex = 3;
            this.radioUninstall.Text = "Uninstall";
            this.radioUninstall.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel1.BackgroundImage")));
            this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.panel1.Controls.Add(this.butInstall);
            this.panel1.Location = new System.Drawing.Point(69, 291);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(524, 152);
            this.panel1.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.label1.Location = new System.Drawing.Point(182, 65);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(230, 22);
            this.label1.TabIndex = 6;
            this.label1.Text = "Welcome to SharpKit Setup";
            // 
            // FormMain
            // 
            this.AcceptButton = this.butInstall;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(594, 444);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbWebInstaller);
            this.Controls.Add(this.radioUninstall);
            this.Controls.Add(this.radioInstall);
            this.Controls.Add(this.tbLog);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormMain";
            this.Text = "SharpKit Setup";
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbLog;
        private System.Windows.Forms.Button butInstall;
        private System.Windows.Forms.CheckBox cbWebInstaller;
        private System.Windows.Forms.RadioButton radioInstall;
        private System.Windows.Forms.RadioButton radioUninstall;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
    }
}

