using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChapterInjector.Api
{
    /// <summary>
    /// Controller for serving the client-side script.
    /// </summary>
    [Route("ExternalChapters")]
    public class ClientScriptController : ControllerBase
    {
        private readonly Assembly _assembly;
        private readonly string _clientScriptPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientScriptController"/> class.
        /// </summary>
        public ClientScriptController()
        {
            _assembly = Assembly.GetExecutingAssembly();
            // Namespace.Folder.File
            _clientScriptPath = "ChapterInjector.Web.client.js";
        }

        /// <summary>
        /// Gets the client-side javascript file.
        /// </summary>
        /// <returns>The javascript file.</returns>
        [HttpGet("ClientScript")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Produces("application/javascript")]
        public ActionResult GetClientScript()
        {
            var scriptStream = _assembly.GetManifestResourceStream(_clientScriptPath);
            if (scriptStream == null)
            {
                return NotFound();
            }

            return File(scriptStream, "application/javascript");
        }
    }
}
