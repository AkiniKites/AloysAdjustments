
namespace AloysAdjustments
{
    partial class SettingsControl
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
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnClearCache = new System.Windows.Forms.Button();
            this.lblCacheSize = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnDeletePack = new System.Windows.Forms.Button();
            this.lblPackStatus = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblArchiverLib = new System.Windows.Forms.Label();
            this.btnArchiver = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.lblGameDir = new System.Windows.Forms.Label();
            this.btnGameDir = new System.Windows.Forms.Button();
            this.tbGameDir = new AloysAdjustments.Utility.TypeDelayTextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.lblUpdateStatus = new System.Windows.Forms.Label();
            this.lblLatestVersion = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblCurrentVersion = new System.Windows.Forms.Label();
            this.btnUpdates = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.btnClearCache);
            this.groupBox3.Controls.Add(this.lblCacheSize);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Location = new System.Drawing.Point(3, 468);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(640, 47);
            this.groupBox3.TabIndex = 18;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Cache";
            // 
            // btnClearCache
            // 
            this.btnClearCache.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearCache.Location = new System.Drawing.Point(534, 15);
            this.btnClearCache.Name = "btnClearCache";
            this.btnClearCache.Size = new System.Drawing.Size(100, 25);
            this.btnClearCache.TabIndex = 13;
            this.btnClearCache.Text = "Clear Cache";
            this.btnClearCache.UseVisualStyleBackColor = true;
            this.btnClearCache.Click += new System.EventHandler(this.btnClearCache_Click);
            // 
            // lblCacheSize
            // 
            this.lblCacheSize.AutoSize = true;
            this.lblCacheSize.Location = new System.Drawing.Point(82, 22);
            this.lblCacheSize.Name = "lblCacheSize";
            this.lblCacheSize.Size = new System.Drawing.Size(30, 15);
            this.lblCacheSize.TabIndex = 12;
            this.lblCacheSize.Text = "0 KB";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 15);
            this.label2.TabIndex = 11;
            this.label2.Text = "Size:";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.btnDeletePack);
            this.groupBox2.Controls.Add(this.lblPackStatus);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Location = new System.Drawing.Point(3, 521);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(640, 47);
            this.groupBox2.TabIndex = 19;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Game Pack";
            // 
            // btnDeletePack
            // 
            this.btnDeletePack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDeletePack.Location = new System.Drawing.Point(534, 15);
            this.btnDeletePack.Name = "btnDeletePack";
            this.btnDeletePack.Size = new System.Drawing.Size(100, 25);
            this.btnDeletePack.TabIndex = 14;
            this.btnDeletePack.Text = "Delete Pack";
            this.btnDeletePack.UseVisualStyleBackColor = true;
            this.btnDeletePack.Click += new System.EventHandler(this.btnDeletePack_Click);
            // 
            // lblPackStatus
            // 
            this.lblPackStatus.AutoSize = true;
            this.lblPackStatus.Location = new System.Drawing.Point(82, 22);
            this.lblPackStatus.Name = "lblPackStatus";
            this.lblPackStatus.Size = new System.Drawing.Size(100, 15);
            this.lblPackStatus.TabIndex = 12;
            this.lblPackStatus.Text = "Pack not installed";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(42, 15);
            this.label5.TabIndex = 10;
            this.label5.Text = "Status:";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.lblArchiverLib);
            this.groupBox1.Controls.Add(this.btnArchiver);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.lblGameDir);
            this.groupBox1.Controls.Add(this.btnGameDir);
            this.groupBox1.Controls.Add(this.tbGameDir);
            this.groupBox1.Location = new System.Drawing.Point(3, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(640, 77);
            this.groupBox1.TabIndex = 17;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "General";
            // 
            // lblArchiverLib
            // 
            this.lblArchiverLib.AutoSize = true;
            this.lblArchiverLib.ForeColor = System.Drawing.Color.ForestGreen;
            this.lblArchiverLib.Location = new System.Drawing.Point(82, 50);
            this.lblArchiverLib.Name = "lblArchiverLib";
            this.lblArchiverLib.Size = new System.Drawing.Size(23, 15);
            this.lblArchiverLib.TabIndex = 12;
            this.lblArchiverLib.Text = "OK";
            // 
            // btnArchiver
            // 
            this.btnArchiver.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnArchiver.Location = new System.Drawing.Point(534, 45);
            this.btnArchiver.Name = "btnArchiver";
            this.btnArchiver.Size = new System.Drawing.Size(100, 25);
            this.btnArchiver.TabIndex = 5;
            this.btnArchiver.Text = "Get Oodle DLL";
            this.btnArchiver.UseVisualStyleBackColor = true;
            this.btnArchiver.Click += new System.EventHandler(this.btnArchiver_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 50);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 15);
            this.label3.TabIndex = 11;
            this.label3.Text = "Oodle DLL:";
            // 
            // lblGameDir
            // 
            this.lblGameDir.AutoSize = true;
            this.lblGameDir.Location = new System.Drawing.Point(6, 20);
            this.lblGameDir.Name = "lblGameDir";
            this.lblGameDir.Size = new System.Drawing.Size(77, 15);
            this.lblGameDir.TabIndex = 14;
            this.lblGameDir.Text = "Game Folder:";
            // 
            // btnGameDir
            // 
            this.btnGameDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGameDir.Location = new System.Drawing.Point(564, 15);
            this.btnGameDir.Name = "btnGameDir";
            this.btnGameDir.Size = new System.Drawing.Size(70, 25);
            this.btnGameDir.TabIndex = 15;
            this.btnGameDir.Text = ". . .";
            this.btnGameDir.UseVisualStyleBackColor = true;
            this.btnGameDir.Click += new System.EventHandler(this.btnGameDir_Click);
            // 
            // tbGameDir
            // 
            this.tbGameDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbGameDir.EnableTypingEvent = true;
            this.tbGameDir.Location = new System.Drawing.Point(86, 16);
            this.tbGameDir.Name = "tbGameDir";
            this.tbGameDir.Size = new System.Drawing.Size(472, 23);
            this.tbGameDir.TabIndex = 16;
            this.tbGameDir.TypingFinished += new System.EventHandler(this.tbGameDir_TypingFinished);
            this.tbGameDir.TextChanged += new System.EventHandler(this.tbGameDir_TextChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.lblUpdateStatus);
            this.groupBox4.Controls.Add(this.lblLatestVersion);
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.lblCurrentVersion);
            this.groupBox4.Controls.Add(this.btnUpdates);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Location = new System.Drawing.Point(3, 83);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(640, 66);
            this.groupBox4.TabIndex = 18;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Updates";
            // 
            // lblUpdateStatus
            // 
            this.lblUpdateStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblUpdateStatus.Location = new System.Drawing.Point(156, 22);
            this.lblUpdateStatus.Name = "lblUpdateStatus";
            this.lblUpdateStatus.Size = new System.Drawing.Size(372, 35);
            this.lblUpdateStatus.TabIndex = 16;
            // 
            // lblLatestVersion
            // 
            this.lblLatestVersion.AutoSize = true;
            this.lblLatestVersion.Location = new System.Drawing.Point(82, 42);
            this.lblLatestVersion.Name = "lblLatestVersion";
            this.lblLatestVersion.Size = new System.Drawing.Size(28, 15);
            this.lblLatestVersion.TabIndex = 14;
            this.lblLatestVersion.Text = "       ";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 42);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 15);
            this.label6.TabIndex = 13;
            this.label6.Text = "Latest:";
            // 
            // lblCurrentVersion
            // 
            this.lblCurrentVersion.AutoSize = true;
            this.lblCurrentVersion.Location = new System.Drawing.Point(82, 22);
            this.lblCurrentVersion.Name = "lblCurrentVersion";
            this.lblCurrentVersion.Size = new System.Drawing.Size(28, 15);
            this.lblCurrentVersion.TabIndex = 12;
            this.lblCurrentVersion.Text = "       ";
            // 
            // btnUpdates
            // 
            this.btnUpdates.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUpdates.Location = new System.Drawing.Point(534, 15);
            this.btnUpdates.Name = "btnUpdates";
            this.btnUpdates.Size = new System.Drawing.Size(100, 25);
            this.btnUpdates.TabIndex = 5;
            this.btnUpdates.Text = "Check Updates";
            this.btnUpdates.UseVisualStyleBackColor = true;
            this.btnUpdates.Click += new System.EventHandler(this.btnUpdates_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(50, 15);
            this.label4.TabIndex = 11;
            this.label4.Text = "Current:";
            // 
            // SettingsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "SettingsControl";
            this.Size = new System.Drawing.Size(646, 572);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnClearCache;
        private System.Windows.Forms.Label lblCacheSize;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnDeletePack;
        private System.Windows.Forms.Label lblPackStatus;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblArchiverLib;
        private System.Windows.Forms.Button btnArchiver;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblGameDir;
        private Utility.TypeDelayTextBox tbGameDir;
        private System.Windows.Forms.Button btnGameDir;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label lblLatestVersion;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblCurrentVersion;
        private System.Windows.Forms.Button btnUpdates;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblUpdateStatus;
    }
}
