
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
            PresentationControls.CheckBoxProperties checkBoxProperties1 = new PresentationControls.CheckBoxProperties();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.label1 = new System.Windows.Forms.Label();
            this.lbOutfits = new AloysAdjustments.Utility.ListBoxNF();
            this.lblModels = new System.Windows.Forms.Label();
            this.clbModels = new System.Windows.Forms.CheckedListBox();
            this.cbAllOutfits = new System.Windows.Forms.CheckBox();
            this.ccbModelFilter = new PresentationControls.CheckBoxComboBox();
            this.label2 = new System.Windows.Forms.Label();
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
            this.splitContainer.Location = new System.Drawing.Point(0, 23);
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
            this.splitContainer.Size = new System.Drawing.Size(646, 549);
            this.splitContainer.SplitterDistance = 261;
            this.splitContainer.TabIndex = 3;
            this.splitContainer.Text = "splitContainer1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 15);
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
            this.lbOutfits.Size = new System.Drawing.Size(258, 531);
            this.lbOutfits.TabIndex = 1;
            this.lbOutfits.SelectedValueChanged += new System.EventHandler(this.lbOutfits_SelectedValueChanged);
            this.lbOutfits.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lbOutfits_KeyDown);
            // 
            // lblModels
            // 
            this.lblModels.AutoSize = true;
            this.lblModels.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblModels.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblModels.Location = new System.Drawing.Point(0, 0);
            this.lblModels.Name = "lblModels";
            this.lblModels.Size = new System.Drawing.Size(93, 15);
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
            this.clbModels.Size = new System.Drawing.Size(378, 531);
            this.clbModels.TabIndex = 0;
            this.clbModels.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.clbModels_ItemCheck);
            // 
            // cbAllOutfits
            // 
            this.cbAllOutfits.AutoSize = true;
            this.cbAllOutfits.Location = new System.Drawing.Point(170, 22);
            this.cbAllOutfits.Name = "cbAllOutfits";
            this.cbAllOutfits.Size = new System.Drawing.Size(89, 19);
            this.cbAllOutfits.TabIndex = 6;
            this.cbAllOutfits.Text = "Apply To All";
            this.cbAllOutfits.UseVisualStyleBackColor = true;
            this.cbAllOutfits.CheckedChanged += new System.EventHandler(this.cbAllOutfits_CheckedChanged);
            // 
            // ccbModelFilter
            // 
            checkBoxProperties1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ccbModelFilter.CheckBoxProperties = checkBoxProperties1;
            this.ccbModelFilter.DisplayMemberSingleItem = "";
            this.ccbModelFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ccbModelFilter.FormattingEnabled = true;
            this.ccbModelFilter.Location = new System.Drawing.Point(472, 2);
            this.ccbModelFilter.Name = "ccbModelFilter";
            this.ccbModelFilter.Size = new System.Drawing.Size(172, 23);
            this.ccbModelFilter.TabIndex = 7;
            this.ccbModelFilter.CheckBoxCheckedChanged += new System.EventHandler(this.ccbModelFilter_CheckBoxCheckedChanged);
            this.ccbModelFilter.DropDownClosed += new System.EventHandler(this.ccbModelFilter_DropDownClosedCommand);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(428, 5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 15);
            this.label2.TabIndex = 8;
            this.label2.Text = "Filter:";
            // 
            // OutfitsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ccbModelFilter);
            this.Controls.Add(this.cbAllOutfits);
            this.Controls.Add(this.splitContainer);
            this.Name = "OutfitsControl";
            this.Size = new System.Drawing.Size(646, 572);
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
        private System.Windows.Forms.CheckBox cbAllOutfits;
        private PresentationControls.CheckBoxComboBox ccbModelFilter;
        private System.Windows.Forms.Label label2;
    }
}
