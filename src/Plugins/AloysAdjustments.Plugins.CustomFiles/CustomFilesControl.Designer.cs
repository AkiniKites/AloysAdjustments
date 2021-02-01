using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloysAdjustments.Plugins.CustomFiles
{
    partial class CustomFilesControl
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
            this.btnAddFolder = new System.Windows.Forms.Button();
            this.lvFiles = new BrightIdeasSoftware.FastObjectListView();
            this.btnAddFiles = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnAddFolder
            // 
            this.btnAddFolder.Location = new System.Drawing.Point(2, 2);
            this.btnAddFolder.Name = "btnAddFolder";
            this.btnAddFolder.Size = new System.Drawing.Size(100, 25);
            this.btnAddFolder.TabIndex = 6;
            this.btnAddFolder.Text = "Add Folder";
            this.btnAddFolder.UseVisualStyleBackColor = true;
            this.btnAddFolder.Click += new System.EventHandler(this.btnAddFolder_Click);
            // 
            // lvFiles
            // 
            this.lvFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvFiles.FullRowSelect = true;
            this.lvFiles.HideSelection = false;
            this.lvFiles.Location = new System.Drawing.Point(2, 33);
            this.lvFiles.Name = "lvFiles";
            this.lvFiles.Size = new System.Drawing.Size(643, 539);
            this.lvFiles.TabIndex = 7;
            this.lvFiles.UseCompatibleStateImageBehavior = false;
            this.lvFiles.View = System.Windows.Forms.View.Details;
            // 
            // btnAddFiles
            // 
            this.btnAddFiles.Location = new System.Drawing.Point(108, 2);
            this.btnAddFiles.Name = "btnAddFiles";
            this.btnAddFiles.Size = new System.Drawing.Size(100, 25);
            this.btnAddFiles.TabIndex = 8;
            this.btnAddFiles.Text = "Add Files";
            this.btnAddFiles.UseVisualStyleBackColor = true;
            this.btnAddFiles.Click += new System.EventHandler(this.btnAddFiles_Click);
            // 
            // CustomFilesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnAddFiles);
            this.Controls.Add(this.lvFiles);
            this.Controls.Add(this.btnAddFolder);
            this.Name = "CustomFilesControl";
            this.Size = new System.Drawing.Size(646, 572);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnAddFolder;
        private BrightIdeasSoftware.FastObjectListView lvFiles;
        private System.Windows.Forms.Button btnAddFiles;
    }
}
