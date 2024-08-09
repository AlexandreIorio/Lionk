// Copyright � 2024 Lionk Project

using Lionk.Auth.Identity;
using Lionk.Auth.Utils;
using Lionk.Core.Component;
using Lionk.Core.TypeRegistery;
using Lionk.Log;
using Lionk.Log.Serilog;
using Lionk.Plugin;
using LionkApp.Components;
using LionkApp.Components.Layout;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using ILoggerFactory = Lionk.Log.ILoggerFactory;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

#if DEBUG

// To use debug users, set "dadmin" or "duser" as username and "password" as password.
string adminUsername = "dadmin";
List<string> adminRoles = new() { "Admin" };
string adminEmail = "debugAdmin@email.com";
User? admin = UserService.GetUserByUsername(adminUsername);
if (admin is not null) UserService.Delete(admin);

string userUsername = "duser";
List<string> userRoles = new() { "User" };
string userEmail = "debugUser@email.com";
User? user = UserService.GetUserByUsername(userUsername);
if (user is not null) UserService.Delete(user);

string salt = "salt";
string password = "password";
string passwordHash = PasswordUtils.HashPassword(password, salt);

admin = new(adminUsername, adminEmail, passwordHash, salt, adminRoles);
user = new(userUsername, userEmail, passwordHash, salt, userRoles);

admin = UserService.Insert(admin);
user = UserService.Insert(user);

if (admin is null) throw new Exception("Failed to create admin user");
if (admin is null) throw new Exception("Failed to create simple user");
#endif

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();

// Add Theme
builder.Services.AddScoped<LionkPalette>();

// Add Basic Authentication services
builder.Services.AddScoped<UserServiceRazor>();
builder.Services.AddScoped<UserAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<UserAuthenticationStateProvider>());

// Configure custom logger
builder.Services.AddSingleton<ILoggerFactory, SerilogFactory>();

// Register PluginManager as both IPluginManager and ITypesProvider
builder.Services.AddSingleton<IPluginManager, PluginManager>();

// Register ComponentService with a factory to resolve ITypesProvider
builder.Services.AddSingleton<IComponentService>(serviceProvider =>
{
    ITypesProvider typesProvider = serviceProvider.GetRequiredService<IPluginManager>();
    return new ComponentService(typesProvider);
});

WebApplication app = builder.Build();

// Configure the LogService with the singleton logger
ILoggerFactory loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
LogService.Configure(loggerFactory);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);

    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
