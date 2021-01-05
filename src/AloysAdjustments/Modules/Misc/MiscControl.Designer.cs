
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloysAdjustments.Modules.Misc
{
    partial class MiscControl
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
            this.cbIntroLogos = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // cbIntroLogos
            // 
            this.cbIntroLogos.AutoSize = true;
            this.cbIntroLogos.Location = new System.Drawing.Point(9, 9);
            this.cbIntroLogos.Name = "cbIntroLogos";
            this.cbIntroLogos.Size = new System.Drawing.Size(132, 19);
            this.cbIntroLogos.TabIndex = 4;
            this.cbIntroLogos.Text = "Remove Intro Logos";
            this.cbIntroLogos.UseVisualStyleBackColor = true;
            this.cbIntroLogos.CheckedChanged += new System.EventHandler(this.cbIntroLogos_CheckedChanged);
            // 
            // MiscControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cbIntroLogos);
            this.Name = "MiscControl";
            this.Size = new System.Drawing.Size(646, 572);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.CheckBox cbIntroLogos;
    }
}
