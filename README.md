# Lista 12 — API Disney

Aplicação de console em .NET 10 que consulta um personagem na API Disney e baixa sua imagem, convertendo JPEG/WebP para PNG com SkiaSharp. Validada no Windows.

## Executar

Requer o SDK .NET 10 e acesso à internet.

```powershell
dotnet run
dotnet run -- 204
dotnet watch run
```

O ID padrão é 308. A imagem é salva na pasta de saída indicada pelo programa.

O download envia User-Agent e Referer, pois o servidor de imagens pode rejeitar requisições sem esses cabeçalhos com HTTP 403. Erros HTTP indicam se a falha ocorreu na consulta do personagem ou no download da imagem.

## Testes de regressão

```powershell
dotnet run --project tests/DisneyService.Tests.csproj
```

Os sete testes usam respostas HTTP simuladas para verificar a consulta, os cabeçalhos de download, a conversão de JPEG e WebP em PNG e o tratamento de respostas inválidas e erros HTTP.
