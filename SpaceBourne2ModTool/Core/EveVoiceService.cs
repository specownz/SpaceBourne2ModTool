using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// Drafts EvE ship-AI lines as WAV files. SpaceBourne 2 plays EvE from FMOD banks,
    /// so a WAV is the source line, not a drop-in replacement. specVoice is the same
    /// idea (installed Windows voices saved to WAV). Its program is launched if found.
    /// Its libraries are not copied into this tool.
    /// </summary>
    public static class EveVoiceService
    {
        public static string LinesFolder(string workingRoot)
        {
            string dir = Path.Combine(workingRoot, "EvE", "Generated");
            Directory.CreateDirectory(dir);
            return dir;
        }

        public static string FindSpecVoice()
        {
            if (!string.IsNullOrEmpty(ConfigService.SpecVoicePath) && File.Exists(ConfigService.SpecVoicePath))
                return ConfigService.SpecVoicePath;
            foreach (string root in new[]
            {
                @"C:\specVoice", @"C:\dev\specVoice",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "specVoice"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "specVoice")
            })
            {
                string exe = Path.Combine(root, "specVoice.exe");
                if (File.Exists(exe)) { ConfigService.SpecVoicePath = exe; ConfigService.Save(); return exe; }
            }
            var hits = ToolAutoDetector.SearchFilesNamed("specVoice.exe", new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                @"C:\dev", @"D:\dev", @"C:\Tools"
            }, 4, 1, null);
            if (hits.Count > 0)
            {
                ConfigService.SpecVoicePath = hits[0];
                ConfigService.Save();
                return hits[0];
            }
            return null;
        }

        public static void LaunchSpecVoice()
        {
            string exe = FindSpecVoice();
            if (exe == null) throw new FileNotFoundException("specVoice.exe was not found. Use Find on the EvE form and point at specVoice.exe.");
            Process.Start(exe);
        }

        public static List<string> InstalledVoices()
        {
            var names = new List<string>();
            try
            {
                object synth = CreateSynth();
                if (synth == null) return names;
                object voices = synth.GetType().GetMethod("GetInstalledVoices", Type.EmptyTypes).Invoke(synth, null);
                foreach (object voice in (System.Collections.IEnumerable)voices)
                {
                    object enabled = voice.GetType().GetProperty("Enabled").GetValue(voice, null);
                    if (enabled is bool && !(bool)enabled) continue;
                    object info = voice.GetType().GetProperty("VoiceInfo").GetValue(voice, null);
                    object name = info.GetType().GetProperty("Name").GetValue(info, null);
                    if (name != null) names.Add(name.ToString());
                }
                DisposeSynth(synth);
            }
            catch { }
            return names;
        }

        public static string SaveWav(string workingRoot, string lineName, string voiceName, string text, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(text)) { error = "Type the line EvE should say."; return null; }
            string safe = string.IsNullOrWhiteSpace(lineName) ? "EveLine" : lineName.Trim();
            foreach (char c in Path.GetInvalidFileNameChars()) safe = safe.Replace(c, '_');
            string path = Path.Combine(LinesFolder(workingRoot), safe + ".wav");
            try
            {
                object synth = CreateSynth();
                if (synth == null)
                {
                    error = "Windows speech is not available on this PC. Open specVoice, save a WAV, and put it in EvE\\Generated.";
                    return null;
                }
                if (!string.IsNullOrWhiteSpace(voiceName))
                {
                    try { synth.GetType().GetMethod("SelectVoice", new[] { typeof(string) }).Invoke(synth, new object[] { voiceName }); }
                    catch { }
                }
                synth.GetType().GetMethod("SetOutputToWaveFile", new[] { typeof(string) }).Invoke(synth, new object[] { path });
                synth.GetType().GetMethod("Speak", new[] { typeof(string) }).Invoke(synth, new object[] { text });
                try { synth.GetType().GetMethod("SetOutputToNull").Invoke(synth, null); } catch { }
                DisposeSynth(synth);
                File.WriteAllText(Path.ChangeExtension(path, ".txt"), text);
                return path;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        static object CreateSynth()
        {
            Assembly asm;
            try { asm = Assembly.Load("System.Speech, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"); }
            catch { try { asm = Assembly.Load("System.Speech"); } catch { return null; } }
            Type t = asm.GetType("System.Speech.Synthesis.SpeechSynthesizer");
            if (t == null) return null;
            return Activator.CreateInstance(t);
        }

        static void DisposeSynth(object synth)
        {
            var d = synth as IDisposable;
            if (d != null) d.Dispose();
        }
    }
}
