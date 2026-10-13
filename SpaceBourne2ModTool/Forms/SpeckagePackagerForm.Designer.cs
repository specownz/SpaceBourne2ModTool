namespace SpaceBourne2ModTool.Forms
{
    partial class SpeckagePackagerForm
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSrc = new System.Windows.Forms.Label();
            this.txtSource = new System.Windows.Forms.TextBox();
            this.btnBrowseSrc = new System.Windows.Forms.Button();
            this.lblName = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.lblAuthor = new System.Windows.Forms.Label();
            this.txtAuthor = new System.Windows.Forms.TextBox();
            this.lblVersion = new System.Windows.Forms.Label();
            this.txtVersion = new System.Windows.Forms.TextBox();
            this.lblOrder = new System.Windows.Forms.Label();
            this.numOrder = new System.Windows.Forms.NumericUpDown();
            this.lblDesc = new System.Windows.Forms.Label();
            this.txtDesc = new System.Windows.Forms.TextBox();
            this.lblOut = new System.Windows.Forms.Label();
            this.txtOut = new System.Windows.Forms.TextBox();
            this.btnBrowseOut = new System.Windows.Forms.Button();
            this.btnPack = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblResult = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numOrder)).BeginInit();
            this.SuspendLayout();
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(16, 12);
            this.lblTitle.Size = new System.Drawing.Size(400, 28);
            this.lblTitle.Text = "Speckage Packager";
            this.lblSrc.Location = new System.Drawing.Point(16, 52);
            this.lblSrc.Text = "Source mod folder";
            this.lblSrc.AutoSize = true;
            this.txtSource.Location = new System.Drawing.Point(16, 72);
            this.txtSource.Size = new System.Drawing.Size(420, 23);
            this.btnBrowseSrc.Location = new System.Drawing.Point(444, 70);
            this.btnBrowseSrc.Size = new System.Drawing.Size(80, 26);
            this.btnBrowseSrc.Text = "Browse";
            this.btnBrowseSrc.Click += new System.EventHandler(this.btnBrowseSrc_Click);
            this.lblName.Location = new System.Drawing.Point(16, 110);
            this.lblName.Text = "Mod name";
            this.lblName.AutoSize = true;
            this.txtName.Location = new System.Drawing.Point(16, 128);
            this.txtName.Size = new System.Drawing.Size(240, 23);
            this.lblAuthor.Location = new System.Drawing.Point(270, 110);
            this.lblAuthor.Text = "Author";
            this.lblAuthor.AutoSize = true;
            this.txtAuthor.Location = new System.Drawing.Point(270, 128);
            this.txtAuthor.Size = new System.Drawing.Size(160, 23);
            this.lblVersion.Location = new System.Drawing.Point(16, 164);
            this.lblVersion.Text = "Version";
            this.lblVersion.AutoSize = true;
            this.txtVersion.Location = new System.Drawing.Point(16, 182);
            this.txtVersion.Size = new System.Drawing.Size(100, 23);
            this.txtVersion.Text = "1.0.0";
            this.lblOrder.Location = new System.Drawing.Point(140, 164);
            this.lblOrder.Text = "Default load order";
            this.lblOrder.AutoSize = true;
            this.numOrder.Location = new System.Drawing.Point(140, 182);
            this.numOrder.Maximum = 9999;
            this.numOrder.Value = 100;
            this.lblDesc.Location = new System.Drawing.Point(16, 220);
            this.lblDesc.Text = "Description";
            this.lblDesc.AutoSize = true;
            this.txtDesc.Location = new System.Drawing.Point(16, 240);
            this.txtDesc.Size = new System.Drawing.Size(508, 60);
            this.txtDesc.Multiline = true;
            this.lblOut.Location = new System.Drawing.Point(16, 316);
            this.lblOut.Text = "Output folder";
            this.lblOut.AutoSize = true;
            this.txtOut.Location = new System.Drawing.Point(16, 336);
            this.txtOut.Size = new System.Drawing.Size(420, 23);
            this.btnBrowseOut.Location = new System.Drawing.Point(444, 334);
            this.btnBrowseOut.Size = new System.Drawing.Size(80, 26);
            this.btnBrowseOut.Text = "Browse";
            this.btnBrowseOut.Click += new System.EventHandler(this.btnBrowseOut_Click);
            this.btnPack.Location = new System.Drawing.Point(16, 380);
            this.btnPack.Size = new System.Drawing.Size(160, 32);
            this.btnPack.Text = "Create .speckage";
            this.btnPack.Click += new System.EventHandler(this.btnPack_Click);
            this.btnClose.Location = new System.Drawing.Point(444, 380);
            this.btnClose.Size = new System.Drawing.Size(80, 32);
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.lblResult.Location = new System.Drawing.Point(16, 424);
            this.lblResult.Size = new System.Drawing.Size(508, 40);
            this.ClientSize = new System.Drawing.Size(548, 480);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblSrc);
            this.Controls.Add(this.txtSource);
            this.Controls.Add(this.btnBrowseSrc);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.lblAuthor);
            this.Controls.Add(this.txtAuthor);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.txtVersion);
            this.Controls.Add(this.lblOrder);
            this.Controls.Add(this.numOrder);
            this.Controls.Add(this.lblDesc);
            this.Controls.Add(this.txtDesc);
            this.Controls.Add(this.lblOut);
            this.Controls.Add(this.txtOut);
            this.Controls.Add(this.btnBrowseOut);
            this.Controls.Add(this.btnPack);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblResult);
            this.Name = "SpeckagePackagerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "specSB2modTool — Speckage";
            ((System.ComponentModel.ISupportInitialize)(this.numOrder)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSrc;
        private System.Windows.Forms.TextBox txtSource;
        private System.Windows.Forms.Button btnBrowseSrc;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblAuthor;
        private System.Windows.Forms.TextBox txtAuthor;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.TextBox txtVersion;
        private System.Windows.Forms.Label lblOrder;
        private System.Windows.Forms.NumericUpDown numOrder;
        private System.Windows.Forms.Label lblDesc;
        private System.Windows.Forms.TextBox txtDesc;
        private System.Windows.Forms.Label lblOut;
        private System.Windows.Forms.TextBox txtOut;
        private System.Windows.Forms.Button btnBrowseOut;
        private System.Windows.Forms.Button btnPack;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblResult;
    }
}
