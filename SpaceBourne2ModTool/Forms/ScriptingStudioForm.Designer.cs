namespace SpaceBourne2ModTool.Forms
{
    partial class ScriptingStudioForm
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
            this.txtHelp = new System.Windows.Forms.TextBox();
            this.lblKind = new System.Windows.Forms.Label();
            this.cmbKind = new System.Windows.Forms.ComboBox();
            this.lblName = new System.Windows.Forms.Label();
            this.txtModName = new System.Windows.Forms.TextBox();
            this.txtOut = new System.Windows.Forms.TextBox();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnExportFolder = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblWin64 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(12, 10);
            this.lblTitle.Size = new System.Drawing.Size(760, 28);
            this.lblTitle.Text = "Live scripts and programming notes";
            //
            this.txtHelp.Location = new System.Drawing.Point(12, 42);
            this.txtHelp.Multiline = true;
            this.txtHelp.ReadOnly = true;
            this.txtHelp.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtHelp.Size = new System.Drawing.Size(760, 88);
            //
            this.lblKind.AutoSize = true;
            this.lblKind.Location = new System.Drawing.Point(12, 140);
            this.lblKind.Text = "What do you want to generate?";
            this.cmbKind.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbKind.Location = new System.Drawing.Point(12, 160);
            this.cmbKind.Size = new System.Drawing.Size(500, 23);
            this.cmbKind.SelectedIndexChanged += new System.EventHandler(this.cmbKind_SelectedIndexChanged);
            //
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(530, 140);
            this.lblName.Text = "Name for a live script mod folder";
            this.txtModName.Location = new System.Drawing.Point(530, 160);
            this.txtModName.Size = new System.Drawing.Size(242, 23);
            this.txtModName.Text = "MyLiveScriptMod";
            //
            this.txtOut.Location = new System.Drawing.Point(12, 196);
            this.txtOut.Multiline = true;
            this.txtOut.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtOut.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtOut.Size = new System.Drawing.Size(760, 250);
            //
            this.lblWin64.Location = new System.Drawing.Point(12, 452);
            this.lblWin64.Size = new System.Drawing.Size(760, 36);
            this.lblWin64.Text = "Game program folder: …";
            //
            this.btnGenerate.Location = new System.Drawing.Point(12, 494);
            this.btnGenerate.Size = new System.Drawing.Size(110, 30);
            this.btnGenerate.Text = "Refresh text";
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            this.btnSave.Location = new System.Drawing.Point(130, 494);
            this.btnSave.Size = new System.Drawing.Size(120, 30);
            this.btnSave.Text = "Save to file…";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            this.btnExportFolder.Location = new System.Drawing.Point(260, 494);
            this.btnExportFolder.Size = new System.Drawing.Size(240, 30);
            this.btnExportFolder.Text = "Create ready live-script folder";
            this.btnExportFolder.Click += new System.EventHandler(this.btnExportFolder_Click);
            this.btnClose.Location = new System.Drawing.Point(692, 494);
            this.btnClose.Size = new System.Drawing.Size(80, 30);
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            //
            this.ClientSize = new System.Drawing.Size(792, 538);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.txtHelp);
            this.Controls.Add(this.lblKind);
            this.Controls.Add(this.cmbKind);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.txtModName);
            this.Controls.Add(this.txtOut);
            this.Controls.Add(this.lblWin64);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnExportFolder);
            this.Controls.Add(this.btnClose);
            this.Name = "ScriptingStudioForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Live scripts and programming notes";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.TextBox txtHelp;
        private System.Windows.Forms.Label lblKind;
        private System.Windows.Forms.ComboBox cmbKind;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtModName;
        private System.Windows.Forms.TextBox txtOut;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnExportFolder;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblWin64;
    }
}
