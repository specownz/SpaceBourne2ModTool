namespace SpaceBourne2ModTool.Forms
{
    partial class CodeEditorForm
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
            this.cmbDocType = new System.Windows.Forms.ComboBox();
            this.lblType = new System.Windows.Forms.Label();
            this.lstSnippets = new System.Windows.Forms.ListBox();
            this.lblSnippets = new System.Windows.Forms.Label();
            this.txtEditor = new System.Windows.Forms.TextBox();
            this.lblFile = new System.Windows.Forms.Label();
            this.btnNew = new System.Windows.Forms.Button();
            this.btnOpen = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnSaveAs = new System.Windows.Forms.Button();
            this.btnInsertPath = new System.Windows.Forms.Button();
            this.btnGenerateResponse = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(12, 8);
            this.lblTitle.Size = new System.Drawing.Size(400, 28);
            this.lblTitle.Text = "SB2 Mod Code Editor";
            this.lblType.AutoSize = true;
            this.lblType.Location = new System.Drawing.Point(12, 44);
            this.lblType.Text = "Document type";
            this.cmbDocType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDocType.Location = new System.Drawing.Point(120, 40);
            this.cmbDocType.Size = new System.Drawing.Size(280, 23);
            this.cmbDocType.SelectedIndexChanged += new System.EventHandler(this.cmbDocType_SelectedIndexChanged);
            this.lblSnippets.AutoSize = true;
            this.lblSnippets.Location = new System.Drawing.Point(12, 72);
            this.lblSnippets.Text = "Snippets (double-click)";
            this.lstSnippets.Location = new System.Drawing.Point(12, 92);
            this.lstSnippets.Size = new System.Drawing.Size(220, 400);
            this.lstSnippets.DoubleClick += new System.EventHandler(this.lstSnippets_DoubleClick);
            this.txtEditor.AcceptsTab = true;
            this.txtEditor.Location = new System.Drawing.Point(244, 92);
            this.txtEditor.Multiline = true;
            this.txtEditor.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtEditor.Size = new System.Drawing.Size(620, 400);
            this.txtEditor.WordWrap = false;
            this.txtEditor.Font = new System.Drawing.Font("Consolas", 11F);
            this.txtEditor.TextChanged += new System.EventHandler(this.txtEditor_TextChanged);
            this.lblFile.Location = new System.Drawing.Point(244, 500);
            this.lblFile.Size = new System.Drawing.Size(620, 20);
            this.lblFile.Text = "Not saved to disk";
            this.btnNew.Location = new System.Drawing.Point(12, 530);
            this.btnNew.Size = new System.Drawing.Size(80, 28);
            this.btnNew.Text = "New";
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            this.btnOpen.Location = new System.Drawing.Point(98, 530);
            this.btnOpen.Size = new System.Drawing.Size(80, 28);
            this.btnOpen.Text = "Open";
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            this.btnSave.Location = new System.Drawing.Point(184, 530);
            this.btnSave.Size = new System.Drawing.Size(80, 28);
            this.btnSave.Text = "Save";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            this.btnSaveAs.Location = new System.Drawing.Point(270, 530);
            this.btnSaveAs.Size = new System.Drawing.Size(80, 28);
            this.btnSaveAs.Text = "Save As";
            this.btnSaveAs.Click += new System.EventHandler(this.btnSaveAs_Click);
            this.btnInsertPath.Location = new System.Drawing.Point(360, 530);
            this.btnInsertPath.Size = new System.Drawing.Size(120, 28);
            this.btnInsertPath.Text = "Content Browser";
            this.btnInsertPath.Click += new System.EventHandler(this.btnInsertPath_Click);
            this.btnGenerateResponse.Location = new System.Drawing.Point(490, 530);
            this.btnGenerateResponse.Size = new System.Drawing.Size(150, 28);
            this.btnGenerateResponse.Text = "Generate Response";
            this.btnGenerateResponse.Click += new System.EventHandler(this.btnGenerateResponse_Click);
            this.btnClose.Location = new System.Drawing.Point(784, 530);
            this.btnClose.Size = new System.Drawing.Size(80, 28);
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.ClientSize = new System.Drawing.Size(880, 572);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblType);
            this.Controls.Add(this.cmbDocType);
            this.Controls.Add(this.lblSnippets);
            this.Controls.Add(this.lstSnippets);
            this.Controls.Add(this.txtEditor);
            this.Controls.Add(this.lblFile);
            this.Controls.Add(this.btnNew);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnSaveAs);
            this.Controls.Add(this.btnInsertPath);
            this.Controls.Add(this.btnGenerateResponse);
            this.Controls.Add(this.btnClose);
            this.Name = "CodeEditorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "specSB2modTool — Code Editor";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.ComboBox cmbDocType;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.ListBox lstSnippets;
        private System.Windows.Forms.Label lblSnippets;
        private System.Windows.Forms.TextBox txtEditor;
        private System.Windows.Forms.Label lblFile;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnSaveAs;
        private System.Windows.Forms.Button btnInsertPath;
        private System.Windows.Forms.Button btnGenerateResponse;
        private System.Windows.Forms.Button btnClose;
    }
}
