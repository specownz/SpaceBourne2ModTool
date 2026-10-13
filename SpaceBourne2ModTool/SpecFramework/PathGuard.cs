using System;
using System.IO;

using SpaceBourne2ModTool.Services;

namespace SpecFramework
{
    /// <summary>
    /// Keeps file writes inside the mod folder or the game pak folder.
    /// </summary>
    public static class PathGuard
    {
        public static bool TryResolveUnderRoot(string root, string relativeOrAbsolute, out string fullPath, out string reason)
        {
            fullPath = "";
            reason = "";
            try
            {
                if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
                {
                    reason = "root does not exist";
                    return false;
                }
                string rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    + Path.DirectorySeparatorChar;

                string candidate = relativeOrAbsolute;
                if (!Path.IsPathRooted(candidate))
                    candidate = Path.Combine(root, relativeOrAbsolute);
                fullPath = Path.GetFullPath(candidate);

                if (fullPath.IndexOf('\0') >= 0)
                {
                    reason = "null byte in path";
                    return false;
                }

                string check = fullPath;
                if (!check.EndsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    // file path — compare directory or file under root
                }
                if (!fullPath.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(fullPath.TrimEnd('\\', '/'), rootFull.TrimEnd('\\', '/'), StringComparison.OrdinalIgnoreCase))
                {
                    reason = "path escapes allowed root";
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                reason = "path resolve failed: " + ex.GetType().Name;
                fullPath = "";
                return false;
            }
        }

        public static string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "unnamed";
            string leaf = Path.GetFileName(name.Trim());
            if (string.IsNullOrEmpty(leaf)) return "unnamed";
            foreach (char c in Path.GetInvalidFileNameChars())
                leaf = leaf.Replace(c, '_');
            leaf = leaf.Replace("..", "_");
            if (leaf == "." || leaf == ".." || leaf.Length == 0) return "unnamed";
            if (leaf.Length > 180) leaf = leaf.Substring(0, 180);
            return leaf;
        }

        public static bool IsProtectedVanillaPak(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return true;
            string n = Path.GetFileName(fileName);
            if (n.IndexOf("WindowsNoEditor", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (n.StartsWith("SB2-", StringComparison.OrdinalIgnoreCase) && n.IndexOf("_P", StringComparison.OrdinalIgnoreCase) < 0) return true;
            if (n.Equals("pakchunk0-WindowsNoEditor.pak", StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        /// <summary>True when the file is vanilla-core and Settings forbids editing it.</summary>
        public static bool IsVanillaEditBlocked(string fileName)
        {
            if (!IsProtectedVanillaPak(fileName)) return false;
            return !ConfigService.AllowVanillaCoreEdits;
        }

        public static bool ConfirmVanillaCoreEdit(System.Windows.Forms.IWin32Window owner, string context)
        {
            if (!ConfigService.AllowVanillaCoreEdits) return false;
            var r = System.Windows.Forms.MessageBox.Show(owner,
                "VANILLA CORE EDIT ENABLED\n\n" +
                "You are about to operate on base game files.\n" +
                (context ?? "") + "\n\n" +
                "This can permanently break the install. Backup first.\nContinue?",
                "Vanilla Core Warning",
                System.Windows.Forms.MessageBoxButtons.YesNo,
                System.Windows.Forms.MessageBoxIcon.Warning,
                System.Windows.Forms.MessageBoxDefaultButton.Button2);
            return r == System.Windows.Forms.DialogResult.Yes;
        }

        public static bool IsAllowedModPayloadExtension(string path)
        {
            string ext = Path.GetExtension(path) ?? "";
            switch (ext.ToLowerInvariant())
            {
                case ".pak":
                case ".uasset":
                case ".uexp":
                case ".ubulk":
                case ".mp4":
                case ".bank":
                case ".ini":
                case ".locres":
                case ".json":
                case ".txt":
                case ".md":
                    return true;
                default:
                    return false;
            }
        }
    }
}
