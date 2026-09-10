using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using OpenAI;
using RagDemo.Web.Api;
using RagDemo.Web.Components;
using RagDemo.Web.Data;
using RagDemo.Web.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOpenApi();

builder.Services.AddDbContextFactory<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<IChunkRepository, ChunkRepository>();

var openAiKey = builder.Configuration["OpenAI:ApiKey"];
var useFakeEmbeddings = string.IsNullOrWhiteSpace(openAiKey) || openAiKey == "REPLACE_ME";
if (useFakeEmbeddings)
{
    builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>, DeterministicEmbeddingGenerator>();
}
else
{
    var embeddingModel = builder.Configuration["OpenAI:EmbeddingModel"] ?? "text-embedding-3-small";
    builder.Services.AddEmbeddingGenerator(
        new OpenAIClient(openAiKey).GetEmbeddingClient(embeddingModel).AsIEmbeddingGenerator());
}

builder.Services.AddSingleton(new ChunkingService(maxChars: 600, overlap: 100));
builder.Services.AddScoped<IngestService>();

var app = builder.Build();

if (useFakeEmbeddings)
{
    app.Logger.LogWarning("OpenAI:ApiKey not set - using DeterministicEmbeddingGenerator (dev only, no real semantics)");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapOpenApi();
app.MapScalarApiReference(options => options.WithTitle("blazor-rag-azure API"));
app.MapRagApi();

app.Run();
