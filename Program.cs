using ConsumerDisneyApi.Services;
using ConsumerDisneyApi.Models;

using HttpClient httpClient = new();
DisneyService disneyService = new(httpClient);

const int characterId = 204;

Console.WriteLine($"Buscando o personagem Disney de ID {characterId}...");

try
{
    DisneyCharacter character = await disneyService.GetCharacterAsync(characterId);
    string imagePath = await disneyService.DownloadCharacterImageAsPngAsync(character);

    Console.WriteLine();
    Console.WriteLine($"Personagem: {character.Name}");
    Console.WriteLine($"ID: {character.Id}");
    Console.WriteLine($"Imagem: {character.ImageUrl}");
    Console.WriteLine($"PNG salvo em: {imagePath}");
}
catch (HttpRequestException exception)
{
    Console.Error.WriteLine($"Não foi possível consultar a API Disney: {exception.Message}");
    Environment.ExitCode = 1;
}
catch (InvalidOperationException exception)
{
    Console.Error.WriteLine(exception.Message);
    Environment.ExitCode = 1;
}
