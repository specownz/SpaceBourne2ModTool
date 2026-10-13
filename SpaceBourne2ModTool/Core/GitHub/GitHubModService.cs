using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Core.GitHub
{
    /// <summary>High-level publish / import of SpaceBourne 2 mod folders via GitHub + git.</summary>
    public static class GitHubModService
    {
        public static string DefaultGitIgnore
        {
            get
            {
                return
                    "# specSB2modTool / SpaceBourne 2 mod\r\n" +
                    "*.user\r\n*.suo\r\n.vs/\r\nbin/\r\nobj/\r\n" +
                    "*.bak_*\r\n*.tmp\r\nThumbs.db\r\nDesktop.ini\r\n" +
                    "# Large cooked archives (prefer releases or Speckage shares)\r\n" +
                    "*.pak\r\n*.ucas\r\n*.utoc\r\n" +
                    "# Local analysis dumps\r\n" +
                    "SB2_*_LATEST.*\r\nAnalysis/\r\n";
            }
        }

        public static void EnsureGitIgnore(string modRoot)
        {
            if (string.IsNullOrEmpty(modRoot) || !Directory.Exists(modRoot)) return;
            string path = Path.Combine(modRoot, ".gitignore");
            if (!File.Exists(path))
                File.WriteAllText(path, DefaultGitIgnore, Encoding.UTF8);
        }

        public static void EnsureReadme(string modRoot, string title, string description)
        {
            string path = Path.Combine(modRoot, "README.md");
            if (File.Exists(path)) return;
            var sb = new StringBuilder();
            sb.AppendLine("# " + (title ?? "SpaceBourne 2 Mod"));
            sb.AppendLine();
            sb.AppendLine(description ?? "Created with [specSB2modTool](https://github.com).");
            sb.AppendLine();
            sb.AppendLine("## Install");
            sb.AppendLine("Import with **specSB2modTool → GitHub → Import**, or place staged files / Speckage package via the Mod Loader.");
            sb.AppendLine();
            sb.AppendLine("## Notes");
            sb.AppendLine("- Keep `.uasset` + `.uexp` pairs together.");
            sb.AppendLine("- Build patches with Unreal Engine 4.27 UnrealPak `-compress` (Oodle).");
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        public class PublishResult
        {
            public bool Ok;
            public string Message;
            public string RepoHtmlUrl;
            public string CloneUrl;
        }

        public static PublishResult PublishNewRepo(string modRoot, string repoName, string description, bool isPrivate, string commitMessage)
        {
            var pr = new PublishResult();
            if (string.IsNullOrEmpty(modRoot) || !Directory.Exists(modRoot))
            {
                pr.Message = "Open a mod folder first.";
                return pr;
            }
            if (!GitHubApiClient.HasToken)
            {
                pr.Message = "Save a GitHub Personal Access Token in the Account tab first.";
                return pr;
            }
            if (!GitCli.IsAvailable())
            {
                pr.Message = "Git for Windows is required to publish. Install git, or use only API create without push.";
                return pr;
            }

            EnsureGitIgnore(modRoot);
            EnsureReadme(modRoot, repoName, description);

            // Create remote
            var created = GitHubApiClient.CreateRepo(SanitizeRepoName(repoName), description, isPrivate);
            if (!created.Ok)
            {
                pr.Message = "Create repo failed: " + created.Error;
                return pr;
            }
            var repo = GitHubApiClient.ParseRepo(created.Raw);
            pr.RepoHtmlUrl = repo != null ? repo.HtmlUrl : null;
            pr.CloneUrl = repo != null ? repo.CloneUrl : null;
            if (string.IsNullOrEmpty(pr.CloneUrl) && repo != null && !string.IsNullOrEmpty(repo.FullName))
                pr.CloneUrl = "https://github.com/" + repo.FullName + ".git";

            var user = GitHubApiClient.ParseUser(GitHubApiClient.GetUser().Raw ?? "");
            string login = user != null ? user.Login : "git";
            string authUrl = GitCli.AuthenticatedHttpsUrl(pr.CloneUrl, login, GitHubApiClient.Token);

            if (!GitCli.IsRepo(modRoot))
            {
                var init = GitCli.Init(modRoot);
                if (!init.Ok) { pr.Message = "git init failed: " + init.Combined; return pr; }
            }

            GitCli.ConfigLocal(modRoot, "user.email", login + "@users.noreply.github.com");
            GitCli.ConfigLocal(modRoot, "user.name", login);

            // remote
            var rem = GitCli.Run(modRoot, "remote");
            if (rem.Combined.IndexOf("origin", StringComparison.OrdinalIgnoreCase) >= 0)
                GitCli.RemoteSetUrl(modRoot, "origin", authUrl);
            else
                GitCli.RemoteAdd(modRoot, "origin", authUrl);

            GitCli.AddAll(modRoot);
            var commit = GitCli.Commit(modRoot, string.IsNullOrEmpty(commitMessage) ? "Initial mod publish from specSB2modTool" : commitMessage);
            // commit may fail if nothing to commit — still try push
            var push = GitCli.Push(modRoot, "origin", "main", true);
            if (!push.Ok)
            {
                // try master
                push = GitCli.Push(modRoot, "origin", "master", true);
            }
            if (!push.Ok)
            {
                pr.Message = "Repo created but push failed:\n" + push.Combined +
                    (commit.Ok ? "" : "\nCommit: " + commit.Combined) +
                    "\nRepo: " + pr.RepoHtmlUrl;
                return pr;
            }

            // Store remote without token in config
            if (!string.IsNullOrEmpty(pr.CloneUrl))
                GitCli.RemoteSetUrl(modRoot, "origin", pr.CloneUrl);

            File.WriteAllText(Path.Combine(modRoot, "GITHUB.txt"),
                "HtmlUrl=" + pr.RepoHtmlUrl + "\r\nCloneUrl=" + pr.CloneUrl + "\r\nPublishedUtc=" + DateTime.UtcNow.ToString("o") + "\r\n",
                Encoding.UTF8);

            pr.Ok = true;
            pr.Message = "Published to GitHub.\n" + pr.RepoHtmlUrl;
            return pr;
        }

        public static PublishResult PushExisting(string modRoot, string commitMessage)
        {
            var pr = new PublishResult();
            if (!GitCli.IsRepo(modRoot))
            {
                pr.Message = "This folder is not a git repository. Use Publish new repository first.";
                return pr;
            }
            if (!GitHubApiClient.HasToken)
            {
                pr.Message = "GitHub token required for authenticated push.";
                return pr;
            }
            var user = GitHubApiClient.ParseUser(GitHubApiClient.GetUser().Raw ?? "");
            string login = user != null ? user.Login : "git";

            // Read origin
            var origin = GitCli.Run(modRoot, "remote get-url origin");
            string url = origin.Combined.Trim();
            if (string.IsNullOrEmpty(url) || !origin.Ok)
            {
                pr.Message = "No origin remote. Publish new repository first.";
                return pr;
            }
            string plain = url;
            if (url.Contains("@github.com"))
            {
                // strip embedded creds for storage later
                int at = url.IndexOf("@");
                int scheme = url.IndexOf("://");
                if (scheme >= 0 && at > scheme)
                    plain = url.Substring(0, scheme + 3) + url.Substring(at + 1);
            }
            string authUrl = GitCli.AuthenticatedHttpsUrl(plain, login, GitHubApiClient.Token);
            GitCli.RemoteSetUrl(modRoot, "origin", authUrl);
            EnsureGitIgnore(modRoot);
            GitCli.AddAll(modRoot);
            GitCli.Commit(modRoot, string.IsNullOrEmpty(commitMessage) ? "Update from specSB2modTool" : commitMessage);
            var push = GitCli.Push(modRoot, "origin", "main", false);
            if (!push.Ok) push = GitCli.Push(modRoot, "origin", "master", false);
            GitCli.RemoteSetUrl(modRoot, "origin", plain);
            pr.Ok = push.Ok;
            pr.CloneUrl = plain;
            pr.Message = push.Ok ? "Push completed." : ("Push failed:\n" + push.Combined);
            return pr;
        }

        public static string ImportClone(string cloneUrl, string destFolderName)
        {
            string root = Path.Combine(ConfigService.ModsOutputPath, "GitHubImports");
            Directory.CreateDirectory(root);
            string dest = Path.Combine(root, SanitizeRepoName(destFolderName ?? "imported_mod"));
            if (Directory.Exists(dest))
                dest = dest + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var r = GitCli.Clone(cloneUrl, dest);
            if (!r.Ok) throw new Exception(r.Combined);
            return dest;
        }

        public static string ImportZip(string owner, string repo, string branch)
        {
            string root = Path.Combine(ConfigService.ModsOutputPath, "GitHubImports");
            Directory.CreateDirectory(root);
            string zip = Path.Combine(root, owner + "_" + repo + ".zip");
            var dl = GitHubApiClient.DownloadZipball(owner, repo, branch, zip);
            if (!dl.Ok) throw new Exception(dl.Error);
            string dest = Path.Combine(root, SanitizeRepoName(owner + "_" + repo));
            if (Directory.Exists(dest))
                dest = dest + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            Directory.CreateDirectory(dest);
            ZipFile.ExtractToDirectory(zip, dest);
            // GitHub zipball has a single top-level folder
            try
            {
                var dirs = Directory.GetDirectories(dest);
                if (dirs.Length == 1 && Directory.GetFiles(dest).Length == 0)
                {
                    // leave as-is; user opens nested folder or we promote
                    string inner = dirs[0];
                    string promoted = dest + "_mod";
                    if (!Directory.Exists(promoted))
                    {
                        Directory.Move(inner, promoted);
                        try { Directory.Delete(dest, true); } catch { }
                        dest = promoted;
                    }
                }
            }
            catch { }
            try { File.Delete(zip); } catch { }
            return dest;
        }

        public static bool TryParseOwnerRepo(string input, out string owner, out string repo)
        {
            owner = repo = null;
            if (string.IsNullOrWhiteSpace(input)) return false;
            input = input.Trim().TrimEnd('/');
            if (input.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
                input = input.Substring(0, input.Length - 4);
            // URL forms
            int gh = input.IndexOf("github.com/", StringComparison.OrdinalIgnoreCase);
            if (gh >= 0)
            {
                string rest = input.Substring(gh + "github.com/".Length);
                var parts = rest.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    owner = parts[0];
                    repo = parts[1];
                    return true;
                }
            }
            // owner/repo
            if (input.IndexOf('/') > 0 && input.IndexOf(' ') < 0)
            {
                var parts = input.Split('/');
                if (parts.Length == 2)
                {
                    owner = parts[0];
                    repo = parts[1];
                    return true;
                }
            }
            return false;
        }

        static string SanitizeRepoName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "sb2-mod";
            var sb = new StringBuilder();
            foreach (char c in name.Trim())
            {
                if (char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '.') sb.Append(c);
                else if (c == ' ') sb.Append('-');
            }
            string s = sb.ToString().Trim('-');
            return string.IsNullOrEmpty(s) ? "sb2-mod" : s.ToLowerInvariant();
        }
    }
}
