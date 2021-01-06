
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloysAdjustments.Modules.Upgrades
{
    partial class UpgradesControl
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
            this.dgvUpgrades = new System.Windows.Forms.DataGridView();
            this.btnMulti10 = new System.Windows.Forms.Button();
            this.btnMulti2 = new System.Windows.Forms.Button();
            this.btnMulti5 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUpgrades)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvUpgrades
            // 
            this.dgvUpgrades.AllowUserToAddRows = false;
            this.dgvUpgrades.AllowUserToDeleteRows = false;
            this.dgvUpgrades.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvUpgrades.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgvUpgrades.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvUpgrades.Location = new System.Drawing.Point(0, 0);
            this.dgvUpgrades.Name = "dgvUpgrades";
            this.dgvUpgrades.RowTemplate.Height = 25;
            this.dgvUpgrades.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvUpgrades.Size = new System.Drawing.Size(580, 572);
            this.dgvUpgrades.TabIndex = 0;
            this.dgvUpgrades.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvUpgrades_CellValueChanged);
            this.dgvUpgrades.SelectionChanged += new System.EventHandler(this.dgvUpgrades_SelectionChanged);
            // 
            // btnMulti10
            // 
            this.btnMulti10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMulti10.Location = new System.Drawing.Point(586, 62);
            this.btnMulti10.Name = "btnMulti10";
            this.btnMulti10.Size = new System.Drawing.Size(60, 25);
            this.btnMulti10.TabIndex = 1;
            this.btnMulti10.Text = "x10";
            this.btnMulti10.UseVisualStyleBackColor = true;
            this.btnMulti10.Click += new System.EventHandler(this.btnMulti10_Click);
            // 
            // btnMulti2
            // 
            this.btnMulti2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMulti2.Location = new System.Drawing.Point(586, 0);
            this.btnMulti2.Name = "btnMulti2";
            this.btnMulti2.Size = new System.Drawing.Size(60, 25);
            this.btnMulti2.TabIndex = 2;
            this.btnMulti2.Text = "x2";
            this.btnMulti2.UseVisualStyleBackColor = true;
            this.btnMulti2.Click += new System.EventHandler(this.btnMulti2_Click);
            // 
            // btnMulti5
            // 
            this.btnMulti5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMulti5.Location = new System.Drawing.Point(586, 31);
            this.btnMulti5.Name = "btnMulti5";
            this.btnMulti5.Size = new System.Drawing.Size(60, 25);
            this.btnMulti5.TabIndex = 3;
            this.btnMulti5.Text = "x5";
            this.btnMulti5.UseVisualStyleBackColor = true;
            this.btnMulti5.Click += new System.EventHandler(this.btnMulti5_Click);
            // 
            // UpgradesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnMulti5);
            this.Controls.Add(this.btnMulti2);
            this.Controls.Add(this.btnMulti10);
            this.Controls.Add(this.dgvUpgrades);
            this.Name = "UpgradesControl";
            this.Size = new System.Drawing.Size(646, 572);
            ((System.ComponentModel.ISupportInitialize)(this.dgvUpgrades)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvUpgrades;
        private System.Windows.Forms.Button btnMulti10;
        private System.Windows.Forms.Button btnMulti2;
        private System.Windows.Forms.Button btnMulti5;
    }
}
