using System.Net.Http.Json;
using ConsumerDisneyApi.Models;
using SkiaSharp;

namespace ConsumerDisneyApi.Services;

public sealed class DisneyService(HttpClient httpClient)
{
    private const string CharacterEndpoint = "https://api.disneyapi.dev/character/";

    public async Task<DisneyCharacter> GetCharacterAsync(int characterId)
    {
        DisneyCharacterResponse? response;
        try
        {
            response = await httpClient
                .GetFromJsonAsync<DisneyCharacterResponse>($"{CharacterEndpoint}{characterId}");
        }
        catch (HttpRequestException exception)
        {
            throw new HttpRequestException(
                $"Falha ao consultar o personagem {characterId} na API Disney: {exception.Message}",
                exception, exception.StatusCode);
        }

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
        using HttpRequestMessage request = new(HttpMethod.Get, character.ImageUrl);
        request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (compatible; ConsumerDisneyApi/1.0)");
        request.Headers.Referrer = new Uri("https://disney.fandom.com/");

        using HttpResponseMessage response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Falha ao baixar a imagem do personagem {character.Id}: " +
                $"HTTP {(int)response.StatusCode} ({response.ReasonPhrase}). URL: {character.ImageUrl}",
                null, response.StatusCode);
        }

        await using Stream imageStream = await response.Content.ReadAsStreamAsync();
        using SKBitmap image = SKBitmap.Decode(imageStream)
            ?? throw new InvalidOperationException("O servidor retornou uma imagem inválida ou não reconhecida.");
        using SKData png = image.Encode(SKEncodedImageFormat.Png, 100)
            ?? throw new InvalidOperationException("Não foi possível converter a imagem para PNG.");
        string imagePath = Path.Combine(AppContext.BaseDirectory, $"character-{character.Id}.png");

        await File.WriteAllBytesAsync(imagePath, png.ToArray());
        return imagePath;
    }
}
