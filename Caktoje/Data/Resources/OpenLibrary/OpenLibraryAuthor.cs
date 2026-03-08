using System.Text.Json.Serialization;

namespace Caktoje.Data.Resources.OpenLibrary;

public class OpenLibraryAuthor
{
    [JsonPropertyName("personal_name")]
    public required string PersonalName { get; set; }
    public required List<int> Photos { get; set; } = [];
    public OpenLibraryAuthorBio? Bio { get; set; }

}
public class OpenLibraryAuthorBio
{
    public required string Type { get; set; }
    public required string Value { get; set; }
}