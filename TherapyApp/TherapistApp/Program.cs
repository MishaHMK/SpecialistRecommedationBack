using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using TherapyApp;
using TherapyApp.Entities;
using TherapyApp.Extensions;
using TherapyApp.Helpers.Mapper;
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

builder.Services.AddHttpClient<ChatGptService>();

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IJWTService, JWTService>();
builder.Services.AddScoped<ICsvService, CsvService>();
builder.Services.AddScoped<MLModelTrainer>();
builder.Services.AddScoped<MLModelPredictor>();
builder.Services.AddSingleton<JsonWebTokenHandler>();
builder.Services.AddAutoMapper(typeof(UsersProfile));
builder.Services.ConfigureJwt(builder); 
builder.Services.ConfigureGpt(builder);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddJwtAuthentication(builder);
builder.Services.AddSwaggerServices();

builder.Services.AddScoped<ChatGptService>();

builder.Services.AddHangfire(config =>
{
    config.UseSqlServerStorage(connectionString);
});

builder.Services.AddHangfireServer();

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

app.UseHangfireDashboard("/dash");

RecurringJob.AddOrUpdate<MLModelTrainer>(
    recurringJobId: "TrainModelDaily",
    methodCall: trainer => trainer.TrainModelFromCsv("training_data.csv", "therapist_model.zip"),
    cronExpression: Cron.Daily
);

app.UseCors(options => options.WithOrigins("http://localhost:3000")
                                .AllowAnyMethod().AllowAnyHeader().AllowCredentials());
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
    
app.Run();
