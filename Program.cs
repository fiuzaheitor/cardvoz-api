using System.Text.Json.Serialization;
using cardvoz_api.Data;
using cardvoz_api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

// Carrega variáveis do arquivo .env (se existir) para o ambiente ANTES de montar
// a configuração do ASP.NET Core.
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Validação do contêiner de DI em ambiente de desenvolvimento
builder.Host.UseDefaultServiceProvider((context, options) =>
{
    options.ValidateOnBuild = context.HostingEnvironment.IsDevelopment();
    options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
});

// === CONFIGURAÇÃO DE CONTROLLERS E SERIALIZAÇÃO JSON ===
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// === CONFIGURAÇÃO DO SWAGGER/OPENAPI ===
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CardVoz API",
        Version = "v1",
        Description = "API RESTful acessível para estudo com flashcards e comandos de voz do CardVoz.",
        Contact = new OpenApiContact
        {
            Name = "Equipe CardVoz",
            Url = new Uri("https://github.com/gvtenberg/cardvoz")
        }
    });

    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// === CONFIGURAÇÃO DE CORS ===
var configuredOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[]
    {
        "http://localhost:3000",
        "http://localhost:3001",
        "http://localhost:3002",
        "http://127.0.0.1:3000"
    };

var envCorsOrigins = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS");
if (!string.IsNullOrWhiteSpace(envCorsOrigins))
{
    var extra = envCorsOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    configuredOrigins = configuredOrigins.Concat(extra).Distinct().ToArray();
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(configuredOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// === CONFIGURAÇÃO DO BANCO DE DADOS (PostgreSQL / Supabase) ===
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Defina a conexão do Supabase na variável ConnectionStrings__DefaultConnection " +
        "(ex.: postgresql://postgres.<projeto>:<senha>@aws-0-<regiao>.pooler.supabase.com:5432/postgres).");
}

builder.Services.AddDbContext<CardVozDbContext>(options =>
    options.UseNpgsql(PostgresSupport.NormalizeConnectionString(connectionString)));

// === INJEÇÃO DE DEPENDÊNCIA (SERVICES) ===
builder.Services.AddScoped<IDeckService, DeckService>();
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<IStudyService, StudyService>();

var app = builder.Build();

// === VERIFICAÇÃO DO BANCO DE DADOS ===
// O esquema e os dados iniciais vêm das migrations do repositório cardvoz-db.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<CardVozDbContext>();
        var deckCount = context.Decks.Count();
        logger.LogInformation("Banco de dados conectado ({DeckCount} baralhos).", deckCount);
    }
    catch (Exception ex)
    {
        logger.LogError(ex,
            "Não foi possível consultar o banco. Confira a conexão do Supabase e se as " +
            "migrations do repositório cardvoz-db foram aplicadas (supabase db push).");
    }
}

// === PIPELINE HTTP ===
app.UseCors("AllowFrontend");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CardVoz API v1");
    c.RoutePrefix = "swagger";
});

// Redireciona a raiz para o Swagger UI
app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapControllers();

app.Run();
