using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Svea.UserService.Data;
using Svea.UserService.Messaging;
using Svea.UserService.Services;
using System.Text;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("UserDb")));

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IUserValidationService, UserValidationService>();

builder.Services.AddSingleton<UserServiceRabbitMq>();

builder.Services.AddHostedService(provider =>
    provider.GetRequiredService<UserServiceRabbitMq>());



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
builder.Services.AddSwaggerGen();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    var maxRetries = 10;
    var delay = TimeSpan.FromSeconds(5);
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            db.Database.Migrate();
            Console.WriteLine("Database migrated successfully.");
            await DataSeeder.SeedAsync(db);
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
app.UsePathBase("/users");


app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok("Healthy"));

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/users/swagger/v1/swagger.json", "User API V1");
    c.RoutePrefix = "swagger";
});

app.Run();
