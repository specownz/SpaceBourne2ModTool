using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SpaceBourne2ModTool.Core
{
    /// <summary>
    /// In-app English cleanup helpers for SB2 dialogue/quest text (Steam request: grammar).
    /// Heuristic — not a full NLP engine; designed for batch quest/dialog stub cleaning.
    /// </summary>
    public static class DialogueGrammarTool
    {
        static readonly string[][] PhraseFixes =
        {
            new[] { "i am", "I am" }, new[] { "i'm", "I'm" }, new[] { " i ", " I " },
            new[] { "dont", "don't" }, new[] { "doesnt", "doesn't" }, new[] { "wont", "won't" },
            new[] { "cant", "can't" }, new[] { "isnt", "isn't" }, new[] { "arent", "aren't" },
            new[] { "wasnt", "wasn't" }, new[] { "werent", "weren't" }, new[] { "havent", "haven't" },
            new[] { "hasnt", "hasn't" }, new[] { "wouldnt", "wouldn't" }, new[] { "couldnt", "couldn't" },
            new[] { "shouldnt", "shouldn't" }, new[] { "theres", "there's" }, new[] { "thats", "that's" },
            new[] { "whats", "what's" }, new[] { "youre", "you're" }, new[] { "theyre", "they're" },
            new[] { "weve", "we've" }, new[] { "ive", "I've" }, new[] { "lets ", "let's " },
            new[] { "  ", " " },
        };

        public static string Clean(string input, bool capitalizeSentences = true)
        {
            if (string.IsNullOrEmpty(input)) return input;
            string s = input.Replace("\r\n", "\n");
            foreach (var pair in PhraseFixes)
                s = ReplaceInsensitive(s, pair[0], pair[1]);
            s = Regex.Replace(s, @"\s+([,.!?;:])", "$1");
            s = Regex.Replace(s, @"([,.!?;:])(\S)", "$1 $2");
            s = Regex.Replace(s, @"\s{2,}", " ");
            if (capitalizeSentences)
                s = CapitalizeSentences(s);
            return s.TrimEnd() + (input.EndsWith("\n") ? "\n" : "");
        }

        public static List<string> DiffLines(string before, string after)
        {
            var list = new List<string>();
            var a = (before ?? "").Replace("\r\n", "\n").Split('\n');
            var b = (after ?? "").Replace("\r\n", "\n").Split('\n');
            int n = Math.Max(a.Length, b.Length);
            for (int i = 0; i < n; i++)
            {
                string x = i < a.Length ? a[i] : "";
                string y = i < b.Length ? b[i] : "";
                if (!string.Equals(x, y, StringComparison.Ordinal))
                    list.Add((i + 1) + ": " + x + "  =>  " + y);
            }
            return list;
        }

        static string ReplaceInsensitive(string hay, string old, string neu)
        {
            return Regex.Replace(hay, Regex.Escape(old), m =>
            {
                // preserve simple case patterns
                return neu;
            }, RegexOptions.IgnoreCase);
        }

        static string CapitalizeSentences(string s)
        {
            var sb = new StringBuilder(s.Length);
            bool cap = true;
            foreach (char ch in s)
            {
                if (cap && char.IsLetter(ch))
                {
                    sb.Append(char.ToUpperInvariant(ch));
                    cap = false;
                }
                else sb.Append(ch);
                if (ch == '.' || ch == '!' || ch == '?' || ch == '\n') cap = true;
            }
            return sb.ToString();
        }
    }
}
