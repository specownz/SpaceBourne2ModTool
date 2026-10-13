using System;

namespace SpaceBourne2ModTool.Core.GitHub
{
    public class GhUser
    {
        public string Login, Name, HtmlUrl, AvatarUrl;
        public int PublicRepos, TotalPrivateRepos;
        public override string ToString() { return Login ?? ""; }
    }

    public class GhRepo
    {
        public string Name, FullName, Description, HtmlUrl, CloneUrl, DefaultBranch;
        public bool Private, Fork;
        public string UpdatedAt, Language;
        public int Stars, Forks;
        public override string ToString()
        {
            return (Private ? "[private] " : "") + (FullName ?? Name) +
                   (string.IsNullOrEmpty(Description) ? "" : " — " + Description);
        }
    }

    public class GhRelease
    {
        public string TagName, Name, HtmlUrl, Body, PublishedAt;
        public bool Draft, Prerelease;
        public override string ToString() { return (TagName ?? "") + " " + (Name ?? ""); }
    }

    public class GhApiResult
    {
        public bool Ok;
        public string Error;
        public string Raw;
        public int StatusCode;
        public static GhApiResult Fail(string e, int code = 0) { return new GhApiResult { Ok = false, Error = e, StatusCode = code }; }
        public static GhApiResult Success(string raw, int code = 200) { return new GhApiResult { Ok = true, Raw = raw, StatusCode = code }; }
    }
}
