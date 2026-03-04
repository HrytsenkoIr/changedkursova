using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OnlineStoreSystem.EFModels;
using OnlineStoreSystem.Repositories;
using OnlineStoreSystem.Repositories.Interfaces;
using OnlineStoreSystem.Services;
using OnlineStoreSystem.Constants; // Додано
using System.Security.Claims;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.SlidingExpiration = true;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() 
            ? CookieSecurePolicy.None 
            : CookieSecurePolicy.Always;
    });

builder.Services.AddAuthorization(options =>
{
    // Використання констант замість рядків
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim(ClaimTypes.Role, UserRoles.Admin));

    options.AddPolicy("ManagerOrAdmin", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => 
                c.Type == ClaimTypes.Role && 
                (c.Value == UserRoles.Admin || c.Value == UserRoles.Manager))));

    options.AddPolicy("WorkerOrAbove", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => 
                c.Type == ClaimTypes.Role && 
                (c.Value == UserRoles.Admin || c.Value == UserRoles.Manager || c.Value == UserRoles.Worker))));
});

builder.Services.AddDbContext<OnlineStoreDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("OnlineStore"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    ));

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

builder.Services.AddScoped<DatabaseConnection>();
builder.Services.AddScoped<StoredProcedureService>();
builder.Services.AddScoped<MigrationRunner>(); 

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() 
        ? CookieSecurePolicy.None 
        : CookieSecurePolicy.Always;
});

builder.Services.AddMemoryCache();

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Information);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    try
    {
        using var scope = app.Services.CreateScope();
        var migrationRunner = scope.ServiceProvider.GetRequiredService<MigrationRunner>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Запуск міграцій...");
        await migrationRunner.RunMigrationsAsync();
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Помилка під час виконання міграцій");
    }
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(name: "areas", pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == 401)
    {
        context.Response.Redirect("/Account/Login?returnUrl=" + WebUtility.UrlEncode(context.Request.Path));
    }
    else if (context.Response.StatusCode == 403)
    {
        context.Response.Redirect("/Account/AccessDenied");
    }
});

app.Run();