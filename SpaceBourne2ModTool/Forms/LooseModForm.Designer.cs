namespace SpaceBourne2ModTool.Forms
{
    partial class LooseModForm
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
            this.lblHelp = new System.Windows.Forms.Label();
            this.lblInstall = new System.Windows.Forms.Label();
            this.txtInstall = new System.Windows.Forms.TextBox();
            this.btnBrowseInstall = new System.Windows.Forms.Button();
            this.lblTarget = new System.Windows.Forms.Label();
            this.cmbTarget = new System.Windows.Forms.ComboBox();
            this.lblFolder = new System.Windows.Forms.Label();
            this.lblCount = new System.Windows.Forms.Label();
            this.lstFiles = new System.Windows.Forms.ListBox();
            this.lblReplacement = new System.Windows.Forms.Label();
            this.txtReplacement = new System.Windows.Forms.TextBox();
            this.btnBrowseReplacement = new System.Windows.Forms.Button();
            this.btnReplace = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(16, 12);
            this.lblTitle.Text = "Loose File Mods (Movies / FMOD)";
            this.lblHelp.Location = new System.Drawing.Point(16, 45);
            this.lblHelp.Size = new System.Drawing.Size(700, 40);
            this.lblHelp.Text = "Easiest SB2 mods: files already on disk outside the 11GB pak. Analyzer reported ~82 Movies and FMOD banks. Replace with same extension; backups go to _SB2ModBackup.";
            this.lblInstall.AutoSize = true;
            this.lblInstall.Location = new System.Drawing.Point(16, 95);
            this.lblInstall.Text = "Game install";
            this.txtInstall.Location = new System.Drawing.Point(16, 113);
            this.txtInstall.Size = new System.Drawing.Size(580, 22);
            this.btnBrowseInstall.Location = new System.Drawing.Point(604, 111);
            this.btnBrowseInstall.Size = new System.Drawing.Size(100, 26);
            this.btnBrowseInstall.Text = "Browse...";
            this.btnBrowseInstall.Click += new System.EventHandler(this.btnBrowseInstall_Click);
            this.lblTarget.AutoSize = true;
            this.lblTarget.Location = new System.Drawing.Point(16, 145);
            this.lblTarget.Text = "Target folder";
            this.cmbTarget.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTarget.Location = new System.Drawing.Point(16, 163);
            this.cmbTarget.Size = new System.Drawing.Size(300, 24);
            this.cmbTarget.SelectedIndexChanged += new System.EventHandler(this.cmbTarget_SelectedIndexChanged);
            this.btnRefresh.Location = new System.Drawing.Point(330, 161);
            this.btnRefresh.Size = new System.Drawing.Size(90, 26);
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            this.lblFolder.Location = new System.Drawing.Point(16, 195);
            this.lblFolder.Size = new System.Drawing.Size(700, 18);
            this.lblFolder.Text = "";
            this.lblCount.Location = new System.Drawing.Point(16, 215);
            this.lblCount.Size = new System.Drawing.Size(200, 18);
            this.lblCount.Text = "0 files";
            this.lstFiles.Location = new System.Drawing.Point(16, 235);
            this.lstFiles.Size = new System.Drawing.Size(688, 220);
            this.lblReplacement.AutoSize = true;
            this.lblReplacement.Location = new System.Drawing.Point(16, 470);
            this.lblReplacement.Text = "Replacement file";
            this.txtReplacement.Location = new System.Drawing.Point(16, 488);
            this.txtReplacement.Size = new System.Drawing.Size(580, 22);
            this.btnBrowseReplacement.Location = new System.Drawing.Point(604, 486);
            this.btnBrowseReplacement.Size = new System.Drawing.Size(100, 26);
            this.btnBrowseReplacement.Text = "Browse...";
            this.btnBrowseReplacement.Click += new System.EventHandler(this.btnBrowseReplacement_Click);
            this.btnReplace.Location = new System.Drawing.Point(16, 525);
            this.btnReplace.Size = new System.Drawing.Size(160, 32);
            this.btnReplace.Text = "Replace Selected";
            this.btnReplace.Click += new System.EventHandler(this.btnReplace_Click);
            this.btnClose.Location = new System.Drawing.Point(629, 525);
            this.btnClose.Size = new System.Drawing.Size(75, 32);
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.ClientSize = new System.Drawing.Size(720, 575);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblHelp);
            this.Controls.Add(this.lblInstall);
            this.Controls.Add(this.txtInstall);
            this.Controls.Add(this.btnBrowseInstall);
            this.Controls.Add(this.lblTarget);
            this.Controls.Add(this.cmbTarget);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.lblFolder);
            this.Controls.Add(this.lblCount);
            this.Controls.Add(this.lstFiles);
            this.Controls.Add(this.lblReplacement);
            this.Controls.Add(this.txtReplacement);
            this.Controls.Add(this.btnBrowseReplacement);
            this.Controls.Add(this.btnReplace);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "LooseModForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "specSB2modTool";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblHelp;
        private System.Windows.Forms.Label lblInstall;
        private System.Windows.Forms.TextBox txtInstall;
        private System.Windows.Forms.Button btnBrowseInstall;
        private System.Windows.Forms.Label lblTarget;
        private System.Windows.Forms.ComboBox cmbTarget;
        private System.Windows.Forms.Label lblFolder;
        private System.Windows.Forms.Label lblCount;
        private System.Windows.Forms.ListBox lstFiles;
        private System.Windows.Forms.Label lblReplacement;
        private System.Windows.Forms.TextBox txtReplacement;
        private System.Windows.Forms.Button btnBrowseReplacement;
        private System.Windows.Forms.Button btnReplace;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnClose;
    }
}
