using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// Lists a folder with Win32 FindFirstFile. Access denied returns an empty
    /// list and does not throw, so first-load auto-detect does not spam
    /// UnauthorizedAccessException while it walks Program Files.
    /// </summary>
    internal static class SafeDirectory
    {
        const uint FILE_ATTRIBUTE_DIRECTORY = 0x10;
        const uint FILE_ATTRIBUTE_REPARSE_POINT = 0x400;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct WIN32_FIND_DATA
        {
            public uint dwFileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern IntPtr FindFirstFileW(string lpFileName, out WIN32_FIND_DATA data);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool FindNextFileW(IntPtr handle, out WIN32_FIND_DATA data);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FindClose(IntPtr handle);

        public static string[] GetDirectories(string dir)
        {
            List<string> dirs, files;
            TryList(dir, "*", out dirs, out files);
            return dirs.ToArray();
        }

        public static string[] GetFiles(string dir, string pattern)
        {
            List<string> dirs, files;
            TryList(dir, string.IsNullOrEmpty(pattern) ? "*" : pattern, out dirs, out files);
            return files.ToArray();
        }

        static void TryList(string dir, string pattern, out List<string> dirs, out List<string> files)
        {
            dirs = new List<string>();
            files = new List<string>();
            if (string.IsNullOrEmpty(dir)) return;
            string spec = Path.Combine(dir, "*");
            WIN32_FIND_DATA data;
            IntPtr handle = FindFirstFileW(spec, out data);
            if (handle == new IntPtr(-1)) return;
            try
            {
                do
                {
                    string name = data.cFileName;
                    if (string.IsNullOrEmpty(name) || name == "." || name == "..") continue;
                    if ((data.dwFileAttributes & FILE_ATTRIBUTE_REPARSE_POINT) != 0) continue;
                    string full = Path.Combine(dir, name);
                    if ((data.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) != 0)
                        dirs.Add(full);
                    else if (Matches(name, pattern))
                        files.Add(full);
                }
                while (FindNextFileW(handle, out data));
            }
            finally
            {
                FindClose(handle);
            }
        }

        static bool Matches(string name, string pattern)
        {
            if (pattern == "*" || pattern == "*.*") return true;
            if (pattern.StartsWith("*.", StringComparison.Ordinal))
                return name.EndsWith(pattern.Substring(1), StringComparison.OrdinalIgnoreCase);
            return name.Equals(pattern, StringComparison.OrdinalIgnoreCase);
        }
    }
}
