namespace SpaceBourne2ModTool.Forms
{
    partial class GamePathDialog
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
            this.txtPath = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.btnAutoDetect = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            this.lblInfo.Location = new System.Drawing.Point(12, 12);
            this.lblInfo.Size = new System.Drawing.Size(460, 40);
            this.lblInfo.Text = "Select the root folder of SpaceBourne 2 (usually ...\\steamapps\\common\\SpaceBourne 2). The tool expects an SB2\\Content\\Paks structure.";
            // 
            this.txtPath.Location = new System.Drawing.Point(12, 60);
            this.txtPath.Size = new System.Drawing.Size(380, 22);
            // 
            this.btnBrowse.Location = new System.Drawing.Point(400, 58);
            this.btnBrowse.Size = new System.Drawing.Size(75, 26);
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            this.btnAutoDetect.Location = new System.Drawing.Point(16, 100);
            this.btnAutoDetect.Name = "btnAutoDetect";
            this.btnAutoDetect.Size = new System.Drawing.Size(140, 28);
            this.btnAutoDetect.Text = "Auto-Detect Steam";
            this.btnAutoDetect.Click += new System.EventHandler(this.btnAutoDetect_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(300, 100);
            this.btnOK.Size = new System.Drawing.Size(85, 28);
            this.btnOK.Text = "OK";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            this.btnCancel.Location = new System.Drawing.Point(390, 100);
            this.btnCancel.Size = new System.Drawing.Size(85, 28);
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            this.ClientSize = new System.Drawing.Size(490, 145);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.txtPath);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.btnAutoDetect);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GamePathDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Set Game Path";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnAutoDetect;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}
