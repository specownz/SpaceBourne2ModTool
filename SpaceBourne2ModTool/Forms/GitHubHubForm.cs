using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SpaceBourne2ModTool.Core;
using SpaceBourne2ModTool.Core.GitHub;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Forms
{
    public partial class GitHubHubForm : Form
    {
        public GitHubHubForm()
        {
            InitializeComponent();
            AppBranding.Apply(this, AppBranding.ProductTitle + " — GitHub");
            FormScaleHelper.Enable(this);
            try { UiTheme.Apply(this); } catch { }
            txtToken.Text = ConfigService.GitHubToken ?? "";
            txtToken.UseSystemPasswordChar = true;
            txtSearch.Text = "spacebourne mod";
            txtRepoName.Text = CurrentModContext.IsOpen
                ? Path.GetFileName(CurrentModContext.ModRoot).Replace(" ", "-")
                : "my-sb2-mod";
            txtCommit.Text = "Update from specSB2modTool";
            RefreshStatus();
        }

        void RefreshStatus()
        {
            bool git = GitCli.IsAvailable();
            bool tok = GitHubApiClient.HasToken;
            lblStatus.Text = "Git: " + (git ? "found" : "not found (ZIP import still works)")
                + "  ·  Token: " + (tok ? "saved" : "not set")
                + "  ·  Open mod: " + (CurrentModContext.IsOpen ? CurrentModContext.DisplayName : "(none)");
            if (CurrentModContext.IsOpen && GitCli.IsRepo(CurrentModContext.ModRoot))
            {
                var st = GitCli.Status(CurrentModContext.ModRoot);
                txtLog.AppendText("--- git status ---\r\n" + st.Combined + "\r\n");
            }
        }

        void Log(string m)
        {
            txtLog.AppendText(m.TrimEnd() + "\r\n");
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.ScrollToCaret();
        }

        void btnSaveToken_Click(object sender, EventArgs e)
        {
            ConfigService.GitHubToken = (txtToken.Text ?? "").Trim();
            ConfigService.Save();
            Log("Token saved locally in application settings.");
            RefreshStatus();
        }

        void btnTest_Click(object sender, EventArgs e)
        {
            GitHubApiClient.Token = (txtToken.Text ?? "").Trim();
            if (!GitHubApiClient.HasToken)
            {
                MessageBox.Show(this, "Paste a GitHub Personal Access Token first.\n\nCreate one at GitHub → Settings → Developer settings → Personal access tokens.\nScopes: repo (for private) or public_repo.",
                    "GitHub", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var r = GitHubApiClient.GetUser();
            if (!r.Ok)
            {
                MessageBox.Show(this, "Authentication failed:\n" + r.Error, "GitHub", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Log("Auth failed: " + r.Error);
                return;
            }
            var u = GitHubApiClient.ParseUser(r.Raw);
            ConfigService.GitHubUsername = u != null ? u.Login : "";
            lblAccount.Text = "Signed in as: " + (u != null ? u.Login : "?") +
                (u != null && !string.IsNullOrEmpty(u.HtmlUrl) ? "  " + u.HtmlUrl : "");
            Log("Authenticated as " + (u != null ? u.Login : ""));
            MessageBox.Show(this, "Connected as " + (u != null ? u.Login : "user") + ".", "GitHub", MessageBoxButtons.OK, MessageBoxIcon.Information);
            RefreshStatus();
        }

        void btnListMine_Click(object sender, EventArgs e)
        {
            lstRepos.Items.Clear();
            if (!GitHubApiClient.HasToken)
            {
                MessageBox.Show(this, "Token required to list your repositories.");
                return;
            }
            Log("Listing your repositories…");
            var repos = GitHubApiClient.ListUserRepos();
            foreach (var repo in repos) lstRepos.Items.Add(repo);
            Log("Listed " + repos.Count + " repositories.");
        }

        void btnSearch_Click(object sender, EventArgs e)
        {
            lstRepos.Items.Clear();
            Log("Searching: " + txtSearch.Text);
            var repos = GitHubApiClient.SearchRepos(txtSearch.Text);
            foreach (var repo in repos) lstRepos.Items.Add(repo);
            Log("Search returned " + repos.Count + " repositories.");
        }

        void btnPublish_Click(object sender, EventArgs e)
        {
            if (!CurrentModContext.IsOpen)
            {
                MessageBox.Show(this, "Open a mod project first (File → Open Mod Folder).", "GitHub", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            GitHubApiClient.Token = (txtToken.Text ?? "").Trim();
            Log("Publishing " + CurrentModContext.ModRoot + " …");
            var pr = GitHubModService.PublishNewRepo(
                CurrentModContext.ModRoot,
                txtRepoName.Text.Trim(),
                txtRepoDesc.Text.Trim(),
                chkPrivate.Checked,
                txtCommit.Text.Trim());
            Log(pr.Message);
            if (pr.Ok && !string.IsNullOrEmpty(pr.RepoHtmlUrl))
            {
                if (MessageBox.Show(this, pr.Message + "\n\nOpen on GitHub?", "Published", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    try { Process.Start(pr.RepoHtmlUrl); } catch { }
            }
            else
                MessageBox.Show(this, pr.Message, "Publish", MessageBoxButtons.OK, pr.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            RefreshStatus();
        }

        void btnPush_Click(object sender, EventArgs e)
        {
            if (!CurrentModContext.IsOpen)
            {
                MessageBox.Show(this, "Open a mod project first.");
                return;
            }
            GitHubApiClient.Token = (txtToken.Text ?? "").Trim();
            var pr = GitHubModService.PushExisting(CurrentModContext.ModRoot, txtCommit.Text.Trim());
            Log(pr.Message);
            MessageBox.Show(this, pr.Message, "Push", MessageBoxButtons.OK, pr.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            RefreshStatus();
        }

        void btnPull_Click(object sender, EventArgs e)
        {
            if (!CurrentModContext.IsOpen || !GitCli.IsRepo(CurrentModContext.ModRoot))
            {
                MessageBox.Show(this, "Open a mod that is already a git repository.");
                return;
            }
            var r = GitCli.Pull(CurrentModContext.ModRoot);
            Log(r.Combined);
            MessageBox.Show(this, r.Ok ? "Pull finished." : r.Combined, "Pull", MessageBoxButtons.OK, r.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        void btnClone_Click(object sender, EventArgs e)
        {
            string input = txtImport.Text.Trim();
            if (string.IsNullOrEmpty(input))
            {
                MessageBox.Show(this, "Paste a clone URL or owner/repo.");
                return;
            }
            try
            {
                string url = input;
                string name = "imported";
                string owner, repo;
                if (GitHubModService.TryParseOwnerRepo(input, out owner, out repo))
                {
                    url = "https://github.com/" + owner + "/" + repo + ".git";
                    name = owner + "_" + repo;
                }
                Log("Cloning " + url + " …");
                string dest = GitHubModService.ImportClone(url, name);
                Log("Cloned to " + dest);
                if (MessageBox.Show(this, "Imported to:\n" + dest + "\n\nOpen as current mod?", "Import", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    CurrentModContext.Open(dest);
                    RefreshStatus();
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                MessageBox.Show(this, ex.Message, "Clone failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        void btnZipImport_Click(object sender, EventArgs e)
        {
            string input = txtImport.Text.Trim();
            string owner, repo;
            if (!GitHubModService.TryParseOwnerRepo(input, out owner, out repo))
            {
                MessageBox.Show(this, "Enter owner/repo or a github.com URL.");
                return;
            }
            try
            {
                Log("Downloading ZIP for " + owner + "/" + repo + " …");
                string dest = GitHubModService.ImportZip(owner, repo, "main");
                Log("Extracted to " + dest);
                if (MessageBox.Show(this, "Imported to:\n" + dest + "\n\nOpen as current mod?", "Import", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    CurrentModContext.Open(dest);
                    RefreshStatus();
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                MessageBox.Show(this, ex.Message, "ZIP import failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        void btnOpenSelected_Click(object sender, EventArgs e)
        {
            var repo = lstRepos.SelectedItem as GhRepo;
            if (repo == null || string.IsNullOrEmpty(repo.HtmlUrl)) return;
            try { Process.Start(repo.HtmlUrl); } catch { }
        }

        void btnImportSelected_Click(object sender, EventArgs e)
        {
            var repo = lstRepos.SelectedItem as GhRepo;
            if (repo == null) return;
            txtImport.Text = repo.FullName ?? (repo.CloneUrl ?? "");
            if (GitCli.IsAvailable() && !string.IsNullOrEmpty(repo.CloneUrl))
                btnClone_Click(sender, e);
            else
                btnZipImport_Click(sender, e);
        }

        void btnReleases_Click(object sender, EventArgs e)
        {
            var repo = lstRepos.SelectedItem as GhRepo;
            string owner, name;
            if (repo != null && GitHubModService.TryParseOwnerRepo(repo.FullName ?? "", out owner, out name))
            {
                var list = GitHubApiClient.ListReleases(owner, name);
                lstReleases.Items.Clear();
                foreach (var rel in list) lstReleases.Items.Add(rel);
                Log("Releases: " + list.Count);
            }
            else
                MessageBox.Show(this, "Select a repository first.");
        }

        void btnGitIgnore_Click(object sender, EventArgs e)
        {
            if (!CurrentModContext.IsOpen) { MessageBox.Show(this, "Open a mod first."); return; }
            GitHubModService.EnsureGitIgnore(CurrentModContext.ModRoot);
            GitHubModService.EnsureReadme(CurrentModContext.ModRoot, Path.GetFileName(CurrentModContext.ModRoot), "SpaceBourne 2 mod");
            Log("Ensured .gitignore and README.md in open mod.");
            MessageBox.Show(this, "Wrote .gitignore and README.md if they were missing.");
        }

        void btnHelpPat_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this,
                "Create a Personal Access Token on GitHub:\n\n" +
                "1. GitHub → Settings → Developer settings → Personal access tokens\n" +
                "2. Classic token with 'repo' scope (or fine-grained with Contents: Read/Write)\n" +
                "3. Paste it here and click Save token, then Test connection\n\n" +
                "Tokens are stored in local application settings on this PC only.\n" +
                "Git for Windows is required for clone/push/pull; ZIP import works without git.",
                "GitHub token help", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        void btnClose_Click(object sender, EventArgs e) => Close();
    }
}
