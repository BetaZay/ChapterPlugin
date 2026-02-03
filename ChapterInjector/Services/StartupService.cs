using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using ChapterInjector.Helpers;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

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
            _logger.LogInformation("ChapterInjector: Executing StartupService to inject client script.");

            List<JObject> payloads = new List<JObject>
            {
                new JObject
                {
                    { "id", Plugin.Instance!.Id.ToString() },
                    { "fileNamePattern", "index.html" },
                    { "callbackAssembly", GetType().Assembly.FullName },
                    { "callbackClass", typeof(IndexHtmlInjector).FullName },
                    { "callbackMethod", nameof(IndexHtmlInjector.FileTransformer) }
                }
            };

            Assembly? fileTransformationAssembly = AssemblyLoadContext.All
                .SelectMany(x => x.Assemblies)
                .FirstOrDefault(x => x.FullName?.Contains(".FileTransformation", StringComparison.OrdinalIgnoreCase) ?? false);

            if (fileTransformationAssembly == null)
            {
                _logger.LogWarning("ChapterInjector: FileTransformation plugin not found. Fallback to direct injection (requires write permissions).");
                IndexHtmlInjector.Direct();
                return Task.CompletedTask;
            }

            Type? pluginInterfaceType = fileTransformationAssembly.GetType("Jellyfin.Plugin.FileTransformation.PluginInterface");
            if (pluginInterfaceType == null)
            {
                _logger.LogWarning("ChapterInjector: FileTransformation plugin interface not found. Fallback to direct injection.");
                IndexHtmlInjector.Direct();
                return Task.CompletedTask;
            }

            _logger.LogInformation("ChapterInjector: Registering ChapterInjector for FileTransformation plugin.");
            foreach (JObject payload in payloads)
            {
                pluginInterfaceType.GetMethod("RegisterTransformation")?.Invoke(null, new object[] { payload });
            }

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
