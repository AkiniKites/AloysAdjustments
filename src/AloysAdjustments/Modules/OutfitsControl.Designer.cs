
namespace AloysAdjustments.Modules
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
            this.label2 = new System.Windows.Forms.Label();
            this.clbModels = new System.Windows.Forms.CheckedListBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.label1);
            this.splitContainer.Panel1.Controls.Add(this.lbOutfits);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.label2);
            this.splitContainer.Panel2.Controls.Add(this.clbModels);
            this.splitContainer.Size = new System.Drawing.Size(616, 566);
            this.splitContainer.SplitterDistance = 250;
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
            this.lbOutfits.Size = new System.Drawing.Size(247, 548);
            this.lbOutfits.TabIndex = 1;
            this.lbOutfits.SelectedValueChanged += new System.EventHandler(this.lbOutfits_SelectedValueChanged);
            this.lbOutfits.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lbOutfits_KeyDown);
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
            this.clbModels.Size = new System.Drawing.Size(362, 548);
            this.clbModels.TabIndex = 0;
            this.clbModels.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.clbModels_ItemCheck);
            // 
            // OutfitsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer);
            this.Name = "OutfitsControl";
            this.Size = new System.Drawing.Size(616, 566);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel1.PerformLayout();
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.Label label1;
        private Utility.ListBoxNF lbOutfits;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckedListBox clbModels;
    }
}
