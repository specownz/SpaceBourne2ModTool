using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SpaceBourne2ModTool.Core;
using SpaceBourne2ModTool.Models;
using SpaceBourne2ModTool.Services;

namespace SpecFramework
{
    /// <summary>Scaffold, stage, package, then inject in load order.</summary>
    public static class SpecPipeline
    {
        public class StepResult
        {
            public bool Ok;
            public string Message;
            public string Path;
        }

        /// <summary>UE 4.27 cooked asset pipeline (in-app checklist for SB2).</summary>
        public static string AssetPipelineGuide()
        {
            var sb = new StringBuilder();
            sb.AppendLine("UE 4.27 asset pipeline (SpaceBourne 2)");
            sb.AppendLine("=====================================");
            sb.AppendLine("1. DISCOVER  — Game Analyzer (UnrealPak -List) → SB2_*_LATEST reports");
            sb.AppendLine("2. PLAN      — Content Browser / SpecVert / Mod Templates / Steam Studio");
            sb.AppendLine("3. EXPORT    — FModel (4.27) for packages; UModel for meshes; PNG for textures");
            sb.AppendLine("4. EDIT      — UAssetGUI 4.27 for properties; Image/Script editors in-app for text/art notes");
            sb.AppendLine("5. STAGE     — SB2/Content/... mirroring game paths; keep .uasset+.uexp(+.ubulk)");
            sb.AppendLine("6. PACK      — UnrealPak 4.27 -Create=response.txt -compress → z_<order>_name_P.pak");
            sb.AppendLine("   or Speckage (.speckage) for shareable offline packages");
            sb.AppendLine("7. INJECT    — Mod Loader ordered inject (PathGuard blocks vanilla unless enabled)");
            sb.AppendLine("8. VERIFY    — Launch game; Unload Managed restores if needed");
            sb.AppendLine();
            sb.AppendLine("Mount: ../../../   Compression: Oodle via Engine 4.27 UnrealPak");
            sb.AppendLine("Loose path: Content/Movies and Content/FMOD can skip packing");
            return sb.ToString();
        }

        public static StepResult QuickModFromTemplate(string templateId, string modName)
        {
            try
            {
                var t = FindTemplate(templateId);
                if (t == null) return Fail("Template not found: " + templateId);
                string root = ModTemplateService.CreateModFromTemplate(t, modName);
                return new StepResult { Ok = true, Message = "Mod created from template " + t.Name, Path = root };
            }
            catch (Exception ex) { return Fail(ex.Message); }
        }

        public static StepResult PackSpeckage(string modRoot, string outputPath, int loadOrder)
        {
            try
            {
                if (string.IsNullOrEmpty(modRoot) || !Directory.Exists(modRoot))
                    return Fail("Mod root missing");
                string staging = FindStaging(modRoot);
                if (staging == null) return Fail("No *_P staging folder");
                var man = new SpeckageManifest
                {
                    Name = Path.GetFileName(modRoot),
                    Version = "1.0",
                    Author = "spec",
                    DefaultLoadOrder = loadOrder,
                    Notes = "Packed via SpecPipeline"
                };
                string outp = string.IsNullOrEmpty(outputPath)
                    ? Path.Combine(modRoot, man.Name + ".speckage")
                    : outputPath;
                SpeckagePack.CreateFromFolder(staging, outp, man);
                return new StepResult { Ok = true, Message = "Speckage created", Path = outp };
            }
            catch (Exception ex) { return Fail(ex.Message); }
        }

        public static StepResult InjectAndOptionalLaunch(bool launch)
        {
            try
            {
                var svc = new ModLoaderService();
                string backup;
                var deployed = svc.InjectEnabledMods(out backup);
                string msg = "Injected " + deployed.Count + " file(s). Backup: " + backup;
                if (launch) { string msg2; svc.StartGame(out msg2); }
                return new StepResult { Ok = true, Message = msg, Path = backup };
            }
            catch (Exception ex) { return Fail(ex.Message); }
        }

        public static string StreamlineChecklist()
        {
            var sb = new StringBuilder();
            sb.AppendLine("SpecPipeline streamline checklist");
            sb.AppendLine("- [ ] Analyzer LATEST reports present");
            sb.AppendLine("- [ ] Mod from template or Steam Studio");
            sb.AppendLine("- [ ] Stage cooked trios / loose media");
            sb.AppendLine("- [ ] Speckage or UnrealPak z_*_P.pak");
            sb.AppendLine("- [ ] Mod Loader enable + order + inject");
            sb.AppendLine("- [ ] PathGuard: vanilla edits " + (ConfigService.AllowVanillaCoreEdits ? "ALLOWED" : "blocked"));
            sb.AppendLine();
            sb.Append(AssetPipelineGuide());
            return sb.ToString();
        }

        static ModTemplateService.Template FindTemplate(string id)
        {
            foreach (var t in ModTemplateService.All())
                if (string.Equals(t.Id, id, StringComparison.OrdinalIgnoreCase)) return t;
            return null;
        }

        static string FindStaging(string modRoot)
        {
            foreach (string d in Directory.GetDirectories(modRoot))
                if (d.EndsWith("_P", StringComparison.OrdinalIgnoreCase)) return d;
            return null;
        }

        static StepResult Fail(string m) { return new StepResult { Ok = false, Message = m }; }
    }
}
