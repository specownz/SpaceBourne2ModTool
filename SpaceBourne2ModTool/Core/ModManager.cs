using System;
using System.Collections.Generic;
using System.IO;
using SpaceBourne2ModTool.Models;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// High-level orchestrator for creating, listing and packaging mods.
    /// </summary>
    public class ModManager
    {
        public string OutputRoot { get; private set; }

        public ModManager(string outputRoot = null)
        {
            OutputRoot = outputRoot ?? ConfigService.ModsOutputPath;
            FileService.EnsureDirectory(OutputRoot);
        }

        public ModProject CreateNewMod(ModInfo info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            var project = new ModProject(info, OutputRoot);
            project.CreateStructure();
            ConfigService.LastModName = info.Name;
            return project;
        }

        public List<string> ListExistingProjects()
        {
            var list = new List<string>();
            if (!Directory.Exists(OutputRoot)) return list;
            foreach (var dir in Directory.GetDirectories(OutputRoot))
            {
                if (File.Exists(Path.Combine(dir, "modinfo.json")))
                    list.Add(Path.GetFileName(dir));
            }
            return list;
        }

        public string GetProjectPath(string modName)
        {
            return Path.Combine(OutputRoot, modName);
        }
    }
}
