using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ChapterInjector.Api
{
    /// <summary>
    /// Controller for retrieving external chapters.
    /// </summary>
    [Route("ExternalChapters")]
    public class ExternalChaptersController : ControllerBase
    {
        private readonly ILibraryManager _libraryManager;
        private readonly ILogger<ExternalChaptersController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalChaptersController"/> class.
        /// </summary>
        /// <param name="libraryManager">The library manager.</param>
        /// <param name="logger">The logger.</param>
        public ExternalChaptersController(ILibraryManager libraryManager, ILogger<ExternalChaptersController> logger)
        {
            _libraryManager = libraryManager;
            _logger = logger;
        }

        /// <summary>
        /// Gets the external chapters for a specific item.
        /// </summary>
        /// <param name="itemId">The item id.</param>
        /// <returns>A list of chapters.</returns>
        [HttpGet("{itemId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA3003:Review code for file path injection vulnerabilities", Justification = "Path is derived from internal LibraryManager")]
        public ActionResult<List<ChapterInfo>> Get(Guid itemId)
        {
            var item = _libraryManager.GetItemById(itemId);

            if (item == null)
            {
                return NotFound("Item not found");
            }

            var path = item.Path;
            if (string.IsNullOrEmpty(path))
            {
                 return NotFound("Item path is empty");
            }

            var directory = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(directory))
            {
                return NotFound("Item directory not found");
            }

            var filenameNoExt = Path.GetFileNameWithoutExtension(path);

            // Potential chapter files to look for
            var potentialFiles = new List<string>();

            // 1. Specific filename match (Preferred)
            potentialFiles.Add(Path.Combine(directory, filenameNoExt + ".chapters.xml"));
            potentialFiles.Add(Path.Combine(directory, filenameNoExt + ".chapters.txt"));

            // 2. Episode specific (e.g. 1_chapters.txt)
            if (item is MediaBrowser.Controller.Entities.TV.Episode episode && episode.IndexNumber.HasValue)
            {
                 var index = episode.IndexNumber.Value;
                 potentialFiles.Add(Path.Combine(directory, $"{index}_chapters.xml"));
                 potentialFiles.Add(Path.Combine(directory, $"{index}_chapters.txt"));
            }

            // 3. Generic (Movies/Folders)
            potentialFiles.Add(Path.Combine(directory, "chapters.xml"));
            potentialFiles.Add(Path.Combine(directory, "chapters.txt"));
            potentialFiles.Add(Path.Combine(directory, "chapter.xml"));
            potentialFiles.Add(Path.Combine(directory, "chapter.txt"));

            foreach (var file in potentialFiles)
            {
                if (System.IO.File.Exists(file))
                {
                    try
                    {
                        var chapters = ChapterParser.Parse(file);
                        if (chapters.Count > 0)
                        {
                            return Ok(chapters);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error parsing chapter file {ChapterFile}", file);
                    }
                }
            }

            return NotFound("No external chapters found");
        }
    }
}
