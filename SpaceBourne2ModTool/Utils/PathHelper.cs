using System;
using System.IO;

namespace SpaceBourne2ModTool.Utils
{
    public static class PathHelper
    {
        public static string Normalize(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return path;
            return Path.GetFullPath(path.Trim().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        }

        public static bool IsSubPathOf(string path, string baseDir)
        {
            path = Normalize(path);
            baseDir = Normalize(baseDir);
            return path.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase);
        }

        public static string MakeRelative(string fullPath, string baseDir)
        {
            fullPath = Normalize(fullPath);
            baseDir = Normalize(baseDir);
            if (!fullPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
                return fullPath;
            return fullPath.Substring(baseDir.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }
}
