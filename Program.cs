using ConsumerDisneyApi.Services;
using ConsumerDisneyApi.Models;

using HttpClient httpClient = new();
DisneyService disneyService = new(httpClient);

int characterId = 308;
if (args.Length > 0 && (!int.TryParse(args[0], out characterId) || characterId <= 0))
{
    Console.Error.WriteLine("Informe um ID de personagem inteiro e positivo.");
    Environment.ExitCode = 1;
    return;
}

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
    Console.Error.WriteLine(exception.Message);
    Environment.ExitCode = 1;
}
catch (InvalidOperationException exception)
{
    Console.Error.WriteLine(exception.Message);
    Environment.ExitCode = 1;
}
