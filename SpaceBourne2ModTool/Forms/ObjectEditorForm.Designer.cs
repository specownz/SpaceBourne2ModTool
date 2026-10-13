namespace SpaceBourne2ModTool.Forms
{
    partial class ObjectEditorForm
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
            this.lblType = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.lblGamePath = new System.Windows.Forms.Label();
            this.txtGamePath = new System.Windows.Forms.TextBox();
            this.lblStaging = new System.Windows.Forms.Label();
            this.txtStaging = new System.Windows.Forms.TextBox();
            this.lblNotes = new System.Windows.Forms.Label();
            this.txtNotes = new System.Windows.Forms.TextBox();
            this.grid = new System.Windows.Forms.DataGridView();
            this.colKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colVal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnAddProp = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnSaveAs = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(12, 10);
            this.lblTitle.Size = new System.Drawing.Size(300, 24);
            this.lblTitle.Text = "Object Editor";
            this.lblType.Location = new System.Drawing.Point(320, 14);
            this.lblType.Size = new System.Drawing.Size(200, 20);
            this.lblType.Text = "Type:";
            this.lblName.Location = new System.Drawing.Point(12, 44);
            this.lblName.Text = "Display name";
            this.lblName.AutoSize = true;
            this.txtName.Location = new System.Drawing.Point(12, 62);
            this.txtName.Size = new System.Drawing.Size(500, 23);
            this.lblGamePath.Location = new System.Drawing.Point(12, 94);
            this.lblGamePath.Text = "Game path (SB2/Content/...)";
            this.lblGamePath.AutoSize = true;
            this.txtGamePath.Location = new System.Drawing.Point(12, 112);
            this.txtGamePath.Size = new System.Drawing.Size(500, 23);
            this.lblStaging.Location = new System.Drawing.Point(12, 144);
            this.lblStaging.Text = "Staging relative path";
            this.lblStaging.AutoSize = true;
            this.txtStaging.Location = new System.Drawing.Point(12, 162);
            this.txtStaging.Size = new System.Drawing.Size(500, 23);
            this.lblNotes.Location = new System.Drawing.Point(12, 194);
            this.lblNotes.Text = "Notes";
            this.lblNotes.AutoSize = true;
            this.txtNotes.Location = new System.Drawing.Point(12, 212);
            this.txtNotes.Multiline = true;
            this.txtNotes.Size = new System.Drawing.Size(500, 50);
            this.grid.Location = new System.Drawing.Point(12, 274);
            this.grid.Size = new System.Drawing.Size(500, 180);
            this.grid.AllowUserToAddRows = true;
            this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { this.colKey, this.colVal });
            this.colKey.HeaderText = "Property";
            this.colKey.Name = "colKey";
            this.colKey.Width = 160;
            this.colVal.HeaderText = "Value";
            this.colVal.Name = "colVal";
            this.colVal.Width = 300;
            this.btnAddProp.Location = new System.Drawing.Point(12, 466);
            this.btnAddProp.Size = new System.Drawing.Size(100, 28);
            this.btnAddProp.Text = "Add property";
            this.btnAddProp.Click += new System.EventHandler(this.btnAddProp_Click);
            this.btnSave.Location = new System.Drawing.Point(250, 466);
            this.btnSave.Size = new System.Drawing.Size(80, 28);
            this.btnSave.Text = "Save";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            this.btnSaveAs.Location = new System.Drawing.Point(336, 466);
            this.btnSaveAs.Size = new System.Drawing.Size(80, 28);
            this.btnSaveAs.Text = "Save As…";
            this.btnSaveAs.Click += new System.EventHandler(this.btnSaveAs_Click);
            this.btnCancel.Location = new System.Drawing.Point(432, 466);
            this.btnCancel.Size = new System.Drawing.Size(80, 28);
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            this.ClientSize = new System.Drawing.Size(530, 510);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblType);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.lblGamePath);
            this.Controls.Add(this.txtGamePath);
            this.Controls.Add(this.lblStaging);
            this.Controls.Add(this.txtStaging);
            this.Controls.Add(this.lblNotes);
            this.Controls.Add(this.txtNotes);
            this.Controls.Add(this.grid);
            this.Controls.Add(this.btnAddProp);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnSaveAs);
            this.Controls.Add(this.btnCancel);
            this.Name = "ObjectEditorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Object Editor";
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblGamePath;
        private System.Windows.Forms.TextBox txtGamePath;
        private System.Windows.Forms.Label lblStaging;
        private System.Windows.Forms.TextBox txtStaging;
        private System.Windows.Forms.Label lblNotes;
        private System.Windows.Forms.TextBox txtNotes;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.DataGridViewTextBoxColumn colKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn colVal;
        private System.Windows.Forms.Button btnAddProp;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnSaveAs;
        private System.Windows.Forms.Button btnCancel;
    }
}
