using System.Text.Json.Serialization;

namespace ChapterInjector.Models
{
    public class PatchRequestPayload
    {
        [JsonPropertyName("contents")]
        public string? Contents { get; set; }
    }
}
