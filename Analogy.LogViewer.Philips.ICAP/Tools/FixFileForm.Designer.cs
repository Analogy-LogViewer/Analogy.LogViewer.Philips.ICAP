namespace Analogy.LogViewer.Philips.ICAP.Tools
{
    partial class FixFileForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FixFileForm));
            this.labelControl1 = new System.Windows.Forms.Label();
            this.fixFileUC1 = new global::Analogy.LogViewer.Philips.ICAP.FixFileUC();
            this.SuspendLayout();
            // 
            // labelControl1
            // 
            this.labelControl1.Font = new System.Drawing.Font("Tahoma", 12F);
            this.labelControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelControl1.Location = new System.Drawing.Point(0, 0);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(920, 29);
            this.labelControl1.TabIndex = 0;
            this.labelControl1.Text = "Use this form to remove invalid XML characters in a xml log file";
            // 
            // fixFileUC1
            // 
            this.fixFileUC1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fixFileUC1.Location = new System.Drawing.Point(0, 29);
            this.fixFileUC1.Name = "fixFileUC1";
            this.fixFileUC1.Size = new System.Drawing.Size(920, 456);
            this.fixFileUC1.TabIndex = 1;
            // 
            // FixFileForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(920, 485);
            this.Controls.Add(this.fixFileUC1);
            this.Controls.Add(this.labelControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FixFileForm";
            this.Text = "Fix corrupted XML file";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelControl1;
        private FixFileUC fixFileUC1;
    }
}