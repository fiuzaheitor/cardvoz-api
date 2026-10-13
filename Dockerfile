# Etapa 1: compila e publica a API
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY cardvoz-api.csproj ./
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# Etapa 2: imagem final, só com o runtime do ASP.NET
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

# A conexão com o Supabase vem da variável ConnectionStrings__DefaultConnection
USER $APP_UID
EXPOSE 8080

# O Render informa a porta na variável PORT; localmente, usa 8080
CMD ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} exec dotnet cardvoz-api.dll"]
