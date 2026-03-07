using System.Text;
using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Services;
using CatCatGo.Server.Infrastructure.Cache;
using CatCatGo.Server.Infrastructure.External;
using CatCatGo.Server.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "CatCatGo_Dev_Secret_Key_Min32Chars!!";
var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? "Host=localhost;Database=catcatgo;Username=postgres;Password=postgres";
var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(redisConnection));
builder.Services.AddSingleton<RedisSessionStore>();

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ISaveRepository, SaveRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();
builder.Services.AddScoped<IArenaRepository, ArenaRepository>();
builder.Services.AddScoped<ICheatFlagRepository, CheatFlagRepository>();
builder.Services.AddScoped<IResourceRepository, ResourceRepository>();
builder.Services.AddScoped<ITalentRepository, TalentRepository>();
builder.Services.AddScoped<IEquipmentRepository, EquipmentRepository>();
builder.Services.AddScoped<IChapterRepository, ChapterRepository>();
builder.Services.AddScoped<IGachaRepository, GachaRepository>();
builder.Services.AddScoped<IPetRepository, PetRepository>();
builder.Services.AddScoped<IHeritageRepository, HeritageRepository>();
builder.Services.AddScoped<IDailyRepository, DailyRepository>();
builder.Services.AddScoped<IContentRepository, ContentRepository>();

builder.Services.AddScoped<IReceiptVerifier, GooglePlayVerifier>();
builder.Services.AddScoped<IReceiptVerifier, AppStoreVerifier>();

builder.Services.AddScoped(sp => new AuthService(
    sp.GetRequiredService<IAccountRepository>(), jwtSecret));
builder.Services.AddScoped<SaveService>();
builder.Services.AddScoped<ShopService>();
builder.Services.AddScoped<ArenaService>();
builder.Services.AddScoped<BattleVerifier>();
builder.Services.AddScoped<ResourceService>();
builder.Services.AddScoped<TalentService>();
builder.Services.AddScoped<EquipmentService>();
builder.Services.AddScoped<ChapterService>();
builder.Services.AddScoped<GachaService>();
builder.Services.AddScoped<PetService>();
builder.Services.AddScoped<HeritageService>();
builder.Services.AddScoped<DailyService>();
builder.Services.AddScoped<ContentService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "CatCatGo.Server",
            ValidAudience = "CatCatGo.Client",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
