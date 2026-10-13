namespace SpaceBourne2ModTool.Forms
{
    partial class PakBrowserForm
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblInfo = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            this.lblInfo.Location = new System.Drawing.Point(12, 12);
            this.lblInfo.Size = new System.Drawing.Size(460, 80);
            this.lblInfo.Text = "For browsing game PAK contents use external tools:\r\n• FModel (recommended) – set path in Settings\r\n• UModel / UE Viewer\r\n\r\nThis form is a placeholder for future integrated browsing.";
            this.btnClose.Location = new System.Drawing.Point(397, 110);
            this.btnClose.Size = new System.Drawing.Size(75, 28);
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.ClientSize = new System.Drawing.Size(484, 151);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "PakBrowserForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "PAK Browser";
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Button btnClose;
    }
}
