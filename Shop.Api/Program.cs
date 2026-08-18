using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.OData;
using Shop.Api.Filters;
using Shop.Api.Middleware;
using Shop.Api.OData;
using Shop.Api.OpenApi;
using Shop.Application;
using Shop.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// OData is intentionally excluded from API versioning and its controller is
// marked ApiVersionNeutral, so the versioned-OData integration is not required.
builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
        options.ReportApiVersions = true;
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

// Add OData services
builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ValidationFilter>();
        options.Filters.Add<ApiResultFilter>();
    })
    .AddOData(options =>
    {
        options.Select()
            .Filter()
            .OrderBy()
            .Expand()
            .Count()
            .SetMaxTop(100)
            .AddRouteComponents("odata", ODataEdmModelBuilder.GetEdmModel());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DisplayRequestDuration();

        foreach (var description in app.DescribeApiVersions())
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant());
        }
    });
}

app.UseHttpsRedirection();

// ApiResultMiddleware is outside the exception handler so both normal action
// results and errors produced by ExceptionHandlingMiddleware share one envelope.
//app.UseMiddleware<ApiResultMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
