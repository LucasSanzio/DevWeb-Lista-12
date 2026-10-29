using System.Net.Http.Json;
using ConsumerDisneyApi.Models;

namespace ConsumerDisneyApi.Services;

public sealed class DisneyService(HttpClient httpClient)
{
    private const string CharacterEndpoint = "https://api.disneyapi.dev/character/";

    public async Task<DisneyCharacter> GetCharacterAsync(int characterId)
    {
        DisneyCharacterResponse? response = await httpClient
            .GetFromJsonAsync<DisneyCharacterResponse>($"{CharacterEndpoint}{characterId}");

        DisneyCharacter? character = response?.Data;

        if (character is null || string.IsNullOrWhiteSpace(character.Name))
        {
            throw new InvalidOperationException("A API não retornou um personagem válido.");
        }

        if (string.IsNullOrWhiteSpace(character.ImageUrl))
        {
            throw new InvalidOperationException("A API não retornou uma imagem para este personagem.");
        }

        return character;
    }

    public async Task<string> DownloadCharacterImageAsPngAsync(DisneyCharacter character)
    {
        byte[] image = await httpClient.GetByteArrayAsync(character.ImageUrl);
        string imagePath = Path.Combine(AppContext.BaseDirectory, $"character-{character.Id}.png");

        await File.WriteAllBytesAsync(imagePath, image);
        return imagePath;
    }
}
