using Abysalto.StefanParch.Api.Extensions;
using Abysalto.StefanParch.Api.Middleware;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseApplicationSerilog();

try
{
    builder.Services
        .AddApplicationServices(builder.Configuration)
        .AddPresentation()
        .AddSwaggerDocumentation();

    var app = builder.Build();

    app.UseApplicationRequestLogging();
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "Application terminated unexpectedly.");
}
finally
{
    await Log.CloseAndFlushAsync();
}
