using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Logging;

namespace ChapterInjector.Helpers
{
    /// <summary>
    /// Helper class to inject the client-side script into index.html.
    /// </summary>
    public static class IndexHtmlInjector
    {
        private const string ScriptTagRegex = "<script plugin=\"ChapterInjector\".*?></script>";

        /// <summary>
        /// Injects the client script into the index.html file.
        /// </summary>
        public static void Inject()
        {
            if (Plugin.Instance == null)
            {
                return;
            }

            var applicationPaths = Plugin.Instance.ViewableApplicationPaths;
            var logger = Plugin.Instance.Logger;

            if (string.IsNullOrWhiteSpace(applicationPaths.WebPath))
            {
                logger.LogWarning("WebPath is empty. Cannot inject client script.");
                return;
            }

            var indexFile = Path.Combine(applicationPaths.WebPath, "index.html");
            if (!File.Exists(indexFile))
            {
                logger.LogWarning("index.html not found at {Path}", indexFile);
                return;
            }

            try
            {
                string indexContents = File.ReadAllText(indexFile);
                string scriptElement = GetScriptElement();

                if (indexContents.Contains(scriptElement, StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogDebug("Client script already injected in {File}", indexFile);
                    return;
                }

                logger.LogInformation("Injecting ChapterInjector client script into {File}", indexFile);

                // Remove old script tag if exists (regex)
                indexContents = Regex.Replace(indexContents, ScriptTagRegex, string.Empty);

                // Insert at end of body
                int bodyClosing = indexContents.LastIndexOf("</body>", StringComparison.Ordinal);
                if (bodyClosing == -1)
                {
                    logger.LogWarning("Could not find closing body tag in {File}", indexFile);
                    return;
                }

                indexContents = indexContents.Insert(bodyClosing, scriptElement);
                File.WriteAllText(indexFile, indexContents);
                logger.LogInformation("Successfully injected client script.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to inject client script into {File}", indexFile);
            }
        }

        private static string GetScriptElement()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0.0";
            return $"<script plugin=\"ChapterInjector\" version=\"{version}\" src=\"ExternalChapters/ClientScript\" defer></script>";
        }
    }
}
