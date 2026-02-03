using System.Text.Json.Serialization;

namespace ChapterInjector.Models
{
    /// <summary>
    /// Payload for patch requests.
    /// </summary>
    public class PatchRequestPayload
    {
        /// <summary>
        /// Gets or sets the content to patch.
        /// </summary>
        [JsonPropertyName("contents")]
        public string? Contents { get; set; }
    }
}
