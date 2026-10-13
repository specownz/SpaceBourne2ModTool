namespace SpaceBourne2ModTool.Forms
{
    partial class ModTemplateForm
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
            this.lst = new System.Windows.Forms.ListBox();
            this.txtDetails = new System.Windows.Forms.TextBox();
            this.lblModName = new System.Windows.Forms.Label();
            this.txtModName = new System.Windows.Forms.TextBox();
            this.btnCreate = new System.Windows.Forms.Button();
            this.btnApplyCurrent = new System.Windows.Forms.Button();
            this.btnSaveUser = new System.Windows.Forms.Button();
            this.btnDeleteUser = new System.Windows.Forms.Button();
            this.btnOpenDir = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(12, 10);
            this.lblTitle.Size = new System.Drawing.Size(400, 28);
            this.lblTitle.Text = "Ready-made mod layouts (templates)";
            this.lst.Location = new System.Drawing.Point(12, 48);
            this.lst.Size = new System.Drawing.Size(260, 360);
            this.lst.SelectedIndexChanged += new System.EventHandler(this.lst_SelectedIndexChanged);
            this.txtDetails.Location = new System.Drawing.Point(284, 48);
            this.txtDetails.Multiline = true;
            this.txtDetails.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDetails.Size = new System.Drawing.Size(360, 300);
            this.txtDetails.ReadOnly = true;
            this.lblModName.Location = new System.Drawing.Point(284, 358);
            this.lblModName.Text = "Name for the new mod or saved template";
            this.lblModName.AutoSize = true;
            this.txtModName.Location = new System.Drawing.Point(284, 376);
            this.txtModName.Size = new System.Drawing.Size(240, 23);
            this.btnCreate.Location = new System.Drawing.Point(12, 420);
            this.btnCreate.Size = new System.Drawing.Size(160, 32);
            this.btnCreate.Text = "Create a new mod from this template";
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            this.btnApplyCurrent.Location = new System.Drawing.Point(180, 420);
            this.btnApplyCurrent.Size = new System.Drawing.Size(140, 32);
            this.btnApplyCurrent.Text = "Add this layout to the open mod";
            this.btnApplyCurrent.Click += new System.EventHandler(this.btnApplyCurrent_Click);
            this.btnSaveUser.Location = new System.Drawing.Point(328, 420);
            this.btnSaveUser.Size = new System.Drawing.Size(130, 32);
            this.btnSaveUser.Text = "Save open mod as my template";
            this.btnSaveUser.Click += new System.EventHandler(this.btnSaveUser_Click);
            this.btnDeleteUser.Location = new System.Drawing.Point(466, 420);
            this.btnDeleteUser.Size = new System.Drawing.Size(90, 32);
            this.btnDeleteUser.Text = "Delete User";
            this.btnDeleteUser.Click += new System.EventHandler(this.btnDeleteUser_Click);
            this.btnOpenDir.Location = new System.Drawing.Point(12, 460);
            this.btnOpenDir.Size = new System.Drawing.Size(140, 28);
            this.btnOpenDir.Text = "Open Templates Folder";
            this.btnOpenDir.Click += new System.EventHandler(this.btnOpenDir_Click);
            this.btnClose.Location = new System.Drawing.Point(564, 460);
            this.btnClose.Size = new System.Drawing.Size(80, 28);
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.ClientSize = new System.Drawing.Size(660, 504);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lst);
            this.Controls.Add(this.txtDetails);
            this.Controls.Add(this.lblModName);
            this.Controls.Add(this.txtModName);
            this.Controls.Add(this.btnCreate);
            this.Controls.Add(this.btnApplyCurrent);
            this.Controls.Add(this.btnSaveUser);
            this.Controls.Add(this.btnDeleteUser);
            this.Controls.Add(this.btnOpenDir);
            this.Controls.Add(this.btnClose);
            this.Name = "ModTemplateForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "specSB2modTool — Mod Templates";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.ListBox lst;
        private System.Windows.Forms.TextBox txtDetails;
        private System.Windows.Forms.Label lblModName;
        private System.Windows.Forms.TextBox txtModName;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnApplyCurrent;
        private System.Windows.Forms.Button btnSaveUser;
        private System.Windows.Forms.Button btnDeleteUser;
        private System.Windows.Forms.Button btnOpenDir;
        private System.Windows.Forms.Button btnClose;
    }
}
