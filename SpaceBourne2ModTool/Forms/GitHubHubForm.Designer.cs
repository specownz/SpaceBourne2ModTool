namespace SpaceBourne2ModTool.Forms
{
    partial class GitHubHubForm
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.tabs = new System.Windows.Forms.TabControl();
            this.tabAccount = new System.Windows.Forms.TabPage();
            this.tabPublish = new System.Windows.Forms.TabPage();
            this.tabImport = new System.Windows.Forms.TabPage();
            this.tabBrowse = new System.Windows.Forms.TabPage();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.btnClose = new System.Windows.Forms.Button();
            // account
            this.lblTok = new System.Windows.Forms.Label();
            this.txtToken = new System.Windows.Forms.TextBox();
            this.btnSaveToken = new System.Windows.Forms.Button();
            this.btnTest = new System.Windows.Forms.Button();
            this.btnHelpPat = new System.Windows.Forms.Button();
            this.lblAccount = new System.Windows.Forms.Label();
            // publish
            this.lblRepoName = new System.Windows.Forms.Label();
            this.txtRepoName = new System.Windows.Forms.TextBox();
            this.lblRepoDesc = new System.Windows.Forms.Label();
            this.txtRepoDesc = new System.Windows.Forms.TextBox();
            this.chkPrivate = new System.Windows.Forms.CheckBox();
            this.lblCommit = new System.Windows.Forms.Label();
            this.txtCommit = new System.Windows.Forms.TextBox();
            this.btnPublish = new System.Windows.Forms.Button();
            this.btnPush = new System.Windows.Forms.Button();
            this.btnPull = new System.Windows.Forms.Button();
            this.btnGitIgnore = new System.Windows.Forms.Button();
            // import
            this.lblImport = new System.Windows.Forms.Label();
            this.txtImport = new System.Windows.Forms.TextBox();
            this.btnClone = new System.Windows.Forms.Button();
            this.btnZipImport = new System.Windows.Forms.Button();
            // browse
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnListMine = new System.Windows.Forms.Button();
            this.lstRepos = new System.Windows.Forms.ListBox();
            this.btnOpenSelected = new System.Windows.Forms.Button();
            this.btnImportSelected = new System.Windows.Forms.Button();
            this.btnReleases = new System.Windows.Forms.Button();
            this.lstReleases = new System.Windows.Forms.ListBox();
            this.tabs.SuspendLayout();
            this.tabAccount.SuspendLayout();
            this.tabPublish.SuspendLayout();
            this.tabImport.SuspendLayout();
            this.tabBrowse.SuspendLayout();
            this.SuspendLayout();
            //
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(12, 8);
            this.lblTitle.Size = new System.Drawing.Size(600, 28);
            this.lblTitle.Text = "GitHub — publish and import SpaceBourne 2 mods";
            this.lblStatus.Location = new System.Drawing.Point(12, 40);
            this.lblStatus.Size = new System.Drawing.Size(860, 20);
            this.lblStatus.Text = "Status";
            this.tabs.Location = new System.Drawing.Point(12, 68);
            this.tabs.Size = new System.Drawing.Size(860, 320);
            this.tabs.Controls.Add(this.tabAccount);
            this.tabs.Controls.Add(this.tabPublish);
            this.tabs.Controls.Add(this.tabImport);
            this.tabs.Controls.Add(this.tabBrowse);
            // Account
            this.tabAccount.Text = "Account";
            this.tabAccount.Controls.Add(this.lblTok);
            this.tabAccount.Controls.Add(this.txtToken);
            this.tabAccount.Controls.Add(this.btnSaveToken);
            this.tabAccount.Controls.Add(this.btnTest);
            this.tabAccount.Controls.Add(this.btnHelpPat);
            this.tabAccount.Controls.Add(this.lblAccount);
            this.lblTok.Location = new System.Drawing.Point(12, 16);
            this.lblTok.AutoSize = true;
            this.lblTok.Text = "Personal Access Token";
            this.txtToken.Location = new System.Drawing.Point(12, 40);
            this.txtToken.Size = new System.Drawing.Size(520, 23);
            this.btnSaveToken.Location = new System.Drawing.Point(540, 38);
            this.btnSaveToken.Size = new System.Drawing.Size(100, 28);
            this.btnSaveToken.Text = "Save token";
            this.btnSaveToken.Click += new System.EventHandler(this.btnSaveToken_Click);
            this.btnTest.Location = new System.Drawing.Point(648, 38);
            this.btnTest.Size = new System.Drawing.Size(120, 28);
            this.btnTest.Text = "Test connection";
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            this.btnHelpPat.Location = new System.Drawing.Point(12, 80);
            this.btnHelpPat.Size = new System.Drawing.Size(160, 28);
            this.btnHelpPat.Text = "How to get a token…";
            this.btnHelpPat.Click += new System.EventHandler(this.btnHelpPat_Click);
            this.lblAccount.Location = new System.Drawing.Point(12, 120);
            this.lblAccount.Size = new System.Drawing.Size(700, 40);
            this.lblAccount.Text = "Not signed in.";
            // Publish
            this.tabPublish.Text = "Publish open mod";
            this.tabPublish.Controls.Add(this.lblRepoName);
            this.tabPublish.Controls.Add(this.txtRepoName);
            this.tabPublish.Controls.Add(this.lblRepoDesc);
            this.tabPublish.Controls.Add(this.txtRepoDesc);
            this.tabPublish.Controls.Add(this.chkPrivate);
            this.tabPublish.Controls.Add(this.lblCommit);
            this.tabPublish.Controls.Add(this.txtCommit);
            this.tabPublish.Controls.Add(this.btnPublish);
            this.tabPublish.Controls.Add(this.btnPush);
            this.tabPublish.Controls.Add(this.btnPull);
            this.tabPublish.Controls.Add(this.btnGitIgnore);
            this.lblRepoName.Location = new System.Drawing.Point(12, 16);
            this.lblRepoName.AutoSize = true;
            this.lblRepoName.Text = "New repository name";
            this.txtRepoName.Location = new System.Drawing.Point(12, 36);
            this.txtRepoName.Size = new System.Drawing.Size(280, 23);
            this.lblRepoDesc.Location = new System.Drawing.Point(12, 70);
            this.lblRepoDesc.AutoSize = true;
            this.lblRepoDesc.Text = "Description";
            this.txtRepoDesc.Location = new System.Drawing.Point(12, 90);
            this.txtRepoDesc.Size = new System.Drawing.Size(500, 23);
            this.txtRepoDesc.Text = "SpaceBourne 2 mod created with specSB2modTool";
            this.chkPrivate.Location = new System.Drawing.Point(530, 92);
            this.chkPrivate.AutoSize = true;
            this.chkPrivate.Text = "Private repository";
            this.lblCommit.Location = new System.Drawing.Point(12, 124);
            this.lblCommit.AutoSize = true;
            this.lblCommit.Text = "Commit message";
            this.txtCommit.Location = new System.Drawing.Point(12, 144);
            this.txtCommit.Size = new System.Drawing.Size(500, 23);
            this.btnPublish.Location = new System.Drawing.Point(12, 190);
            this.btnPublish.Size = new System.Drawing.Size(180, 32);
            this.btnPublish.Text = "Create repo + push";
            this.btnPublish.Click += new System.EventHandler(this.btnPublish_Click);
            this.btnPush.Location = new System.Drawing.Point(200, 190);
            this.btnPush.Size = new System.Drawing.Size(140, 32);
            this.btnPush.Text = "Commit + push";
            this.btnPush.Click += new System.EventHandler(this.btnPush_Click);
            this.btnPull.Location = new System.Drawing.Point(348, 190);
            this.btnPull.Size = new System.Drawing.Size(100, 32);
            this.btnPull.Text = "Pull";
            this.btnPull.Click += new System.EventHandler(this.btnPull_Click);
            this.btnGitIgnore.Location = new System.Drawing.Point(460, 190);
            this.btnGitIgnore.Size = new System.Drawing.Size(160, 32);
            this.btnGitIgnore.Text = "Add .gitignore / README";
            this.btnGitIgnore.Click += new System.EventHandler(this.btnGitIgnore_Click);
            // Import
            this.tabImport.Text = "Import";
            this.tabImport.Controls.Add(this.lblImport);
            this.tabImport.Controls.Add(this.txtImport);
            this.tabImport.Controls.Add(this.btnClone);
            this.tabImport.Controls.Add(this.btnZipImport);
            this.lblImport.Location = new System.Drawing.Point(12, 16);
            this.lblImport.Size = new System.Drawing.Size(700, 36);
            this.lblImport.Text = "Paste owner/repo (for example YourName/my-sb2-mod) or a full https://github.com/… URL";
            this.txtImport.Location = new System.Drawing.Point(12, 56);
            this.txtImport.Size = new System.Drawing.Size(600, 23);
            this.btnClone.Location = new System.Drawing.Point(12, 100);
            this.btnClone.Size = new System.Drawing.Size(160, 32);
            this.btnClone.Text = "Clone with Git";
            this.btnClone.Click += new System.EventHandler(this.btnClone_Click);
            this.btnZipImport.Location = new System.Drawing.Point(180, 100);
            this.btnZipImport.Size = new System.Drawing.Size(200, 32);
            this.btnZipImport.Text = "Download ZIP (no Git needed)";
            this.btnZipImport.Click += new System.EventHandler(this.btnZipImport_Click);
            // Browse
            this.tabBrowse.Text = "Browse / search";
            this.tabBrowse.Controls.Add(this.txtSearch);
            this.tabBrowse.Controls.Add(this.btnSearch);
            this.tabBrowse.Controls.Add(this.btnListMine);
            this.tabBrowse.Controls.Add(this.lstRepos);
            this.tabBrowse.Controls.Add(this.btnOpenSelected);
            this.tabBrowse.Controls.Add(this.btnImportSelected);
            this.tabBrowse.Controls.Add(this.btnReleases);
            this.tabBrowse.Controls.Add(this.lstReleases);
            this.txtSearch.Location = new System.Drawing.Point(12, 12);
            this.txtSearch.Size = new System.Drawing.Size(320, 23);
            this.btnSearch.Location = new System.Drawing.Point(340, 10);
            this.btnSearch.Size = new System.Drawing.Size(90, 28);
            this.btnSearch.Text = "Search";
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            this.btnListMine.Location = new System.Drawing.Point(436, 10);
            this.btnListMine.Size = new System.Drawing.Size(120, 28);
            this.btnListMine.Text = "My repositories";
            this.btnListMine.Click += new System.EventHandler(this.btnListMine_Click);
            this.lstRepos.Location = new System.Drawing.Point(12, 48);
            this.lstRepos.Size = new System.Drawing.Size(540, 200);
            this.btnOpenSelected.Location = new System.Drawing.Point(560, 48);
            this.btnOpenSelected.Size = new System.Drawing.Size(120, 28);
            this.btnOpenSelected.Text = "Open on web";
            this.btnOpenSelected.Click += new System.EventHandler(this.btnOpenSelected_Click);
            this.btnImportSelected.Location = new System.Drawing.Point(560, 84);
            this.btnImportSelected.Size = new System.Drawing.Size(120, 28);
            this.btnImportSelected.Text = "Import selected";
            this.btnImportSelected.Click += new System.EventHandler(this.btnImportSelected_Click);
            this.btnReleases.Location = new System.Drawing.Point(560, 120);
            this.btnReleases.Size = new System.Drawing.Size(120, 28);
            this.btnReleases.Text = "List releases";
            this.btnReleases.Click += new System.EventHandler(this.btnReleases_Click);
            this.lstReleases.Location = new System.Drawing.Point(560, 156);
            this.lstReleases.Size = new System.Drawing.Size(260, 92);
            // log + close
            this.txtLog.Location = new System.Drawing.Point(12, 400);
            this.txtLog.Multiline = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(860, 140);
            this.txtLog.ReadOnly = true;
            this.btnClose.Location = new System.Drawing.Point(792, 548);
            this.btnClose.Size = new System.Drawing.Size(80, 28);
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.ClientSize = new System.Drawing.Size(892, 588);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.tabs);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.btnClose);
            this.MinimumSize = new System.Drawing.Size(800, 560);
            this.Name = "GitHubHubForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "GitHub";
            this.tabs.ResumeLayout(false);
            this.tabAccount.ResumeLayout(false);
            this.tabPublish.ResumeLayout(false);
            this.tabImport.ResumeLayout(false);
            this.tabBrowse.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage tabAccount, tabPublish, tabImport, tabBrowse;
        private System.Windows.Forms.Label lblTitle, lblStatus, lblTok, lblAccount, lblRepoName, lblRepoDesc, lblCommit, lblImport;
        private System.Windows.Forms.TextBox txtToken, txtRepoName, txtRepoDesc, txtCommit, txtImport, txtSearch, txtLog;
        private System.Windows.Forms.Button btnSaveToken, btnTest, btnHelpPat, btnPublish, btnPush, btnPull, btnGitIgnore, btnClone, btnZipImport, btnSearch, btnListMine, btnOpenSelected, btnImportSelected, btnReleases, btnClose;
        private System.Windows.Forms.CheckBox chkPrivate;
        private System.Windows.Forms.ListBox lstRepos, lstReleases;
    }
}
