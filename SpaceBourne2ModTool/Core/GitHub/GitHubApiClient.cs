using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Core.GitHub
{
    /// <summary>
    /// GitHub REST API v3 client for mod publish/import. Uses a Personal Access Token (PAT).
    /// </summary>
    public static class GitHubApiClient
    {
        const string Api = "https://api.github.com";
        const string Ua = "specSB2modTool";

        public static string Token
        {
            get { return ConfigService.GitHubToken; }
            set { ConfigService.GitHubToken = value ?? ""; }
        }

        public static bool HasToken { get { return !string.IsNullOrWhiteSpace(Token); } }

        static HttpClient CreateClient()
        {
            var c = new HttpClient();
            c.Timeout = TimeSpan.FromMinutes(5);
            c.DefaultRequestHeaders.UserAgent.ParseAdd(Ua);
            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            c.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
            if (HasToken)
                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token.Trim());
            return c;
        }

        public static GhApiResult Request(string method, string pathOrUrl, string jsonBody = null)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                using (var client = CreateClient())
                {
                    string url = pathOrUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                        ? pathOrUrl : (Api + pathOrUrl);
                    HttpResponseMessage resp;
                    if (string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase))
                        resp = client.GetAsync(url).GetAwaiter().GetResult();
                    else if (string.Equals(method, "POST", StringComparison.OrdinalIgnoreCase))
                    {
                        var content = new StringContent(jsonBody ?? "{}", Encoding.UTF8, "application/json");
                        resp = client.PostAsync(url, content).GetAwaiter().GetResult();
                    }
                    else if (string.Equals(method, "PATCH", StringComparison.OrdinalIgnoreCase))
                    {
                        var req = new HttpRequestMessage(new HttpMethod("PATCH"), url)
                        {
                            Content = new StringContent(jsonBody ?? "{}", Encoding.UTF8, "application/json")
                        };
                        resp = client.SendAsync(req).GetAwaiter().GetResult();
                    }
                    else if (string.Equals(method, "DELETE", StringComparison.OrdinalIgnoreCase))
                        resp = client.DeleteAsync(url).GetAwaiter().GetResult();
                    else
                        return GhApiResult.Fail("Unsupported method " + method);

                    string body = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    if (!resp.IsSuccessStatusCode)
                        return GhApiResult.Fail(ParseError(body) ?? resp.ReasonPhrase + " (" + (int)resp.StatusCode + ")", (int)resp.StatusCode);
                    return GhApiResult.Success(body, (int)resp.StatusCode);
                }
            }
            catch (Exception ex)
            {
                return GhApiResult.Fail(ex.Message);
            }
        }

        static string ParseError(string body)
        {
            if (string.IsNullOrEmpty(body)) return null;
            string m = JsonLite.GetString(body, "message");
            return m;
        }

        public static GhApiResult GetUser()
        {
            return Request("GET", "/user");
        }

        public static GhUser ParseUser(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            return new GhUser
            {
                Login = JsonLite.GetString(json, "login"),
                Name = JsonLite.GetString(json, "name"),
                HtmlUrl = JsonLite.GetString(json, "html_url"),
                AvatarUrl = JsonLite.GetString(json, "avatar_url")
            };
        }

        public static List<GhRepo> ListUserRepos(string username = null, int perPage = 50)
        {
            string path = string.IsNullOrEmpty(username)
                ? "/user/repos?sort=updated&per_page=" + perPage
                : "/users/" + Uri.EscapeDataString(username) + "/repos?sort=updated&per_page=" + perPage;
            var r = Request("GET", path);
            if (!r.Ok) return new List<GhRepo>();
            return ParseRepoArray(r.Raw);
        }

        public static List<GhRepo> SearchRepos(string query, int perPage = 30)
        {
            if (string.IsNullOrWhiteSpace(query)) query = "spacebourne mod";
            string path = "/search/repositories?q=" + Uri.EscapeDataString(query) + "&sort=updated&per_page=" + perPage;
            var r = Request("GET", path);
            if (!r.Ok) return new List<GhRepo>();
            // items array
            int items = r.Raw.IndexOf("\"items\"", StringComparison.OrdinalIgnoreCase);
            if (items < 0) return ParseRepoArray(r.Raw);
            return ParseRepoArray(r.Raw.Substring(items));
        }

        public static GhApiResult CreateRepo(string name, string description, bool isPrivate)
        {
            string json = "{"
                + "\"name\":\"" + JsonLite.Esc(name) + "\","
                + "\"description\":\"" + JsonLite.Esc(description ?? "") + "\","
                + "\"private\":" + (isPrivate ? "true" : "false") + ","
                + "\"auto_init\":false"
                + "}";
            return Request("POST", "/user/repos", json);
        }

        public static GhRepo ParseRepo(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            return new GhRepo
            {
                Name = JsonLite.GetString(json, "name"),
                FullName = JsonLite.GetString(json, "full_name"),
                Description = JsonLite.GetString(json, "description"),
                HtmlUrl = JsonLite.GetString(json, "html_url"),
                CloneUrl = JsonLite.GetString(json, "clone_url"),
                DefaultBranch = JsonLite.GetString(json, "default_branch") ?? "main",
                Private = json.IndexOf("\"private\":true", StringComparison.OrdinalIgnoreCase) >= 0
                       || json.IndexOf("\"private\": true", StringComparison.OrdinalIgnoreCase) >= 0,
                Fork = json.IndexOf("\"fork\":true", StringComparison.OrdinalIgnoreCase) >= 0,
                UpdatedAt = JsonLite.GetString(json, "updated_at"),
                Language = JsonLite.GetString(json, "language")
            };
        }

        public static List<GhRepo> ParseRepoArray(string json)
        {
            var list = new List<GhRepo>();
            if (string.IsNullOrEmpty(json)) return list;
            // Split on full_name occurrences for robust-enough listing
            var matches = Regex.Matches(json, "\"full_name\"\\s*:\\s*\"([^\"]+)\"");
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (Match m in matches)
            {
                string full = m.Groups[1].Value;
                if (!seen.Add(full)) continue;
                // Find object slice roughly
                int start = json.LastIndexOf('{', m.Index);
                int end = json.IndexOf("\"full_name\"", m.Index + 10, StringComparison.Ordinal);
                if (end < 0) end = Math.Min(json.Length, m.Index + 800);
                else end = Math.Min(json.Length, end);
                string slice = start >= 0 ? json.Substring(start, Math.Min(1200, json.Length - start)) : m.Value;
                var repo = ParseRepo(slice);
                if (repo != null && !string.IsNullOrEmpty(repo.FullName))
                {
                    if (string.IsNullOrEmpty(repo.Name) && full.Contains("/"))
                        repo.Name = full.Substring(full.IndexOf('/') + 1);
                    if (string.IsNullOrEmpty(repo.CloneUrl))
                        repo.CloneUrl = "https://github.com/" + full + ".git";
                    if (string.IsNullOrEmpty(repo.HtmlUrl))
                        repo.HtmlUrl = "https://github.com/" + full;
                    list.Add(repo);
                }
            }
            return list;
        }

        public static GhApiResult GetRepo(string owner, string repo)
        {
            return Request("GET", "/repos/" + Uri.EscapeDataString(owner) + "/" + Uri.EscapeDataString(repo));
        }

        public static List<GhRelease> ListReleases(string owner, string repo)
        {
            var r = Request("GET", "/repos/" + Uri.EscapeDataString(owner) + "/" + Uri.EscapeDataString(repo) + "/releases?per_page=20");
            var list = new List<GhRelease>();
            if (!r.Ok || string.IsNullOrEmpty(r.Raw)) return list;
            foreach (Match m in Regex.Matches(r.Raw, "\"tag_name\"\\s*:\\s*\"([^\"]+)\""))
            {
                int start = r.Raw.LastIndexOf('{', m.Index);
                string slice = start >= 0 ? r.Raw.Substring(start, Math.Min(900, r.Raw.Length - start)) : "";
                list.Add(new GhRelease
                {
                    TagName = m.Groups[1].Value,
                    Name = JsonLite.GetString(slice, "name"),
                    HtmlUrl = JsonLite.GetString(slice, "html_url"),
                    Body = JsonLite.GetString(slice, "body"),
                    PublishedAt = JsonLite.GetString(slice, "published_at"),
                    Draft = slice.IndexOf("\"draft\":true", StringComparison.OrdinalIgnoreCase) >= 0,
                    Prerelease = slice.IndexOf("\"prerelease\":true", StringComparison.OrdinalIgnoreCase) >= 0
                });
            }
            return list;
        }

        /// <summary>Download repository zipball (default branch) without git.</summary>
        public static GhApiResult DownloadZipball(string owner, string repo, string branch, string destZipPath)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                if (string.IsNullOrEmpty(branch)) branch = "main";
                string url = Api + "/repos/" + Uri.EscapeDataString(owner) + "/" + Uri.EscapeDataString(repo)
                    + "/zipball/" + Uri.EscapeDataString(branch);
                using (var client = CreateClient())
                {
                    var bytes = client.GetByteArrayAsync(url).GetAwaiter().GetResult();
                    Directory.CreateDirectory(Path.GetDirectoryName(destZipPath) ?? ".");
                    File.WriteAllBytes(destZipPath, bytes);
                    return GhApiResult.Success(destZipPath);
                }
            }
            catch (Exception ex)
            {
                // try master
                if (!string.Equals(branch, "master", StringComparison.OrdinalIgnoreCase))
                    return DownloadZipball(owner, repo, "master", destZipPath);
                return GhApiResult.Fail(ex.Message);
            }
        }
    }
}
