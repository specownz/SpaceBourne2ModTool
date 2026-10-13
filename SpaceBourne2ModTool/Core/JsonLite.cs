using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>Minimal JSON helpers shared by object/script documents (no external deps).</summary>
    public static class JsonLite
    {
        public static string Esc(string s)
        {
            if (s == null) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "\\n");
        }

        public static string Unesc(string s)
        {
            if (s == null) return "";
            return s.Replace("\\n", "\n").Replace("\\\"", "\"").Replace("\\\\", "\\");
        }

        public static string GetString(string json, string key)
        {
            string needle = "\"" + key + "\"";
            int i = json.IndexOf(needle, StringComparison.OrdinalIgnoreCase);
            if (i < 0) return null;
            int c = json.IndexOf(':', i);
            int q1 = json.IndexOf('"', c + 1);
            if (q1 < 0) return null;
            int q2 = q1 + 1;
            while (q2 < json.Length && !(json[q2] == '"' && json[q2 - 1] != '\\')) q2++;
            return q2 >= json.Length ? null : Unesc(json.Substring(q1 + 1, q2 - q1 - 1));
        }

        public static Dictionary<string, string> GetObjectMap(string json, string objectKey)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            int props = json.IndexOf("\"" + objectKey + "\"", StringComparison.OrdinalIgnoreCase);
            if (props < 0) return map;
            int brace = json.IndexOf('{', props);
            int end = json.IndexOf('}', brace + 1);
            if (brace < 0 || end < 0) return map;
            foreach (string line in json.Substring(brace + 1, end - brace - 1).Split('\n'))
            {
                string s = line.Trim().TrimEnd(',');
                if (!s.StartsWith("\"")) continue;
                int q2 = s.IndexOf('"', 1);
                if (q2 < 0) continue;
                string key = s.Substring(1, q2 - 1);
                int c = s.IndexOf(':', q2);
                int v1 = s.IndexOf('"', c + 1);
                int v2 = s.LastIndexOf('"');
                if (v1 >= 0 && v2 > v1) map[key] = Unesc(s.Substring(v1 + 1, v2 - v1 - 1));
            }
            return map;
        }

        public static string ObjectToJson(string type, string id, string name, string gamePath, string staging, string notes,
            Dictionary<string, string> props, List<string> tags, DateTime modifiedUtc)
        {
            var sb = new StringBuilder();
            sb.Append("{\n  \"ObjectType\": \"").Append(Esc(type)).Append("\",\n");
            sb.Append("  \"Id\": \"").Append(Esc(id)).Append("\",\n");
            sb.Append("  \"DisplayName\": \"").Append(Esc(name)).Append("\",\n");
            sb.Append("  \"GamePath\": \"").Append(Esc(gamePath)).Append("\",\n");
            sb.Append("  \"StagingRelative\": \"").Append(Esc(staging)).Append("\",\n");
            sb.Append("  \"Notes\": \"").Append(Esc(notes)).Append("\",\n");
            sb.Append("  \"ModifiedUtc\": \"").Append(modifiedUtc.ToString("o")).Append("\",\n  \"Properties\": {\n");
            int i = 0;
            foreach (var kv in props)
            {
                i++;
                sb.Append("    \"").Append(Esc(kv.Key)).Append("\": \"").Append(Esc(kv.Value)).Append("\"");
                sb.Append(i < props.Count ? ",\n" : "\n");
            }
            sb.Append("  },\n  \"Tags\": [");
            for (int t = 0; t < tags.Count; t++)
            {
                if (t > 0) sb.Append(", ");
                sb.Append("\"").Append(Esc(tags[t])).Append("\"");
            }
            sb.Append("]\n}");
            return sb.ToString();
        }
    }
}
