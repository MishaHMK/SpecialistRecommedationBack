using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TherapyApp.Helpers.Secrets;

namespace TherapyApp.Extensions;

public static class AuthenticationServiceExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, WebApplicationBuilder builder)
    {
        var jwtOptions = builder.Services.BuildServiceProvider().GetRequiredService<IOptions<JwtVariables>>();
        JwtVariables environment = jwtOptions.Value;

        builder.Services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(o =>
        {
            o.RequireHttpsMetadata = false;
            o.TokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(environment.Secret)),
                ValidIssuer = environment.Issuer,
                ValidAudience = environment.Audience,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };
        });

        return services;
    }

    public static void AddSwaggerServices(this IServiceCollection services)
    {
        var securityType = "Bearer";

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(opt =>
        {
            opt.SwaggerDoc("v1", new OpenApiInfo { Title = "MyApi", Version = "v1" });
            opt.CustomSchemaIds(x => x.FullName);
            opt.AddSecurityDefinition(securityType, new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "JWT Authentication",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = securityType
            });

            opt.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = securityType
                        },
                        Scheme = "oauth2",
                        Name = securityType,
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
        });
        });
    }
}