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
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.clbModels = new System.Windows.Forms.CheckedListBox();
            this.ssMain = new System.Windows.Forms.StatusStrip();
            this.tssStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnPatch = new System.Windows.Forms.Button();
            this.btnDecima = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.lblGameDir = new System.Windows.Forms.Label();
            this.btnGameDir = new System.Windows.Forms.Button();
            this.lblDecima = new System.Windows.Forms.Label();
            this.btnResetSelected = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.ssMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnLoadPatch
            // 
            this.btnLoadPatch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLoadPatch.Location = new System.Drawing.Point(729, 93);
            this.btnLoadPatch.Name = "btnLoadPatch";
            this.btnLoadPatch.Size = new System.Drawing.Size(100, 28);
            this.btnLoadPatch.TabIndex = 0;
            this.btnLoadPatch.Text = "Load Patch";
            this.btnLoadPatch.UseVisualStyleBackColor = true;
            this.btnLoadPatch.Click += new System.EventHandler(this.btnLoadPatch_Click);
            // 
            // splitContainer
            // 
            this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer.Location = new System.Drawing.Point(12, 75);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.label2);
            this.splitContainer.Panel2.Controls.Add(this.clbModels);
            this.splitContainer.Size = new System.Drawing.Size(711, 566);
            this.splitContainer.SplitterDistance = 355;
            this.splitContainer.TabIndex = 2;
            this.splitContainer.Text = "splitContainer1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 15);
            this.label1.TabIndex = 4;
            this.label1.Text = "Outfits";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(92, 15);
            this.label2.TabIndex = 5;
            this.label2.Text = "Model Mapping";
            // 
            // clbModels
            // 
            this.clbModels.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.clbModels.CheckOnClick = true;
            this.clbModels.FormattingEnabled = true;
            this.clbModels.IntegralHeight = false;
            this.clbModels.Location = new System.Drawing.Point(0, 18);
            this.clbModels.Name = "clbModels";
            this.clbModels.Size = new System.Drawing.Size(349, 519);
            this.clbModels.TabIndex = 0;
            this.clbModels.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.clbModels_ItemCheck);
            // 
            // ssMain
            // 
            this.ssMain.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ssMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tssStatus});
            this.ssMain.Location = new System.Drawing.Point(0, 615);
            this.ssMain.Name = "ssMain";
            this.ssMain.Size = new System.Drawing.Size(841, 22);
            this.ssMain.TabIndex = 3;
            // 
            // tssStatus
            // 
            this.tssStatus.Name = "tssStatus";
            this.tssStatus.Size = new System.Drawing.Size(63, 17);
            this.tssStatus.Text = "Status Text";
            // 
            // btnPatch
            // 
            this.btnPatch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPatch.Location = new System.Drawing.Point(729, 570);
            this.btnPatch.Name = "btnPatch";
            this.btnPatch.Size = new System.Drawing.Size(100, 42);
            this.btnPatch.TabIndex = 4;
            this.btnPatch.Text = "Create Patch";
            this.btnPatch.UseVisualStyleBackColor = true;
            this.btnPatch.Click += new System.EventHandler(this.btnPatch_Click);
            // 
            // btnDecima
            // 
            this.btnDecima.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDecima.Location = new System.Drawing.Point(729, 26);
            this.btnDecima.Name = "btnDecima";
            this.btnDecima.Size = new System.Drawing.Size(100, 25);
            this.btnDecima.TabIndex = 5;
            this.btnDecima.Text = "Setup Decima";
            this.btnDecima.UseVisualStyleBackColor = true;
            this.btnDecima.Click += new System.EventHandler(this.btnDecima_Click);
            // 
            // btnReset
            // 
            this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReset.Location = new System.Drawing.Point(729, 195);
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
            this.lblGameDir.Location = new System.Drawing.Point(12, 9);
            this.lblGameDir.Name = "lblGameDir";
            this.lblGameDir.Size = new System.Drawing.Size(74, 15);
            this.lblGameDir.TabIndex = 7;
            this.lblGameDir.Text = "Game Folder";
            // 
            // btnGameDir
            // 
            this.btnGameDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGameDir.Location = new System.Drawing.Point(623, 26);
            this.btnGameDir.Name = "btnGameDir";
            this.btnGameDir.Size = new System.Drawing.Size(100, 25);
            this.btnGameDir.TabIndex = 9;
            this.btnGameDir.Text = "Browse";
            this.btnGameDir.UseVisualStyleBackColor = true;
            this.btnGameDir.Click += new System.EventHandler(this.btnGameDir_Click);
            // 
            // lblDecima
            // 
            this.lblDecima.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDecima.AutoSize = true;
            this.lblDecima.Location = new System.Drawing.Point(729, 9);
            this.lblDecima.Name = "lblDecima";
            this.lblDecima.Size = new System.Drawing.Size(47, 15);
            this.lblDecima.TabIndex = 10;
            this.lblDecima.Text = "Decima";
            // 
            // btnResetSelected
            // 
            this.btnResetSelected.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnResetSelected.Enabled = false;
            this.btnResetSelected.Location = new System.Drawing.Point(729, 161);
            this.btnResetSelected.Name = "btnResetSelected";
            this.btnResetSelected.Size = new System.Drawing.Size(100, 28);
            this.btnResetSelected.TabIndex = 12;
            this.btnResetSelected.Text = "Reset Selected";
            this.btnResetSelected.UseVisualStyleBackColor = true;
            this.btnResetSelected.Click += new System.EventHandler(this.btnResetSelected_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(841, 637);
            this.Controls.Add(this.btnResetSelected);
            this.Controls.Add(this.lblDecima);
            this.Controls.Add(this.btnGameDir);
            this.Controls.Add(this.lblGameDir);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.btnDecima);
            this.Controls.Add(this.btnPatch);
            this.Controls.Add(this.ssMain);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.btnLoadPatch);
            this.Name = "Main";
            this.Text = "Outfit Editor";
            this.Load += new System.EventHandler(this.Main_Load);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel1.PerformLayout();
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.ssMain.ResumeLayout(false);
            this.ssMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        
        private ListBoxNF lbOutfits;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.CheckedListBox clbModels;
        private System.Windows.Forms.StatusStrip ssMain;
        private System.Windows.Forms.ToolStripStatusLabel tssStatus;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnPatch;
        private System.Windows.Forms.Button btnDecima;
        private System.Windows.Forms.Button btnLoadPatch;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Label lblGameDir;
        private System.Windows.Forms.Button btnGameDir;
        private System.Windows.Forms.Label lblDecima;
        private TypeDelayTextBox tbGameDir;
        private System.Windows.Forms.Button btnResetSelected;
    }
}

