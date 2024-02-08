using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Asp.Versioning.Conventions;
using NSwag.AspNetCore;
using WeatherForecastController = testSwagger.Controllers;

namespace testSwagger;

public class Startup {
    public void ConfigureServices(IServiceCollection services) {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.ConfigureVersioning()
            .AddMvc(options => {
                var versions = new ApiVersioningDefinition().Versions;
                var controllerVersion1Builder = options.Conventions.Controller<Controllers.v1.WeatherForecastController>();
                controllerVersion1Builder
                    .HasApiVersion(1.0);
                var controllerVersion2Builder = options.Conventions.Controller<WeatherForecastController.WeatherForecastController>();
                controllerVersion2Builder
                    .HasApiVersion(2.0);
            })
            .AddApiExplorer(options => {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
        services.ConfigureSwagger("testSwagger");
    }
    
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider) {
        if (env.IsDevelopment())
        {
            app.UseOpenApi();
            app.UseSwaggerUi(settings => {
                foreach (var description in provider.ApiVersionDescriptions.Reverse()) {
                    settings.SwaggerRoutes.Add(new SwaggerUiRoute(description.GroupName.ToUpperInvariant(),
                        $"/swagger/{description.GroupName}/swagger.json"));
                }
            });
        }

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints => {
            endpoints.MapControllers();
        });
    }
}

public static class VersioningStartup {
    public static IApiVersioningBuilder ConfigureVersioning(this IServiceCollection services) {
        return services.AddApiVersioning(options => {
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(ApiVersioning.Current, 0);
        });
    }
}

public class ApiVersioningDefinition(int min = ApiVersioning.Min, int max = ApiVersioning.Current) {
    public List<ApiVersion> Versions { get; } = ApiVersioning.Versions(min, max);
}

public static class ApiVersioning {
    public const int Min = 1;
    public const int Current = 2;
    public static List<ApiVersion> Versions(int min = Min, int max = Current) {
        return Enumerable.Range(min, max-min+1).Select(x => new ApiVersion(x, 0)).ToList();
    }
}


public static class SwaggerStartUp {
    public static IServiceCollection  ConfigureSwagger(this IServiceCollection services, string productName) {
        for (var v = ApiVersioning.Min; v <= ApiVersioning.Current; v++) {
            AddSwaggerDocFor(services, productName, v);
        }
        return services;
    }

    private static void AddSwaggerDocFor(IServiceCollection services, string productName, int v) {
        var documentName = $"v{v}";
        services.AddSwaggerDocument(document => {
            document.DocumentName = documentName;
            document.Title = productName.ToUpper();
            document.Version = documentName;
            document.ApiGroupNames = new[] { $"v{v}" };
            document.PostProcess = apiDocument => {
                apiDocument.Info.Version = documentName;
            };
        });
    }
}