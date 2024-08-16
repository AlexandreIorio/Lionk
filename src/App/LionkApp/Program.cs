// Copyright � 2024 Lionk Project

using System.Reflection;
using Lionk.Auth.Abstraction;
using Lionk.Auth.Identity;
using Lionk.Auth.Utils;
using Lionk.Core.Component;
using Lionk.Core.Model.Component.Cyclic;
using Lionk.Core.Service;
using Lionk.Core.TypeRegistery;
using Lionk.Log;
using Lionk.Log.Serilog;
using Lionk.Plugin;
using Lionk.Plugin.Blazor;
using Lionk.TemperatureSensor;
using Lionk.Utils;
using LionkApp.Components;
using LionkApp.Components.Layout;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using Newtonsoft.Json;
using ILoggerFactory = Lionk.Log.ILoggerFactory;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on all IP addresses and port 5000
// builder.WebHost.UseKestrel(options => options.Listen(System.Net.IPAddress.Any, 5000));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();

// Add Theme
builder.Services.AddScoped<LionkPalette>();

// Add Basic Authentication services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<UserServiceRazor>();
builder.Services.AddSingleton<IUserRepository, UserFileHandler>();
builder.Services.AddSingleton<IUserService>(sp => new UserService(sp.GetRequiredService<IUserRepository>()));

builder.Services.AddScoped(sp =>
    new UserAuthenticationStateProvider(
        sp.GetRequiredService<UserServiceRazor>(),
        sp.GetRequiredService<IUserService>()));

builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<UserAuthenticationStateProvider>());

// Configure custom logger
builder.Services.AddSingleton<ILoggerFactory, SerilogFactory>();

// Register PluginManager as both IPluginManager and ITypesProvider
builder.Services.AddSingleton<IPluginManager, PluginManager>();

builder.Services.AddSingleton(
    new FileUploadService(
        Lionk.Utils.ConfigurationUtils.GetFolderPath(
            Lionk.Utils.FolderType.Plugin)));

// Register ComponentService with a factory to resolve ITypesProvider
builder.Services.AddSingleton<IComponentService>(serviceProvider =>
{
    ITypesProvider typesProvider = serviceProvider.GetRequiredService<IPluginManager>();
    return new ComponentService(typesProvider);
});

// Register CyclicExecutorService with a factory to resolve IComponentService
builder.Services.AddSingleton<ICyclicExecutorService>(serviceProvider =>
{
    IComponentService componentService = serviceProvider.GetRequiredService<IComponentService>();
    return new CyclicExecutorService(componentService);
});
WebApplication app = builder.Build();

// Give all services access to the service provider
ServiceProviderAccessor.ServiceProvider = app.Services;

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

// Component simulation
SimulatedTemperatureSensor simSensor = new();
DS18B20 dsSensor = new();
simSensor.InstanceName = "Simulated Sensor";
dsSensor.InstanceName = "DS18B20 Sensor";

IComponentService componentService = app.Services.GetRequiredService<IComponentService>();
componentService.RegisterComponentInstance(simSensor);
componentService.RegisterComponentInstance(dsSensor);

ICyclicExecutorService cyclicExecutorService = app.Services.GetRequiredService<ICyclicExecutorService>();
cyclicExecutorService.Start();

// Dashboard savedComponent simulation
Type sensor1 = typeof(TemperatureSensorWidget);
Type sensor2 = typeof(TemperatureSensorWidget);

Dictionary<string, string> param1 = new();
Dictionary<string, string> param2 = new();

var sensors = componentService.GetInstancesOfType<ITemperatureSensor>().ToList();

param1.Add("Sensor", sensors[0]?.InstanceName ?? string.Empty);
param2.Add("Sensor", sensors[1]?.InstanceName ?? string.Empty);

DashBoardItemModel item1 = new() { View = sensor1, PropertyAndInstanceName = param1 };
DashBoardItemModel item2 = new() { View = sensor2, PropertyAndInstanceName = param2 };

List<DashBoardItemModel> list = new();

list.Add(item1);
list.Add(item2);

string dashboradJson = Newtonsoft.Json.JsonConvert.SerializeObject(list, Formatting.Indented);

ConfigurationUtils.SaveFile("dashboard.json", dashboradJson, FolderType.Data);

// Load all assemblies
LoadAllAssemblies();

#if DEBUG
// Configure a default user for debug purposes if compiled in debug mode
SetupDebugUser(app);
#endif

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

static void LoadAllAssemblies()
{
    var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
    string currentPath = AppDomain.CurrentDomain.BaseDirectory;
    string[] allFiles = Directory.GetFiles(currentPath, "*.dll");

    foreach (string dll in allFiles)
    {
        var assemblyName = AssemblyName.GetAssemblyName(dll);
        if (!loadedAssemblies.Any(a => a.FullName == assemblyName.FullName))
        {
            Assembly.Load(assemblyName);
        }
    }

    var fraichementLoadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();

    foreach (Assembly? assembly in fraichementLoadedAssemblies)
    {
        Type[] types = assembly.GetTypes();
        foreach (Type type in types)
        {
            type.GetCustomAttribute<ComponentView>();
        }
    }
}

static void SetupDebugUser(WebApplication app)
{
    IUserService userService = app.Services.GetRequiredService<IUserService>();

    // To use debug users, set "dadmin" or "duser" as username and "password" as password.
    string adminUsername = "dadmin";
    List<string> adminRoles = ["Admin"];
    string adminEmail = "debugAdmin@email.com";
    User? admin = userService.GetUserByUsername(adminUsername);
    if (admin is not null) userService.Delete(admin);

    string userUsername = "duser";
    List<string> userRoles = ["User"];
    string userEmail = "debugUser@email.com";
    User? user = userService.GetUserByUsername(userUsername);
    if (user is not null) userService.Delete(user);

    string salt = "salt";
    string password = "password";
    string passwordHash = PasswordUtils.HashPassword(password, salt);

    admin = new(adminUsername, adminEmail, passwordHash, salt, adminRoles);
    user = new(userUsername, userEmail, passwordHash, salt, userRoles);

    admin = userService.Insert(admin);
    user = userService.Insert(user);

    if (admin is null) throw new Exception("Failed to create admin user");
    if (admin is null) throw new Exception("Failed to create simple user");
}
