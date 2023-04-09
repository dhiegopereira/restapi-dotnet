using Microsoft.EntityFrameworkCore;
using TodoList.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configurações do banco de dados
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurações de autenticação JWT
ConfigureJWT(builder);

// Configurações do Swagger
ConfigureSwagger(builder);

// Configurações de autorização
builder.Services.AddAuthorization();

// Mapeamento dos controllers
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Uso do Swagger
app.UseSwagger();
app.UseSwaggerUI(options => {
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "TodoList API v1");
    options.RoutePrefix = "";
});

// Mapeamento das rotas dos controllers
app.MapControllers();

app.Run();

// Método de configuração da autenticação JWT
void ConfigureJWT(WebApplicationBuilder builder) {
    var SecretKey = builder.Configuration.GetSection("JWT")["SecretKey"];
    var Issuer = builder.Configuration.GetSection("JWT")["Issuer"];
    var Audience = builder.Configuration.GetSection("JWT")["Audience"];

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options => {
            options.TokenValidationParameters = new TokenValidationParameters {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Issuer,
                ValidAudience = Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey))
            };
        });
}

// Método de configuração do Swagger
void ConfigureSwagger(WebApplicationBuilder builder) {
    builder.Services.AddSwaggerGen(options => {
        options.SwaggerDoc("v1", new OpenApiInfo {
            Title = "TodoList API",
            Version = "v1",
            Description = "A simple API for managing tasks",
            Contact = new OpenApiContact {
                Name = "Your Name",
                Email = "your.email@example.com",
                Url = new Uri("https://example.com/contact")
            }
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
            In = ParameterLocation.Header,
            Description = "Please enter your JWT token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement {
            {
                new OpenApiSecurityScheme {
                    Reference = new OpenApiReference {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        });
    });
}
