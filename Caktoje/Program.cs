using System.Text.Json.Serialization;
using DotNetEnv;
using Caktoje.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Serilog;
using Caktoje.Models;
using Caktoje.Services;
using Caktoje.Services.Admin;

var builder = WebApplication.CreateBuilder(args);

var envPath = Path.Combine(builder.Environment.ContentRootPath, ".env");
if (System.IO.File.Exists(envPath))
{
    Env.Load(envPath);
}else
{
    throw new CriticalConfigurationException(".env file is missing.");
}

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddScoped<StorageService>();
builder.Services.AddScoped<AuthorAdminService>();
builder.Services.AddScoped<CategoryAdminService>();
builder.Services.AddScoped<BookAdminService>();
builder.Services.AddScoped<BookStockAdminService>();
builder.Services.AddScoped<BookRentAdminService>();
builder.Services.AddScoped<DayClosedAdminService>();
builder.Services.AddScoped<PutwallAdminService>();
builder.Services.AddScoped<PutwallSectionAdminService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");


Console.WriteLine($"Using connection string: {connectionString}");

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
//builder.Services.AddIdentityApiEndpoints<User>().AddEntityFrameworkStores<KeybeeDbContext>();
    
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddHttpContextAccessor();

builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext();
});

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// app.MapGroup("/twilio").ValidateTwilioRequest();

app.UseMiddleware<ExceptionHandlingMiddleware>();

using (var scope = app.Services.CreateScope())
{
    // var db = scope.ServiceProvider.GetRequiredService<CaktojeDb>();
    // var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    
    // await db.Database.EnsureCreatedAsync();

    // await Seeder.SeedAdminUserAsync(scope.ServiceProvider);
    // await Seeder.SeedBasicDataAsync(scope.ServiceProvider);
}

var filePath = Path.Combine(builder.Environment.ContentRootPath, builder.Configuration["Files:MessageAttachmentsPath"] ?? throw new CriticalConfigurationException("MessageAttachmentsPath is not configured in Files section"));

if (!Directory.Exists(filePath))
{
    Directory.CreateDirectory(filePath);
}


app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(filePath),
    RequestPath = builder.Configuration["Files:MessageAttachmentsRelativeUrl"] ?? throw new CriticalConfigurationException("MessageAttachmentsRelativeUrl is not configured in Files section")
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

//alllow any origin for testing purposes
app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseAuthentication();
app.MapControllers();
app.UseAuthorization();

app.MapIdentityApi<User>().AddEndpointFilter(async (context, next) =>
   {
       if (context.HttpContext.Request.Path.Value?.EndsWith("/register", StringComparison.OrdinalIgnoreCase) == true)
       {
           if (context.HttpContext.User.Identity?.IsAuthenticated != true || 
               !context.HttpContext.User.IsInRole("Admin"))
           {
               return Results.Forbid(); 
           }
       }
       return await next(context);
   });

app.Run();