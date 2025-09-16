using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SummitAPI.Data;
using SummitAPI.Service;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- EF Core / SQLite ---
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- App services ---
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddHttpClient();

// Resolve once (used for DI + later for StaticFiles)
var localRoot = builder.Configuration["Storage:LocalRoot"] ?? "/data/uploads";
var publicBase = builder.Configuration["Storage:PublicBasePath"] ?? "/uploads";

// Storage service selection (Local on Render disk OR Supabase)
var storageMode = builder.Configuration["Storage:Mode"] ?? "Local";
if (storageMode.Equals("Supabase", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<IStorageService>(sp =>
        new SupabaseStorageService(
            sp.GetRequiredService<HttpClient>(),
            builder.Configuration["Storage:Supabase:Url"]!,
            builder.Configuration["Storage:Supabase:Key"]!,
            builder.Configuration["Storage:Supabase:Bucket"]!,
            builder.Configuration["Storage:Supabase:PublicUrlBase"]!
        ));
}
else
{
    builder.Services.AddSingleton<IStorageService>(_ => new LocalStorageService(localRoot, publicBase));
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// --- Swagger (robust + JWT button) ---
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SummitApi", Version = "v1" });
    c.CustomSchemaIds(t => t.FullName);                 // avoid name collisions (e.g., Configuration)
    c.SupportNonNullableReferenceTypes();

    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header. Example: Bearer {token}"
    };
    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { scheme, Array.Empty<string>() }
    });
});

// --- CORS for Android dev ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("android", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// --- JWT (with friendly guard) ---
var issuer = builder.Configuration["Jwt:Issuer"] ?? "SummitApi";
var keyString = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(keyString))
{
    if (builder.Environment.IsDevelopment())
    {
        // Dev-only fallback so you can run without secrets; DO NOT use in prod
        keyString = "DEV_ONLY_FALLBACK_p7N2t5Q8w1E4y7U0r3K6m9Z2x5C8v1B4n7M0q3T6z9L2f5H8";
    }
    else
    {
        throw new InvalidOperationException("Missing Jwt:Key. Set it in appsettings, user-secrets, or environment.");
    }
}
var key = Encoding.UTF8.GetBytes(keyString);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

var app = builder.Build();

// Show detailed errors in Dev (helps when /swagger/v1/swagger.json fails)
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Ensure DB exists / migrate
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try { db.Database.Migrate(); } catch { db.Database.EnsureCreated(); }
}

// ---- Static files for Local storage (/uploads/* -> <absolute path>) ----
var configuredLocalRoot = builder.Configuration["Storage:LocalRoot"] ?? "/data/uploads";
var configuredPublicBase = builder.Configuration["Storage:PublicBasePath"] ?? "/uploads";

// Normalize to absolute path for PhysicalFileProvider
string localRootPath = configuredLocalRoot;
if (!Path.IsPathRooted(localRootPath))
    localRootPath = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, configuredLocalRoot));

var requestPath = configuredPublicBase.StartsWith("/") ? configuredPublicBase : "/" + configuredPublicBase;

Directory.CreateDirectory(localRootPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(localRootPath),
    RequestPath = requestPath
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("android");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
