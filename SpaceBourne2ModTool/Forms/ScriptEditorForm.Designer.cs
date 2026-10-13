namespace SpaceBourne2ModTool.Forms
{
    partial class ScriptEditorForm
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
            this.tree = new System.Windows.Forms.TreeView();
            this.txtBody = new System.Windows.Forms.TextBox();
            this.lblKind = new System.Windows.Forms.Label();
            this.txtKind = new System.Windows.Forms.TextBox();
            this.lblCategory = new System.Windows.Forms.Label();
            this.txtCategory = new System.Windows.Forms.TextBox();
            this.lblTitleField = new System.Windows.Forms.Label();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.lblPath = new System.Windows.Forms.Label();
            this.txtGamePath = new System.Windows.Forms.TextBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnSaveAs = new System.Windows.Forms.Button();
            this.btnNew = new System.Windows.Forms.Button();
            this.btnImportMod = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnUAsset = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(12, 8);
            this.lblTitle.Size = new System.Drawing.Size(400, 28);
            this.lblTitle.Text = "Script Editor (Dialogs / Quests)";
            this.tree.Location = new System.Drawing.Point(12, 44);
            this.tree.Size = new System.Drawing.Size(260, 400);
            this.tree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tree_AfterSelect);
            this.lblKind.Location = new System.Drawing.Point(288, 44);
            this.lblKind.Text = "Kind";
            this.lblKind.AutoSize = true;
            this.txtKind.Location = new System.Drawing.Point(288, 62);
            this.txtKind.Size = new System.Drawing.Size(120, 23);
            this.lblCategory.Location = new System.Drawing.Point(420, 44);
            this.lblCategory.Text = "Category (NPC / Quest)";
            this.lblCategory.AutoSize = true;
            this.txtCategory.Location = new System.Drawing.Point(420, 62);
            this.txtCategory.Size = new System.Drawing.Size(200, 23);
            this.lblTitleField.Location = new System.Drawing.Point(288, 94);
            this.lblTitleField.Text = "Title";
            this.lblTitleField.AutoSize = true;
            this.txtTitle.Location = new System.Drawing.Point(288, 112);
            this.txtTitle.Size = new System.Drawing.Size(332, 23);
            this.lblPath.Location = new System.Drawing.Point(288, 144);
            this.lblPath.Text = "Game path";
            this.lblPath.AutoSize = true;
            this.txtGamePath.Location = new System.Drawing.Point(288, 162);
            this.txtGamePath.Size = new System.Drawing.Size(480, 23);
            this.txtBody.Location = new System.Drawing.Point(288, 196);
            this.txtBody.Multiline = true;
            this.txtBody.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtBody.Size = new System.Drawing.Size(480, 248);
            this.txtBody.WordWrap = false;
            this.btnNew.Location = new System.Drawing.Point(12, 456);
            this.btnNew.Size = new System.Drawing.Size(80, 28);
            this.btnNew.Text = "New";
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            this.btnSave.Location = new System.Drawing.Point(100, 456);
            this.btnSave.Size = new System.Drawing.Size(80, 28);
            this.btnSave.Text = "Save";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            this.btnSaveAs.Location = new System.Drawing.Point(188, 456);
            this.btnSaveAs.Size = new System.Drawing.Size(80, 28);
            this.btnSaveAs.Text = "Save As…";
            this.btnSaveAs.Click += new System.EventHandler(this.btnSaveAs_Click);
            this.btnImportMod.Location = new System.Drawing.Point(288, 456);
            this.btnImportMod.Size = new System.Drawing.Size(140, 28);
            this.btnImportMod.Text = "Import into Mod";
            this.btnImportMod.Click += new System.EventHandler(this.btnImportMod_Click);
            this.btnRefresh.Location = new System.Drawing.Point(436, 456);
            this.btnRefresh.Size = new System.Drawing.Size(80, 28);
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            this.btnUAsset.Location = new System.Drawing.Point(524, 456);
            this.btnUAsset.Size = new System.Drawing.Size(100, 28);
            this.btnUAsset.Text = "UAssetGUI";
            this.btnUAsset.Click += new System.EventHandler(this.btnUAsset_Click);
            this.btnClose.Location = new System.Drawing.Point(688, 456);
            this.btnClose.Size = new System.Drawing.Size(80, 28);
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.lblStatus.Location = new System.Drawing.Point(12, 492);
            this.lblStatus.Size = new System.Drawing.Size(750, 20);
            this.ClientSize = new System.Drawing.Size(788, 524);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.tree);
            this.Controls.Add(this.lblKind);
            this.Controls.Add(this.txtKind);
            this.Controls.Add(this.lblCategory);
            this.Controls.Add(this.txtCategory);
            this.Controls.Add(this.lblTitleField);
            this.Controls.Add(this.txtTitle);
            this.Controls.Add(this.lblPath);
            this.Controls.Add(this.txtGamePath);
            this.Controls.Add(this.txtBody);
            this.Controls.Add(this.btnNew);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnSaveAs);
            this.Controls.Add(this.btnImportMod);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnUAsset);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblStatus);
            this.Name = "ScriptEditorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "specSB2modTool — Script Editor";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.TreeView tree;
        private System.Windows.Forms.TextBox txtBody;
        private System.Windows.Forms.Label lblKind;
        private System.Windows.Forms.TextBox txtKind;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.TextBox txtCategory;
        private System.Windows.Forms.Label lblTitleField;
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.Label lblPath;
        private System.Windows.Forms.TextBox txtGamePath;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnSaveAs;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.Button btnImportMod;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnUAsset;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblStatus;
    }
}
