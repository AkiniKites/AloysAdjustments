namespace HZDUtility
{
    partial class Form1
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
            this.btnUpdateDefaultMaps = new System.Windows.Forms.Button();
            this.lbOutfits = new System.Windows.Forms.ListBox();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.clbMappedOutfits = new System.Windows.Forms.CheckedListBox();
            this.ssMain = new System.Windows.Forms.StatusStrip();
            this.tssStatus = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnUpdateDefaultMaps
            // 
            this.btnUpdateDefaultMaps.Location = new System.Drawing.Point(12, 12);
            this.btnUpdateDefaultMaps.Name = "btnUpdateDefaultMaps";
            this.btnUpdateDefaultMaps.Size = new System.Drawing.Size(219, 28);
            this.btnUpdateDefaultMaps.TabIndex = 0;
            this.btnUpdateDefaultMaps.Text = "Update Default Maps";
            this.btnUpdateDefaultMaps.UseVisualStyleBackColor = true;
            this.btnUpdateDefaultMaps.Click += new System.EventHandler(this.btnUpdateDefaultMaps_Click);
            // 
            // lbOutfits
            // 
            this.lbOutfits.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbOutfits.FormattingEnabled = true;
            this.lbOutfits.ItemHeight = 15;
            this.lbOutfits.Location = new System.Drawing.Point(0, 0);
            this.lbOutfits.Name = "lbOutfits";
            this.lbOutfits.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lbOutfits.Size = new System.Drawing.Size(361, 529);
            this.lbOutfits.TabIndex = 1;
            this.lbOutfits.SelectedValueChanged += new System.EventHandler(this.lbxArmors_SelectedValueChanged);
            // 
            // splitContainer
            // 
            this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer.Location = new System.Drawing.Point(12, 46);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.lbOutfits);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.clbMappedOutfits);
            this.splitContainer.Size = new System.Drawing.Size(1085, 529);
            this.splitContainer.SplitterDistance = 361;
            this.splitContainer.TabIndex = 2;
            this.splitContainer.Text = "splitContainer1";
            // 
            // clbMappedOutfits
            // 
            this.clbMappedOutfits.Dock = System.Windows.Forms.DockStyle.Fill;
            this.clbMappedOutfits.FormattingEnabled = true;
            this.clbMappedOutfits.Location = new System.Drawing.Point(0, 0);
            this.clbMappedOutfits.Name = "clbMappedOutfits";
            this.clbMappedOutfits.Size = new System.Drawing.Size(720, 529);
            this.clbMappedOutfits.TabIndex = 0;
            // 
            // ssMain
            // 
            this.ssMain.Location = new System.Drawing.Point(0, 578);
            this.ssMain.Name = "ssMain";
            this.ssMain.Size = new System.Drawing.Size(1109, 22);
            this.ssMain.TabIndex = 3;
            // 
            // tssStatus
            // 
            this.tssStatus.Name = "tssStatus";
            this.tssStatus.Size = new System.Drawing.Size(53, 17);
            this.tssStatus.Text = "tssStatus";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1109, 600);
            this.Controls.Add(this.ssMain);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.btnUpdateDefaultMaps);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnUpdateDefaultMaps;
        private System.Windows.Forms.ListBox lbOutfits;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.CheckedListBox clbMappedOutfits;
        private System.Windows.Forms.StatusStrip ssMain;
        private System.Windows.Forms.ToolStripStatusLabel tssStatus;
    }
}

