using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChapterInjector.Helpers;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace ChapterInjector.Services
{
    /// <summary>
    /// Service to inject the client script on startup.
    /// </summary>
    public class StartupService : IScheduledTask
    {
        private readonly ILogger<StartupService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartupService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public StartupService(ILogger<StartupService> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public string Name => "ChapterInjector Injection";

        /// <inheritdoc />
        public string Key => "ChapterInjector.Startup";

        /// <inheritdoc />
        public string Description => "Injects client-side script into Jellyfin web interface.";

        /// <inheritdoc />
        public string Category => "Startup Services";

        /// <inheritdoc />
        public Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Executing StartupService to inject client script.");
            IndexHtmlInjector.Inject();
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            return new[]
            {
                new TaskTriggerInfo
                {
                    Type = TaskTriggerInfoType.StartupTrigger
                }
            };
        }
    }
}
