using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using MQTTnet.AspNetCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Serilog;
using Serilog.Settings.Configuration;
using System.Globalization;
using Entrvo.DAL;
using Entrvo.Identity;
using Entrvo.Identity.EntityFramework;
using T2WebApplication;
using T2WebApplication.Hubs;
using T2WebApplication.Identity;
using T2WebApplication.Services;
using Entrvo.Services;

var builder = WebApplication.CreateBuilder(args);

var aspnetEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

if (aspnetEnvironment == "Production")
{
  var options = new WebApplicationOptions()
  {
    ContentRootPath = AppContext.BaseDirectory,
    ApplicationName = System.Diagnostics.Process.GetCurrentProcess().ProcessName,
    Args = args
  };

  builder = WebApplication.CreateBuilder(options);
  builder.Host.UseWindowsService();
}

var logbookService = new LogbookService(CultureInfo.InvariantCulture);

builder.Host.UseSerilog((ctx, lc) =>
{
  var path = AppDomain.CurrentDomain.BaseDirectory;
  var logFileTemplate = Path.Combine(path, "Logs", "log.txt");
  var outputTemplate = @"===> [{Timestamp:HH:mm:ss} {Level} {SourceContext}] {Message:lj}{NewLine}{Exception}";
  var configuration = new ConfigurationBuilder()
        .AddJsonFile(path + "appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile(path + $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build();
  var options = new ConfigurationReaderOptions(typeof(ConsoleLoggerConfigurationExtensions).Assembly, typeof(FileLoggerConfigurationExtensions).Assembly);
  lc.ReadFrom.Configuration(configuration, options);
  lc.WriteTo.File(logFileTemplate, outputTemplate: outputTemplate, rollingInterval: RollingInterval.Day);
  lc.WriteTo.Console();
  lc.WriteTo.LogbookService(logbookService);
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
  serverOptions.ConfigureEndpoints();
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");;
// Add framework services.
builder.Services
	.AddRazorPages()
	// Maintain property names during serialization. See:
	// https://github.com/aspnet/Announcements/issues/194
	.AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver());
// Add Kendo UI services to the services container
builder.Services.AddKendo();


builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
  options.UseSqlServer(connectionString);
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<CustomerPortalUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager<ApplicationSignInManager>()
    .AddUserStore<UserStore<CustomerPortalUser, CustomerPortalUserLogin>>();

var jsonOptions = new Action<MvcNewtonsoftJsonOptions>(o =>
{
  o.UseCamelCasing(true);
  o.SerializerSettings.Converters.Add(new StringEnumConverter());         // can not use string enum because of Kendo ui controls
  o.SerializerSettings.ContractResolver = new DefaultContractResolver();    // has to use DefaultContractResolver because of Kendo ui controls
  o.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
  o.AllowInputFormatterExceptionMessages = true;
});

builder.Services.AddControllers().AddNewtonsoftJson(jsonOptions);
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation().AddControllersAsServices().AddNewtonsoftJson(jsonOptions);

var configuration = builder.Configuration;
builder.Services.AddAuthentication();
  //.AddGoogle(googleOptions =>
  //{
  //  googleOptions.ClientId = configuration["Authentication:Google:ClientId"];
  //  googleOptions.ClientSecret = configuration["Authentication:Google:ClientSecret"];
  //})
  //.AddMicrosoftAccount(microsoftOptions =>
  //{
  //  microsoftOptions.ClientId = configuration["Authentication:Microsoft:ClientId"];
  //  microsoftOptions.ClientSecret = configuration["Authentication:Microsoft:ClientSecret"];
  //});



builder.Services.Configure<CookiePolicyOptions>(options =>
{
  options.CheckConsentNeeded = context => true;
  options.MinimumSameSitePolicy = SameSiteMode.None;
});
builder.Services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();
builder.Services.Configure<CookieTempDataProviderOptions>(options =>
{
  options.Cookie.IsEssential = true;
});


builder.Services.AddHealthChecks();
builder.Services.ConfigureHttpsRedirection(builder.Configuration.GetSection("HttpServer:Endpoints"));

builder.Services.AddTransient<IClaimsTransformation, ClaimsTransformer<CustomerPortalUser>>();
builder.Services.AddTransient<ITransformService, TransformService>();
builder.Services.AddTransient<IConsumerService, ConsumerService>();
builder.Services.AddTransient<IFtpDownloadService, FtpDownloadService>();

builder.Services.AddScoped<IParkingApi, ParkingApi>();

builder.Services.AddSingleton<IImportingService, ImportingService>();

builder.Services.AddMqttService();
builder.Services.AddScheduleService(builder.Configuration.GetSection("Schedule"));
builder.Services.AddSettingsService();
builder.Services.AddHttpContextAccessor();
builder.Services.AddChannelSink(logbookService);
builder.Services.AddSignalR();
builder.Services.AddEmailService(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddWindowsService();

builder.Services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();
builder.Services.AddSession();
builder.Services.AddDistributedMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseMigrationsEndPoint();
}
else
{
  app.UseExceptionHandler("/Home/Error");
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}


if (app.Environment.IsProduction())
{
  app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseSession();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseRequestResponseLogging();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=customers}/{action=index}/{id?}");
app.MapRazorPages();
app.MapHub<ProgressHub>("/progressHub");
app.MapHub<LogbookHub>("/logbookHub");
app.MapHub<ImportProgressHub>("/importProgressHub");

app.UseMqttServer(server => app.Services.GetRequiredService<MqttService>().ConfigureMqttServer(server));


app.Run();
