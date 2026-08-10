using FluentValidation;
using Shop.Api.Filters;
using Shop.Api.Middleware;
using Shop.Application;
using Shop.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
    //options.Filters.Add<ApiResultFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(opt => { opt.DisplayRequestDuration(); });
}

app.UseHttpsRedirection();

// ApiResultMiddleware is outside the exception handler so both normal action
// results and errors produced by ExceptionHandlingMiddleware share one envelope.
app.UseMiddleware<ApiResultMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();