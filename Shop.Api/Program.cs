using Shop.Api;
using Shop.Api.Extensions;
using Shop.Api.Middleware;
using Shop.Application;
using Shop.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddApi();

var app = builder.Build();

await app.SeedIdentityAsync();

app.UseApiDocumentation();
app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
