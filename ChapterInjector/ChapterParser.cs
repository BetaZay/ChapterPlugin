using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using MediaBrowser.Model.Entities;

namespace ChapterInjector
{
    /// <summary>
    /// Parsers for external chapter files.
    /// </summary>
    public static class ChapterParser
    {
        /// <summary>
        /// Parses the chapter file.
        /// </summary>
        /// <param name="filePath">The path to the chapter file.</param>
        /// <returns>A list of chapters.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA3003:Review code for file path injection vulnerabilities", Justification = "Path is vetted by caller")]
        public static IReadOnlyList<ChapterInfo> Parse(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return new List<ChapterInfo>();
            }

            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            if (extension == ".xml")
            {
                return ParseXml(filePath);
            }
            else if (extension == ".txt")
            {
                return ParseTxt(filePath);
            }

            return new List<ChapterInfo>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA3003:Review code for file path injection vulnerabilities", Justification = "Path is vetted by caller")]
        private static List<ChapterInfo> ParseTxt(string filePath)
        {
            var chapters = new List<ChapterInfo>();
            // CHAPTER01=00:00:00.000
            // CHAPTER01NAME=Chapter 1
            var lines = File.ReadAllLines(filePath);
            var timeMap = new Dictionary<string, long>();
            var nameMap = new Dictionary<string, string>();

            var timeRegex = new Regex(@"^CHAPTER(\d+)=(\d{2}):(\d{2}):(\d{2}\.\d{3})");
            var nameRegex = new Regex(@"^CHAPTER(\d+)NAME=(.*)");

            foreach (var line in lines)
            {
                var timeMatch = timeRegex.Match(line);
                if (timeMatch.Success)
                {
                    var id = timeMatch.Groups[1].Value;
                    if (TimeSpan.TryParse(timeMatch.Groups[2].Value + ":" + timeMatch.Groups[3].Value + ":" + timeMatch.Groups[4].Value, out var ts))
                    {
                        // Jellyfin uses ticks (10,000 ticks = 1ms)
                        timeMap[id] = ts.Ticks;
                    }

                    continue;
                }

                var nameMatch = nameRegex.Match(line);
                if (nameMatch.Success)
                {
                    var id = nameMatch.Groups[1].Value;
                    nameMap[id] = nameMatch.Groups[2].Value;
                }
            }

            foreach (var kvp in timeMap)
            {
                var chapter = new ChapterInfo
                {
                    StartPositionTicks = kvp.Value,
                };

                if (nameMap.TryGetValue(kvp.Key, out var name))
                {
                    chapter.Name = name;
                }

                chapters.Add(chapter);
            }

            return chapters.OrderBy(c => c.StartPositionTicks).ToList();
        }

        private static List<ChapterInfo> ParseXml(string filePath)
        {
            var chapters = new List<ChapterInfo>();
            try
            {
                var doc = XDocument.Load(filePath);
                var atoms = doc.Descendants("ChapterAtom");

                foreach (var atom in atoms)
                {
                    var timeStart = atom.Element("ChapterTimeStart")?.Value;
                    var display = atom.Element("ChapterDisplay");
                    var name = display?.Element("ChapterString")?.Value;

                    if (timeStart != null)
                    {
                         if (TimeSpan.TryParse(timeStart, out var ts))
                         {
                             chapters.Add(new ChapterInfo
                             {
                                 Name = name,
                                 StartPositionTicks = ts.Ticks
                             });
                         }
                    }
                }
            }
            catch (Exception)
            {
                // Log or ignore invalid XML
            }

            return chapters.OrderBy(c => c.StartPositionTicks).ToList();
        }
    }
}
