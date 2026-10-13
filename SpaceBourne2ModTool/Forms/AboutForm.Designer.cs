namespace SpaceBourne2ModTool.Forms
{
    partial class AboutForm
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.txtBody = new System.Windows.Forms.TextBox();
            this.lblGrok = new System.Windows.Forms.Label();
            this.lblTagline = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.SuspendLayout();
            this.picLogo.Location = new System.Drawing.Point(24, 20);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new System.Drawing.Size(96, 96);
            this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picLogo.TabStop = false;
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(136, 24);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Text = "specSB2modTool";
            this.lblVersion.AutoSize = true;
            this.lblVersion.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblVersion.Location = new System.Drawing.Point(140, 60);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Text = "Version 1.9.0";
            this.lblSubtitle.Location = new System.Drawing.Point(140, 84);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(360, 36);
            this.lblSubtitle.Text = "Unofficial Mod Creation Tool for SpaceBourne 2";
            this.txtBody.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtBody.Location = new System.Drawing.Point(24, 132);
            this.txtBody.Multiline = true;
            this.txtBody.Name = "txtBody";
            this.txtBody.ReadOnly = true;
            this.txtBody.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtBody.Size = new System.Drawing.Size(480, 220);
            this.lblGrok.Location = new System.Drawing.Point(24, 360);
            this.lblGrok.Name = "lblGrok";
            this.lblGrok.Size = new System.Drawing.Size(480, 36);
            this.lblGrok.Text = "Special thanks to Grok (xAI) for research, architecture, and feature development assistance.";
            this.lblTagline.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Italic);
            this.lblTagline.Location = new System.Drawing.Point(24, 404);
            this.lblTagline.Name = "lblTagline";
            this.lblTagline.Size = new System.Drawing.Size(360, 24);
            this.lblTagline.Text = "a creation by spec";
            this.btnClose.Location = new System.Drawing.Point(404, 400);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(100, 32);
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(528, 450);
            this.Controls.Add(this.picLogo);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.lblSubtitle);
            this.Controls.Add(this.txtBody);
            this.Controls.Add(this.lblGrok);
            this.Controls.Add(this.lblTagline);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "specSB2modTool — About";
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.TextBox txtBody;
        private System.Windows.Forms.Label lblGrok;
        private System.Windows.Forms.Label lblTagline;
        private System.Windows.Forms.Button btnClose;
    }
}
