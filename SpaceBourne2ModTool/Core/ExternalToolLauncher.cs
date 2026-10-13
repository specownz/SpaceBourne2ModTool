using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// Launch configured external tools with sensible working directories / args for SB2.
    /// </summary>
    public static class ExternalToolLauncher
    {
        public static bool Launch(string key, IWin32Window owner = null)
        {
            string path = null;
            string args = "";
            string workDir = null;
            switch ((key ?? "").ToLowerInvariant())
            {
                case "unrealpak":
                    path = ConfigService.UnrealPakPath;
                    break;
                case "fmodel":
                    path = ConfigService.FModelPath;
                    break;
                case "umodel":
                    path = ConfigService.UModelPath;
                    break;
                case "uassetgui":
                    path = ConfigService.UAssetGuiPath;
                    break;
                case "codeeditor":
                case "code":
                    path = ConfigService.CodeEditorPath;
                    if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                        path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "notepad.exe");
                    break;
                case "imageeditor":
                case "image":
                    path = ConfigService.ImageEditorPath;
                    break;
                case "modeling":
                case "blender":
                    path = ConfigService.ModelingToolPath;
                    break;
                case "paks":
                    if (ConfigService.Game.IsValid && !string.IsNullOrEmpty(ConfigService.Game.PaksPath))
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = "explorer.exe",
                                Arguments = "\"" + ConfigService.Game.PaksPath + "\"",
                                UseShellExecute = true
                            });
                            return true;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(owner, ex.Message, "Open Paks", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return false;
                        }
                    }
                    MessageBox.Show(owner, "Set a valid game path first.", "Open Paks", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                default:
                    MessageBox.Show(owner, "Unknown tool: " + key, "Launch", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
            }

            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                MessageBox.Show(owner,
                    key + " is not configured or missing.\nRun Auto-Detect or set the path in Settings.",
                    "Launch " + key,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            try
            {
                workDir = Path.GetDirectoryName(path);
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    Arguments = args,
                    WorkingDirectory = workDir ?? "",
                    UseShellExecute = true
                });
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(owner, "Could not launch:\n" + ex.Message, "Launch", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
