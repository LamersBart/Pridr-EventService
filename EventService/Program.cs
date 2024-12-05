using EventService.AsyncDataServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using EventService.Data;
using EventService.EventProcessing;

var builder = WebApplication.CreateBuilder(args);
var environment = builder.Environment;
var  corsConfig = "_corsConfig";
builder.Services.AddCors(o =>
{
    o.AddPolicy("_corsConfig", policy =>
    {
        policy.WithOrigins("http://localhost:5039", "https://localhost:7009")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
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
var securityScheme = new OpenApiSecurityScheme
{
    Type = SecuritySchemeType.OAuth2,
    Flows = new OpenApiOAuthFlows
    {
        Implicit = new OpenApiOAuthFlow
        {
            AuthorizationUrl = new Uri(builder.Configuration["Keyclaok:AuthURL"]!),
            // Scopes = new Dictionary<string, string>
            // {
            //     {"openid", "openid"},
            //     {"profile", "profile"}
            // }
        }
    }
};
var securityReq = new OpenApiSecurityRequirement
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Keycloak"
            },
            In = ParameterLocation.Header,
            Name = "Bearer",
            Scheme = "Bearer",
        },
        []
    }
};
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", info);
    o.AddSecurityDefinition("Keycloak", securityScheme);
    o.AddSecurityRequirement(securityReq);
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.Audience = builder.Configuration["Authentication:Audience"];
        x.MetadataAddress = builder.Configuration["Authentication:MetaDataAddress"]!;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Authentication:Issuer"],
            ValidAudience = builder.Configuration["Authentication:Audience"]
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    Console.WriteLine("Using in Postgres DB");
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection"));
});
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<IUserEventRepo, UserEventRepo>();
builder.Services.AddSingleton<IEventProcessor, EventProcessor>();
builder.Services.AddControllers();
builder.Services.AddHostedService<MessageBusSubscriber>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
prepDb.PrepPopulation(app, environment.IsProduction());
// app.UseHttpsRedirection();
app.UseCors(corsConfig);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();