using DashboardAPI.Context;
using DashboardAPI.Models.Account;
using DashboardAPI.Services;
using DashboardAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("MyCorsPolicy", builder =>
    {
        builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
    });
});


builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IBalanceCalculator, BalanceCalculator>();

builder.Services.AddDbContext<DashboardDbContext>(
    options => options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default")));

builder.Services.AddDbContext<DashboardIdentityDbContext>(
    options => options.UseNpgsql(
        builder.Configuration.GetConnectionString("Identity")));

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<DashboardIdentityDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.RequireUniqueEmail = true;
});

var app = builder.Build();

app.UseCors("MyCorsPolicy");
app.UseRouting();

app.UseStaticFiles(new StaticFileOptions // Настройка для папки uploads
{
    FileProvider = new PhysicalFileProvider(
            Path.Combine(builder.Environment.ContentRootPath, "uploads") // Путь к папке uploads
        ),
    RequestPath = "/uploads" // Виртуальный путь для доступа к файлам
});

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<DashboardDbContext>();
        var dbIdentityContext = scope.ServiceProvider.GetRequiredService<DashboardIdentityDbContext>();

        dbContext.Database.Migrate();
        dbIdentityContext.Database.Migrate();

        await DbInitializer.Seed(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}



app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();