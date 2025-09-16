using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using SummitAPI.Data;
using SummitAPI.Service;   // if your files use SummitAPI.Services, change to .Services
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- EF Core / SQLite ---
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Services ---
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddHttpClient();

// Resolve these ONCE so names aren't reused later
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
builder.Services.AddSwaggerGen();

// --- CORS for Android dev ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("android", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// --- JWT ---
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

var app = builder.Build();

// Ensure DB exists / migrate
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try { db.Database.Migrate(); } catch { db.Database.EnsureCreated(); }
}

// Static files for Local storage (/uploads/* → /data/uploads or your local path)
Directory.CreateDirectory(localRoot);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(localRoot),
    RequestPath = publicBase
});

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseCors("android");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
