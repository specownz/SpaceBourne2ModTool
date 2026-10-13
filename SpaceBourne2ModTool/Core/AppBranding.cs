using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// Application branding for specSB2modTool — product name, logo, and window icons.
    /// </summary>
    public static class AppBranding
    {
        public const string ProductName = "specSB2modTool";
        public const string ProductTitle = "specSB2modTool";
        public const string Tagline = "a creation by spec";
        public const string Version = "2.1.0";
        public const string Description = "Unofficial Mod Creation Tool for SpaceBourne 2 (Unreal Engine 4.27)";

        private static Icon _appIcon;
        private static Image _logoImage;
        private static readonly object _lock = new object();

        public static Icon AppIcon
        {
            get
            {
                EnsureLoaded();
                return _appIcon;
            }
        }

        public static Image LogoImage
        {
            get
            {
                EnsureLoaded();
                return _logoImage;
            }
        }

        private static void EnsureLoaded()
        {
            if (_appIcon != null && _logoImage != null) return;
            lock (_lock)
            {
                if (_appIcon != null && _logoImage != null) return;
                try
                {
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    string[] icoCandidates =
                    {
                        Path.Combine(baseDir, "Resources", "app.ico"),
                        Path.Combine(baseDir, "app.ico"),
                        Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? baseDir, "Resources", "app.ico"),
                    };
                    foreach (string p in icoCandidates)
                    {
                        if (File.Exists(p))
                        {
                            _appIcon = new Icon(p);
                            break;
                        }
                    }
                    // Embedded resource fallback
                    if (_appIcon == null)
                    {
                        var asm = Assembly.GetExecutingAssembly();
                        foreach (string name in asm.GetManifestResourceNames())
                        {
                            if (name.EndsWith("app.ico", StringComparison.OrdinalIgnoreCase))
                            {
                                using (var s = asm.GetManifestResourceStream(name))
                                {
                                    if (s != null) _appIcon = new Icon(s);
                                }
                                break;
                            }
                        }
                    }
                }
                catch { }

                try
                {
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    foreach (string name in new[] { "spec_128.png", "spec.png" })
                    {
                        string p = Path.Combine(baseDir, "Resources", name);
                        if (File.Exists(p))
                        {
                            _logoImage = Image.FromFile(p);
                            break;
                        }
                    }
                    if (_logoImage == null)
                    {
                        var asm = Assembly.GetExecutingAssembly();
                        foreach (string res in asm.GetManifestResourceNames())
                        {
                            if (res.EndsWith("spec_128.png", StringComparison.OrdinalIgnoreCase)
                                || res.EndsWith("spec.png", StringComparison.OrdinalIgnoreCase))
                            {
                                using (var s = asm.GetManifestResourceStream(res))
                                {
                                    if (s != null) _logoImage = Image.FromStream(s);
                                }
                                break;
                            }
                        }
                    }
                }
                catch { }
            }
        }

        /// <summary>Apply product icon and standard title suffix to any form.</summary>
        public static void Apply(Form form, string windowTitle = null)
        {
            if (form == null) return;
            try
            {
                if (AppIcon != null)
                    form.Icon = AppIcon;
            }
            catch { }
            if (!string.IsNullOrEmpty(windowTitle))
                form.Text = windowTitle;
            else if (string.IsNullOrEmpty(form.Text) || form.Text.IndexOf("SpaceBourne", StringComparison.OrdinalIgnoreCase) >= 0)
                form.Text = ProductTitle;
            try { FormScaleHelper.Enable(form); } catch { }
            try { UiTheme.Apply(form); } catch { }
        }

        public static string AboutBodyText()
        {
            return
                ProductTitle + "  v" + Version + "\r\n" +
                Description + "\r\n\r\n" +
                "Game: SpaceBourne 2 (Steam AppID 1646850)\r\n" +
                "Engine: Unreal Engine 4.27\r\n" +
                "Modding: Patch-pack (_P.pak), loose Movies/FMOD, cooked asset overrides\r\n\r\n" +
                "Features include:\r\n" +
                "• Game File Analyzer with depth-3 content maps\r\n" +
                "• Content Browser presets (Majorcharacters, NPCs, armor sets)\r\n" +
                "• Loose File Mods with automatic backups\r\n" +
                "• PAK packaging helpers (UnrealPak 4.27)\r\n" +
                "• Working folder for ripped pak files, with a rip ledger so unchanged files are not read again\r\n" +
                "• EvE voice drafts saved as WAV (same idea as specVoice). specVoice is not included.\r\n" +
                "  The game still speaks EvE from FMOD banks, so a rebuilt bank is what ships.\r\n" +
                "• In-app package, locres, and pak header checks (public UE 4.27 layout;\r\n" +
                "  FModel and UAssetGUI are not embedded — they remain optional external tools)\r\n" +
                "• Auto-detect for Steam install and external tools\r\n" +
                "• Localization & trainer template helpers\r\n\r\n" +
                "Built with assistance from Grok (xAI) — design, UE4 modding research,\r\n" +
                "analyzer architecture, and iterative feature development.\r\n\r\n" +
                "This is unofficial community software and is not affiliated with\r\n" +
                "the SpaceBourne 2 developers.";
        }
    }
}
