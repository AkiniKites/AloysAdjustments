using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Utility;

namespace AloysAdjustments
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnLoadPatch = new System.Windows.Forms.Button();
            this.ssMain = new System.Windows.Forms.StatusStrip();
            this.tssStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnPatch = new System.Windows.Forms.Button();
            this.btnArchiver = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.lblGameDir = new System.Windows.Forms.Label();
            this.btnGameDir = new System.Windows.Forms.Button();
            this.tbGameDir = new AloysAdjustments.Utility.TypeDelayTextBox();
            this.btnResetSelected = new System.Windows.Forms.Button();
            this.tcMain = new System.Windows.Forms.TabControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnDeletePack = new System.Windows.Forms.Button();
            this.lblPackStatus = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblArchiverLib = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tpbStatus = new System.Windows.Forms.ToolStripProgressBar();
            this.ssMain.SuspendLayout();
            this.tcMain.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnLoadPatch
            // 
            this.btnLoadPatch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLoadPatch.Location = new System.Drawing.Point(648, 12);
            this.btnLoadPatch.Name = "btnLoadPatch";
            this.btnLoadPatch.Size = new System.Drawing.Size(100, 28);
            this.btnLoadPatch.TabIndex = 0;
            this.btnLoadPatch.Text = "Load";
            this.btnLoadPatch.UseVisualStyleBackColor = true;
            this.btnLoadPatch.Click += new System.EventHandler(this.btnLoadPatch_Click);
            // 
            // ssMain
            // 
            this.ssMain.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ssMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tssStatus,
            this.tpbStatus});
            this.ssMain.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.ssMain.Location = new System.Drawing.Point(0, 615);
            this.ssMain.Name = "ssMain";
            this.ssMain.Size = new System.Drawing.Size(760, 22);
            this.ssMain.TabIndex = 3;
            // 
            // tssStatus
            // 
            this.tssStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.tssStatus.Name = "tssStatus";
            this.tssStatus.Size = new System.Drawing.Size(70, 17);
            this.tssStatus.Text = "Status Text";
            // 
            // btnPatch
            // 
            this.btnPatch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPatch.Location = new System.Drawing.Point(648, 570);
            this.btnPatch.Name = "btnPatch";
            this.btnPatch.Size = new System.Drawing.Size(100, 42);
            this.btnPatch.TabIndex = 4;
            this.btnPatch.Text = "Install Pack";
            this.btnPatch.UseVisualStyleBackColor = true;
            this.btnPatch.Click += new System.EventHandler(this.btnPatch_Click);
            // 
            // btnArchiver
            // 
            this.btnArchiver.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnArchiver.Location = new System.Drawing.Point(510, 15);
            this.btnArchiver.Name = "btnArchiver";
            this.btnArchiver.Size = new System.Drawing.Size(100, 25);
            this.btnArchiver.TabIndex = 5;
            this.btnArchiver.Text = "Get Oodle DLL";
            this.btnArchiver.UseVisualStyleBackColor = true;
            this.btnArchiver.Click += new System.EventHandler(this.btnArchiver_Click);
            // 
            // btnReset
            // 
            this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReset.Location = new System.Drawing.Point(648, 114);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(100, 28);
            this.btnReset.TabIndex = 6;
            this.btnReset.Text = "Reset All";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // lblGameDir
            // 
            this.lblGameDir.AutoSize = true;
            this.lblGameDir.Location = new System.Drawing.Point(3, 9);
            this.lblGameDir.Name = "lblGameDir";
            this.lblGameDir.Size = new System.Drawing.Size(74, 15);
            this.lblGameDir.TabIndex = 7;
            this.lblGameDir.Text = "Game Folder";
            // 
            // btnGameDir
            // 
            this.btnGameDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGameDir.Location = new System.Drawing.Point(549, 6);
            this.btnGameDir.Name = "btnGameDir";
            this.btnGameDir.Size = new System.Drawing.Size(70, 25);
            this.btnGameDir.TabIndex = 9;
            this.btnGameDir.Text = ". . .";
            this.btnGameDir.UseVisualStyleBackColor = true;
            this.btnGameDir.Click += new System.EventHandler(this.btnGameDir_Click);
            // 
            // tbGameDir
            // 
            this.tbGameDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbGameDir.EnableTypingEvent = true;
            this.tbGameDir.Location = new System.Drawing.Point(83, 6);
            this.tbGameDir.Name = "tbGameDir";
            this.tbGameDir.Size = new System.Drawing.Size(460, 23);
            this.tbGameDir.TabIndex = 11;
            this.tbGameDir.TypingFinished += new System.EventHandler(this.tbGameDir_TypingFinished);
            this.tbGameDir.TextChanged += new System.EventHandler(this.tbGameDir_TextChanged);
            // 
            // btnResetSelected
            // 
            this.btnResetSelected.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnResetSelected.Enabled = false;
            this.btnResetSelected.Location = new System.Drawing.Point(648, 80);
            this.btnResetSelected.Name = "btnResetSelected";
            this.btnResetSelected.Size = new System.Drawing.Size(100, 28);
            this.btnResetSelected.TabIndex = 12;
            this.btnResetSelected.Text = "Reset Selected";
            this.btnResetSelected.UseVisualStyleBackColor = true;
            this.btnResetSelected.Click += new System.EventHandler(this.btnResetSelected_Click);
            // 
            // tcMain
            // 
            this.tcMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tcMain.Controls.Add(this.tabPage2);
            this.tcMain.Location = new System.Drawing.Point(12, 12);
            this.tcMain.Name = "tcMain";
            this.tcMain.SelectedIndex = 0;
            this.tcMain.Size = new System.Drawing.Size(630, 600);
            this.tcMain.TabIndex = 13;
            this.tcMain.SelectedIndexChanged += new System.EventHandler(this.tcMain_SelectedIndexChanged);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.groupBox2);
            this.tabPage2.Controls.Add(this.groupBox1);
            this.tabPage2.Controls.Add(this.lblGameDir);
            this.tabPage2.Controls.Add(this.tbGameDir);
            this.tabPage2.Controls.Add(this.btnGameDir);
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(622, 572);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Settings";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.btnDeletePack);
            this.groupBox2.Controls.Add(this.lblPackStatus);
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Location = new System.Drawing.Point(3, 522);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(616, 47);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Game Pack";
            // 
            // btnDeletePack
            // 
            this.btnDeletePack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDeletePack.Location = new System.Drawing.Point(510, 15);
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
            this.lblPackStatus.Location = new System.Drawing.Point(79, 22);
            this.lblPackStatus.Name = "lblPackStatus";
            this.lblPackStatus.Size = new System.Drawing.Size(100, 15);
            this.lblPackStatus.TabIndex = 12;
            this.lblPackStatus.Text = "Pack not installed";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(926, 14);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 25);
            this.button1.TabIndex = 5;
            this.button1.Text = "Update Decima";
            this.button1.UseVisualStyleBackColor = true;
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
            this.groupBox1.Location = new System.Drawing.Point(3, 36);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(616, 47);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Packager";
            // 
            // lblArchiverLib
            // 
            this.lblArchiverLib.AutoSize = true;
            this.lblArchiverLib.ForeColor = System.Drawing.Color.ForestGreen;
            this.lblArchiverLib.Location = new System.Drawing.Point(79, 22);
            this.lblArchiverLib.Name = "lblArchiverLib";
            this.lblArchiverLib.Size = new System.Drawing.Size(23, 15);
            this.lblArchiverLib.TabIndex = 12;
            this.lblArchiverLib.Text = "OK";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 15);
            this.label3.TabIndex = 11;
            this.label3.Text = "Oodle DLL:";
            // 
            // tpbStatus
            // 
            this.tpbStatus.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tpbStatus.Name = "tpbStatus";
            this.tpbStatus.Size = new System.Drawing.Size(200, 16);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(760, 637);
            this.Controls.Add(this.btnResetSelected);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.btnPatch);
            this.Controls.Add(this.ssMain);
            this.Controls.Add(this.btnLoadPatch);
            this.Controls.Add(this.tcMain);
            this.Name = "Main";
            this.Text = "Aloy\'s Adjustments";
            this.Load += new System.EventHandler(this.Main_Load);
            this.ssMain.ResumeLayout(false);
            this.ssMain.PerformLayout();
            this.tcMain.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.StatusStrip ssMain;
        private System.Windows.Forms.ToolStripStatusLabel tssStatus;
        private System.Windows.Forms.Button btnPatch;
        private System.Windows.Forms.Button btnArchiver;
        private System.Windows.Forms.Button btnLoadPatch;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Label lblGameDir;
        private System.Windows.Forms.Button btnGameDir;
        private TypeDelayTextBox tbGameDir;
        private System.Windows.Forms.Button btnResetSelected;
        private System.Windows.Forms.TabControl tcMain;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblArchiverLib;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnDeletePack;
        private System.Windows.Forms.Label lblPackStatus;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ToolStripProgressBar tpbStatus;
    }
}

