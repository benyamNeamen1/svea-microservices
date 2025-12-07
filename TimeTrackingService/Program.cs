using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Svea.TimeTrackingService.Data;
using Svea.TimeTrackingService.Messaging;
using Svea.TimeTrackingService.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TimeDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TimeDb")));

builder.Services.AddScoped<IWorkService, WorkService>();


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });

builder.Services.AddControllers();


builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and your token"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference 
                { 
                    Type = ReferenceType.SecurityScheme, 
                    Id = "Bearer" 
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddSingleton<TimeTrackingRabbitMq>();


builder.Services.AddHostedService(provider =>
    provider.GetRequiredService<TimeTrackingRabbitMq>());


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TimeDbContext>();
    var maxRetries = 10;
    var delay = TimeSpan.FromSeconds(5);
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            db.Database.Migrate(); 
            Console.WriteLine("Database migrated successfully.");
            break;
        }
        catch
        {
            Console.WriteLine($"Database not ready yet. Retry {i + 1}/{maxRetries}...");
            if (i == maxRetries - 1) throw; 
            await Task.Delay(delay);
        }
    }
}

app.UsePathBase("/api/timetracking");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok("Healthy"));

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/api/timetracking/swagger/v1/swagger.json", "Time API V1");
    c.RoutePrefix = "swagger";
});


app.Run();
