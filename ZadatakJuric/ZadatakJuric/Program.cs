using MediatR;
using Microsoft.EntityFrameworkCore;
using ZadatakJuric.Application.DBCParser.Commands;
using ZadatakJuric.Infrastructure.Data;
using ZadatakJuric.Infrastructure.Repositories;
using ZadatakJuric.Server.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Add controllers
builder.Services.AddControllers();

// Add HttpClient for Blazor Server components
builder.Services.AddHttpClient();

// Configure HttpClient with base address for API calls
builder.Services.AddScoped(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient();
    
    // In Blazor Server, we need to set the base address to the current host
    var navigationManager = sp.GetService<Microsoft.AspNetCore.Components.NavigationManager>();
    if (navigationManager != null)
    {
        httpClient.BaseAddress = new Uri(navigationManager.BaseUri);
    }
    
    return httpClient;
});

// Add Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<DBCDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add Repository
builder.Services.AddScoped<IDbcRepository, DbcRepository>();

// Add MediatR
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(ParseDBCFileCommand).Assembly);
});

var app = builder.Build();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DBCDbContext>();
    try
    {
        // This will create the database if it doesn't exist and apply pending migrations
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        
        // In development, you might want to ensure the database is created even if migrations fail
        if (app.Environment.IsDevelopment())
        {
            context.Database.EnsureCreated();
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

// Map controllers
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(ZadatakJuric.Client._Imports).Assembly);

app.Run();
