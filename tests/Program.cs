using System.Net;
using System.Net.Http.Json;
using ConsumerDisneyApi.Models;
using ConsumerDisneyApi.Services;
using SkiaSharp;

int passed = 0;
await Check("API retorna personagem e preserva o ID", async () =>
{
    using HttpClient client = Client(request =>
    {
        Require(request.RequestUri!.AbsoluteUri == "https://api.disneyapi.dev/character/308", "URL incorreta");
        return new(HttpStatusCode.OK) { Content = JsonContent.Create(new { data = new { _id = 308, name = "Queen Arianna", imageUrl = "https://example.com/image" } }) };
    });
    DisneyCharacter character = await new DisneyService(client).GetCharacterAsync(308);
    Require(character.Id == 308 && character.Name == "Queen Arianna", "Personagem incorreto");
});

foreach (string format in new[] { "jpeg", "webp" })
{
    await Check($"Download com cabeçalhos e conversão de {format} para PNG", async () =>
    {
        using SKBitmap original = new(2, 3);
        original.Erase(SKColors.Red);
        using SKData source = original.Encode(format == "jpeg" ? SKEncodedImageFormat.Jpeg : SKEncodedImageFormat.Webp, 100);
        using HttpClient client = Client(request =>
        {
            Require(request.Headers.UserAgent.ToString().Contains("Mozilla/5.0"), "User-Agent ausente");
            Require(request.Headers.Referrer?.AbsoluteUri == "https://disney.fandom.com/", "Referer ausente");
            return new(HttpStatusCode.OK) { Content = new ByteArrayContent(source.ToArray()) };
        });
        string path = await new DisneyService(client).DownloadCharacterImageAsPngAsync(Character());
        try
        {
            byte[] png = await File.ReadAllBytesAsync(path);
            Require(png.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }), "Assinatura PNG inválida");
            using SKBitmap decoded = SKBitmap.Decode(png);
            Require(decoded.Width == 2 && decoded.Height == 3, "Dimensões alteradas");
        }
        finally { File.Delete(path); }
    });
}

await Check("Erro 403 da imagem identifica etapa e status", async () =>
{
    using HttpClient client = Client(_ => new(HttpStatusCode.Forbidden));
    try { await new DisneyService(client).DownloadCharacterImageAsPngAsync(Character()); }
    catch (HttpRequestException error) when (error.StatusCode == HttpStatusCode.Forbidden && error.Message.Contains("baixar a imagem")) { return; }
    throw new Exception("Erro de download não foi identificado");
});

await Check("Erro da API identifica etapa e status", async () =>
{
    using HttpClient client = Client(_ => new(HttpStatusCode.NotFound));
    try { await new DisneyService(client).GetCharacterAsync(308); }
    catch (HttpRequestException error) when (error.StatusCode == HttpStatusCode.NotFound && error.Message.Contains("consultar o personagem")) { return; }
    throw new Exception("Erro da API não foi identificado");
});

await Check("API sem personagem é rejeitada", async () =>
{
    using HttpClient client = Client(_ => new(HttpStatusCode.OK) { Content = JsonContent.Create(new { data = (object?)null }) });
    try { await new DisneyService(client).GetCharacterAsync(308); }
    catch (InvalidOperationException) { return; }
    throw new Exception("Resposta vazia foi aceita");
});

await Check("HTML recebido como imagem é rejeitado", async () =>
{
    using HttpClient client = Client(_ => new(HttpStatusCode.OK) { Content = new StringContent("<html>Access denied</html>") });
    try { await new DisneyService(client).DownloadCharacterImageAsPngAsync(Character()); }
    catch (InvalidOperationException) { return; }
    throw new Exception("HTML foi aceito como PNG");
});

Console.WriteLine($"{passed} testes passaram.");

async Task Check(string name, Func<Task> test)
{
    await test();
    passed++;
    Console.WriteLine($"OK: {name}");
}
static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
static DisneyCharacter Character() => new() { Id = 999999, Name = "Teste", ImageUrl = "https://example.com/image" };
static HttpClient Client(Func<HttpRequestMessage, HttpResponseMessage> response) => new(new StubHandler(response));

sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> response) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(response(request));
}
