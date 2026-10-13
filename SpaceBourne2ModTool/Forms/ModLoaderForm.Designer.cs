namespace SpaceBourne2ModTool.Forms
{
    partial class ModLoaderForm
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
            this.listMods = new System.Windows.Forms.ListBox();
            this.lblLib = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.btnImport = new System.Windows.Forms.Button();
            this.btnToggle = new System.Windows.Forms.Button();
            this.btnOrderUp = new System.Windows.Forms.Button();
            this.btnOrderDown = new System.Windows.Forms.Button();
            this.btnInject = new System.Windows.Forms.Button();
            this.btnInjectLaunch = new System.Windows.Forms.Button();
            this.btnUnload = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblHelp = new System.Windows.Forms.Label();
            this.SuspendLayout();
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(12, 10);
            this.lblTitle.Size = new System.Drawing.Size(400, 28);
            this.lblTitle.Text = "Mod Loader (Speckage)";
            this.lblHelp.Location = new System.Drawing.Point(12, 42);
            this.lblHelp.Size = new System.Drawing.Size(660, 36);
            this.lblHelp.Text = "Import .speckage packages, set order, enable/disable, then Inject into Content/Paks. Vanilla base PAKs are never modified. Backup is created before each inject.";
            this.listMods.Location = new System.Drawing.Point(12, 84);
            this.listMods.Size = new System.Drawing.Size(520, 200);
            this.lblLib.Location = new System.Drawing.Point(12, 290);
            this.lblLib.Size = new System.Drawing.Size(660, 20);
            this.txtLog.Location = new System.Drawing.Point(12, 318);
            this.txtLog.Multiline = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(660, 140);
            this.txtLog.ReadOnly = true;
            this.btnImport.Location = new System.Drawing.Point(548, 84);
            this.btnImport.Size = new System.Drawing.Size(120, 28);
            this.btnImport.Text = "Import…";
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            this.btnToggle.Location = new System.Drawing.Point(548, 118);
            this.btnToggle.Size = new System.Drawing.Size(120, 28);
            this.btnToggle.Text = "Enable/Disable";
            this.btnToggle.Click += new System.EventHandler(this.btnToggle_Click);
            this.btnOrderUp.Location = new System.Drawing.Point(548, 152);
            this.btnOrderUp.Size = new System.Drawing.Size(120, 28);
            this.btnOrderUp.Text = "Order Up";
            this.btnOrderUp.Click += new System.EventHandler(this.btnOrderUp_Click);
            this.btnOrderDown.Location = new System.Drawing.Point(548, 186);
            this.btnOrderDown.Size = new System.Drawing.Size(120, 28);
            this.btnOrderDown.Text = "Order Down";
            this.btnOrderDown.Click += new System.EventHandler(this.btnOrderDown_Click);
            this.btnRemove.Location = new System.Drawing.Point(548, 220);
            this.btnRemove.Size = new System.Drawing.Size(120, 28);
            this.btnRemove.Text = "Remove";
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            this.btnInject.Location = new System.Drawing.Point(12, 470);
            this.btnInject.Size = new System.Drawing.Size(140, 32);
            this.btnInject.Text = "Inject Enabled";
            this.btnInject.Click += new System.EventHandler(this.btnInject_Click);
            this.btnInjectLaunch.Location = new System.Drawing.Point(160, 470);
            this.btnInjectLaunch.Size = new System.Drawing.Size(180, 32);
            this.btnInjectLaunch.Text = "Inject + Launch Game";
            this.btnInjectLaunch.Click += new System.EventHandler(this.btnInjectLaunch_Click);
            this.btnUnload.Location = new System.Drawing.Point(350, 470);
            this.btnUnload.Size = new System.Drawing.Size(140, 32);
            this.btnUnload.Text = "Unload Managed";
            this.btnUnload.Click += new System.EventHandler(this.btnUnload_Click);
            this.btnClose.Location = new System.Drawing.Point(552, 470);
            this.btnClose.Size = new System.Drawing.Size(120, 32);
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.ClientSize = new System.Drawing.Size(688, 518);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblHelp);
            this.Controls.Add(this.listMods);
            this.Controls.Add(this.lblLib);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.btnImport);
            this.Controls.Add(this.btnToggle);
            this.Controls.Add(this.btnOrderUp);
            this.Controls.Add(this.btnOrderDown);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.btnInject);
            this.Controls.Add(this.btnInjectLaunch);
            this.Controls.Add(this.btnUnload);
            this.Controls.Add(this.btnClose);
            this.Name = "ModLoaderForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "specSB2modTool — Mod Loader";
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.ListBox listMods;
        private System.Windows.Forms.Label lblLib;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Button btnToggle;
        private System.Windows.Forms.Button btnOrderUp;
        private System.Windows.Forms.Button btnOrderDown;
        private System.Windows.Forms.Button btnInject;
        private System.Windows.Forms.Button btnInjectLaunch;
        private System.Windows.Forms.Button btnUnload;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblHelp;
    }
}
