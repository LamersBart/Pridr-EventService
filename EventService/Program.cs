using EventService.AsyncDataServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using EventService.Data;
using EventService.EventProcessing;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata = false;
        o.Audience = builder.Configuration["Authentication:Audience"];
        o.MetadataAddress = builder.Configuration["Authentication:MetadataAddress"]!;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Authentication:ValidIssuer"],
        };
    });

builder.Services.AddAuthorization();

var environment = builder.Environment;
// var  corsConfig = "_corsConfig";
// builder.Services.AddCors(o =>
// {
//     o.AddPolicy("_corsConfig", policy =>
//     {
//         policy.WithOrigins("http://localhost:5039", "https://localhost:7009")
//             .AllowAnyMethod()
//             .AllowAnyHeader();
//     });
// });
var contact = new OpenApiContact
{
    Name = "Bart Lamers",
    Email = "mail@bartlamers.nl",
    Url = new Uri("https://bartlamers.nl")
};
var license = new OpenApiLicense
{
    Name = "My License",
    Url = new Uri("https://bartlamers.nl")
};
var info = new OpenApiInfo
{
    Version = "v1",
    Title = "Swagger API",
    Description = "This is the API Swagger page",
    TermsOfService = new Uri("https://bartlamers.nl"),
    Contact = contact,
    License = license
};
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.CustomSchemaIds(id => id.FullName!.Replace("+", "-"));
    o.AddSecurityDefinition("Keycloak", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            Implicit = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri(builder.Configuration["Keycloak:AuthorizationUrl"]!),
                Scopes = new Dictionary<string, string>
                {
                    {"openid", "openid"},
                    {"profile", "profile"}
                }
            }
        }
    });
    var securityRequirement = new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Keycloak",
                    Type = ReferenceType.SecurityScheme
                },
                In = ParameterLocation.Header,
                Name = "Bearer",
                Scheme = "Bearer",
            },
            []
        }
    };
    o.SwaggerDoc("v1", info);
    o.AddSecurityRequirement(securityRequirement);
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    Console.WriteLine("Using in Postgres DB");
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection"));
});
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<IUserEventRepo, UserEventRepo>();
builder.Services.AddSingleton<IEventProcessor, EventProcessor>();
builder.Services.AddHostedService<MessageBusSubscriber>();
builder.Services.AddControllers();
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("reactApp", p =>
    {
        p.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(o => o.EnableTryItOutByDefault());
}
app.UseSwagger();
app.UseSwaggerUI(o => o.EnableTryItOutByDefault());
PrepDb.PrepPopulation(app, environment.IsProduction());
app.UseHttpsRedirection();
// app.UseCors(corsConfig);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseCors("reactApp");
app.Run();