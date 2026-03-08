using System.Text.Json.Serialization;
using DotNetEnv;
using Caktoje.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Serilog;
using Caktoje.Models;
using Caktoje.Services;
using Caktoje.Services.Admin;
using Caktoje.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var envPath = Path.Combine(builder.Environment.ContentRootPath, ".env");
if (System.IO.File.Exists(envPath))
{
    Env.Load(envPath);
}else
{
    throw new CriticalConfigurationException(".env file is missing.");
}
builder.Services.AddDbContext<CaktojeDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Configuration.AddEnvironmentVariables();


builder.Services.AddHttpClient();
builder.Services.AddScoped<StorageService>();
builder.Services.AddScoped<OpenLibraryService>();
builder.Services.AddScoped<AuthorAdminService>();
builder.Services.AddScoped<CategoryAdminService>();
builder.Services.AddScoped<BookAdminService>();
builder.Services.AddScoped<BookStockAdminService>();
builder.Services.AddScoped<BookRentAdminService>();
builder.Services.AddScoped<DayClosedAdminService>();
builder.Services.AddScoped<PutwallAdminService>();
builder.Services.AddScoped<PutwallSectionAdminService>();
builder.Services.AddScoped<UserAdminService>();

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
builder.Services.AddIdentityApiEndpoints<User>((options)=>{
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddRoles<Role>()
    .AddEntityFrameworkStores<CaktojeDbContext>();

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
        .Enrich.FromLogContext()
        .WriteTo.Console(); 
});
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

using (var scope = app.Services.CreateScope())
{   
    await Seeder.Seed(scope.ServiceProvider);
    // await Seeder.SeedBasicDataAsync(scope.ServiceProvider);
}

var filePath = Path.Combine(builder.Environment.ContentRootPath, builder.Configuration["Files:ImagesPath"] ?? throw new CriticalConfigurationException("Images is not configured in Files section"));

Console.WriteLine($"Serving static files from: {filePath}");

if (!Directory.Exists(filePath))
{
    Directory.CreateDirectory(filePath);
}

app.UseCors();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(filePath),
    RequestPath = $"/{builder.Configuration["Files:ImagesPath"]}" ?? throw new CriticalConfigurationException("ImagesPath is not configured in Files section")
});


app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();

app.UseAuthentication();
app.MapControllers();

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