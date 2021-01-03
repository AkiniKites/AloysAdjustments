
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloysAdjustments.Modules.Outfits
{
    partial class OutfitsControl
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
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.label1 = new System.Windows.Forms.Label();
            this.lbOutfits = new AloysAdjustments.Utility.ListBoxNF();
            this.lblModels = new System.Windows.Forms.Label();
            this.clbModels = new System.Windows.Forms.CheckedListBox();
            this.cbShowAll = new System.Windows.Forms.CheckBox();
            this.btnMode = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer.Location = new System.Drawing.Point(0, 15);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.label1);
            this.splitContainer.Panel1.Controls.Add(this.lbOutfits);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.lblModels);
            this.splitContainer.Panel2.Controls.Add(this.clbModels);
            this.splitContainer.Size = new System.Drawing.Size(646, 551);
            this.splitContainer.SplitterDistance = 262;
            this.splitContainer.TabIndex = 3;
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
            // lbOutfits
            // 
            this.lbOutfits.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbOutfits.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lbOutfits.FormattingEnabled = true;
            this.lbOutfits.IntegralHeight = false;
            this.lbOutfits.ItemHeight = 15;
            this.lbOutfits.Location = new System.Drawing.Point(0, 18);
            this.lbOutfits.Name = "lbOutfits";
            this.lbOutfits.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lbOutfits.Size = new System.Drawing.Size(259, 533);
            this.lbOutfits.TabIndex = 1;
            this.lbOutfits.SelectedValueChanged += new System.EventHandler(this.lbOutfits_SelectedValueChanged);
            this.lbOutfits.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lbOutfits_KeyDown);
            // 
            // lblModels
            // 
            this.lblModels.AutoSize = true;
            this.lblModels.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblModels.Location = new System.Drawing.Point(0, 0);
            this.lblModels.Name = "lblModels";
            this.lblModels.Size = new System.Drawing.Size(92, 15);
            this.lblModels.TabIndex = 5;
            this.lblModels.Text = "Model Mapping";
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
            this.clbModels.Size = new System.Drawing.Size(380, 533);
            this.clbModels.TabIndex = 0;
            this.clbModels.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.clbModels_ItemCheck);
            // 
            // cbShowAll
            // 
            this.cbShowAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbShowAll.AutoSize = true;
            this.cbShowAll.Location = new System.Drawing.Point(396, 14);
            this.cbShowAll.Name = "cbShowAll";
            this.cbShowAll.Size = new System.Drawing.Size(72, 19);
            this.cbShowAll.TabIndex = 4;
            this.cbShowAll.Text = "Show All";
            this.cbShowAll.UseVisualStyleBackColor = true;
            this.cbShowAll.CheckedChanged += new System.EventHandler(this.cbShowAll_CheckedChanged);
            // 
            // btnMode
            // 
            this.btnMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMode.Location = new System.Drawing.Point(474, 2);
            this.btnMode.Name = "btnMode";
            this.btnMode.Size = new System.Drawing.Size(172, 27);
            this.btnMode.TabIndex = 5;
            this.btnMode.Text = "Character Mode";
            this.btnMode.UseVisualStyleBackColor = true;
            this.btnMode.Click += new System.EventHandler(this.btnMode_Click);
            // 
            // OutfitsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnMode);
            this.Controls.Add(this.cbShowAll);
            this.Controls.Add(this.splitContainer);
            this.Name = "OutfitsControl";
            this.Size = new System.Drawing.Size(646, 566);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel1.PerformLayout();
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.Label label1;
        private Utility.ListBoxNF lbOutfits;
        private System.Windows.Forms.Label lblModels;
        private System.Windows.Forms.CheckedListBox clbModels;
        private System.Windows.Forms.CheckBox cbShowAll;
        private System.Windows.Forms.Button btnMode;
    }
}
