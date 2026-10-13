using System;
using System.Configuration;
using System.IO;
using SpaceBourne2ModTool.Models;

namespace SpaceBourne2ModTool.Services
{
    public static class ConfigService
    {
        private static GameConfig _gameConfig = new GameConfig();

        public static GameConfig Game => _gameConfig;

        public static string ModsOutputPath
        {
            get => Get("ModsOutputPath", Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "SpaceBourne2Mods"));
            set => Set("ModsOutputPath", value);
        }

        /// <summary>Ripped assets live here. Never the Steam game folder.</summary>
        public static string WorkingProjectPath
        {
            get => Get("WorkingProjectPath", Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "specSB2modTool", "WorkingProject"));
            set => Set("WorkingProjectPath", value);
        }

        public static string LastModName
        {
            get => Get("LastModName", "MyFirstMod");
            set => Set("LastModName", value);
        }

        public static string UnrealPakPath
        {
            get => Get("UnrealPakPath", "");
            set => Set("UnrealPakPath", value);
        }

        public static string FModelPath
        {
            get => Get("FModelPath", "");
            set => Set("FModelPath", value);
        }

        public static string UModelPath
        {
            get => Get("UModelPath", "");
            set => Set("UModelPath", value);
        }

        public static string UAssetGuiPath
        {
            get => Get("UAssetGuiPath", "");
            set => Set("UAssetGuiPath", value);
        }

        public static string CodeEditorPath
        {
            get => Get("CodeEditorPath", "");
            set => Set("CodeEditorPath", value);
        }

        public static string ImageEditorPath
        {
            get => Get("ImageEditorPath", "");
            set => Set("ImageEditorPath", value);
        }

        public static string ModelingToolPath
        {
            get => Get("ModelingToolPath", "");
            set => Set("ModelingToolPath", value);
        }

        public static string SpecVoicePath
        {
            get => Get("SpecVoicePath", "");
            set => Set("SpecVoicePath", value ?? "");
        }

        /// <summary>When true, auto-detect prefers paid apps (Sublime, Photoshop, Maya…) over free (VS Code, GIMP, Blender).</summary>
        public static bool PreferPaidTools
        {
            get => !string.Equals(Get("PreferPaidTools", "true"), "false", StringComparison.OrdinalIgnoreCase);
            set => Set("PreferPaidTools", value ? "true" : "false");
        }

        /// <summary>When true, PathGuard/ModLoader may target vanilla base PAKs and core paths. Default false. Extremely dangerous.</summary>
        public static bool AllowVanillaCoreEdits
        {
            get => string.Equals(Get("AllowVanillaCoreEdits", "false"), "true", StringComparison.OrdinalIgnoreCase);
            set => Set("AllowVanillaCoreEdits", value ? "true" : "false");
        }

        public static bool FirstRunAutoDetectDone
        {
            get => string.Equals(Get("FirstRunAutoDetectDone", "false"), "true", StringComparison.OrdinalIgnoreCase);
            set => Set("FirstRunAutoDetectDone", value ? "true" : "false");
        }

        public static string GitHubToken
        {
            get => Get("GitHubToken", "");
            set => Set("GitHubToken", value ?? "");
        }

        public static string GitHubUsername
        {
            get => Get("GitHubUsername", "");
            set => Set("GitHubUsername", value ?? "");
        }

        public static string RecentModsJson
        {
            get => Get("RecentModsJson", "[]");
            set => Set("RecentModsJson", value ?? "[]");
        }

        public static int DefaultChunkId
        {
            get
            {
                int.TryParse(Get("DefaultChunkId", "1000"), out int v);
                return v;
            }
            set => Set("DefaultChunkId", value.ToString());
        }

        public static void Load()
        {
            try
            {
                string path = Get("GameInstallPath", "");
                if (!string.IsNullOrEmpty(path))
                {
                    _gameConfig.InstallPath = path;
                    _gameConfig.Validate();
                }
                LoggerService.Log("Configuration loaded.");
            }
            catch (Exception ex)
            {
                LoggerService.LogError("Failed to load configuration", ex);
            }
        }

        public static void Save()
        {
            try
            {
                Set("GameInstallPath", _gameConfig.InstallPath ?? "");
                // Force write of appSettings if needed (simplified for .NET Framework)
                LoggerService.Log("Configuration saved.");
            }
            catch (Exception ex)
            {
                LoggerService.LogError("Failed to save configuration", ex);
            }
        }

        public static void SetGamePath(string path)
        {
            _gameConfig.InstallPath = path;
            _gameConfig.Validate();
            Set("GameInstallPath", path);
        }

        private static string Get(string key, string defaultValue)
        {
            try
            {
                string val = ConfigurationManager.AppSettings[key];
                return string.IsNullOrEmpty(val) ? defaultValue : val;
            }
            catch
            {
                return defaultValue;
            }
        }

        private static void Set(string key, string value)
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (config.AppSettings.Settings[key] == null)
                    config.AppSettings.Settings.Add(key, value);
                else
                    config.AppSettings.Settings[key].Value = value;
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
                LoggerService.LogError($"Failed to set config key {key}", ex);
            }
        }
    }
}
