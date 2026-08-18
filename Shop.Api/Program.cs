using Asp.Versioning;
using FluentValidation;
// using Microsoft.AspNetCore.OData;
using Scalar.AspNetCore;
using Shop.Api.Filters;
using Shop.Api.Middleware;
// using Shop.Api.OData;
using Shop.Api.OpenApi;
using Shop.Application;
using Shop.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
        options.ReportApiVersions = true;
        options.AssumeDefaultVersionWhenUnspecified = true;
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    })
    .AddOpenApi(options => options.Document.AddScalarTransformers());

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
    options.Filters.Add<ApiResultFilter>();
});

// OData is temporarily disabled.
// builder.Services.AddControllers()
//     .AddOData(options =>
//     {
//         options.Select()
//             .Filter()
//             .OrderBy()
//             .Expand()
//             .Count()
//             .SetMaxTop(100)
//             .AddRouteComponents("odata", ODataEdmModelBuilder.GetEdmModel());
//     });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().WithDocumentPerVersion();

    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Shop API");

        var descriptions = app.DescribeApiVersions();

        for (var index = 0; index < descriptions.Count; index++)
        {
            var description = descriptions[index];
            //var isDefault = index == descriptions.Count - 1;
            var isDefault = index == 0;

            options.AddDocument(
                description.GroupName,
                description.GroupName.ToUpperInvariant(),
                isDefault: isDefault);
        }
    });

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