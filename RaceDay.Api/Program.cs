var builder = WebApplication.CreateBuilder(args);

var startup = new RaceDay.Api.Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();
startup.Configure(app, app.Environment);

app.Run();

/// <summary>Exposes the implicit Program class for integration tests.</summary>
public partial class Program { }
