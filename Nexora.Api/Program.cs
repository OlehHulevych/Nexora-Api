using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Nexora.Domain.Entities;
using Nexora.Infrastructure.Context;
using Nexora.Infrastructure.JWT;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using Nexora.Api.Configs;
using Nexora.Api.ExceptionHandler;
using Nexora.Application.Category.Services;
using Nexora.Application.Interfaces.Config;
using Nexora.Application.Interfaces.Context;
using Nexora.Application.Interfaces.IBlobStorage;
using Nexora.Application.Interfaces.JwtService;
using Nexora.Application.Interfaces.Repositories;
using Nexora.Application.Interfaces.Services;
using Nexora.Application.Product.Services;
using Nexora.Application.Review.Services;
using Nexora.Application.Users.Commands.GettingUsers;
using Nexora.Application.Users.Commands.Login;
using Nexora.Application.Users.Commands.Register;
using Nexora.Application.Users.Commands.Update;
using Nexora.Application.Users.Commands.UploadAvatar;
using Nexora.Application.Users.Commands.Validation;
using Nexora.Application.Users.Commands.Validation.Login;
using Nexora.Application.Users.Services;
using Nexora.Infrastructure.Repository;
using Nexora.Infrastructure.Storage;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options=>options.UseNpgsql(connectionString));
builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
builder.Services.AddScoped<IJwtService, JwtTokenHandler>();
builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<IUserBlobStorage, UserBlobStorageService>();
builder.Services.AddScoped<AvatarService>();
builder.Services.Configure<BlobStorageOptions>(builder.Configuration.GetSection(BlobStorageOptions.Section));
builder.Services.AddScoped<RegisterUser>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductBlobStorage, ProductBlobStorageService>();
builder.Services.AddScoped<CreateProduct>();
builder.Services.AddScoped<CategoryRepository>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<LoginUser>();
builder.Services.AddScoped<ValidationErrors>();

builder.Services.AddScoped<GetProduct>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
var googleConfig = builder.Configuration.GetSection("Google").Get<GoogleConfig>();
if (googleConfig != null) builder.Services.AddSingleton<IGoogleConfig>(googleConfig);
builder.Services.AddScoped<UpdateProductService>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();


builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty))
    };
});
builder.Services.AddAuthentication().AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Google:ClientId"];
    options.ClientSecret = builder.Configuration["Google:ClientSecret"];
});
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().WithMethods("GET", "POST", "PUT", "DELETE").AllowAnyHeader();
    });
});
builder.Services
    .AddValidatorsFromAssemblyContaining<RegisterUserValidator>();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Nexora MarketPlace",
        Description = "API for convenient, secure online shopping"
    });
    options.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using Bearer scheme"
    });
    
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "bearerAuth"
                }
            },
            new string[] {}
        }
    });
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
   
});
builder.Services.AddValidatorsFromAssemblyContaining<LoginValidation>();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});;



var app = builder.Build();
await EnsureRolesAsync(app);
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(o=>{});
app.UseHttpsRedirection();

app.MapControllers();




app.Run();
async Task EnsureRolesAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = { "USER", "ADMIN" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}


