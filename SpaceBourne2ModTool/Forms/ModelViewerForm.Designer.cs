namespace SpaceBourne2ModTool.Forms
{
    partial class ModelViewerForm
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelTop = new System.Windows.Forms.Panel();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.btnBrowseFolder = new System.Windows.Forms.Button();
            this.lblMode = new System.Windows.Forms.Label();
            this.cmbMode = new System.Windows.Forms.ComboBox();
            this.btnOpenUModel = new System.Windows.Forms.Button();
            this.btnSpecVertMeshes = new System.Windows.Forms.Button();
            this.btnImages = new System.Windows.Forms.Button();
            this.btnResetView = new System.Windows.Forms.Button();
            this.panelPreview = new System.Windows.Forms.Panel();
            this.lblModeHint = new System.Windows.Forms.Label();
            this.lstMeshes = new System.Windows.Forms.ListBox();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.panelTop.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // Top
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Height = 72;
            this.panelTop.Controls.Add(this.lblTitle);
            this.panelTop.Controls.Add(this.txtPath);
            this.panelTop.Controls.Add(this.btnBrowse);
            this.panelTop.Controls.Add(this.btnBrowseFolder);
            this.panelTop.Controls.Add(this.lblMode);
            this.panelTop.Controls.Add(this.cmbMode);
            this.panelTop.Controls.Add(this.btnResetView);
            this.panelTop.Controls.Add(this.btnOpenUModel);
            this.panelTop.Controls.Add(this.btnSpecVertMeshes);
            this.panelTop.Controls.Add(this.btnImages);
            this.lblTitle.Location = new System.Drawing.Point(8, 4);
            this.lblTitle.Size = new System.Drawing.Size(420, 22);
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Text = "3D Model Viewer";
            this.txtPath.Location = new System.Drawing.Point(8, 30);
            this.txtPath.Size = new System.Drawing.Size(360, 23);
            this.btnBrowse.Location = new System.Drawing.Point(376, 28);
            this.btnBrowse.Size = new System.Drawing.Size(70, 26);
            this.btnBrowse.Text = "Open…";
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            this.btnBrowseFolder.Location = new System.Drawing.Point(450, 28);
            this.btnBrowseFolder.Size = new System.Drawing.Size(80, 26);
            this.btnBrowseFolder.Text = "Folder…";
            this.btnBrowseFolder.Click += new System.EventHandler(this.btnBrowseFolder_Click);
            this.lblMode.Location = new System.Drawing.Point(540, 32);
            this.lblMode.AutoSize = true;
            this.lblMode.Text = "Draw";
            this.cmbMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMode.Location = new System.Drawing.Point(580, 28);
            this.cmbMode.Size = new System.Drawing.Size(110, 23);
            this.cmbMode.Items.AddRange(new object[] { "Wireframe", "Unlit grey", "Shaded" });
            this.cmbMode.SelectedIndex = 0;
            this.cmbMode.SelectedIndexChanged += new System.EventHandler(this.cmbMode_SelectedIndexChanged);
            this.btnResetView.Location = new System.Drawing.Point(700, 28);
            this.btnResetView.Size = new System.Drawing.Size(70, 26);
            this.btnResetView.Text = "Reset";
            this.btnResetView.Click += new System.EventHandler(this.btnResetView_Click);
            this.btnOpenUModel.Location = new System.Drawing.Point(778, 28);
            this.btnOpenUModel.Size = new System.Drawing.Size(90, 26);
            this.btnOpenUModel.Text = "UModel…";
            this.btnOpenUModel.Click += new System.EventHandler(this.btnOpenUModel_Click);
            this.btnSpecVertMeshes.Location = new System.Drawing.Point(872, 28);
            this.btnSpecVertMeshes.Size = new System.Drawing.Size(90, 26);
            this.btnSpecVertMeshes.Text = "SpecVert";
            this.btnSpecVertMeshes.Click += new System.EventHandler(this.btnSpecVertMeshes_Click);
            this.btnImages.Location = new System.Drawing.Point(966, 28);
            this.btnImages.Size = new System.Drawing.Size(90, 26);
            this.btnImages.Text = "Images…";
            this.btnImages.Click += new System.EventHandler(this.btnImages_Click);
            // Preview full width
            this.panelPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelPreview.BackColor = System.Drawing.Color.FromArgb(18, 18, 22);
            this.panelPreview.Paint += new System.Windows.Forms.PaintEventHandler(this.panelPreview_Paint);
            this.panelPreview.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelPreview_MouseDown);
            this.panelPreview.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelPreview_MouseMove);
            this.panelPreview.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelPreview_MouseUp);
            // Bottom strip
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Height = 120;
            this.panelBottom.Controls.Add(this.lblModeHint);
            this.panelBottom.Controls.Add(this.lstMeshes);
            this.panelBottom.Controls.Add(this.txtLog);
            this.panelBottom.Controls.Add(this.btnClose);
            this.lblModeHint.Location = new System.Drawing.Point(8, 4);
            this.lblModeHint.Size = new System.Drawing.Size(900, 18);
            this.lblModeHint.Text = "Drag to orbit · Open .obj or .stl for in-app preview · Use UModel for cooked .uasset meshes";
            this.lstMeshes.Location = new System.Drawing.Point(8, 26);
            this.lstMeshes.Size = new System.Drawing.Size(280, 80);
            this.lstMeshes.SelectedIndexChanged += new System.EventHandler(this.lstMeshes_SelectedIndexChanged);
            this.txtLog.Location = new System.Drawing.Point(296, 26);
            this.txtLog.Multiline = true;
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(680, 80);
            this.txtLog.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom;
            this.btnClose.Location = new System.Drawing.Point(988, 50);
            this.btnClose.Size = new System.Drawing.Size(72, 28);
            this.btnClose.Text = "Close";
            this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Top;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // Form
            this.ClientSize = new System.Drawing.Size(1080, 720);
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.Controls.Add(this.panelPreview);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelTop);
            this.Name = "ModelViewerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "3D Model Viewer";
            this.panelTop.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnBrowseFolder;
        private System.Windows.Forms.Label lblMode;
        private System.Windows.Forms.ComboBox cmbMode;
        private System.Windows.Forms.Button btnOpenUModel;
        private System.Windows.Forms.Button btnSpecVertMeshes;
        private System.Windows.Forms.Button btnImages;
        private System.Windows.Forms.Button btnResetView;
        private System.Windows.Forms.Panel panelPreview;
        private System.Windows.Forms.Label lblModeHint;
        private System.Windows.Forms.ListBox lstMeshes;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Button btnClose;
    }
}
