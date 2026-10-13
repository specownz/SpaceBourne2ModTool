using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using SpaceBourne2ModTool.Models;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// UE 4.27 PAK packaging for SpaceBourne 2, based on current community + Epic practices.
    ///
    /// Key facts for UE 4.27 / SpaceBourne 2:
    /// - Game has NO AES encryption → plain UnrealPak works.
    /// - Standard mount point relative to the shipping executable is "../../../"
    ///   (i.e. project root). Paths inside the pak therefore look like:
    ///     SB2/Content/...
    /// - Patch / override paks MUST end with "_P" (or "_p"). The engine gives higher
    ///   priority to files whose names end with _P so they replace vanilla assets.
    /// - Naming convention used by the tool:
    ///     pakchunk{ChunkId}-{ModName}_P.pak
    ///   Higher ChunkId / later alphabetical order loads after vanilla.
    /// Real install (analyzer 2026-09-26):
    /// - Single monolithic SB2-WindowsNoEditor.pak (~11 GB), mount ../../../
    /// - Paths inside: Engine/Content/..., SB2/Content/...
    /// - Compression: Oodle on many assets (pack with UE 4.27 UnrealPak -compress)
    /// - Mod names: z_pakchunkN-Name_P.pak so they sort after the main pak + patch priority
    /// - Preferred creation method: response file listing
    ///     "absolute\source\file" "../../../SB2/Content/relative/path"
    ///   then: UnrealPak.exe output.pak -Create=response.txt -compress
    /// - Alternative tools: repak (faster, version-aware), u4pak.
    /// - IoStore (.utoc/.ucas) is rare on pure 4.27 titles that still ship classic paks;
    ///   SpaceBourne 2 uses classic .pak files.
    /// </summary>
    public static class PakHelper
    {
        /// <summary>Default mount point used by most UE4 shipping builds (relative to Binaries/Win64).</summary>
        public const string DefaultMountPoint = "../../../";

        /// <summary>Game content root as it appears inside the pak for SpaceBourne 2.</summary>
        public const string GameContentPrefix = "SB2/Content/";

        public static bool CreatePak(ModProject project, out string outputPakPath, out string message)
        {
            return CreatePak(project, compress: true, out outputPakPath, out message);
        }

        public static bool CreatePak(ModProject project, bool compress, out string outputPakPath, out string message)
        {
            outputPakPath = null;
            message = "";

            if (project == null)
            {
                message = "No mod project provided.";
                return false;
            }

            string staging = project.StagingRoot;
            if (!Directory.Exists(staging))
            {
                message = "Staging folder does not exist. Create the mod structure first.";
                return false;
            }

            string contentRoot = Path.Combine(staging, "SB2", "Content");
            if (!Directory.Exists(contentRoot))
            {
                // Allow loose layout where user put files directly under staging
                contentRoot = staging;
            }

            string outputDir = Path.Combine(project.ProjectRoot, "Output");
            FileService.EnsureDirectory(outputDir);
            string pakName = project.Info.GetPakFileName();
            outputPakPath = Path.Combine(outputDir, pakName);

            // Always write a response file + batch so the user can re-run offline
            string responseFile = Path.Combine(outputDir, "paklist.txt");
            int fileCount = CreateResponseFile(contentRoot, responseFile);
            if (fileCount == 0)
            {
                message = "No files found under the staging Content folder. Add assets first.";
                return false;
            }

            string batPath = Path.Combine(outputDir, "CreatePak.bat");
            GenerateUnrealPakBatch(responseFile, outputPakPath, batPath, compress);

            // Also write a human-readable verification script
            WriteVerifyScript(outputDir, outputPakPath);

            string unrealPak = ConfigService.UnrealPakPath;
            if (string.IsNullOrEmpty(unrealPak) || !File.Exists(unrealPak))
            {
                message =
                    "UnrealPak.exe not configured.\n" +
                    $"Generated:\n  • {responseFile} ({fileCount} entries)\n  • {batPath}\n\n" +
                    "1. Install UE 4.27 (or extract UnrealPak from it).\n" +
                    "2. Set the path in Settings → UnrealPak.exe\n" +
                    "   (usually Engine\\Binaries\\Win64\\UnrealPak.exe)\n" +
                    "3. Or edit CreatePak.bat and run it manually.\n\n" +
                    "Command that will be used:\n" +
                    $"  UnrealPak.exe \"{pakName}\" -Create=\"paklist.txt\"" +
                    (compress ? " -compress" : "");
                LoggerService.LogWarning(message);
                return false;
            }

            try
            {
                string args = $"\"{outputPakPath}\" -Create=\"{responseFile}\"";
                if (compress) args += " -compress";

                var psi = new ProcessStartInfo
                {
                    FileName = unrealPak,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = outputDir
                };

                using (var proc = Process.Start(psi))
                {
                    if (proc == null)
                    {
                        message = "Failed to start UnrealPak process.";
                        return false;
                    }

                    string stdout = proc.StandardOutput.ReadToEnd();
                    string stderr = proc.StandardError.ReadToEnd();
                    bool exited = proc.WaitForExit(180000); // 3 min

                    if (!exited)
                    {
                        try { proc.Kill(); } catch { }
                        message = "UnrealPak timed out after 3 minutes.";
                        LoggerService.LogError(message);
                        return false;
                    }

                    if (proc.ExitCode == 0 && File.Exists(outputPakPath))
                    {
                        long size = new FileInfo(outputPakPath).Length;
                        message =
                            $"Successfully created {pakName}\n" +
                            $"  Size: {FileService.FormatSize(size)}\n" +
                            $"  Files: {fileCount}\n" +
                            $"  Compression: {(compress ? "enabled" : "off")}\n" +
                            $"  Mount point: {DefaultMountPoint}\n" +
                            $"  Install to: SB2\\Content\\Paks\\";
                        LoggerService.Log(message);
                        return true;
                    }

                    message =
                        $"UnrealPak failed (exit code {proc.ExitCode}).\n\n" +
                        "Stdout:\n" + Truncate(stdout, 800) + "\n\n" +
                        "Stderr:\n" + Truncate(stderr, 800) + "\n\n" +
                        "Tip: verify paths in paklist.txt use forward slashes for the mount side " +
                        "and that every source file exists.";
                    LoggerService.LogError(message);
                    return false;
                }
            }
            catch (Exception ex)
            {
                message = "Exception while creating PAK: " + ex.Message;
                LoggerService.LogError(message, ex);
                return false;
            }
        }

        /// <summary>
        /// Builds the response file that UnrealPak expects.
        /// Each line: "C:\absolute\source.uasset" "../../../SB2/Content/relative/path.uasset"
        /// </summary>
        public static int CreateResponseFile(string contentRoot, string responseFilePath)
        {
            var sb = new StringBuilder();
            int count = 0;

            if (!Directory.Exists(contentRoot))
            {
                File.WriteAllText(responseFilePath, "", Encoding.UTF8);
                return 0;
            }

            foreach (string file in Directory.GetFiles(contentRoot, "*", SearchOption.AllDirectories))
            {
                // Skip junk
                string name = Path.GetFileName(file);
                if (name.EndsWith(".bak", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("Thumbs.db", StringComparison.OrdinalIgnoreCase) ||
                    name.StartsWith(".", StringComparison.Ordinal))
                    continue;

                string rel = file.Substring(contentRoot.Length)
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Replace('\\', '/');

                // Mount path as the game expects it
                string mountPath = DefaultMountPoint + GameContentPrefix + rel;

                // UnrealPak response format: "source" "mount"
                sb.Append('"').Append(file).Append("\" \"").Append(mountPath).Append('"').AppendLine();
                count++;
            }

            File.WriteAllText(responseFilePath, sb.ToString(), Encoding.UTF8);
            LoggerService.Log($"Wrote response file with {count} entries → {responseFilePath}");
            return count;
        }

        private static void GenerateUnrealPakBatch(string responseFile, string outputPak, string batPath, bool compress)
        {
            var sb = new StringBuilder();
            sb.AppendLine("@echo off");
            sb.AppendLine("REM ============================================================");
            sb.AppendLine("REM Auto-generated by SpaceBourne 2 Mod Creation Tool");
            sb.AppendLine("REM UE 4.27 UnrealPak – correct response-file + mount point");
            sb.AppendLine("REM ============================================================");
            sb.AppendLine();
            sb.AppendLine("REM Edit the next line if UnrealPak is not on PATH");
            sb.AppendLine("set UNREALPAK=UnrealPak.exe");
            sb.AppendLine();
            sb.AppendLine("if not exist \"%UNREALPAK%\" (");
            sb.AppendLine("  echo.");
            sb.AppendLine("  echo ERROR: UnrealPak.exe not found.");
            sb.AppendLine("  echo Download UE 4.27 or extract UnrealPak from Engine\\Binaries\\Win64");
            sb.AppendLine("  echo then set UNREALPAK= to the full path, e.g.");
            sb.AppendLine("  echo   set UNREALPAK=C:\\UE_4.27\\Engine\\Binaries\\Win64\\UnrealPak.exe");
            sb.AppendLine("  echo.");
            sb.AppendLine("  pause");
            sb.AppendLine("  exit /b 1");
            sb.AppendLine(")");
            sb.AppendLine();
            sb.AppendLine("echo Creating PAK with correct mount point ../../../SB2/Content/ ...");
            sb.AppendLine($"\"%UNREALPAK%\" \"{outputPak}\" -Create=\"{responseFile}\"{(compress ? " -compress" : "")}");
            sb.AppendLine("if %ERRORLEVEL% EQU 0 (");
            sb.AppendLine("  echo.");
            sb.AppendLine("  echo SUCCESS. Copy the .pak into:");
            sb.AppendLine("  echo   ...\\SpaceBourne 2\\SB2\\Content\\Paks\\");
            sb.AppendLine("  echo Filename MUST end with _P.pak for override priority.");
            sb.AppendLine(") else (");
            sb.AppendLine("  echo FAILED – check paths in paklist.txt");
            sb.AppendLine(")");
            sb.AppendLine("pause");
            File.WriteAllText(batPath, sb.ToString(), Encoding.ASCII);
        }

        private static void WriteVerifyScript(string outputDir, string pakPath)
        {
            // Helper the user can run to inspect mount point after packing
            string verifyBat = Path.Combine(outputDir, "VerifyPak.bat");
            var sb = new StringBuilder();
            sb.AppendLine("@echo off");
            sb.AppendLine("REM Lists contents + mount point of the generated pak");
            sb.AppendLine("set UNREALPAK=UnrealPak.exe");
            sb.AppendLine($"\"%UNREALPAK%\" \"{pakPath}\" -List");
            sb.AppendLine("pause");
            File.WriteAllText(verifyBat, sb.ToString(), Encoding.ASCII);
        }

        public static string GetSuggestedInstallPath(GameConfig game)
        {
            if (game == null || !game.IsValid) return "";
            return game.PaksPath;
        }

        /// <summary>
        /// Returns a short checklist the UI can show the user.
        /// </summary>
        public static string GetPackagingChecklist()
        {
            return
                "UE 4.27 / SpaceBourne 2 packaging checklist\n" +
                "─────────────────────────────────────────\n" +
                "1. Staging layout must be:\n" +
                "     {ModName}_P\\SB2\\Content\\...  (mirrors game)\n" +
                "2. Only include files you changed or added.\n" +
                "3. Response file uses mount point ../../../SB2/Content/...\n" +
                "4. Output filename MUST end with _P.pak\n" +
                "5. Prefer pakchunk ID ≥ 1000 so it loads after vanilla.\n" +
                "6. Drop the .pak into SB2\\Content\\Paks\\\n" +
                "7. Verify with: UnrealPak.exe your.pak -List\n" +
                "   Mount point should be ../../../\n" +
                "8. Game has no AES key – no encryption needed.";
        }

        private static string Truncate(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return "";
            s = s.Trim();
            return s.Length <= max ? s : s.Substring(0, max) + "…";
        }
    }
}
