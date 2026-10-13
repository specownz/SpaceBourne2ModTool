namespace SpaceBourne2ModTool.Forms
{
    partial class ContentBrowserForm
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
            this.lblSearch = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblCat = new System.Windows.Forms.Label();
            this.cmbCategory = new System.Windows.Forms.ComboBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.lblFolders = new System.Windows.Forms.Label();
            this.lstFolders = new System.Windows.Forms.ListBox();
            this.lblSamples = new System.Windows.Forms.Label();
            this.lstSamples = new System.Windows.Forms.ListBox();
            this.txtDetails = new System.Windows.Forms.TextBox();
            this.lblCount = new System.Windows.Forms.Label();
            this.btnScaffold = new System.Windows.Forms.Button();
            this.btnCopyPath = new System.Windows.Forms.Button();
            this.btnCopyStage = new System.Windows.Forms.Button();
            this.btnExportList = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(12, 8);
            this.lblTitle.Size = new System.Drawing.Size(500, 26);
            this.lblTitle.Text = "Content Browser";
            this.lblHelp.Location = new System.Drawing.Point(12, 36);
            this.lblHelp.Size = new System.Drawing.Size(860, 36);
            this.lblSearch.Location = new System.Drawing.Point(12, 78);
            this.lblSearch.AutoSize = true;
            this.lblSearch.Text = "Search";
            this.txtSearch.Location = new System.Drawing.Point(60, 74);
            this.txtSearch.Size = new System.Drawing.Size(280, 23);
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            this.lblCat.Location = new System.Drawing.Point(350, 78);
            this.lblCat.AutoSize = true;
            this.lblCat.Text = "Category";
            this.cmbCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCategory.Location = new System.Drawing.Point(410, 74);
            this.cmbCategory.Size = new System.Drawing.Size(200, 23);
            this.cmbCategory.SelectedIndexChanged += new System.EventHandler(this.cmbCategory_SelectedIndexChanged);
            this.btnRefresh.Location = new System.Drawing.Point(620, 72);
            this.btnRefresh.Size = new System.Drawing.Size(140, 26);
            this.btnRefresh.Text = "Reload from scan";
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            this.lblFolders.Location = new System.Drawing.Point(12, 108);
            this.lblFolders.AutoSize = true;
            this.lblFolders.Text = "Locations";
            this.lstFolders.Location = new System.Drawing.Point(12, 128);
            this.lstFolders.Size = new System.Drawing.Size(360, 360);
            this.lstFolders.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstFolders.SelectedIndexChanged += new System.EventHandler(this.lstFolders_SelectedIndexChanged);
            this.lblSamples.Location = new System.Drawing.Point(384, 108);
            this.lblSamples.AutoSize = true;
            this.lblSamples.Text = "Sample paths";
            this.lstSamples.Location = new System.Drawing.Point(384, 128);
            this.lstSamples.Size = new System.Drawing.Size(240, 160);
            this.txtDetails.Location = new System.Drawing.Point(384, 296);
            this.txtDetails.Multiline = true;
            this.txtDetails.ReadOnly = true;
            this.txtDetails.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDetails.Size = new System.Drawing.Size(480, 192);
            this.lblCount.Location = new System.Drawing.Point(12, 496);
            this.lblCount.Size = new System.Drawing.Size(300, 20);
            this.lblCount.Text = "0 locations";
            this.btnScaffold.Location = new System.Drawing.Point(384, 500);
            this.btnScaffold.Size = new System.Drawing.Size(160, 30);
            this.btnScaffold.Text = "Scaffold into open mod";
            this.btnScaffold.Click += new System.EventHandler(this.btnScaffold_Click);
            this.btnCopyPath.Location = new System.Drawing.Point(552, 500);
            this.btnCopyPath.Size = new System.Drawing.Size(120, 30);
            this.btnCopyPath.Text = "Copy /Game path";
            this.btnCopyPath.Click += new System.EventHandler(this.btnCopyPath_Click);
            this.btnCopyStage.Location = new System.Drawing.Point(678, 500);
            this.btnCopyStage.Size = new System.Drawing.Size(100, 30);
            this.btnCopyStage.Text = "Copy stage";
            this.btnCopyStage.Click += new System.EventHandler(this.btnCopyStage_Click);
            this.btnExportList.Location = new System.Drawing.Point(784, 500);
            this.btnExportList.Size = new System.Drawing.Size(80, 30);
            this.btnExportList.Text = "Export…";
            this.btnExportList.Click += new System.EventHandler(this.btnExportList_Click);
            this.btnClose.Location = new System.Drawing.Point(300, 496);
            this.btnClose.Size = new System.Drawing.Size(70, 28);
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.ClientSize = new System.Drawing.Size(880, 548);
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblHelp);
            this.Controls.Add(this.lblSearch);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.lblCat);
            this.Controls.Add(this.cmbCategory);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.lblFolders);
            this.Controls.Add(this.lstFolders);
            this.Controls.Add(this.lblSamples);
            this.Controls.Add(this.lstSamples);
            this.Controls.Add(this.txtDetails);
            this.Controls.Add(this.lblCount);
            this.Controls.Add(this.btnScaffold);
            this.Controls.Add(this.btnCopyPath);
            this.Controls.Add(this.btnCopyStage);
            this.Controls.Add(this.btnExportList);
            this.Controls.Add(this.btnClose);
            this.Name = "ContentBrowserForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Content Browser";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblHelp;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblCat;
        private System.Windows.Forms.ComboBox cmbCategory;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Label lblFolders;
        private System.Windows.Forms.ListBox lstFolders;
        private System.Windows.Forms.Label lblSamples;
        private System.Windows.Forms.ListBox lstSamples;
        private System.Windows.Forms.TextBox txtDetails;
        private System.Windows.Forms.Label lblCount;
        private System.Windows.Forms.Button btnScaffold;
        private System.Windows.Forms.Button btnCopyPath;
        private System.Windows.Forms.Button btnCopyStage;
        private System.Windows.Forms.Button btnExportList;
        private System.Windows.Forms.Button btnClose;
    }
}
