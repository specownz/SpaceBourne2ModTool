namespace SpaceBourne2ModTool.Forms
{
    partial class ObjectEditorHubForm
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
            this.lblMod = new System.Windows.Forms.Label();
            this.btnOpenMod = new System.Windows.Forms.Button();
            this.lstTypes = new System.Windows.Forms.ListBox();
            this.lstObjects = new System.Windows.Forms.ListBox();
            this.txtDesc = new System.Windows.Forms.TextBox();
            this.btnNew = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnImportStaging = new System.Windows.Forms.Button();
            this.btnScript = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblTypes = new System.Windows.Forms.Label();
            this.lblObjs = new System.Windows.Forms.Label();
            this.SuspendLayout();
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(12, 10);
            this.lblTitle.Size = new System.Drawing.Size(400, 28);
            this.lblTitle.Text = "Modular Object Editors";
            this.lblMod.Location = new System.Drawing.Point(12, 44);
            this.lblMod.Size = new System.Drawing.Size(520, 20);
            this.lblMod.Text = "Current mod: (none)";
            this.btnOpenMod.Location = new System.Drawing.Point(540, 40);
            this.btnOpenMod.Size = new System.Drawing.Size(120, 28);
            this.btnOpenMod.Text = "Open Mod…";
            this.btnOpenMod.Click += new System.EventHandler(this.btnOpenMod_Click);
            this.lblTypes.Location = new System.Drawing.Point(12, 72);
            this.lblTypes.Text = "Object type";
            this.lblTypes.AutoSize = true;
            this.lstTypes.Location = new System.Drawing.Point(12, 92);
            this.lstTypes.Size = new System.Drawing.Size(220, 280);
            this.lstTypes.SelectedIndexChanged += new System.EventHandler(this.lstTypes_SelectedIndexChanged);
            this.lblObjs.Location = new System.Drawing.Point(250, 72);
            this.lblObjs.Text = "Objects in mod";
            this.lblObjs.AutoSize = true;
            this.lstObjects.Location = new System.Drawing.Point(250, 92);
            this.lstObjects.Size = new System.Drawing.Size(280, 200);
            this.lstObjects.DoubleClick += new System.EventHandler(this.lstObjects_DoubleClick);
            this.txtDesc.Location = new System.Drawing.Point(250, 300);
            this.txtDesc.Multiline = true;
            this.txtDesc.ReadOnly = true;
            this.txtDesc.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDesc.Size = new System.Drawing.Size(410, 72);
            this.btnNew.Location = new System.Drawing.Point(548, 92);
            this.btnNew.Size = new System.Drawing.Size(112, 28);
            this.btnNew.Text = "New…";
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            this.btnEdit.Location = new System.Drawing.Point(548, 126);
            this.btnEdit.Size = new System.Drawing.Size(112, 28);
            this.btnEdit.Text = "Edit…";
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            this.btnImportStaging.Location = new System.Drawing.Point(548, 160);
            this.btnImportStaging.Size = new System.Drawing.Size(112, 40);
            this.btnImportStaging.Text = "Import into\rmod staging";
            this.btnImportStaging.Click += new System.EventHandler(this.btnImportStaging_Click);
            this.btnScript.Location = new System.Drawing.Point(548, 210);
            this.btnScript.Size = new System.Drawing.Size(112, 28);
            this.btnScript.Text = "Script Editor…";
            this.btnScript.Click += new System.EventHandler(this.btnScript_Click);
            this.btnClose.Location = new System.Drawing.Point(548, 344);
            this.btnClose.Size = new System.Drawing.Size(112, 28);
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.ClientSize = new System.Drawing.Size(680, 392);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblMod);
            this.Controls.Add(this.btnOpenMod);
            this.Controls.Add(this.lblTypes);
            this.Controls.Add(this.lstTypes);
            this.Controls.Add(this.lblObjs);
            this.Controls.Add(this.lstObjects);
            this.Controls.Add(this.txtDesc);
            this.Controls.Add(this.btnNew);
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.btnImportStaging);
            this.Controls.Add(this.btnScript);
            this.Controls.Add(this.btnClose);
            this.Name = "ObjectEditorHubForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "specSB2modTool — Object Editors";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblMod;
        private System.Windows.Forms.Button btnOpenMod;
        private System.Windows.Forms.ListBox lstTypes;
        private System.Windows.Forms.ListBox lstObjects;
        private System.Windows.Forms.TextBox txtDesc;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnImportStaging;
        private System.Windows.Forms.Button btnScript;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblTypes;
        private System.Windows.Forms.Label lblObjs;
    }
}
