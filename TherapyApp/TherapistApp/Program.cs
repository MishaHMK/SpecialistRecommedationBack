using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Text;
using TherapyApp;
using TherapyApp.Entities;
using TherapyApp.Extensions;
using TherapyApp.Helpers.ML;
using TherapyApp.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<TherapyDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddIdentity<AppUser, AppRole>()
    .AddEntityFrameworkStores<TherapyDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
{
    p.WithOrigins(
        "http://localhost:3000"
    );
    p.AllowAnyHeader();
    p.AllowAnyMethod();
    p.AllowCredentials();

    p.SetPreflightMaxAge(TimeSpan.FromDays(1));
}
));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme.\nExample: 'Bearer {your token}'"
    });

    o.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

builder.Services.AddTransient<IAccountService, AccountService>();
builder.Services.AddTransient<IJWTService, JWTService>();
builder.Services.AddScoped<ICsvService, CsvService>();
builder.Services.AddScoped<MLModelTrainer>();
builder.Services.AddScoped<MLModelPredictor>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        var Key = Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]);
        o.SaveToken = true;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Key)
        };

        o.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accsessToken = context.Request.Query["access_token"];

                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accsessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accsessToken;
                }

                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddControllersWithViews().AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
);

var app = builder.Build();

//await app.SeedDataAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(options => options.WithOrigins("http://localhost:3000")
                                .AllowAnyMethod().AllowAnyHeader().AllowCredentials());
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
    
app.Run();
