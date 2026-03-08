using System.Text.Json;
using System.Text.Json.Serialization;

namespace Caktoje.Data.Resources.OpenLibrary;

public class OpenLibraryBook
{
    [JsonPropertyName("thumbnail_url")]
    public required string ThumbnailUrl { get; set; }
    public OpenLibraryBookDetail? Details { get; set; }
}
public class OpenLibraryBookDetail
{
    public required string Title { get; set; }
    public required List<OpenLibraryBookDetailsAuthor> Authors { get; set; } = [];

    [JsonConverter(typeof(OpenLibraryDescriptionConverter))]
    public string? Description { get; set; }
    public required List<string> Genres { get; set; } = []; 
}
public class OpenLibraryBookDetailsAuthor
{
    public required string Key { get; set; }
    public required string Name { get; set; }
}

public class OpenLibraryDescriptionConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String) return reader.GetString();
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            return doc.RootElement.GetProperty("value").GetString();
        }
        return null;
    }
    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) => writer.WriteStringValue(value);
}