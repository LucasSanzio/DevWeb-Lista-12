using System.Text.Json.Serialization;

namespace ConsumerDisneyApi.Models;

public sealed class DisneyCharacterResponse
{
    [JsonPropertyName("data")]
    public DisneyCharacter? Data { get; init; }
}

public sealed class DisneyCharacter
{
    [JsonPropertyName("_id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("imageUrl")]
    public string ImageUrl { get; init; } = string.Empty;
}
